using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Android.OS;
using ZXing.Mobile;

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
			//SetContentView (Resource.Layout.Scanner);
			//TestScanner ();

			var webView = FindViewById<WebView> (Resource.Id.webView);
			webView.Settings.JavaScriptEnabled = true;

			// Use subclassed WebViewClient to intercept hybrid native calls
			webView.SetWebViewClient (new HybridWebViewClient (this));

			webView.LoadUrl("file:///android_asset/Web/Main.html");

		}

		private void TestScanner()
		{
			//Create a new instance of our Scanner
			var scanner = new MobileBarcodeScanner(this);

			var buttonScanDefaultView = this.FindViewById<Button>(Resource.Id.buttonScanDefaultView);
			buttonScanDefaultView.Click += async delegate {

				//Tell our scanner to use the default overlay
				scanner.UseCustomOverlay = false;

				//We can customize the top and bottom text of the default overlay
				scanner.TopText = "Hold the camera up to the barcode\nAbout 6 inches away";
				scanner.BottomText = "Wait for the barcode to automatically scan!";

				//Start scanning
				var result = await scanner.Scan();

				if (result != null)
					Console.WriteLine("Scanned Barcode: " + result.Text);
			};
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

				switch (method) 
				{
				case "popup":
					ShowPopup (webView);
					break;
				case "scan":
					Scan (webView);
					break;
				}
			
				return true;
			}

			private void ShowPopup(WebView webView)
			{
				var builder = new AlertDialog.Builder (_context);
				builder.SetTitle ("Alert!");

				builder.SetNeutralButton ("OK!", (senderAlert, args) => {});

				var alert = builder.Create ();
				webView.Post (() => alert.Show ());
			}

			private void Scan(WebView webView)
			{
				var scanner = new MobileBarcodeScanner(_context);
				scanner.UseCustomOverlay = false;
				scanner.TopText = "Hold the camera up to the barcode\nAbout 6 inches away";
				scanner.BottomText = "Wait for the barcode to automatically scan!";
		
				var resultTask = scanner.Scan();
				resultTask.ContinueWith (result => RenderResult(result.Result.Text, webView));
			}

			private void RenderResult(string result, WebView webView)
			{
				if (result != null) 
				{
					//Console.WriteLine("Scanned Barcode: " + result);
					webView.LoadUrl("javascript:api.scanComplete('" + result + "');"); 
				}
			}
		}
	}
}

