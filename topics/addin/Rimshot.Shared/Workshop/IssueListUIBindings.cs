using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.DocumentParts;
using CefSharp;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using Speckle.Core.Transports;
using Speckle.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Color = System.Drawing.Color;
using ComApi = Autodesk.Navisworks.Api.Interop.ComApi;
using ComBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge;
using Navis = Autodesk.Navisworks.Api.Application;
using NavisworksApp = Autodesk.Navisworks.Api.Application;

namespace Rimshot.Shared.Workshop {

  public abstract class UIBindings {
#if DEBUG
    const string rimshotUrl = "http://192.168.86.29:8080/issues";
#else
    const String rimshotUrl = "https://rimshot.app/issues";
#endif
    public const string Url = rimshotUrl;
    private const int V = 131004;
    private const int Modulo = 5;
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

    public virtual void CommitStoreMutationUI ( string storeMutationName, string args = null ) {
      string script = string.Format( "window.Store.commit('{0}', '{1}')", storeMutationName, args );
      try {
        Browser.GetMainFrame().EvaluateScriptAsync( script );
      } catch ( Exception e ) {
        Logging.ErrorLog( e.Message );
      }
    }

    public virtual void DispatchStoreActionUI ( string storeActionName, string args = null ) {
      string script = string.Format( "window.Store.dispatch('{0}', '{1}')", storeActionName, args );
      try {
        Browser.GetMainFrame().EvaluateScriptAsync( script );
      } catch ( Exception e ) {
        Logging.ErrorLog( e.Message );
      }
    }

    public virtual void ShowDev () => this.Browser.ShowDevTools();
    public virtual void Refresh ( bool force = false ) {
      this.Browser.Reload( force );
      this.Browser.GetMainFrame().LoadUrl( Url );
    }

    public virtual void AddImage () => this.SendIssueView();

    public virtual void CommitSelection ( object payload ) => CommitSelectedObjectsToSpeckle( payload );


    public virtual string UpdateView ( string camera ) {
      Logging.ConsoleLog( camera );
      return camera;
    }

    /// <summary>
    /// Adds an hierarchical object based on the current selection and commits to the selected issue Branch. If no branch exists, one is created.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="commitMessage">Allows for the calling UI to override this and include a custom commit message.</param>
    private async void CommitSelectedObjectsToSpeckle ( object payload, string commitMessage = "Rimshot commit." ) {

      Logging.ConsoleLog( "Commit Selection commenced." );

      dynamic commitPayload = ( dynamic )payload;

      string branchName = commitPayload.branch;
      string streamId = commitPayload.stream;
      string host = commitPayload.host;
      string issueId = commitPayload.issueId;

      UIBindings app = this;
      Logging.ConsoleLog( $"Stream: {streamId}, Host: {host}, Branch: {branchName}, Issue: {issueId}" );

      string description = $"issueId:{issueId}";

      Account defaultAccount = AccountManager.GetDefaultAccount();

      if ( defaultAccount == null ) {
        Logging.ErrorLog( new SpeckleException( $"You do not have any account. Please create one or add it to the Speckle Manager." ), app );
        return;
      }

      Logging.ConsoleLog( defaultAccount.userInfo.email.ToString() );
      Logging.ConsoleLog( defaultAccount.serverInfo.url.ToString() );

      Client client = new Client( defaultAccount );

      try {
        await client.StreamGet( streamId );
      } catch {
        Logging.ErrorLog(
        new SpeckleException(
          $"You don't have access to stream {streamId} on server {host}, or the stream does not exist." ), app );
        return;
      }

      // Get Branch and create if it doesn't exist.
      Branch branch;
      try {
        branch = await client.BranchGet( streamId, branchName );
        if ( branch is null ) {
          branch = await CreateBranch( client, streamId, branchName, description );
        }
      } catch ( Exception ) {
        Logging.ErrorLog( new SpeckleException( $"Unable to find or create an issue branch for {branchName}" ), app );
        return;
      }

      try {
        branch = client.BranchGet( streamId, branchName, 1 ).Result;

        if ( branch != null ) {
          NotifyUI( "branch_updated", JsonConvert.SerializeObject( new { branch = branch.name, issueId } ) );
        }
      } catch ( Exception ) {
        Logging.ErrorLog( new SpeckleException( $"Unable to find an issue branch for {branchName}" ), app );
        return;
      }

      Document activeDocument = NavisworksApp.ActiveDocument;
      DocumentModels models = activeDocument.Models;

      ModelItemCollection appSelectedItems = NavisworksApp.ActiveDocument.CurrentSelection.SelectedItems;

      // Were the selection to change mid-commit, accessing the selected set would fail.
      ModelItemCollection selectedItems = new ModelItemCollection();
      appSelectedItems.CopyTo( selectedItems );

      List<Base> translatedElements = new List<Base>();

      List<Base> Elements = new List<Base>();

      int elementCount = selectedItems.Count;

      for ( int e = 0; e < elementCount; e++ ) {
        ModelItem element = selectedItems[ e ];
        CommitStoreMutationUI( "SET_ELEMENT_PROGRESS", JsonConvert.SerializeObject( new { current = e + 1, count = elementCount } ) );
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
        message = commitMessage,
        objectId = hash,
        streamId = streamId,
        sourceApplication = "Rimshot"
      } ).Result;

