using System;
using System.Drawing.Imaging;
using System.IO;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace ScreenGrabber
{
    class FrameGrabber
    {
        const int _Adapter = 0;
        const int _Output = 0;

        private int _ScreenWidth;
        private int _ScreenHeight;

        private int _CaptureWidth;
        private int _CaptureHeight;
        private int _CaptureLeft;
        private int _CaptureTop;
        private int _CaptureRight;
        private int _CaptureBottom;

        private Device device;
        private Texture2D captureTexture;
        private OutputDuplication duplicatedOutput;

        public FrameGrabber(int width, int height)
        {
            var factory = new Factory1();
            var adapter = factory.GetAdapter1(_Adapter);
            device = new Device(adapter);
            var output = adapter.GetOutput(_Output);
            var output1 = output.QueryInterface<Output1>();

            _ScreenWidth = ((Rectangle)output.Description.DesktopBounds).Width;
            _ScreenHeight = ((Rectangle)output.Description.DesktopBounds).Height;

            _CaptureLeft = (_ScreenWidth / 2) - (width / 2);
            _CaptureTop = (_ScreenHeight / 2) - (height / 2);
            _CaptureWidth = width;
            _CaptureHeight = height;
            _CaptureRight = _CaptureLeft + width;
            _CaptureBottom = _CaptureTop + height;

            var textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = _CaptureWidth,
                Height = _CaptureHeight,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };
            captureTexture = new Texture2D(device, textureDesc);
            duplicatedOutput = output1.DuplicateOutput(device);
        }

        public void Render(RenderController renderController)
        {
            var rc_context = renderController.GetDeviceContext();
            
            try
            {
                SharpDX.DXGI.Resource screenResource;
                OutputDuplicateFrameInformation frameInfo;

                duplicatedOutput.AcquireNextFrame(1000, out frameInfo, out screenResource);

                //Console.WriteLine(frameInfo.AccumulatedFrames);
                if(screenResource != null)
                { 
                    using (var screenTexture2D = screenResource.QueryInterface<Texture2D>())
                    {
                        device.ImmediateContext.CopySubresourceRegion(
                                screenTexture2D,
                                0,
                                new ResourceRegion(_CaptureLeft, _CaptureTop, 0, _CaptureRight, _CaptureBottom, 1),
                                captureTexture,
                                0
                            );
                    }
                    var mapSource = device.ImmediateContext.MapSubresource(captureTexture, 0, MapMode.Read, MapFlags.None);
                    var outbmp = new SharpDX.Direct2D1.Bitmap(
                        rc_context,
                        new Size2(_CaptureWidth, _CaptureHeight),
                        new SharpDX.Direct2D1.BitmapProperties(
                            new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                            SharpDX.Direct2D1.AlphaMode.Ignore)
                            )
                        );
                    outbmp.CopyFromMemory(mapSource.DataPointer, mapSource.RowPitch);
                    device.ImmediateContext.UnmapSubresource(captureTexture, 0);

                    rc_context.BeginDraw();
                    rc_context.DrawBitmap(outbmp, 1.0f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
                    rc_context.EndDraw();

                    screenResource.Dispose();
                    duplicatedOutput.ReleaseFrame();
                }

            }
            catch (SharpDXException e)
            {
                if (e.ResultCode.Code != SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                {
                    Console.WriteLine(e.Message);
                    Console.ReadLine();
                }
            }
        }


    }
}
