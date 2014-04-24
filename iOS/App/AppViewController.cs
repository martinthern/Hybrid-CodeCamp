using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.IO;
using System.Collections.Generic;
using MonoTouch.AVFoundation;
using MonoTouch.CoreFoundation;

namespace App
{
	public partial class AppViewController : UIViewController
	{
		private UIWebView _webView;

		private string _baseTitle = "Hybrid";

		public event Action<string> QrScan;

		AVCaptureSession session;
		AVCaptureMetadataOutput metadataOutput;
		private int counter;

		public AppViewController () : base ("AppViewController", null)
		{
			Title = _baseTitle;
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			_webView = new UIWebView (View.Bounds);
			View.AddSubview (_webView);
			string fileName = "Main.html";
			string localHtmlUrl = Path.Combine (NSBundle.MainBundle.BundlePath, fileName);
			_webView.LoadRequest (new NSUrlRequest (new NSUrl (localHtmlUrl, false)));

			_webView.LoadFinished += (object sender, EventArgs e) => {
				_webView.EvaluateJavascript("api.isHybrid = true");
			};
			_webView.ShouldStartLoad = ShouldLoad;

			// Perform any additional setup after loading the view, typically from a nib.
		}

		private void ResetNavigation ()
		{
			Title = _baseTitle;
			NavigationItem.SetLeftBarButtonItem (null, true);
			NavigationController.SetNavigationBarHidden (true, true);
		}

		private void HandleJSCall (string url)
		{
			var jsCall = new JSCallCommand (url);

			if (jsCall.Command == CommandType.Popup) {
				new UIAlertView ("Alert", jsCall.Options, null, "OK").Show();
				return;
			}
			if (jsCall.Command == CommandType.Scan) {
				StartScan (result => {
					_webView.EvaluateJavascript(string.Format("api.scanComplete('{0}');", result));
				});
			}

		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			counter = 0;
		}

		private bool ShouldLoad (UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
		{
			ResetNavigation ();

			if (request.Url.Scheme == "js-call") {
				HandleJSCall (request.Url.AbsoluteString);
				return false;
			}

			if (request.Url.AbsoluteString.EndsWith(".pdf")) {
				var fileName = Path.GetFileName (request.Url.AbsoluteString);
				NavigationController.SetNavigationBarHidden (false, true);
				Title = fileName;
				NavigationItem.SetLeftBarButtonItem(new UIBarButtonItem ("< Back", UIBarButtonItemStyle.Plain, (sender, args) => {
					_webView.GoBack();
				}), true); 
			}

			return true;
		}

		private void StartScan (Action<string> callback)
		{
			session = new AVCaptureSession ();
			var camera = AVCaptureDevice.DefaultDeviceWithMediaType (AVMediaType.Video);
			var input = AVCaptureDeviceInput.FromDevice (camera);
			session.AddInput (input);

			//Add the metadata output channel
			metadataOutput = new AVCaptureMetadataOutput ();
			var metadataDelegate = new MyMetadataOutputDelegate (this);
			metadataOutput.SetDelegate (metadataDelegate, DispatchQueue.MainQueue);
			session.AddOutput (metadataOutput);
			//Confusing! *After* adding to session, tell output what to recognize...
			foreach (var t in metadataOutput.AvailableMetadataObjectTypes) {
				Console.WriteLine (t);
			}
			metadataOutput.MetadataObjectTypes = new NSString[] {
				AVMetadataObject.TypeQRCode,
				AVMetadataObject.TypeEAN8Code,
				AVMetadataObject.TypeEAN13Code,
				AVMetadataObject.TypeCode128Code,
				AVMetadataObject.TypeCode39Code,
				AVMetadataObject.TypeCode39Mod43Code,
				AVMetadataObject.TypeCode93Code
			};

			AVCaptureVideoPreviewLayer previewLayer = new AVCaptureVideoPreviewLayer (session);
			previewLayer.Frame = new RectangleF (0, 0, View.Frame.Size.Width, View.Frame.Size.Height);
			previewLayer.VideoGravity = AVLayerVideoGravity.ResizeAspectFill.ToString ();
			View.Layer.AddSublayer (previewLayer);

			session.StartRunning ();

			QrScan += (result) => {
				if (counter > 0)
					return;

				counter++;

				session.StopRunning ();
				previewLayer.RemoveFromSuperLayer();
				callback (result);
				counter = 0;
			};
		}


		class MyMetadataOutputDelegate : AVCaptureMetadataOutputObjectsDelegate
		{
			private AppViewController _parent;

			public MyMetadataOutputDelegate (AppViewController homeScreen)
			{
				_parent = homeScreen;		
			}

			public override void DidOutputMetadataObjects (AVCaptureMetadataOutput captureOutput, AVMetadataObject[] metadataObjects, AVCaptureConnection connection)
			{
				string code = "";

				foreach (var metadata in metadataObjects) {
					if (metadata.Type == AVMetadataObject.TypeQRCode) {
						code = ((AVMetadataMachineReadableCodeObject)metadata).StringValue;
						Console.WriteLine ("qrcode: " + code);
					} else if (metadata.Type == AVMetadataObject.TypeEAN13Code) {
						code = ((AVMetadataMachineReadableCodeObject)metadata).StringValue;
						Console.WriteLine ("ean13code: " + code); 
					} else if (metadata.Type == AVMetadataObject.TypeEAN8Code) {
						code = ((AVMetadataMachineReadableCodeObject)metadata).StringValue;
						Console.WriteLine ("ean8code: " + code); 
					} else if (metadata.Type == AVMetadataObject.TypeCode128Code) {
						code = ((AVMetadataMachineReadableCodeObject)metadata).StringValue;
						Console.WriteLine ("code128: " + code); 
					} else {
						Console.WriteLine ("type: " + metadata.Type);
						code = ((AVMetadataMachineReadableCodeObject)metadata).StringValue;
						Console.WriteLine ("----: " + code);
					}
				}

				if (_parent.QrScan != null && !string.IsNullOrEmpty (code)) {
					_parent.QrScan (code);
				}
			}
		}
	}

	public class JSCallCommand
	{
		public JSCallCommand (string url)
		{
			var data = url.Replace ("js-call:", "").Split ('?');

			Command = data [0];

			if (data.Length > 1) {
			
			Options = data [1];
			}
		}

		public string Command { get; set; }
		public string Options { get; set; }


	}
	public class CommandType
	{
		public const string Popup = "popup";
		public const string Scan = "scan";
	}
}

