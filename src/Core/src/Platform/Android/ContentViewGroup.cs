﻿using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Platform
{
	// TODO ezhart At this point, this is almost exactly a clone of LayoutViewGroup; we may be able to drop this class entirely
	public class ContentViewGroup : ViewGroup
	{
		IBorderStroke? _clip;

		public ContentViewGroup(Context context) : base(context)
		{
		}

		public ContentViewGroup(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public ContentViewGroup(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		public ContentViewGroup(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
		}

		public ContentViewGroup(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
		{
		}

		protected override void DispatchDraw(Canvas? canvas)
		{
			if (Clip != null)
				ClipChild(canvas);

			base.DispatchDraw(canvas);
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (Context == null)
			{
				return;
			}

			if (CrossPlatformMeasure == null)
			{
				base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
				return;
			}

			var deviceIndependentWidth = widthMeasureSpec.ToDouble(Context);
			var deviceIndependentHeight = heightMeasureSpec.ToDouble(Context);

			var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
			var heightMode = MeasureSpec.GetMode(heightMeasureSpec);

			var measure = CrossPlatformMeasure(deviceIndependentWidth, deviceIndependentHeight);

			// If the measure spec was exact, we should return the explicit size value, even if the content
			// measure came out to a different size
			var width = widthMode == MeasureSpecMode.Exactly ? deviceIndependentWidth : measure.Width;
			var height = heightMode == MeasureSpecMode.Exactly ? deviceIndependentHeight : measure.Height;

			var platformWidth = Context.ToPixels(width);
			var platformHeight = Context.ToPixels(height);

			// Minimum values win over everything
			platformWidth = Math.Max(MinimumWidth, platformWidth);
			platformHeight = Math.Max(MinimumHeight, platformHeight);

			SetMeasuredDimension((int)platformWidth, (int)platformHeight);
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			if (CrossPlatformArrange == null || Context == null)
			{
				return;
			}

			var destination = Context!.ToCrossPlatformRectInReferenceFrame(left, top, right, bottom);

			CrossPlatformArrange(destination);
		}

		internal IBorderStroke? Clip
		{
			get => _clip;
			set
			{
				_clip = value;
				PostInvalidate();
			}
		}

		internal Func<double, double, Graphics.Size>? CrossPlatformMeasure { get; set; }
		internal Func<Graphics.Rect, Graphics.Size>? CrossPlatformArrange { get; set; }

		void ClipChild(Canvas? canvas)
		{
			if (Clip == null || canvas == null)
				return;

			float density = Context.GetDisplayDensity();

			float strokeThickness = (float)(Clip.StrokeThickness * density);
			float offset = strokeThickness / 2;
			float w = (canvas.Width / density) - strokeThickness;
			float h = (canvas.Height / density) - strokeThickness;

			var bounds = new Graphics.RectF(offset, offset, w, h);
			var path = Clip.Shape?.PathForBounds(bounds);
			var currentPath = path?.AsAndroidPath(scaleX: density, scaleY: density);

			if (currentPath != null)
				canvas.ClipPath(currentPath);
		}
	}
}