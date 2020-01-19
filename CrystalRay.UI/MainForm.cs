using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace CrystalRay.UI
{
    public sealed partial class MainForm : Form
	{
		static readonly Vector3 luminanceVector = new Vector3(0.2125, 0.7154, 0.0721);

		Scene scene;
		RenderBuffer renderBuffer;
		Bitmap renderBitmap;
		bool rendered, needRedraw;

		public MainForm()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.ResizeRedraw, true);
			InitializeComponent();
			InitializeBuffers(800, 600);
			InitializeScene();
			BeginRendering();
		}

		public void InitializeBuffers(int width, int height)
		{
			renderBuffer = new RenderBuffer(width, height);
			renderBuffer.FinishedRendering += new EventHandler(OnFinishedRendering);
			renderBitmap = new Bitmap(width, height);
			needRedraw = false;
			rendered = false;
			ClientSize = new Size(width, height);
		}

		public void InitializeScene()
		{
			Material bluePlastic = new Material(new Vector4(0, 0, 1, 1)),
				redPlastic = new Material(new Vector4(1, 0, 0, 1)),
				greenPlastic = new Material(new Vector4(0, 1, 0, 1)),
				whitePlaster = new Material(new Vector4(1, 1, 1, 1), new Vector4(1, 1, 1, 1), 1000),
				mirror = new Material(new Vector4(0.5, 0.5, 0.5, 1), new Vector4(1, 1, 1, 1), 500, 1.0),
				glass = new Material(new Vector4(1.0, 1.0, 1.0, 0.1), new Vector4(1, 1, 1, 1), 100, 0.1, 1.5);

			scene = new Scene();

			//scene.Ambient = new Vector4(0.1, 0.1, 0.1, 1.0);

			redPlastic.Shininess = 70;
			greenPlastic.Shininess = 70;
			bluePlastic.Shininess = 70;

			scene.Elements.Add(new Sphere(new Vector3(-1.25, 0, 5), 1.5, redPlastic));
			scene.Elements.Add(new Sphere(new Vector3(0, 0, 5), 1.5, greenPlastic));
			scene.Elements.Add(new Sphere(new Vector3(1.25, 0, 5), 1.5, bluePlastic));
			scene.Elements.Add(new Plane(new Ray(new Vector3(0, -1.51, 0), new Vector3(0, 1, 0)), mirror));
			scene.Elements.Add(new Plane(new Ray(new Vector3(0, 0, 10), new Vector3(0, 0, -1)), whitePlaster));
			scene.Elements.Add(new Plane(new Ray(new Vector3(0, 0, -3), new Vector3(0, 0, -1)), whitePlaster));
			scene.Elements.Add(new Plane(new Ray(new Vector3(-10, 0, 0), new Vector3(1, 0, 0)), whitePlaster));
			scene.Elements.Add(new Plane(new Ray(new Vector3(10, 0, 0), new Vector3(-1, 0, 0)), whitePlaster));
			scene.Elements.Add(new Plane(new Ray(new Vector3(0, 8.0, 0), new Vector3(0, 1, 0)), whitePlaster));
			scene.Elements.Add(new Sphere(new Vector3(-1, -1, 0.5), 0.5, mirror));
			scene.Elements.Add(new Sphere(new Vector3(0, -1, 1), 0.5, mirror));
			scene.Elements.Add(new Sphere(new Vector3(1, -1, 0.5), 0.5, mirror));

			scene.Elements.Add(new PointLight(new Vector3(-2, 2, 3), new Vector4(1, 1, 1, 1), new LightAttenuation(0.0, 0.0, 0.6)));
			scene.Elements.Add(new PointLight(new Vector3(2, 2, 3), new Vector4(1, 1, 1, 1), new LightAttenuation(0.0, 0.0, 0.6)));
			scene.Elements.Add(new PointLight(new Vector3(0, 6, 6), new Vector4(1, 1, 1, 1), new LightAttenuation(0.0, 0.0, 0.4)));

			scene.Elements.Add(new SpotLight(new Ray(new Vector3(0, -1.5, 2), new Vector3(-1, 1, 1)), new Vector4(1, 0, 1, 1), new SpotLightAttenuation(0, 0, 0.2, 0.5 * Math.PI, 0.4 * Math.PI, 1)));
			scene.Elements.Add(new SpotLight(new Ray(new Vector3(0, -1.5, 2), new Vector3(0, 1, 1)), new Vector4(1, 1, 0, 1), new SpotLightAttenuation(0, 0, 0.2, 0.5 * Math.PI, 0.4 * Math.PI, 1)));
			scene.Elements.Add(new SpotLight(new Ray(new Vector3(0, -1.5, 2), new Vector3(1, 1, 1)), new Vector4(0, 1, 1, 1), new SpotLightAttenuation(0, 0, 0.2, 0.5 * Math.PI, 0.4 * Math.PI, 1)));
		}

		public void BeginRendering()
		{
			renderBuffer.BeginRendering(scene, new Camera());
		}

		public void CancelRendering()
		{
			try { renderBuffer.CancelRendering(); }
			catch { }
		}

		void OnFinishedRendering(object sender, EventArgs e)
		{
			needRedraw = true;
			Invalidate();
		}

		byte ClampToByte(double v)
		{
			if (v > 1)
				return 255;
			else if (v < 0)
				return 0;
			else
				return (byte)(255 * v);
		}

		unsafe void RenderToBitmap(object data, Vector3* pixels, int width, int height)
		{
			BitmapData bitmapData;
			byte* pLine, pPixel;

			bitmapData = renderBitmap.LockBits(new Rectangle(0, 0, renderBitmap.Width, renderBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

			pLine = (byte*)bitmapData.Scan0;

			for (int i = 0; i < height; i++)
			{
				pPixel = pLine;
				for (int j = 0; j < width; j++)
				{
					Vector3 color = *pixels++;

					*pPixel++ = ClampToByte(color.Z); // B
					*pPixel++ = ClampToByte(color.Y); // G
					*pPixel++ = ClampToByte(color.X); // R
					*pPixel++ = 255; // A
				}
				pLine += bitmapData.Stride;
			}

			renderBitmap.UnlockBits(bitmapData);
		}

		unsafe double ComputeAverageLuminance(Vector3* pixels, int width, int height)
		{
			int n = width * height;
			double accum = 0;

			for (int i = 0; i < n; i++)
				accum += Vector3.DotProduct(luminanceVector, *pixels++);

			return accum / n;
		}

		protected override unsafe void OnPaint(PaintEventArgs e)
		{
			if (needRedraw)
			{
				renderBuffer.FilterPixels<object>(RenderToBitmap, null);
				needRedraw = false;
				rendered = true;
			}
			if (rendered)
			{
				//e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
				e.Graphics.DrawImage(renderBitmap, 0, 0, ClientSize.Width, ClientSize.Height);
			}
			else
				e.Graphics.FillRectangle(Brushes.Black, ClientRectangle);
			base.OnPaint(e);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (renderBuffer.Rendering && MessageBox.Show(this, "The rendering is not finished yet. Do you really want to abort it ?", "CrystalRT", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.No)
				e.Cancel = true;
			base.OnClosing(e);
		}

		protected override void OnClosed(EventArgs e)
		{
			CancelRendering();
			base.OnClosed(e);
		}
	}
}
