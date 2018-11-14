using System;

using AppKit;
using Foundation;
using AVFoundation;
using CoreVideo;

namespace SalsifyMacClient
{
    public partial class ViewController : NSViewController
    {
        FrameCapturer capturer;

        public ViewController(IntPtr handle) : base(handle)
        {
            var config = new FrameCapturerConfig()
            {
                LayerGravity = AVLayerVideoGravity.ResizeAspect,
                Device = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video),
                PixelFormat = CVPixelFormatType.CV422YpCbCr8,
                FrameQualityPreset = AVCaptureSession.PresetHigh,
            };

            capturer = FrameCapturer.WithConfiguration(config);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            FrameDisplay.WantsLayer = true;
            capturer.BeginCapture(FrameDisplay);
            // Do any additional setup after loading the view.
        }

        public override void ViewDidDisappear()
        {
            base.ViewDidDisappear();

            capturer.StopCapture();
        }
    }
}