      Commit commitObject = client.CommitGet( streamId, commitId ).Result;
      string referencedObject = commitObject.referencedObject;

      NotifyUI( "commit_sent", new { commit = commitId, issueId, stream = streamId, objectId = referencedObject } );

      client.Dispose();
    }


    public Base TranslateElement ( ModelItem element ) {
      Base elementBase = new Base();

      int descendantsCount = element.Children.Count();

      if ( element.HasGeometry && descendantsCount == 0 ) {
        List<Objects.Geometry.Mesh> geometryToSpeckle = TranslateGeometry( element );
        if ( geometryToSpeckle.Count > 0 ) {
          elementBase[ "displayValue" ] = geometryToSpeckle;
          elementBase[ "units" ] = NavisworksApp.ActiveDocument.Units;
        }
      }

      List<Base> children = new List<Base>();

      if ( descendantsCount > 0 ) {
        int c = 0;
        for ( int d = 0; d < descendantsCount; d++ ) {
          ModelItem child = element.Children.ElementAt( d );
          double progress = ( ( double )d + 1 ) / ( double )descendantsCount * 100;
          int c2 = ( int )Math.Truncate( progress );

          if ( Math.Truncate( progress ) % Modulo == 0 && c != c2 ) {
            c = c2;
          }
          children.Add( TranslateElement( child ) );
        }
        elementBase[ "@Elements" ] = children;
      }

      Base propertiesBase = new Base();

      PropertyCategoryCollection propertyCategories = element.PropertyCategories;

      foreach ( PropertyCategory propCategory in propertyCategories ) {

        DataPropertyCollection properties = propCategory.Properties;
        Base propertyCategoryBase = new Base();
        string propertyCategoryName = SanitizePropertyName( propCategory.DisplayName );

        foreach ( DataProperty property in properties ) {
          string key;
          try {

            key = SanitizePropertyName( property.DisplayName.ToString() );
          } catch ( Exception err ) {
            Logging.ErrorLog( $"Property Name not converted. {err.Message}" );
            break;
          }

          dynamic propValue = null;

          switch ( property.Value.DataType ) {
            case VariantDataType.Boolean: propValue = property.Value.ToBoolean().ToString(); break;
            case VariantDataType.DisplayString: propValue = property.Value.ToDisplayString(); break;
            case VariantDataType.IdentifierString: propValue = property.Value.ToIdentifierString(); break;
            case VariantDataType.Int32: propValue = property.Value.ToInt32().ToString(); break;
            case VariantDataType.Double: propValue = property.Value.ToDouble().ToString(); break;
            case VariantDataType.DoubleAngle: propValue = property.Value.ToDoubleAngle().ToString(); break;
            case VariantDataType.DoubleArea: propValue = property.Value.ToDoubleArea().ToString(); break;
            case VariantDataType.DoubleLength: propValue = property.Value.ToDoubleLength().ToString(); break;
            case VariantDataType.DoubleVolume: propValue = property.Value.ToDoubleVolume().ToString(); break;
            case VariantDataType.DateTime: propValue = property.Value.ToDateTime().ToString(); break;
            case VariantDataType.NamedConstant: propValue = property.Value.ToNamedConstant().ToString(); break;
            case VariantDataType.Point3D: propValue = property.Value.ToPoint3D().ToString(); break;
            case VariantDataType.None: break;
          }

          if ( propValue != null ) {

            object keyPropValue = propertyCategoryBase[ key ];

            if ( keyPropValue == null ) {
              propertyCategoryBase[ key ] = propValue;
            } else if ( keyPropValue.GetType().IsArray ) {
              List<dynamic> arrayPropValue = ( List<dynamic> )keyPropValue;
              arrayPropValue.Add( propValue );
              propertyCategoryBase[ key ] = arrayPropValue;
            } else {
              dynamic existingValue = keyPropValue;
              List<dynamic> arrayPropValue = new List<dynamic> {
                existingValue,
                propValue
              };
              propertyCategoryBase[ key ] = arrayPropValue;
            }

          }
        }

        if ( propertyCategoryBase.GetDynamicMembers().Count() > 0 && propertyCategoryName != null ) {
          if ( propertiesBase != null ) {

            if ( propertyCategoryName == "Geometry" ) {
              continue;
            }

            if ( propertyCategoryName == "Item" ) {
              propertiesBase[ "NavisItem" ] = propertyCategoryBase;
            } else {
              propertiesBase[ propertyCategoryName ] = propertyCategoryBase;
            }
          }
        }
      }

      elementBase[ "Properties" ] = propertiesBase;

      return elementBase;
    }

