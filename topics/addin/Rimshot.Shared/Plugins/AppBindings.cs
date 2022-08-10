using CefSharp;
using Speckle.Newtonsoft.Json;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Rimshot.Bindings {
  public abstract class DefaultBindings {
    public const string Url = "";

    [DllImport( "user32.dll" )]
    public static extern bool UpdateWindow ( IntPtr hWnd );
    public string AppName { get; set; }
    public virtual object GetAppDetails () => this.AppName;

    public IWebBrowser Browser { get; set; }

    public System.Windows.Forms.Control UiThread { get; set; }
    public SynchronizationContext Context { get; set; }

    public delegate void GoDelegate ();

    public virtual void NotifyUi ( string eventName, dynamic eventInfo = null ) {
      string script = string.Format( "window.EventBus.$emit('{0}',{1})", eventName, JsonConvert.SerializeObject( eventInfo ) );
      this.Browser.GetMainFrame().EvaluateScriptAsync( script );
    }

    public virtual void CommitStoreMutationUi ( string storeMutationName, string args = null ) {
      string script = $"window.Store.commit('{storeMutationName}', '{args}')";
      try {
        this.Browser.GetMainFrame().EvaluateScriptAsync( script );
      } catch ( Exception e ) {
        Logging.ErrorLog( e.Message );
      }
    }

    public virtual void DispatchStoreActionUi ( string storeActionName, string args = null ) {
      string script = $"window.Store.dispatch('{storeActionName}', '{args}')";
      try {
        this.Browser.GetMainFrame().EvaluateScriptAsync( script );
      } catch ( Exception e ) {
        Logging.ErrorLog( e.Message );
      }
    }

    public virtual void ShowDev () => this.Browser.ShowDevTools();

    public virtual void Refresh ( bool force = false ) {
      this.Browser.Reload( force );
      this.Browser.GetMainFrame().LoadUrl( Url );
    }
  }
}
