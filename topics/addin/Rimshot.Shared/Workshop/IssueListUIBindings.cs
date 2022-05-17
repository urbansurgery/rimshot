using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.DocumentParts;
using CefSharp;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Models;
using Speckle.Core.Transports;
using Speckle.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Color = System.Drawing.Color;
using ComApi = Autodesk.Navisworks.Api.Interop.ComApi;
using ComBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge; //
using Navis = Autodesk.Navisworks.Api.Application;
using NavisworksApp = Autodesk.Navisworks.Api.Application;

namespace Rimshot.Shared.Workshop {
  public abstract class UIBindings {
#if DEBUG
    const string rimshotUrl = "https://rimshot.app/issues";
#else
    const String rimshotUrl = "https://rimshot.app/issues";
#endif
    public const string Url = rimshotUrl;
    private const int V = 131004;
    private string _tempFolder = "";




    public IWebBrowser Browser { get; set; }
    public IssueListPane Window { get; set; }
    private string image;

    // base64 encoding of the image
    public virtual string GetImage () => this.image;

    // base64 encoding of the image
    public virtual void SetImage ( string value ) => this.image = value;

    public virtual void NotifyUI ( string eventName, dynamic eventInfo ) {
      string script = string.Format( "window.EventBus.$emit('{0}',{1})", eventName, JsonConvert.SerializeObject( eventInfo ) );
      Browser.GetMainFrame().EvaluateScriptAsync( script );
    }

    public virtual void DispatchStoreActionUI ( string storeActionName, string args = null ) {
      string script = string.Format( "window.Store.dispatch('{0}', '{1}')", storeActionName, args );
      Browser.GetMainFrame().EvaluateScriptAsync( script );
    }

    public virtual void ShowDev () => this.Browser.ShowDevTools();
    public virtual void Refresh ( bool force = false ) {
      this.Browser.Reload( force );
      this.Browser.GetMainFrame().LoadUrl( Url );
    }

    public virtual void AddImage () => this.SendIssueView();

    public virtual void CommitSelection ( object payload ) => this.CommitSelectedObjectsToSpeckle( payload );
    public virtual string UpdateView ( string camera ) {
      Console.WriteLine( camera );

      return camera;
    }

    private async void CommitSelectedObjectsToSpeckle ( object payload ) {

      Type payloadType = payload.GetType();

      PropertyInfo streamProp = payloadType.GetProperty( "stream" );
      PropertyInfo hostProp = payloadType.GetProperty( "host" );
      PropertyInfo branchProp = payloadType.GetProperty( "branch" );
      PropertyInfo issueProp = payloadType.GetProperty( "issueId" );

      dynamic commitPayload = ( dynamic )payload;

      string branchName = commitPayload.branch;
      string streamId = commitPayload.stream;
      string host = commitPayload.host;
      string issueId = commitPayload.issueId;

      string description = $"issueId:{issueId}";

      Account defaultAccount = AccountManager.GetDefaultAccount();

      Client client = new Client( defaultAccount );

      // Get Branch and create if it doesn't exist.
      Branch branch;
      try {
        branch = await client.BranchGet( streamId, branchName );
        if ( branch is null ) {
          branch = await CreateBranch( client, streamId, branchName, description );
        }
      } catch ( Exception e ) {
        NotifyUI( "error", new { message = e.Message } );
      }

      branch = client.BranchGet( streamId, branchName, 1 ).Result;

      if ( branch != null ) {
        NotifyUI( "branch_updated", new { branch = branch.name, issueId = issueId } );
      }

      Document activeDocument = NavisworksApp.ActiveDocument;
      DocumentModels models = activeDocument.Models;

      ModelItemCollection selectedItems = NavisworksApp.ActiveDocument.CurrentSelection.SelectedItems;

      List<Base> translatedElements = new List<Base>();

      List<Base> Elements = new List<Base>();

      int elementCount = selectedItems.Count;

      for ( int e = 0; e < elementCount; e++ ) {
        ModelItem element = selectedItems[ e ];
        NotifyUI( "elements", JsonConvert.SerializeObject( new { current = e + 1, count = elementCount } ) );
        Elements.Add( TranslateElement( element ) );
      }

      Base myCommit = new Base();
      myCommit[ "@Elements" ] = Elements;

      string[] stringseparators = new string[] { "/" };
      myCommit[ "Issue Number" ] = branchName.Split( stringseparators, StringSplitOptions.None )[ 1 ];
      myCommit[ "applicationId" ] = issueId;
      ServerTransport transport = new ServerTransport( defaultAccount, streamId );
      string hash = Operations.Send( myCommit, new List<ITransport> { transport } ).Result;

      string commitId = client.CommitCreate( new CommitCreateInput() {
        branchName = branchName,
        message = "Rimshot issue commit.",
        objectId = hash,
        streamId = streamId,
        sourceApplication = "Rimshot"
      } ).Result;

      Commit commitObject = client.CommitGet( streamId, commitId ).Result;
      string referencedObject = commitObject.referencedObject;

      NotifyUI( "commit_sent", new { commit = commitId, issueId = issueId, stream = streamId, objectId = referencedObject } );

      client.Dispose();
    }

