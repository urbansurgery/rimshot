using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Wpf;
using Newtonsoft.Json;

namespace Rimshot.Shared.WebView
{
  public abstract class UIBindings
  {

    public IWebBrowser Browser { get; set; }
    public WebViewPane Window { get; set; }

    public virtual void NotifyUI(string eventName, dynamic eventInfo)
    {
      var script = string.Format("window.EventBus.$emit('{0}',{1})", eventName, JsonConvert.SerializeObject(eventInfo));
      Browser.GetMainFrame().EvaluateScriptAsync(script);
    }

    public virtual void DispatchStoreActionUI(string storeActionName, string args = null)
    {
      var script = string.Format("window.Store.dispatch('{0}', '{1}')", storeActionName, args);
      Browser.GetMainFrame().EvaluateScriptAsync(script);
    }

    public virtual void ShowDev()
    {
      Browser.ShowDevTools();
    }

    // https://github.com/speckleworks/SpeckleUi/blob/4d2edaa14c355186a32cae387a1facff22b18a5d/SpeckleUiBase/SpeckleUiBindings.cs
  }
}
