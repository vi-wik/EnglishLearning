
#if ANDROID
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using EnglishLearning.App.Android;
using Microsoft.Maui.Handlers;
using static Android.Webkit.WebChromeClient;
using WebView = Android.Webkit.WebView;
#endif

namespace EnglishLearning.App
{
    internal class WebViewHandler
    {
        public static void EnableVideoFeatures()
        {

#if ANDROID
        Microsoft.Maui.Handlers.WebViewHandler.Mapper.ModifyMapping(
        nameof(WebView.WebChromeClient),
        (handler, view, args) =>
        {
            handler.PlatformView.SetWebChromeClient(new WebPlayerChromeClient(handler));
        });
#endif

        }
    }
}