    public Base TranslateElement ( ModelItem element ) {
      Base elementBase = new Base();

      int descendantsCount = element.Descendants.Count();

      if ( element.HasGeometry && descendantsCount == 0 ) {
        List<Objects.Geometry.Mesh> geometryToSpeckle = TranslateGeometry( element );
        if ( geometryToSpeckle.Count > 0 ) {
          elementBase[ "displayValue" ] = geometryToSpeckle;
        }
      }

      List<Base> children = new List<Base>();

      if ( descendantsCount > 0 ) {
        for ( int c = 0; c < descendantsCount; c++ ) {
          ModelItem child = element.Descendants.ElementAt( c );
          NotifyUI( "nested", JsonConvert.SerializeObject( new { current = c + 1, count = descendantsCount } ) );
          children.Add( TranslateElement( child ) );
        }
        elementBase[ "@Elements" ] = children;
      }

      Dictionary<string, dynamic> propDict = new Dictionary<string, dynamic>();

      PropertyCategoryCollection propertyCategories = element.PropertyCategories;

      foreach ( PropertyCategory propCat in propertyCategories ) {

        DataPropertyCollection properties = propCat.Properties;

        foreach ( DataProperty prop in properties ) {
          string key = prop.CombinedName.ToString();

          dynamic propValue = null;

          switch ( prop.Value.DataType ) {
            case VariantDataType.Boolean: propValue = prop.Value.ToBoolean().ToString(); break;
            case VariantDataType.DisplayString: propValue = prop.Value.ToDisplayString(); break;
            case VariantDataType.IdentifierString: propValue = prop.Value.ToIdentifierString(); break;
            case VariantDataType.Int32: propValue = prop.Value.ToInt32().ToString(); break;
            case VariantDataType.Double: propValue = prop.Value.ToDouble().ToString(); break;
            case VariantDataType.DoubleAngle: propValue = prop.Value.ToDoubleAngle().ToString(); break;
            case VariantDataType.DoubleArea: propValue = prop.Value.ToDoubleArea().ToString(); break;
            case VariantDataType.DoubleLength: propValue = prop.Value.ToDoubleLength().ToString(); break;
            case VariantDataType.DoubleVolume: propValue = prop.Value.ToDoubleVolume().ToString(); break;
            case VariantDataType.DateTime: propValue = prop.Value.ToDateTime().ToString(); break;
            case VariantDataType.NamedConstant: propValue = prop.Value.ToNamedConstant().ToString(); break;
            case VariantDataType.Point3D: propValue = prop.Value.ToPoint3D().ToString(); break;
            case VariantDataType.None: break;

          }

          if ( propValue != null ) {
            if ( propDict.ContainsKey( key ) && propDict[ key ].GetType().IsArray ) {
              propDict[ key ].Add( propValue );
            } else if ( propDict.ContainsKey( key ) ) {
              dynamic existingValue = propDict[ key ];
              propDict[ key ] = new List<dynamic>();
              propDict[ key ].Add( existingValue );
              propDict[ key ].Add( propValue );
            } else {
              propDict.Add( key, propValue );
            }
          }
        }
      }

      elementBase[ "Properties" ] = propDict;

      return elementBase;
    }

