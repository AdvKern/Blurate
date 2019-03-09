using System;
using Android.Views;

namespace Blurate
{
	public interface IGestureDetector
	{
		 bool OnTouchEvent(MotionEvent ev);

		 bool IsScaling();

		 void SetOnGestureListener(IOnGestureListener listener);
	}
}

