using System;
using System.Windows.Forms;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Direct2D1;
using SharpDX.Windows;

using Device = SharpDX.Direct3D11.Device;
using FactoryD2D = SharpDX.Direct2D1.Factory;
using FactoryDXGI = SharpDX.DXGI.Factory1;

namespace ScreenGrabber
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            var form = new MainForm("HeadShotter");

            RenderLoop.Run(form, () =>
            {
                form.Render();
            });
        }
    }
}