    public List<Objects.Geometry.Mesh> TranslateGeometry ( ModelItem geom ) {

      NavisGeometry navisGeometry = new NavisGeometry( geom );
      List<Shared.CallbackGeomListener> cb = navisGeometry.GetFragments();

      List<Objects.Geometry.Mesh> baseMeshes = new List<Objects.Geometry.Mesh>();

      foreach ( Shared.CallbackGeomListener callback in cb ) {

        List<double> vertices = new List<double>();
        List<int> faces = new List<int>();
        List<NavisTriangle> Triangles = callback.Triangles;
        int triangleCount = Triangles.Count;

        if ( triangleCount > 0 ) {
          for ( int t = 0; t < triangleCount; t += 1 ) {

            NotifyUI( "triangles", JsonConvert.SerializeObject( new { current = t + 1, count = triangleCount } ) );

            double scale = 0.001;

            vertices.AddRange( new List<double>() { Triangles[ t ].Vertex1.X * scale, Triangles[ t ].Vertex1.Y * scale, Triangles[ t ].Vertex1.Z * scale } );
            vertices.AddRange( new List<double>() { Triangles[ t ].Vertex2.X * scale, Triangles[ t ].Vertex2.Y * scale, Triangles[ t ].Vertex2.Z * scale } );
            vertices.AddRange( new List<double>() { Triangles[ t ].Vertex3.X * scale, Triangles[ t ].Vertex3.Y * scale, Triangles[ t ].Vertex3.Z * scale } );

            // TODO: Move this back to Geometry.cs
            faces.Add( 0 );
            faces.Add( t * 3 );
            faces.Add( t * 3 + 1 );
            faces.Add( t * 3 + 2 );
          }


          Objects.Geometry.Mesh baseMesh = new Objects.Geometry.Mesh( vertices, faces );
          baseMesh[ "renderMaterial" ] = TranslateMaterial( geom );
          baseMeshes.Add( baseMesh );
        }
      }

      return baseMeshes;
    }

    public Objects.Other.RenderMaterial TranslateMaterial ( ModelItem geom ) {

      Color original = Color.FromArgb(
        Convert.ToInt32( geom.Geometry.OriginalColor.R * 255 ),
        Convert.ToInt32( geom.Geometry.OriginalColor.G * 255 ),
        Convert.ToInt32( geom.Geometry.OriginalColor.B * 255 ) );

      Objects.Other.RenderMaterial r = new Objects.Other.RenderMaterial( 1 - geom.Geometry.OriginalTransparency, 0, 1, original );

      return r;
    }

    public Task<Branch> CreateBranch ( Client client, string streamId, string branchName, string description ) {
      try {
        Task<string> branchId = client.BranchCreate( new BranchCreateInput() {
          streamId = streamId,
          name = branchName,
          description = description
        } );
      } catch ( Exception e ) {
        NotifyUI( "error", new { message = e.Message } );
      }

      Task<Branch> branch = client.BranchGet( streamId, branchName );

      return branch;
    }

    private void SendIssueView () {

      MemoryStream stream;
      byte[] imageBytes;
      Guid id = Guid.NewGuid();
      SavedViewpoint oNewViewPt1 = new SavedViewpoint( Navis.ActiveDocument.CurrentViewpoint.ToViewpoint() ) {
        Guid = id,
        DisplayName = string.Format( "View - {0}", id )
      };
      Navis.ActiveDocument.SavedViewpoints.AddCopy( oNewViewPt1 );
      Image image = new Image( oNewViewPt1 ) { name = "View" };

      ComApi.InwOpState10 oState = ComBridge.State;
      ComApi.InwOaPropertyVec options = oState.GetIOPluginOptions( "lcodpimage" );

      _tempFolder = Path.Combine( Path.GetTempPath(), "Rimshot.ExportBCF", Path.GetRandomFileName() );

      string imageFolder = Path.Combine( this._tempFolder, image.guid.ToString() );
      string snapshot = Path.Combine( imageFolder, image.name + ".png" );

      if ( !Directory.Exists( this._tempFolder ) ) {
        Directory.CreateDirectory( this._tempFolder );
      }

      _ = Directory.CreateDirectory( imageFolder );

      foreach ( ComApi.InwOaProperty opt in options.Properties() ) {
        if ( opt.name == "export.image.format" ) {
          opt.value = "lcodpexpng";
        }

        if ( opt.name == "export.image.width" ) {
          opt.value = 1600;
        }

        if ( opt.name == "export.image.height" ) {
          opt.value = 900;
        }
      }
      oState.DriveIOPlugin( "lcodpimage", snapshot, options );

      try {
        stream = new MemoryStream();

        Bitmap oBitmap = new Bitmap( snapshot );
        Bitmap tBitmap = new Bitmap( snapshot );

        oBitmap.Save( stream, System.Drawing.Imaging.ImageFormat.Jpeg );
        imageBytes = stream.ToArray();
        image.image = Convert.ToBase64String( imageBytes );

        ImageViewpoint viewpoint = new ImageViewpoint( oNewViewPt1 );

      } catch ( Exception err ) {
        _ = MessageBox.Show( err.Message );
      }
      string imageString = JsonConvert.SerializeObject( image );

      SetImage( imageString );
      NotifyUI( "new-image", imageString );
    }
  }

