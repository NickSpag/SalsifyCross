using System;
using AVFoundation;
using CoreMedia;
using CoreVideo;
using CoreFoundation;

#if __MACOS__
using AppKit;
using Foundation;
#else
using UIKit;
#endif

namespace SalsifyMacClient
{

    using View =
#if __MACOS__
    NSView;
#else
    UIView;
#endif

    /// <summary>
    /// The core delegate class for AVFoundation, captures video frames from a device. Use by 
    /// insantiating, configuring the live stream with an NSView or UIView, 
    /// </summary>
    public sealed class FrameCapturer : AVCaptureVideoDataOutputSampleBufferDelegate
    {
        private AVCaptureSession session;
        private AVCaptureDevice captureDevice;
        private AVCaptureDeviceInput deviceInput;
        private AVCaptureVideoDataOutput deviceOutput;
        private AVCaptureVideoPreviewLayer videoPreviewLayer;

        private DispatchQueue queue = new DispatchQueue("videoQueue");

        private FrameCapturerConfig config;
        private Action<CVPixelBuffer> nextFrameHandler;
        private Action<CVPixelBuffer> frameHandler;
        private View liveVideoStream;

        private bool isCapturing = false;
        private bool framesNeedHandling = false;
        private bool needNextFrame = false;
        private nfloat framesPerHandling = 0;
        private nfloat framesSinceLastHandling = 0;

        private FrameCapturer(FrameCapturerConfig config)
        {
            this.config = config;
            PrepareCaptureSession();
        }

        public static FrameCapturer WithConfiguration(FrameCapturerConfig config) => new FrameCapturer(config);

        private void ConfigureDisplay(View view)
        {
            videoPreviewLayer = new AVCaptureVideoPreviewLayer(session)
            {
                Frame = view.Bounds,
                VideoGravity = config.LayerGravity,
            };

            liveVideoStream = view;
            liveVideoStream.Layer.AddSublayer(videoPreviewLayer);
        }

        private void PrepareCaptureSession()
        {
            try
            {
                session = new AVCaptureSession
                {
                    SessionPreset = config.FrameQualityPreset,
                };

                captureDevice = config.Device;

                deviceInput = AVCaptureDeviceInput.FromDevice(captureDevice);
                deviceOutput = new AVCaptureVideoDataOutput();

                deviceOutput.WeakVideoSettings = new CVPixelBufferAttributes { PixelFormatType = config.PixelFormat }.Dictionary;
                deviceOutput.SetSampleBufferDelegateQueue(this, queue);

                session.AddInput(deviceInput);
                session.AddOutput(deviceOutput);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        public void BeginCapture(View videoStream)
        {
            isCapturing = true;
            ConfigureDisplay(videoStream);
            session.StartRunning();
        }

        public void ConfigureDetection(Action<CVPixelBuffer> handler)
        {
            frameHandler = handler;
        }

        /// <summary>
        /// Run the detection every X frames
        /// </summary>
        /// <param name="everyFrameCount">Number of frames per 1 handler invocation.</param>
        public void StartHandlingFrames(Action<CVPixelBuffer> handler, uint everyFrameCount = 1)
        {
            if (handler is null) throw new ArgumentNullException(nameof(handler));
            if (everyFrameCount == 0) throw new ArgumentException("Must be greater than 0", nameof(everyFrameCount));

            frameHandler = handler;
            framesNeedHandling = true;
            framesPerHandling = everyFrameCount;
        }

        public void StopHandlingFrames()
        {
            framesNeedHandling = false;
            framesSinceLastHandling = 0;
        }

        public void PauseCapture()
        {
            isCapturing = false;
            session?.StopRunning();
        }

        public void ResumeCapture()
        {
            if (!isCapturing)
            {
                session?.StartRunning();
                isCapturing = true;
            }
        }

        public void StopCapture()
        {
            if (session != null)
            {
                StopHandlingFrames();
                session.StopRunning();
                session.Dispose();
            }

            isCapturing = false;
        }

        /// <summary>
        /// Sends the next frame, out of band from the continual frame handling process if that is also running. This should not
        /// be used as a replacement for an every-frame-processing sceneario.
        /// </summary>
        /// <param name="frameHandler">Next frame delegate.</param>
        public void SendNextFrame(Action<CVPixelBuffer> frameHandler)
        {
            if (isCapturing)
            {
                //todo handle if there is already a request for the next frame- maybe a "you're doing that too quickly" exception.
                nextFrameHandler = frameHandler;
                needNextFrame = true;
            }
            else throw new InvalidOperationException("The Preview Video Layer is currently null or has no content.");
        }

        #region Methods: IAVCaptureVideoDataOutputSampleBufferDelegate
        public override void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
        {
            try
            {
                //process any immediate request first, and dont count it against the continual frame count
                if (needNextFrame)
                {
                    using (var pixelBuffer = sampleBuffer.GetImageBuffer() as CVPixelBuffer)
                    {
                        nextFrameHandler?.Invoke(pixelBuffer);
                    }

                    needNextFrame = false;
                    nextFrameHandler = null;
                }
                else if (framesNeedHandling)
                {
                    if (framesSinceLastHandling % framesPerHandling == 0)
                    {
                        using (var pixelBuffer = sampleBuffer.GetImageBuffer() as CVPixelBuffer)
                        {
                            frameHandler?.Invoke(pixelBuffer);
                        }
                        framesSinceLastHandling = 1;
                    }
                    else
                    {
                        framesSinceLastHandling++;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("**Error: Invoking Handler - " + ex.Message);
            }
            finally
            {
                sampleBuffer.Dispose();
            }
        }

        public override void DidDropSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
        {
            System.Console.WriteLine("Dropped frame.");
            sampleBuffer.Dispose();
        }
        #endregion
    }

    public sealed class FrameCapturerConfig
    {
        public AVLayerVideoGravity LayerGravity { get; set; } = AVLayerVideoGravity.ResizeAspectFill;
        public AVCaptureDevice Device { get; set; } = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
        public CVPixelFormatType PixelFormat { get; set; } = CVPixelFormatType.CV422YpCbCr8;
        public NSString FrameQualityPreset { get; set; } = AVCaptureSession.PresetHigh;

        public FrameCapturerConfig()
        {

        }
    }
}
