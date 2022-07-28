using CefSharp;
using CefSharp.Wpf;
using Rimshot.Bindings;
using System;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;

namespace Rimshot {

  public partial class SpecklePane : UserControl {
    public class Bindings : SpeckleAppBindings { }

    public Bindings bindings;

    //private readonly Document activeDocument = NavisworksApp.ActiveDocument;

    public SpecklePane ( string address = SpeckleAppBindings.Url ) {

      InitializeCef();
      InitializeComponent();

      this.bindings = new Bindings {
        Browser = this.Browser,
        Window = this
      };

      this.Browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;

      // Bound Methods
      this.Browser.JavascriptObjectRepository.Register( "UIBindings", this.bindings, isAsync: true, options: BindingOptions.DefaultBinder );

      // Bound Properties
      this.Browser.JavascriptObjectRepository.Register( "AppName", "Speckle", isAsync: false );

      this.Browser.Address = address;
      this.Browser.BrowserSettings.WebGl = CefState.Enabled;
    }
    private void InitializeCef () {
      if ( Cef.IsInitialized ) {
        return;
      }

      Cef.EnableHighDPISupport();

      CefSettings settings = new CefSettings() {
        CachePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), "CefSharp\\Cache" ),
        RemoteDebuggingPort = 8099,
        PersistUserPreferences = true,
        LogSeverity = LogSeverity.Error
      };

      try {
        Cef.Initialize( settings, performDependencyCheck: true, browserProcessHandler: null );
      } catch ( Exception e ) {
        _ = MessageBox.Show( e.ToString() );
      }
    }

    private void ShowDevTools ( object sender, EventArgs e ) => this.Browser.ShowDevTools();

    private void Refresh ( object sender, EventArgs e ) => this.Browser.Reload( true );
  }
}
