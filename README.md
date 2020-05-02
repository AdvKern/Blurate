This is a full Xamarin-Android OpenCL template developed by Hashem Hashemi.
Tested on Samsung, Motorola, and Sony devices. Google and Huawai block OpenCL usage as of release of this code.
The template streams camera images through the device's GPU with OpenCL kernels compiled on-device from source.
You can modify it to use your own OpenCL kernels.

Try built version here:
https://play.google.com/store/apps/details?id=com.advancedkernels.blurate

Based on source code from: 
https://github.com/chrisbanes/PhotoView 
and
Hans Wolff's OpenClooVision project (v0.4.1):
https://opencloovision.codeplex.com/

REQUIRED DISCLOSURES:

This is an export of [PhotoView](https://github.com/chrisbanes/PhotoView) to Xamarin android platform 

![PhotoView](https://raw.github.com/chrisbanes/PhotoView/master/header_graphic.png)

PhotoView aims to help produce an easily usable implementation of a zooming Android ImageView. It is currently being used in [photup](https://play.google.com/store/apps/details?id=uk.co.senab.photup).

## Features
- Out of the box zooming, using multi-touch and double-tap.
- Scrolling, with smooth scrolling fling.
- Works perfectly when using used in a scrolling parent (such as ViewPager).
- Allows the application to be notified when the displayed Matrix has changed. Useful for when you need to update your UI based on the current zoom/scroll position.
- Allows the application to be notified when the user taps on the Photo.


## Sample Usage
There is a [sample](https://github.com/samerzmd/Xamarin-Android-Photo-Viewer/blob/master/XamarinAndroidPhotoViewer/MainActivity.cs) provided which shows how to use the library in a more advanced way, but for completeness here is all that is required to get PhotoView working:

``` C#
ImageView mImageView;
PhotoViewAttacher mAttacher;

protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);
			PhotoView mImageView = FindViewById<PhotoView>(Resource.Id.iv_photo);

			Drawable bitmap = Resources.GetDrawable(Resource.Drawable.wallpaper);
			mImageView.SetImageDrawable(bitmap);

			// The MAGIC happens here!
			mAttacher = new PhotoViewAttacher(mImageView);
			
		}


// If you later call mImageView.SetImageDrawable/SetImageBitmap/SetImageResource/etc then you just need to call
mAttacher.Update();
```

## License

    Copyright 2014, 2015

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.

The MIT License (MIT)
Copyright (c) 2010-2011 Hans Wolff
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    