    public string SanitizePropertyName ( string name ) {
      // Regex pattern from speckle-sharp/Core/Core/Models/DynamicBase.cs IsPropNameValid
      string cleanName = Regex.Replace( name, @"[\.\/]", "_" );
      return cleanName;
    }

    public List<Objects.Geometry.Mesh> TranslateGeometry ( ModelItem geom ) { // TODO: This should move to Geometry.cs or Conversions.cs

      NavisGeometry navisGeometry = new NavisGeometry( geom );
      List<Shared.CallbackGeomListener> cb = navisGeometry.GetFragments();

      List<Objects.Geometry.Mesh> baseMeshes = new List<Objects.Geometry.Mesh>();

      foreach ( Shared.CallbackGeomListener callback in cb ) {

        List<double> vertices = new List<double>();
        List<int> faces = new List<int>();
        List<NavisDoubleTriangle> Triangles = callback.Triangles;
        int triangleCount = Triangles.Count;

        if ( triangleCount > 0 ) {

          int c = 0;
          for ( int t = 0; t < triangleCount; t += 1 ) {

            // TODO: work out a performant way to keep a progress UI element up to date.
            double progress = ( ( double )t + 1 ) / ( double )triangleCount * 100;
            int c2 = ( int )Math.Truncate( progress );

            if ( Math.Truncate( progress ) % Modulo == 0 && c != c2 ) {
              c = c2;
              CommitStoreMutationUI( "SET_GEOMETRY_PROGRESS", JsonConvert.SerializeObject( new { current = t + 1, count = triangleCount } ) );
            }
            double scale = ( double )0.001; // TODO: This will need to relate to the ActiveDocument reality and the target units. Probably metres.

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

    public Objects.Other.RenderMaterial TranslateMaterial ( ModelItem geom ) { // TODO: Extract this to a Conversions.cs or similar

      Color original = Color.FromArgb(
        Convert.ToInt32( geom.Geometry.OriginalColor.R * 255 ),
        Convert.ToInt32( geom.Geometry.OriginalColor.G * 255 ),
        Convert.ToInt32( geom.Geometry.OriginalColor.B * 255 ) );

      Color dark = Color.FromArgb( Convert.ToInt32( 0 ), Convert.ToInt32( 0 ), Convert.ToInt32( 0 ) );

      Objects.Other.RenderMaterial r = new Objects.Other.RenderMaterial( 1 - geom.Geometry.OriginalTransparency, 0, 1, original, dark ) {
        name = "NavisMaterial"
      };

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

  public class CallbackGeomListener : ComApi.InwSimplePrimitivesCB { // TODO: Move this to Geometry.cs
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
