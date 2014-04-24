using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Android.OS;

namespace Hybrid
{
//	internal class HybridWebViewClient : WebViewClient
//	{
//		Context _context;
//
//		public HybridWebViewClient(Context context)
//		{
//			_context = context;
//		}
//
//		public override bool ShouldOverrideUrlLoading (WebView webView, string url)
//		{
//
//			// If the URL is not our own custom scheme, just let the webView load the URL as usual
//			var scheme = "js-call:";
//
//			if (!url.StartsWith (scheme))
//				return false;
//
//			// This handler will treat everything between the protocol and "?"
//			// as the method name.  The querystring has all of the parameters.
//			var resources = url.Substring (scheme.Length).Split ('?');
//			var method = resources [0];
//			var parameters = System.Web.HttpUtility.ParseQueryString (resources [1]);
//
//			if (method == "popup") {
//				ShowPopup ();
//			}
//
//			//				if (method == "UpdateLabel") {
//			//					var textbox = parameters ["textbox"];
//			//
//			//					// Add some text to our string here so that we know something
//			//					// happened on the native part of the round trip.
//			//					var prepended = string.Format ("C# says \"{0}\"", textbox);
//			//
//			//					// Build some javascript using the C#-modified result
//			//					var js = string.Format ("SetLabelText('{0}');", prepended);
//			//
//			//					webView.LoadUrl ("javascript:" + js);
//			//				}
//
//			return true;
//		}
//
//		private void ShowPopup()
//		{
//			var builder = new AlertDialog.Builder (_context);
//			builder.SetTitle ("Alert!");
//
//			builder.SetNeutralButton ("OK!", (senderAlert, args) => {
//				//change value write your own set of instructions
//				//you can also create an event for the same in xamarin
//				//instead of writing things here
//			} );
//
//			var alert = builder.Create ();
//
//			//run the alert in UI thread to display in the screen
//			//_context.RunOnUiThread (() => alert.Show());
//
//		}
//	}
}

