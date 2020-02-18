using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenGrabber
{
    public partial class MainForm : Form
    {
        Size currentSize;

        //graphics stuff
        RenderController renderController;
        FrameGrabber frameGrabber;

        public MainForm(string v)
        {
            InitializeComponent();
            this.Text = v;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //build various controllers and renderers
            renderController = new RenderController(this.Handle);
            //call create resources if needed
            frameGrabber = new FrameGrabber(this.ClientSize.Width, this.ClientSize.Height);

            //other stuff
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
            currentSize = this.ClientSize;
        }

        public void Render()
        {
            if (renderController.CanDraw())
            {
                renderController.ClearScreen();

                frameGrabber.Render(renderController);

                renderController.Present();
            }
        }
    }
}
