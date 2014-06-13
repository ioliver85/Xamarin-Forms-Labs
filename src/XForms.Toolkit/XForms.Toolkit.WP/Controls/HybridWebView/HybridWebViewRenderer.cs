﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XForms.Toolkit.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WinPhone;
using Microsoft.Phone.Controls;
using XForms.Toolkit.WP.Controls;

[assembly: ExportRenderer(typeof(HybridWebView), typeof(HybridWebViewRenderer))]
namespace XForms.Toolkit.WP.Controls
{
    public class HybridWebViewRenderer : ViewRenderer<HybridWebView, WebBrowser>
    {
        protected WebBrowser webView;

        protected override void OnElementChanged(ElementChangedEventArgs<HybridWebView> e)
        {
            base.OnElementChanged(e);

            this.webView = new WebBrowser()
            {
                Source = this.Element.Uri
            };

            this.webView.IsScriptEnabled = true;
            this.webView.Navigating += webView_Navigating;
            this.webView.LoadCompleted += webView_LoadCompleted;
            this.webView.ScriptNotify += WebViewOnScriptNotify;
          
            this.Element.JavaScriptLoadRequested += Inject;

            this.Element.PropertyChanged += Model_PropertyChanged;

            this.SetNativeControl(this.webView);
         
        }

     
        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Uri")
            {
                this.webView.Source = this.Element.Uri;
            }
        }

        private void WebViewOnScriptNotify(object sender, NotifyEventArgs notifyEventArgs)
        {
            Action<string> action;
            var values = notifyEventArgs.Value.Split('/');
            var name = values.FirstOrDefault();

            if (name != null && this.Element.TryGetAction(name, out action))
            {
                var data = Uri.UnescapeDataString(values.ElementAt(1));
                action.Invoke(data);
            }
        }

        void webView_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            this.InjectNativeFunctionScript();
        }

        void webView_Navigating(object sender, NavigatingEventArgs e)
        {

        }

        private void InjectNativeFunctionScript()
        {
            var builder = new StringBuilder();
            builder.Append("function Native(action, data){ ");
            builder.Append("window.external.notify(");
            builder.Append("action + \"/\"");
            builder.Append(" + ((typeof data == \"object\") ? JSON.stringify(data) : data)");
            builder.Append(")");
            builder.Append(" ;}");

            this.Inject(this, builder.ToString());
        }

        private void Inject(object sender, string script)
        {
            //this.webView.InvokeScript(string.Format("javascript: {0}", script));
            this.webView.InvokeScript("eval", script);
        }

        private void Load(object sender, Uri uri)
        {
            this.webView.Navigate(uri);
        }
    }
}