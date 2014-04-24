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

			SetContentView (Resource.Layout.Main);

			var webView = FindViewById<WebView> (Resource.Id.webView);
			webView.Settings.JavaScriptEnabled = true;

			webView.SetWebViewClient (new HybridWebViewClient (this));

			webView.LoadUrl("JavaScript: api.isHybrid = true;");
			webView.LoadUrl("file:///android_asset/Web/Main.html");
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
				if (url.EndsWith (".pdf")) {
					DisplaydPdf (url);
					return true;
				}

				var scheme = "js-call:";
				if (!url.StartsWith (scheme))
					return false;

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

			private void DisplaydPdf(string uri)
			{
				var intent = new Intent(Intent.ActionView);
				intent.SetDataAndType(uri, "application/pdf");
				intent.SetFlags(ActivityFlags.ClearTop);
				StartActivity(intent);
			}
		}
	}
}

