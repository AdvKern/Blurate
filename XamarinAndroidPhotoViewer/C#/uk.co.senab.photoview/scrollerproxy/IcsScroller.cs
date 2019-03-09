using System;
using Android.Content;

namespace Blurate
{
	public class IcsScroller:GingerScroller
	{
		public IcsScroller(Context context):base(context) {

		}
		public override bool ComputeScrollOffset ()
		{
			return mScroller.ComputeScrollOffset();
		}
	}
}

