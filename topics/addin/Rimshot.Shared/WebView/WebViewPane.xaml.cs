using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CefSharp;
using CefSharp.Wpf;
using System.IO;
using Path = System.IO.Path;

namespace Rimshot.Shared.WebView {
  public partial class WebViewPane : UserControl {

    public class Bindings : UIBindings {

    }


    public WebViewPane(string address = "https://Rimshot.io/analysis/Rimshot") {

      Bindings bindings = new Bindings();

      InitializeCef();
      InitializeComponent();

      bindings.Browser = Browser;
      bindings.Window = this;

      Browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
      Browser.JavascriptObjectRepository.Register("UIBindings", bindings, isAsync: true, options: BindingOptions.DefaultBinder);

      Browser.Address = address;
    }
    private void InitializeCef() {
      if (Cef.IsInitialized) return;
      Cef.EnableHighDPISupport();

      //var assemblyLocation = Assembly.GetExecutingAssembly().Location;
      //var assemblyPath = System.IO.Path.GetDirectoryName(assemblyLocation);
      //var pathSubprocess = System.IO.Path.Combine(assemblyPath, "CefSharp.BrowserSubprocess.exe");
      var settings = new CefSettings() {
        CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
      };

      try {
        Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
      }
      catch (System.Exception e) {
        _ = MessageBox.Show(e.ToString());
      }
    }

    protected override void OnLostFocus(RoutedEventArgs e) {
      // Needs a thought as to what happens when the window doesn't have focus.
    }
  }
}
