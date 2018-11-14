// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SalsifyMacClient
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSView FrameDisplay { get; set; }

		[Outlet]
		AppKit.NSButton StartButton { get; set; }

		[Action ("SendClicked:")]
		partial void SendClicked (Foundation.NSObject sender);

		[Action ("StartClicked:")]
		partial void StartClicked (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (FrameDisplay != null) {
				FrameDisplay.Dispose ();
				FrameDisplay = null;
			}

			if (StartButton != null) {
				StartButton.Dispose ();
				StartButton = null;
			}
		}
	}
}
