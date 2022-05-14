using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;
using Newtonsoft.Json;
using Navis = Autodesk.Navisworks.Api.Application;
using System.IO;
using Autodesk.Navisworks.Api;
using ComApi = Autodesk.Navisworks.Api.Interop.ComApi;
using ComBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge; //
using NavisworksApp = Autodesk.Navisworks.Api.Application;
using Autodesk.Navisworks.Api.DocumentParts;

using Speckle.Core.Models;
using Speckle.Core.Credentials;
using Speckle.Core.Transports;
using Speckle.Core.Api;
using System.Reflection;
using System.Drawing;
using Color = System.Drawing.Color;

namespace Rimshot.Shared.Workshop {
  public abstract class UIBindings {
#if DEBUG
    const string rimshotUrl = "http://192.168.86.42:8080/issues";
#else
    const String rimshotUrl = "https://rimshot.app/issues";
#endif
    public const string Url = rimshotUrl;
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

      var commitPayload = ( dynamic )payload;

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
        Console.WriteLine( $"Error: {e.Message}" );
      }

      branch = client.BranchGet( streamId, branchName, 1 ).Result;

      if ( branch != null ) {
        Console.WriteLine( $"Branch {branch.name} established" );
        NotifyUI( "branch_updated", $"{{\"branch\":\"{branch.name}\",\"issueId\":\"{issueId}\"}}" );
      }

      Document activeDocument = NavisworksApp.ActiveDocument;
      DocumentModels models = activeDocument.Models;

      ModelItemCollection selectedItems = NavisworksApp.ActiveDocument.CurrentSelection.SelectedItems;

      var translatedElements = new List<Base>();

      var Elements = new List<Base>();

      foreach ( ModelItem element in selectedItems ) {
        Elements.Add( TranslateElement( element ) );
      }

      Base myCommit = new Base();
      myCommit[ "@Elements" ] = Elements;

      string[] stringseparators = new string[] { "/" };
      myCommit[ "Issue Number" ] = branchName.Split( stringseparators, StringSplitOptions.None )[ 1 ];
      myCommit[ "applicationId" ] = issueId;
      var transport = new ServerTransport( defaultAccount, streamId );
      var hash = Operations.Send( myCommit, new List<ITransport> { transport } ).Result;

      var commitId = client.CommitCreate( new CommitCreateInput() {
        branchName = branchName,
        message = "Rimshot issue commit.",
        objectId = hash,
        streamId = streamId,
        sourceApplication = "Rimshot"
      } ).Result;

      var commitObject = client.CommitGet( streamId, commitId ).Result;
      var referencedObject = commitObject.referencedObject;

      NotifyUI( "commit_sent", $"{{\"commit\":\"{commitId}\",\"issueId\":\"{issueId}\",\"stream\":\"{streamId}\",\"object\":\"{referencedObject}\"}}" );

