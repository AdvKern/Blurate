#region License
/*

Copyright (c) 2010-2011 Hans Wolff

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

*/
#endregion

using System;
using System.Collections.Generic;
using OpenClooVision.Imaging;

namespace OpenClooVision.Kernels.ViolaJones
{
    /// <summary>
    /// Extension classes for Viola & Jones
    /// </summary>
    [CLSCompliant(false)]
    public static class HaarExtensions
    {
        /// <summary>
        /// Adds a Haar feature node to list
        /// </summary>
        /// <param name="nodes">List of HaarFeatureNodes</param>
        /// <param name="threshold">threshold of node</param>
        /// <param name="leftValue">left value</param>
        /// <param name="rightValue">right value</param>
        /// <param name="rectangles">HaarRectangle</param>
        public static void AddHaarFeature(this List<HaarFeatureNode> nodes, float threshold, float leftValue, float rightValue, params HaarRectangle[] rectangles)
        {
            HaarFeatureNode node = new HaarFeatureNode();
            node.Threshold = threshold;
            node.LeftValue = leftValue;
            node.RightValue = rightValue;
            node.RectCount = rectangles.Length;
            if (node.RectCount < 2 || node.RectCount > 3) throw new ArgumentException("Haar feature nodes must have 2 or 3 rectangles only");

            node.Rect1 = rectangles[0];
            node.Rect2 = rectangles[1];
            if (node.RectCount > 2) node.Rect3 = rectangles[2];

            nodes.Add(node);
        }

        /// <summary>
        /// Adds a Haar feature node to list
        /// </summary>
        /// <param name="nodes">List of HaarFeatureNodes</param>
        /// <param name="threshold">threshold of node</param>
        /// <param name="leftValue">left value</param>
        /// <param name="rightValue">right value</param>
        /// <param name="rectangles">HaarRectangle</param>
        public static void AddHaarFeature(this List<HaarFeatureNode> nodes, double threshold, double leftValue, double rightValue, params HaarRectangle[] rectangles)
        {
            HaarFeatureNode node = new HaarFeatureNode();
            node.Threshold = (float)threshold;
            node.LeftValue = (float)leftValue;
            node.RightValue = (float)rightValue;
            node.RectCount = rectangles.Length;
            if (node.RectCount < 2 || node.RectCount > 3) throw new ArgumentException("Haar feature nodes must have 2 or 3 rectangles only");

            node.Rect1 = rectangles[0];
            node.Rect2 = rectangles[1];
            if (node.RectCount > 2) node.Rect3 = rectangles[2];

            nodes.Add(node);
        }

        /// <summary>
        /// Gets the area of a rectangular feature
        /// </summary>
        public static float GetArea(this HaarRectangle rect)
        {
            return rect.ScaledWidth * rect.ScaledHeight;
        }

        /// <summary>
        /// Gets the sum of the pixels in a rectangle of the integral image.
        /// </summary>
        public static float GetIntegralSum(this IImage2DFloatA integralImage, int x, int y, int width, int height)
        {
            int yIndex = y * integralImage.Width;
            int yhIndex = (y + height) * integralImage.Width;
            return integralImage.HostBuffer[yIndex + x] + integralImage.HostBuffer[yhIndex + x + width]
                 - integralImage.HostBuffer[yhIndex + x] - integralImage.HostBuffer[yIndex + x + width];
        }

        /// <summary>
        /// Gets the sum of the pixels in a rectangle of the integral image.
        /// </summary>
        public static uint GetIntegralSum(this IImage2DUIntA integralImage, int x, int y, int width, int height)
        {
            int yIndex = y * integralImage.Width;
            int yhIndex = (y + height) * integralImage.Width;
            return integralImage.HostBuffer[yIndex + x] + integralImage.HostBuffer[yhIndex + x + width]
                 - integralImage.HostBuffer[yhIndex + x] - integralImage.HostBuffer[yIndex + x + width];
        }

        /// <summary>
        /// Gets the sum of a rectangular feature in an integral image.
        /// </summary>
        public static float GetSum(ref HaarRectangle rect, IImage2DFloatA integralImage, int x, int y)
        {
            return integralImage.GetIntegralSum(x + rect.ScaledX, y + rect.ScaledY, rect.ScaledWidth, rect.ScaledHeight) * rect.ScaledWeight;
        }

        /// <summary>
        /// Gets the sum of a rectangular feature in an integral image.
        /// </summary>
        public static float GetSum(ref HaarRectangle rect, IImage2DUIntA integralImage, int x, int y)
        {
            return integralImage.GetIntegralSum(x + rect.ScaledX, y + rect.ScaledY, rect.ScaledWidth, rect.ScaledHeight) * rect.ScaledWeight;
        }

        /// <summary>
        /// Gets the sum of the areas of the rectangular features in an integral image.
        /// </summary>
        public static float GetNodeSum(ref HaarFeatureNode node, IImage2DFloatA integralImage, int x, int y)
        {
            float sum;
            sum = GetSum(ref node.Rect1, integralImage, x, y);
            sum += GetSum(ref node.Rect2, integralImage, x, y);
            if (node.RectCount == 3) sum += GetSum(ref node.Rect3, integralImage, x, y);

            return sum;
        }

        /// <summary>
        /// Gets the sum of the areas of the rectangular features in an integral image.
        /// </summary>
        public static float GetNodeSum(ref HaarFeatureNode node, IImage2DUIntA integralImage, int x, int y)
        {
            float sum;
            sum = GetSum(ref node.Rect1, integralImage, x, y);
            sum += GetSum(ref node.Rect2, integralImage, x, y);
            if (node.RectCount == 3) sum += GetSum(ref node.Rect3, integralImage, x, y);

            return sum;
        }

        /// <summary>
        /// Sets the scale and weight of a Haar-like rectangular feature container.
        /// </summary>
        public static void SetScaleAndWeight(ref HaarFeatureNode node, float scale, float weight)
        {
            // manual loop unfolding
            if (node.RectCount == 2)
            {
                ScaleRectangle(ref node.Rect2, scale, weight);

                ScaleRectangle(ref node.Rect1, scale, weight);
                node.Rect1.ScaledWeight = -(node.Rect2.GetArea() * node.Rect2.ScaledWeight) / node.Rect1.GetArea();
            }
            else // rectangles.Length == 3
            {
                ScaleRectangle(ref node.Rect3, scale, weight);

                ScaleRectangle(ref node.Rect2, scale, weight);

                ScaleRectangle(ref node.Rect1, scale, weight);
                node.Rect1.ScaledWeight =
                    -(node.Rect2.GetArea() * node.Rect2.ScaledWeight + node.Rect3.GetArea() * node.Rect3.ScaledWeight)
                    / (node.Rect1.GetArea());
            }
        }

        /// <summary>
        /// Scales the values of rectangle
        /// </summary>
        public static void ScaleRectangle(ref HaarRectangle rect, float scaleRect, float scaleWeight)
        {
            rect.ScaledX = (int)(rect.X * scaleRect);
            rect.ScaledY = (int)(rect.Y * scaleRect);
            rect.ScaledWidth = (int)(rect.Width * scaleRect);
            rect.ScaledHeight = (int)(rect.Height * scaleRect);
            rect.ScaledWeight = rect.Weight * scaleWeight;
        }
    }
}
