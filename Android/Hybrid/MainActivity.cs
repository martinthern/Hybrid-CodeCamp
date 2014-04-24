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
	[Activity (Label = "Hybrid", MainLauncher = true)]
	public class MainActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			var webView = FindViewById<WebView> (Resource.Id.webView);
			webView.Settings.JavaScriptEnabled = true;

			// Use subclassed WebViewClient to intercept hybrid native calls
			webView.SetWebViewClient (new HybridWebViewClient (this));

			webView.LoadUrl("file:///android_asset/Web/Main.html");

			// Render the view from the type generated from RazorView.cshtml
			//var model = new Model1 () { Text = "Text goes here" };
			//var template = new RazorView () { Model = model };
			//var page = template.GenerateString ();


			// Load the rendered HTML into the view with a base URL 
			// that points to the root of the bundled Assets folder
			//webView.LoadDataWithBaseURL ("file:///android_asset/", page, "text/html", "UTF-8", null);

		}

		private class HybridWebViewClient : WebViewClient
		{
			Context _context;

			public HybridWebViewClient(Context context)
			{
				_context = context;
			}

			public override bool ShouldOverrideUrlLoading (WebView webView, string url)
			{

				// If the URL is not our own custom scheme, just let the webView load the URL as usual
				var scheme = "js-call:";

				if (!url.StartsWith (scheme))
					return false;

				// This handler will treat everything between the protocol and "?"
				// as the method name.  The querystring has all of the parameters.
				var resources = url.Substring (scheme.Length).Split ('?');
				var method = resources [0];
				var parameters = resources.Length > 1 ? System.Web.HttpUtility.ParseQueryString (resources [1]) : null;

				if (method == "popup") {
					ShowPopup ();
				}

				return true;
			}

			private void ShowPopup()
			{
				var builder = new AlertDialog.Builder (_context);
				builder.SetTitle ("Alert!");

				builder.SetNeutralButton ("OK!", (senderAlert, args) => {});

				var alert = builder.Create ();
				alert.Show ();	
			}
		}
	}
}