      client.Dispose();

    }

    public Base TranslateElement ( ModelItem element ) {
      Base elementBase = new Base();

      List<Base> geometry = new List<Base>();

      if ( element.HasGeometry ) {

        elementBase[ "displayValue" ] = TranslateGeometry( element );
      }

      var children = new List<Base>();

      if ( element.Descendants.Count() > 0 ) {
        foreach ( ModelItem child in element.Descendants ) {
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

      Base geomBase = new Base();

      NavisGeometry navisGeometry = new NavisGeometry( geom );
      List<Shared.CallbackGeomListener> cb = navisGeometry.getFragments();


      List<Objects.Geometry.Mesh> baseMeshes = new List<Objects.Geometry.Mesh>();

      List<Mesh> meshes = new List<Mesh>();

      var rand = new Random();


      foreach ( var callback in cb ) {
        var coords = callback.Coords;
        var mesheCount = coords.Count / 131004;
        var mesheCountRem = coords.Count % 131004;

        for ( int j = 0; j < mesheCount; j++ ) {
          var triangles = new List<Triangle>();

          for ( int i = 0; i < 131004; i += 9 ) {
            var vertex1 = new Vertex( coords[ i + ( 131004 * j ) ], coords[ i + ( 131004 * j ) + 1 ], coords[ i + ( 131004 * j ) + 2 ] );
            var vertex2 = new Vertex( coords[ i + ( 131004 * j ) + 3 ], coords[ i + ( 131004 * j ) + 4 ], coords[ i + ( 131004 * j ) + 5 ] );
            var vertex3 = new Vertex( coords[ i + ( 131004 * j ) + 6 ], coords[ i + ( 131004 * j ) + 7 ], coords[ i + ( 131004 * j ) + 8 ] );
            var triangle = new Triangle( vertex1, vertex2, vertex3 );
            triangles.Add( triangle );
          }


          if ( triangles.Count > 0 ) {
            var mesh = new Mesh( triangles );
            meshes.Add( mesh );
          }

        }
        var triangles2 = new List<Triangle>();
        for ( int i = mesheCount * 131004; i < mesheCount * 131004 + mesheCountRem; i += 9 ) {
          var vertex1 = new Vertex( coords[ i ], coords[ i + 1 ], coords[ i + 2 ] );
          var vertex2 = new Vertex( coords[ i + 3 ], coords[ i + 4 ], coords[ i + 5 ] );
          var vertex3 = new Vertex( coords[ i + 6 ], coords[ i + 7 ], coords[ i + 8 ] );
          var triangle = new Triangle( vertex1, vertex2, vertex3 );
          triangles2.Add( triangle );
        }
        if ( triangles2.Count > 0 ) {
          var mesh2 = new Mesh( triangles2 );
          meshes.Add( mesh2 );
        }
      }


      foreach ( Mesh m in meshes ) {



        Objects.Geometry.Mesh baseMesh = new Objects.Geometry.Mesh( m.vertices.Select( Convert.ToDouble ).ToList(), m.Indices );

        baseMesh[ "renderMaterial" ] = TranslateMaterial( geom );

        baseMeshes.Add( baseMesh );
      }


      return baseMeshes;

    }

    public Objects.Other.RenderMaterial TranslateMaterial ( ModelItem geom ) {

      Color original = Color.FromArgb(
        Convert.ToInt32( geom.Geometry.OriginalColor.R * 255 ),
        Convert.ToInt32( geom.Geometry.OriginalColor.G * 255 ),
        Convert.ToInt32( geom.Geometry.OriginalColor.B * 255 ) );

      Objects.Other.RenderMaterial r = new Objects.Other.RenderMaterial( geom.Geometry.OriginalTransparency, 0, 1, original );

      return r;
    }

    public Task<Branch> CreateBranch ( Client client, string streamId, string branchName, string description ) {

      try {
        var branchId = client.BranchCreate( new BranchCreateInput() {
          streamId = streamId,
          name = branchName,
          description = description
        } );
      } catch ( Exception e ) {
        Console.WriteLine( $"Error: {e.Message}" );
      }

      var branch = client.BranchGet( streamId, branchName );

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

      if ( !Directory.Exists( this._tempFolder ) ) Directory.CreateDirectory( this._tempFolder );
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

        System.Drawing.Bitmap oBitmap = new System.Drawing.Bitmap( snapshot );
        System.Drawing.Bitmap tBitmap = new System.Drawing.Bitmap( snapshot );

        oBitmap.Save( stream, System.Drawing.Imaging.ImageFormat.Jpeg );
        imageBytes = stream.ToArray();
        image.image = Convert.ToBase64String( imageBytes );

        var viewpoint = new ImageViewpoint( oNewViewPt1 );

      } catch ( Exception err ) {
        _ = MessageBox.Show( err.Message );
      }
      string imageString = JsonConvert.SerializeObject( image );

      SetImage( imageString );
      NotifyUI( "new-image", imageString );
    }
  }



  public class CallbackGeomListener : ComApi.InwSimplePrimitivesCB {
    public List<float> Coords { get; set; }
    public float[] matrix { get; set; }
    public CallbackGeomListener () {
      Coords = new List<float>();
    }
    public void Line ( ComApi.InwSimpleVertex v1,
                     ComApi.InwSimpleVertex v2 ) {

    }
    public void Point ( ComApi.InwSimpleVertex v1 ) {

    }
    public void SnapPoint ( ComApi.InwSimpleVertex v1 ) {

    }

    public void Triangle ( ComApi.InwSimpleVertex v1,
                         ComApi.InwSimpleVertex v2,
                         ComApi.InwSimpleVertex v3 ) {



      Array array_v1 = ( Array )( object )v1.coord;
      float v1_x = ( float )( array_v1.GetValue( 1 ) );
      float v1_y = ( float )( array_v1.GetValue( 2 ) );
      float v1_z = ( float )( array_v1.GetValue( 3 ) );

      Array array_v2 = ( Array )( object )v2.coord;
      float v2_x = ( float )( array_v2.GetValue( 1 ) );
      float v2_y = ( float )( array_v2.GetValue( 2 ) );
      float v2_z = ( float )( array_v2.GetValue( 3 ) );

      Array array_v3 = ( Array )( object )v3.coord;
      float v3_x = ( float )( array_v3.GetValue( 1 ) );
      float v3_y = ( float )( array_v3.GetValue( 2 ) );
      float v3_z = ( float )( array_v3.GetValue( 3 ) );

      //Matrix

      float w1 = matrix[ 3 ] * v1_x + matrix[ 7 ] * v1_y + matrix[ 11 ] * v1_z + matrix[ 15 ];

      var v1__x = ( matrix[ 0 ] * v1_x + matrix[ 4 ] * v1_y + matrix[ 8 ] * v1_z + matrix[ 12 ] ) / w1;
      var v1__y = ( matrix[ 1 ] * v1_x + matrix[ 5 ] * v1_y + matrix[ 9 ] * v1_z + matrix[ 13 ] ) / w1;
      var v1__z = ( matrix[ 2 ] * v1_x + matrix[ 6 ] * v1_y + matrix[ 10 ] * v1_z + matrix[ 14 ] ) / w1;


      float w2 = matrix[ 3 ] * v2_x + matrix[ 7 ] * v2_y + matrix[ 11 ] * v2_z + matrix[ 15 ];

      var v2__x = ( matrix[ 0 ] * v2_x + matrix[ 4 ] * v2_y + matrix[ 8 ] * v2_z + matrix[ 12 ] ) / w2;
      var v2__y = ( matrix[ 1 ] * v2_x + matrix[ 5 ] * v2_y + matrix[ 9 ] * v2_z + matrix[ 13 ] ) / w2;
      var v2__z = ( matrix[ 2 ] * v2_x + matrix[ 6 ] * v2_y + matrix[ 10 ] * v2_z + matrix[ 14 ] ) / w2;

      float w3 = matrix[ 3 ] * v3_x + matrix[ 7 ] * v3_y + matrix[ 11 ] * v3_z + matrix[ 15 ];

      var v3__x = ( matrix[ 0 ] * v3_x + matrix[ 4 ] * v3_y + matrix[ 8 ] * v3_z + matrix[ 12 ] ) / w3;
      var v3__y = ( matrix[ 1 ] * v3_x + matrix[ 5 ] * v3_y + matrix[ 9 ] * v3_z + matrix[ 13 ] ) / w3;
      var v3__z = ( matrix[ 2 ] * v3_x + matrix[ 6 ] * v3_y + matrix[ 10 ] * v3_z + matrix[ 14 ] ) / w3;



      Coords.Add( ( float )v1__x );
      Coords.Add( ( float )v1__y );
      Coords.Add( ( float )v1__z );

      Coords.Add( ( float )v2__x );
      Coords.Add( ( float )v2__y );
      Coords.Add( ( float )v2__z );

      Coords.Add( ( float )v3__x );
      Coords.Add( ( float )v3__y );
      Coords.Add( ( float )v3__z );
    }

  }



}
