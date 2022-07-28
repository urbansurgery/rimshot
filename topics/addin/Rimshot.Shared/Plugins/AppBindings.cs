using CefSharp;
using Speckle.Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Rimshot.Bindings {

  public abstract class DefaultBindings {
    public const string Url = "";

    public string AppName { get; set; }
    public virtual object GetAppDetails () => this.AppName;

    public List<string> strings = new List<string>() { "1" };

    public IWebBrowser Browser { get; set; }

    public virtual void NotifyUI ( string eventName, dynamic eventInfo = null ) {
      string script = string.Format( "window.EventBus.$emit('{0}',{1})", eventName, JsonConvert.SerializeObject( eventInfo ) );
      this.Browser.GetMainFrame().EvaluateScriptAsync( script );
    }

    public virtual void CommitStoreMutationUI ( string storeMutationName, string args = null ) {
      string script = string.Format( "window.Store.commit('{0}', '{1}')", storeMutationName, args );
      try {
        this.Browser.GetMainFrame().EvaluateScriptAsync( script );
      } catch ( Exception e ) {
        Logging.ErrorLog( e.Message );
      }
    }

    public virtual void DispatchStoreActionUI ( string storeActionName, string args = null ) {
      string script = string.Format( "window.Store.dispatch('{0}', '{1}')", storeActionName, args );
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
