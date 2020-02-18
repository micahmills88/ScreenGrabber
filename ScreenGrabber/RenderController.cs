using System;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Direct2D1;
using System.Collections.Generic;

namespace ScreenGrabber
{
    public sealed class RenderController
    {
        private bool canDraw = false;
        SharpDX.Direct3D11.Device1 device;
        SharpDX.DXGI.SwapChain1 swapChain;
        SharpDX.DXGI.Surface backBuffer;
        SharpDX.Direct2D1.Device d2dDevice;
        SharpDX.Direct2D1.DeviceContext d2dContext;
        SharpDX.Direct2D1.BitmapProperties1 properties;
        SharpDX.Direct2D1.Bitmap1 d2dTarget;
        SharpDX.DirectWrite.Factory writeFactory;

        public RenderController(IntPtr windowHandle)
        {
            writeFactory = new SharpDX.DirectWrite.Factory();

            SharpDX.Direct3D11.Device defaultDevice = new SharpDX.Direct3D11.Device
            (
                DriverType.Hardware,
                DeviceCreationFlags.Debug | DeviceCreationFlags.BgraSupport
            );

            device = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device1>();
            SharpDX.DXGI.Device2 dxgiDevice2 = device.QueryInterface<SharpDX.DXGI.Device2>();
            SharpDX.DXGI.Adapter dxgiAdapter = dxgiDevice2.Adapter;
            SharpDX.DXGI.Factory2 dxgiFactory2 = dxgiAdapter.GetParent<SharpDX.DXGI.Factory2>();

            SwapChainDescription1 description = new SwapChainDescription1()
            {
                Width = 0,
                Height = 0,
                Format = Format.B8G8R8A8_UNorm,
                Stereo = false,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 2,
                Scaling = Scaling.None,
                SwapEffect = SwapEffect.FlipSequential,
            };

            swapChain = new SwapChain1(dxgiFactory2, device, windowHandle, ref description);
            backBuffer = Surface.FromSwapChain(swapChain, 0);

            d2dDevice = new SharpDX.Direct2D1.Device(dxgiDevice2);
            d2dContext = new SharpDX.Direct2D1.DeviceContext(d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.None);
            properties = new BitmapProperties1
            (
                new SharpDX.Direct2D1.PixelFormat
                (
                    SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                    SharpDX.Direct2D1.AlphaMode.Premultiplied
                ),
                0, 0, BitmapOptions.Target | BitmapOptions.CannotDraw
            );
            d2dTarget = new Bitmap1(d2dContext, backBuffer, properties);
            d2dContext.Target = d2dTarget;

            canDraw = true;
        }

        public void Resize()
        {
            canDraw = false;
            device.ImmediateContext.ClearState();
            d2dTarget.Dispose();
            d2dContext.Dispose();
            backBuffer.Dispose();

            swapChain.ResizeBuffers(2, 0, 0, Format.Unknown, SwapChainFlags.None);

            backBuffer = Surface.FromSwapChain(swapChain, 0);
            d2dContext = new SharpDX.Direct2D1.DeviceContext(d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.None);
            d2dTarget = new Bitmap1(d2dContext, backBuffer, properties);
            d2dContext.Target = d2dTarget;
            canDraw = true;
        }

        public void ResetTarget()
        {
            d2dContext.Target = d2dTarget;
        }

        public SharpDX.DirectWrite.Factory GetWriteFactory()
        {
            return writeFactory;
        }

        public SharpDX.Direct2D1.DeviceContext GetDeviceContext()
        {
            return this.d2dContext;
        }

        public Surface GetBackBuffer()
        {
            return backBuffer;
        }

        public bool CanDraw()
        {
            return canDraw;
        }

        public void ClearScreen()
        {
            d2dContext.BeginDraw();
            d2dContext.Clear(Color.White);
            d2dContext.EndDraw();
        }

        public void Present()
        {
            swapChain.Present(0, PresentFlags.None);
        }

        ~RenderController()
        {
            writeFactory.Dispose();
            d2dTarget.Dispose();
            d2dContext.Dispose();
            backBuffer.Dispose();
            swapChain.Dispose();
            device.Dispose();
        }
    }
}
