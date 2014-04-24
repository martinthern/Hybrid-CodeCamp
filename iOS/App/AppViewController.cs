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

		AVCaptureVideoPreviewLayer previewLayer;

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

		private void ResetNavigation()
		{
			NavigationItem.SetLeftBarButtonItem (null, true);
			NavigationItem.SetRightBarButtonItem (null, true);
			_webView.ScrollView.UserInteractionEnabled = true;
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

		private void HandleJSCall (string url)
		{
			var jsCall = new JSCallCommand (url);

			if (jsCall.Command == CommandType.Popup) {
				new UIAlertView ("Alert", jsCall.Options, null, "OK").Show();
				return;
			}
			if (jsCall.Command == CommandType.Scan) {

				this.NavigationItem.SetRightBarButtonItem(
					new UIBarButtonItem(UIBarButtonSystemItem.Cancel, (sender,args) => {
						previewLayer.RemoveFromSuperLayer();
						ResetNavigation();
						session.StopRunning();
					})
					, true);

				StartScan (result => {
					_webView.EvaluateJavascript(string.Format("{0}('{1}');", jsCall.Options, result));
				});
			}

		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			_webView.Frame = View.Bounds;
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
				Title = fileName;
				NavigationItem.SetLeftBarButtonItem(new UIBarButtonItem ("Back", UIBarButtonItemStyle.Plain, (sender, args) => {
					_webView.GoBack();
				}), true); 
			}

			return true;
		}

		private void StartScan (Action<string> callback)
		{
			_webView.ScrollView.UserInteractionEnabled = false;

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

			previewLayer = new AVCaptureVideoPreviewLayer (session);
			previewLayer.Frame = new RectangleF (0, 0, View.Frame.Size.Width, View.Frame.Size.Height);
			previewLayer.VideoGravity = AVLayerVideoGravity.ResizeAspectFill.ToString ();
			View.Layer.AddSublayer (previewLayer);

			session.StartRunning ();

			QrScan += (result) => {
				if (counter > 0)
					return;

				counter++;

				session.StopRunning ();
				_webView.ScrollView.UserInteractionEnabled = true;
				previewLayer.RemoveFromSuperLayer();
				this.NavigationItem.RightBarButtonItem = null;
				callback (result);

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
					var instance = metadata as AVMetadataMachineReadableCodeObject;

					if (instance == null) {
						Console.WriteLine ("Skipping: " + metadata);
						continue;
					}

					code = instance.StringValue;
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

