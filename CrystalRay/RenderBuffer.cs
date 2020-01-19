using System;
using System.Collections.Generic;
using System.Threading;

namespace CrystalRay
{
	public unsafe delegate void PixelFilter<T>(T data, Vector3* pixels, int width, int height);

	public sealed class RenderBuffer
	{
		private readonly Vector3[,] _pixels;
		private Scene _currentScene;
		private Camera _currentCamera;
		private Vector2 _viewPortMin, _viewPortMax;
		private double _viewPortDist;
		private readonly int _width;
		private readonly int _height;
		private int _currentX, _currentY;
		private int _threadCount;
		private int _maxBounces;
		private readonly List<Thread> _renderThreads;
		private readonly object _syncRoot;
		private bool _isRendering;

		public event EventHandler FinishedRendering;

		public RenderBuffer(int width, int height)
		{
			_threadCount = Environment.ProcessorCount;
			_width = width;
			_height = height;
			_maxBounces = 5;
			_pixels = new Vector3[height, width];
			_renderThreads = new List<Thread>(_threadCount);
			_syncRoot = new object();
		}

		/// <summary>
		/// Gets or sets the color of a given pixel
		/// </summary>
		/// <param name="x">X coordinate of the pixel</param>
		/// <param name="y">Y coordinate of the pixel</param>
		/// <returns>Returns the color of the pixel</returns>
		public Vector3 this[int x, int y]
		{
			get => _pixels[y, x];
			set => _pixels[y, x] = value;
		}

		/// <summary>
		/// Gets or sets the number of threads to use for rendering
		/// </summary>
		public int ThreadCount
		{
			get => _threadCount;
			set
			{
				lock (_syncRoot)
				{
					if (IsRendering)
						throw new InvalidOperationException();
					if (value < 1)
						throw new ArgumentOutOfRangeException("value");
					_threadCount = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the maximum number of bounces (reflections and refractions) allowed for a single ray
		/// </summary>
		public int MaximumBounces
		{
			get => _maxBounces;
			set
			{
				lock (_syncRoot)
				{
					if (IsRendering)
						throw new InvalidOperationException();
					if (value < 1)
						throw new ArgumentOutOfRangeException("value");
					_maxBounces = value;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating if there is a rendering operation currently running
		/// </summary>
		public bool IsRendering => Volatile.Read(ref _isRendering);

		/// <summary>
		/// Clears the buffer with black
		/// </summary>
		public void Clear() => Clear(Vector3.Empty);

		/// <summary>
		/// Clears the buffer with the specified color
		/// </summary>
		/// <param name="color">Color to use</param>
		public void Clear(Vector3 color)
		{
			lock (_syncRoot)
			{
				if (IsRendering)
					throw new InvalidOperationException();
				for (int i = 0; i < _height; i++)
				{
					for (int j = 0; j < _width; j++)
						_pixels[i, j] = color;
				}
			}
		}

		/// <summary>
		/// Begins to render the scene to the buffer
		/// </summary>
		/// <param name="scene">The Scene to render</param>
		public void BeginRendering(Scene scene, Camera camera)
		{
			// First, check that we are not already rendering
			lock (_syncRoot)
			{
				if (IsRendering)
					throw new InvalidOperationException();
				if (scene == null)
					throw new ArgumentNullException(nameof(scene));
				if (camera == null)
					throw new ArgumentNullException(nameof(camera));
				Volatile.Write(ref _isRendering, true);
			}
			_currentScene = scene;
			_currentCamera = camera;
			RecalculateViewPort();
			_currentX = 0;
			_currentY = 0;
			_renderThreads.Clear();
			for (int i = 0; i < _threadCount; i++)
			{
				var renderThread = new Thread(RenderThread);

				_renderThreads.Add(renderThread);

				renderThread.Start();
			}
		}

		/// <summary>
		/// Cancels the current rendering operation
		/// </summary>
		public void CancelRendering()
		{
			lock (_syncRoot)
			{
				if (IsRendering)
				{
					Volatile.Write(ref _isRendering, false);
					for (int i = 0; i < _renderThreads.Count; i++)
						_renderThreads[i].Abort();
					_renderThreads.Clear();
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
			fixed (Vector3* pPixels = _pixels)
				filter(data, pPixels, _width, _height);
		}

		private void RecalculateViewPort()
		{
			double angleH, angleV;
			double sinH, cosH, sinV, cosV;

			angleH = _currentCamera.FieldOfVision.X * 0.5f;
			angleV = _currentCamera.FieldOfVision.Y * 0.5f;

			sinH = (double)Math.Sin(angleH);
			cosH = (double)Math.Cos(angleH);

			sinV = (double)Math.Sin(angleV);
			cosV = (double)Math.Cos(angleV);

			_viewPortMin = new Vector2(sinH * cosV, -sinV);
			_viewPortMax = new Vector2(-sinH * cosV, sinV);
			_viewPortDist = cosH * cosV;
		}

		/// <summary>
		/// Render method for a Thread
		/// </summary>
		private void RenderThread()
		{
			int x, y;
			double rx, ry;
			bool lastPixel = false;
			Stack<double> indexStack = null;

			// Render pixels in a multithreaded fashion
			// The pixel to render is chosen in a synchronized manner, which may look heavy
			// But the lock here is negligible compared to the real rendering work
			while (IsRendering)
			{
				Thread.Sleep(0);
				// First lookup for a pixel to render
				lock (_syncRoot)
				{
					if (_currentY >= _height)
						break;

					x = _currentX++;
					y = _currentY;

					if (_currentX >= _width)
					{
						_currentX = 0;
						_currentY++;
					}

					if (_currentY >= _height)
						lastPixel = true;
				}

				rx = (double)x / (_width - 1);
				ry = (double)y / (_height - 1);

				// Then render the pixel
				_pixels[y, x] = (Vector3)_currentScene.Cast(new Ray(_currentCamera.Position, new Vector3(Vector2.Lerp(rx, ry, _viewPortMin, _viewPortMax), _viewPortDist)), _currentCamera, ref indexStack, _maxBounces);
			}

			if (lastPixel)
			{
				Volatile.Write(ref _isRendering, false);
				OnFinishedRendering(EventArgs.Empty);
			}
		}

		/// <summary>
		/// Called when the rendering is finished
		/// </summary>
		/// <param name="e">EventArgs object</param>
		/// <remarks>It is garanteed that this method will only be called once, but it is not possible to predict whic thread will call it</remarks>
		private void OnFinishedRendering(EventArgs e) => FinishedRendering?.Invoke(this, e);
	}
}
