﻿using System;
using Android.Widget;
using Android.Graphics;

namespace Blurate
{
	public class DefaultOnDoubleTapListener:Java.Lang.Object, Android.Views.GestureDetector.IOnDoubleTapListener 
	{
		private PhotoViewAttacher photoViewAttacher;

		public DefaultOnDoubleTapListener(PhotoViewAttacher photoViewAttacher) 
		{
			SetPhotoViewAttacher(photoViewAttacher);
		}
			
		public void SetPhotoViewAttacher(PhotoViewAttacher newPhotoViewAttacher) 
		{
			this.photoViewAttacher = newPhotoViewAttacher;
		}

		#region IOnDoubleTapListener imp
		public bool OnDoubleTap (Android.Views.MotionEvent ev)
		{
			if (photoViewAttacher == null)
				return false;
			try {
				float scale = photoViewAttacher.GetScale();
				float x = ev.GetX();
				float y = ev.GetY();

				if (scale < photoViewAttacher.GetMediumScale()) {
					photoViewAttacher.SetScale(photoViewAttacher.GetMediumScale(), x, y, true);
				} else if (scale >= photoViewAttacher.GetMediumScale() && scale < photoViewAttacher.GetMaximumScale()) {
					photoViewAttacher.SetScale(photoViewAttacher.GetMaximumScale(), x, y, true);
				} else {
					photoViewAttacher.SetScale(photoViewAttacher.GetMinimumScale(), x, y, true);
				}
			} catch (Java.Lang.ArrayIndexOutOfBoundsException e) {
				// Can sometimes happen when getX() and getY() is called
			}

			return true;
		}

		public bool OnDoubleTapEvent (Android.Views.MotionEvent e)
		{
			return false;
		}

		public bool OnSingleTapConfirmed (Android.Views.MotionEvent e)
		{
			if (this.photoViewAttacher == null)
				return false;
			ImageView imageView = photoViewAttacher.GetImageView();
			if (null != photoViewAttacher.GetOnPhotoTapListener ()) {
				RectF displayRect = photoViewAttacher.GetDisplayRect();
				if (null != displayRect) {
					float x = e.GetX(), y = e.GetY();

					if (displayRect.Contains(x, y)) {

						float xResult = (x - displayRect.Left)
							/ displayRect.Width();
						float yResult = (y - displayRect.Top)
							/ displayRect.Height();

						photoViewAttacher.GetOnPhotoTapListener().OnPhotoTap(imageView, xResult, yResult);
						return true;
					}
				}
			}
			if (null != photoViewAttacher.GetOnViewTapListener()) {
				photoViewAttacher.GetOnViewTapListener().OnViewTap(imageView, e.GetX(), e.GetY());
			}

			return false;
		}
		#endregion
	}
}

