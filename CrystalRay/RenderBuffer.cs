using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;

namespace CrystalRay
{
	public unsafe delegate void PixelFilter<T>(T data, Vector3* pixels, int width, int height);

	public sealed class RenderBuffer
	{
		Vector3[,] pixels;
		Scene currentScene;
		Camera currentCamera;
		Vector2 viewPortMin, viewPortMax;
		double viewPortDist;
		int width, height;
		int currentX, currentY;
		int threadCount;
		int maxBounces;
		List<Thread> renderThreads;
		bool rendering;
		object syncRoot;

		public event EventHandler FinishedRendering;

		public RenderBuffer(int width, int height)
		{
#if DEBUG
			// Because debugging multiple threads is really hard, we'll only use one while debugging
			this.threadCount = 1;
#else
			this.threadCount = Environment.ProcessorCount;
#endif
			this.width = width;
			this.height = height;
			this.maxBounces = 5;
			this.pixels = new Vector3[height, width];
            this.renderThreads = new List<Thread>(threadCount);
			this.syncRoot = new object();
		}

		/// <summary>
		/// Gets or sets the color of a given pixel
		/// </summary>
		/// <param name="x">X coordinate of the pixel</param>
		/// <param name="y">Y coordinate of the pixel</param>
		/// <returns>Returns the color of the pixel</returns>
		public Vector3 this[int x, int y]
		{
			get
			{
				return pixels[y, x];
			}
			set
			{
				pixels[y, x] = value;
			}
		}

		/// <summary>
		/// Gets or sets the number of threads to use for rendering
		/// </summary>
		public int ThreadCount
		{
			get
			{
				return threadCount;
			}
			set
			{
				lock (syncRoot)
				{
					if (rendering)
						throw new InvalidOperationException();
					if (value < 1)
						throw new ArgumentOutOfRangeException("value");
					threadCount = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the maximum number of bounces (reflections and refractions) allowed for a single ray
		/// </summary>
		public int MaximumBounces
		{
			get
			{
				return maxBounces;
			}
			set
			{
				lock (syncRoot)
				{
					if (rendering)
						throw new InvalidOperationException();
					if (value < 1)
						throw new ArgumentOutOfRangeException("value");
					maxBounces = value;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating if there is a rendering operation currently running
		/// </summary>
		public bool Rendering
		{
			get
			{
				return rendering;
			}
		}

		/// <summary>
		/// Clears the buffer with black
		/// </summary>
		public void Clear()
		{
			Clear(Vector3.Empty);
		}

		/// <summary>
		/// Clears the buffer with the specified color
		/// </summary>
		/// <param name="color">Color to use</param>
		public void Clear(Vector3 color)
		{
			lock (syncRoot)
			{
				if (rendering)
					throw new InvalidOperationException();
				for (int i = 0; i < height; i++)
					for (int j = 0; j < width; j++)
						pixels[i, j] = color;
			}
		}

		/// <summary>
		/// Begins to render the scene to the buffer
		/// </summary>
		/// <param name="scene">The Scene to render</param>
		public void BeginRendering(Scene scene, Camera camera)
		{
			// First, check that we are not already rendering
			lock (syncRoot)
			{
				if (rendering)
					throw new InvalidOperationException();
				if (scene == null)
					throw new ArgumentNullException("scene");
				if (camera == null)
					throw new ArgumentNullException("camera");
				rendering = true;
			}
			currentScene = scene;
			currentCamera = camera;
			RecalculateViewPort();
			currentX = 0;
			currentY = 0;
			renderThreads.Clear();
			for (int i = 0; i < threadCount; i++)
			{
				Thread renderThread = new Thread(RenderThread);

				renderThreads.Add(renderThread);

				renderThread.Start();
			}
		}

		/// <summary>
		/// Cancels the current rendering operation
		/// </summary>
		public void CancelRendering()
		{
			lock (syncRoot)
			{
				if (rendering)
				{
					rendering = false;
					for (int i = 0; i < renderThreads.Count; i++)
						renderThreads[i].Abort();
					renderThreads.Clear();
				}
			}
		}

		/// <summary>
		/// Filters the pixels with a given delegate
		/// </summary>
		/// <typeparam name="T">Type of the data taken by the delegate</typeparam>
		/// <param name="filter">The PixelFilter delegate to call</param>
		/// <param name="data">The data to pass to the filter</param>
		public unsafe void FilterPixels<T>(PixelFilter<T> filter, T data)
		{
			fixed (Vector3* pPixels = pixels)
				filter(data, pPixels, width, height);
		}

		void RecalculateViewPort()
		{
			double angleH, angleV;
			double sinH, cosH, sinV, cosV;

			angleH = currentCamera.FieldOfVision.X * 0.5f;
			angleV = currentCamera.FieldOfVision.Y * 0.5f;

			sinH = (double)Math.Sin(angleH);
			cosH = (double)Math.Cos(angleH);

			sinV = (double)Math.Sin(angleV);
			cosV = (double)Math.Cos(angleV);

			viewPortMin = new Vector2(sinH * cosV, -sinV);
			viewPortMax = new Vector2(-sinH * cosV, sinV);
			viewPortDist = cosH * cosV;
		}

		/// <summary>
		/// Render method for a Thread
		/// </summary>
		void RenderThread()
		{
			int x, y;
			double rx, ry;
			bool lastPixel = false;
			Stack<double> indexStack = null;

			// Render pixels in a multithreaded fashion
			// The pixel to render is chosen in a synchronized manner, which may look heavy
			// But the lock here is negligible compared to the real rendering work
			while (rendering)
			{
				Thread.Sleep(0);
				// First lookup for a pixel to render
				lock (syncRoot)
				{
					if (currentY >= height)
						break;

					x = currentX++;
					y = currentY;

					if (currentX >= width)
					{
						currentX = 0;
						currentY++;
					}

					if (currentY >= height)
						lastPixel = true;
				}

				rx = (double)x / (width - 1);
				ry = (double)y / (height - 1);

				// Then render the pixel
				pixels[y, x] = (Vector3)currentScene.Cast(new Ray(currentCamera.Position, new Vector3(Vector2.Lerp(rx, ry, viewPortMin, viewPortMax), viewPortDist)), currentCamera, ref indexStack, maxBounces);
			}

			if (lastPixel)
			{
				rendering = false;
				OnFinishedRendering(EventArgs.Empty);
			}
		}

		/// <summary>
		/// Called when the rendering is finished
		/// </summary>
		/// <param name="e">EventArgs object</param>
		/// <remarks>It is garanteed that this method will only be called once, but it is not possible to predict whic thread will call it</remarks>
		void OnFinishedRendering(EventArgs e)
		{
			if (FinishedRendering != null)
				FinishedRendering(this, e);
		}
	}
}
