using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace App
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		AppViewController viewController;
		//
		// This method is invoked when the application has loaded and is ready to run. In this
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			window = new UIWindow (UIScreen.MainScreen.Bounds);
			
			var rootNavigationController = new UINavigationController ();
			rootNavigationController.NavigationBar.BarTintColor = UIColor.FromRGB (141, 198, 63);
			rootNavigationController.NavigationBar.TintColor = UIColor.White;
			rootNavigationController.NavigationBar.BackgroundColor = UIColor.White;
			rootNavigationController.NavigationBar.SetTitleTextAttributes (new UITextAttributes{ TextColor = UIColor.White });
			/*rootNavigationController.NavigationBar.Translucent = false;
			rootNavigationController.NavigationBar.BarStyle = UIBarStyle.Black;*/

			viewController = new AppViewController ();
			rootNavigationController.PushViewController (viewController, false);
			window.RootViewController = rootNavigationController;

			//rootNavigationController.SetNavigationBarHidden (true, true);

			window.MakeKeyAndVisible ();
			
			return true;
		}


	}
}