  public class CallbackGeomListener : ComApi.InwSimplePrimitivesCB {
    public List<decimal> Coords { get; set; }
    public decimal[] Matrix { get; set; }
    public CallbackGeomListener () {
      Coords = new List<decimal>();
    }
    public void Line ( ComApi.InwSimpleVertex v1, ComApi.InwSimpleVertex v2 ) { }
    public void Point ( ComApi.InwSimpleVertex v1 ) { }
    public void SnapPoint ( ComApi.InwSimpleVertex v1 ) { }

    public void Triangle ( ComApi.InwSimpleVertex v1, ComApi.InwSimpleVertex v2, ComApi.InwSimpleVertex v3 ) {

      Array array_v1 = ( Array )v1.coord;
      decimal v1_x = ( decimal )array_v1.GetValue( 1 );
      decimal v1_y = ( decimal )array_v1.GetValue( 2 );
      decimal v1_z = ( decimal )array_v1.GetValue( 3 );

      Array array_v2 = ( Array )v2.coord;
      decimal v2_x = ( decimal )array_v2.GetValue( 1 );
      decimal v2_y = ( decimal )array_v2.GetValue( 2 );
      decimal v2_z = ( decimal )array_v2.GetValue( 3 );

      Array array_v3 = ( Array )v3.coord;
      decimal v3_x = ( decimal )array_v3.GetValue( 1 );
      decimal v3_y = ( decimal )array_v3.GetValue( 2 );
      decimal v3_z = ( decimal )array_v3.GetValue( 3 );

      //Matrix
      decimal w1 = this.Matrix[ 3 ] * v1_x + this.Matrix[ 7 ] * v1_y + this.Matrix[ 11 ] * v1_z + this.Matrix[ 15 ];
      decimal v1__x = ( this.Matrix[ 0 ] * v1_x + this.Matrix[ 4 ] * v1_y + this.Matrix[ 8 ] * v1_z + this.Matrix[ 12 ] ) / w1;
      decimal v1__y = ( this.Matrix[ 1 ] * v1_x + this.Matrix[ 5 ] * v1_y + this.Matrix[ 9 ] * v1_z + this.Matrix[ 13 ] ) / w1;
      decimal v1__z = ( this.Matrix[ 2 ] * v1_x + this.Matrix[ 6 ] * v1_y + this.Matrix[ 10 ] * v1_z + this.Matrix[ 14 ] ) / w1;

      decimal w2 = Matrix[ 3 ] * v2_x + Matrix[ 7 ] * v2_y + Matrix[ 11 ] * v2_z + Matrix[ 15 ];
      decimal v2__x = ( Matrix[ 0 ] * v2_x + Matrix[ 4 ] * v2_y + Matrix[ 8 ] * v2_z + Matrix[ 12 ] ) / w2;
      decimal v2__y = ( Matrix[ 1 ] * v2_x + Matrix[ 5 ] * v2_y + Matrix[ 9 ] * v2_z + Matrix[ 13 ] ) / w2;
      decimal v2__z = ( Matrix[ 2 ] * v2_x + Matrix[ 6 ] * v2_y + Matrix[ 10 ] * v2_z + Matrix[ 14 ] ) / w2;

      decimal w3 = Matrix[ 3 ] * v3_x + Matrix[ 7 ] * v3_y + Matrix[ 11 ] * v3_z + Matrix[ 15 ];
      decimal v3__x = ( this.Matrix[ 0 ] * v3_x + Matrix[ 4 ] * v3_y + Matrix[ 8 ] * v3_z + Matrix[ 12 ] ) / w3;
      decimal v3__y = ( Matrix[ 1 ] * v3_x + Matrix[ 5 ] * v3_y + Matrix[ 9 ] * v3_z + Matrix[ 13 ] ) / w3;
      decimal v3__z = ( Matrix[ 2 ] * v3_x + Matrix[ 6 ] * v3_y + Matrix[ 10 ] * v3_z + Matrix[ 14 ] ) / w3;

      Coords.Add( ( decimal )v1__x );
      Coords.Add( ( decimal )v1__y );
      Coords.Add( ( decimal )v1__z );

      Coords.Add( ( decimal )v2__x );
      Coords.Add( ( decimal )v2__y );
      Coords.Add( ( decimal )v2__z );

      Coords.Add( ( decimal )v3__x );
      Coords.Add( ( decimal )v3__y );
      Coords.Add( ( decimal )v3__z );
    }
  }
}
