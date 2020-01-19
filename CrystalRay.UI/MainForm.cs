using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Windows.Forms;

namespace CrystalRay.UI
{
    public sealed partial class MainForm : Form
	{
		private static readonly Vector3 LuminanceVector = new Vector3(0.2125f, 0.7154f, 0.0721f);

		private Scene _scene;
		private RenderBuffer _renderBuffer;
		private Bitmap _renderBitmap;
		private bool _rendered, _needRedraw;

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
			_renderBuffer = new RenderBuffer(width, height);
			_renderBuffer.FinishedRendering += OnFinishedRendering;
			_renderBitmap = new Bitmap(width, height);
			_needRedraw = false;
			_rendered = false;
			ClientSize = new Size(width, height);
		}

		public void InitializeScene()
		{
			var bluePlastic = new Material(new Vector4(0, 0, 1, 1));
			var redPlastic = new Material(new Vector4(1, 0, 0, 1));
			var greenPlastic = new Material(new Vector4(0, 1, 0, 1));
			var whitePlaster = new Material(new Vector4(1, 1, 1, 1), new Vector4(1, 1, 1, 1), 1000);
			var mirror = new Material(new Vector4(0.5f, 0.5f, 0.5f, 1), new Vector4(1, 1, 1, 1), 500, 1.0f);
			var glass = new Material(new Vector4(1.0f, 1.0f, 1.0f, 0.1f), new Vector4(1, 1, 1, 1), 100, 0.1f, 1.5f);

			_scene = new Scene();

			//scene.Ambient = new Vector4(0.1, 0.1, 0.1, 1.0);

			redPlastic.Shininess = 70;
			greenPlastic.Shininess = 70;
			bluePlastic.Shininess = 70;

			_scene.Elements.Add(new Sphere(new Vector3(-1.25f, 0, 5), 1.5f, redPlastic));
			_scene.Elements.Add(new Sphere(new Vector3(0, 0, 5), 1.5f, greenPlastic));
			_scene.Elements.Add(new Sphere(new Vector3(1.25f, 0, 5), 1.5f, bluePlastic));
			_scene.Elements.Add(new Plane(new Ray(new Vector3(0, -1.51f, 0), new Vector3(0, 1, 0)), mirror));
			_scene.Elements.Add(new Plane(new Ray(new Vector3(0, 0, 10), new Vector3(0, 0, -1)), whitePlaster));
			_scene.Elements.Add(new Plane(new Ray(new Vector3(0, 0, -3), new Vector3(0, 0, -1)), whitePlaster));
			_scene.Elements.Add(new Plane(new Ray(new Vector3(-10, 0, 0), new Vector3(1, 0, 0)), whitePlaster));
			_scene.Elements.Add(new Plane(new Ray(new Vector3(10, 0, 0), new Vector3(-1, 0, 0)), whitePlaster));
			_scene.Elements.Add(new Plane(new Ray(new Vector3(0, 8.0f, 0), new Vector3(0, 1, 0)), whitePlaster));
			_scene.Elements.Add(new Sphere(new Vector3(-1, -1, 0.5f), 0.5f, mirror));
			_scene.Elements.Add(new Sphere(new Vector3(0, -1, 1), 0.5f, mirror));
			_scene.Elements.Add(new Sphere(new Vector3(1, -1, 0.5f), 0.5f, mirror));

			_scene.Elements.Add(new PointLight(new Vector3(-2, 2, 3), new Vector4(1, 1, 1, 1), new LightAttenuation(0, 0, 0.6f)));
			_scene.Elements.Add(new PointLight(new Vector3(2, 2, 3), new Vector4(1, 1, 1, 1), new LightAttenuation(0, 0, 0.6f)));
			_scene.Elements.Add(new PointLight(new Vector3(0, 6, 6), new Vector4(1, 1, 1, 1), new LightAttenuation(0, 0, 0.4f)));

			_scene.Elements.Add(new SpotLight(new Ray(new Vector3(0, -1.5f, 2), new Vector3(-1, 1, 1)), new Vector4(1, 0, 1, 1), new SpotLightAttenuation(0, 0, 0.2f, 0.5f * MathF.PI, 0.4f * MathF.PI, 1)));
			_scene.Elements.Add(new SpotLight(new Ray(new Vector3(0, -1.5f, 2), new Vector3(0, 1, 1)), new Vector4(1, 1, 0, 1), new SpotLightAttenuation(0, 0, 0.2f, 0.5f * MathF.PI, 0.4f * MathF.PI, 1)));
			_scene.Elements.Add(new SpotLight(new Ray(new Vector3(0, -1.5f, 2), new Vector3(1, 1, 1)), new Vector4(0, 1, 1, 1), new SpotLightAttenuation(0, 0, 0.2f, 0.5f * MathF.PI, 0.4f * MathF.PI, 1)));
		}

		public void BeginRendering()
			=> _renderBuffer.BeginRendering(_scene, new Camera());

		public void CancelRendering()
		{
			try { _renderBuffer.CancelRendering(); }
			catch { }
		}

		private void OnFinishedRendering(object sender, EventArgs e)
		{
			_needRedraw = true;
			Invalidate();
		}

		private byte ClampToByte(float v)
		{
			if (v > 1)
				return 255;
			else if (v < 0)
				return 0;
			else
				return (byte)(255 * v);
		}

		private unsafe void RenderToBitmap(object data, Vector3* pixels, int width, int height)
		{
			BitmapData bitmapData;
			byte* pLine, pPixel;

			bitmapData = _renderBitmap.LockBits(new Rectangle(0, 0, _renderBitmap.Width, _renderBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

			pLine = (byte*)bitmapData.Scan0;

			for (int i = 0; i < height; i++)
			{
				pPixel = pLine;
				for (int j = 0; j < width; j++)
				{
					var color = *pixels++;

					*pPixel++ = ClampToByte(color.Z); // B
					*pPixel++ = ClampToByte(color.Y); // G
					*pPixel++ = ClampToByte(color.X); // R
					*pPixel++ = 255; // A
				}
				pLine += bitmapData.Stride;
			}

			_renderBitmap.UnlockBits(bitmapData);
		}

		private unsafe float ComputeAverageLuminance(Vector3* pixels, int width, int height)
		{
			int n = width * height;
			float accum = 0;

			for (int i = 0; i < n; i++)
				accum += Vector3.Dot(LuminanceVector, *pixels++);

			return accum / n;
		}

		protected override unsafe void OnPaint(PaintEventArgs e)
		{
			if (_needRedraw)
			{
				_renderBuffer.FilterPixels<object>(RenderToBitmap, null);
				_needRedraw = false;
				_rendered = true;
			}
			if (_rendered)
			{
				//e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
				e.Graphics.DrawImage(_renderBitmap, 0, 0, ClientSize.Width, ClientSize.Height);
			}
			else
			{
				e.Graphics.FillRectangle(Brushes.Black, ClientRectangle);
			}

			base.OnPaint(e);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (_renderBuffer.IsRendering && MessageBox.Show(this, "The rendering is not finished yet. Do you really want to abort it ?", "CrystalRT", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.No)
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
