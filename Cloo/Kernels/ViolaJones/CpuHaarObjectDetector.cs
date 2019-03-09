#region License
/*

Copyright (c) 2010-2011 by Hans Wolff

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
using System.Drawing;
using Cloo;
using OpenClooVision.Imaging;

namespace OpenClooVision.Kernels.ViolaJones
{
    /// <summary>
    /// Object detector on CPU
    /// </summary>
    [CLSCompliant(false)]
    public class CpuHaarObjectDetector : BaseHaarObjectDetector
    {
        static CpuProgramViolaJones _cpuProgram = CpuProgramViolaJones.Create();

        protected float _invArea = 0;

        protected int _threads = 0;
        /// <summary>
        /// Number of threads to use
        /// </summary>
        public int Threads { get { if (_threads <= 0) _threads = Environment.ProcessorCount; return _threads; } set { _threads = value; } }

        /// <summary>
        /// Performs object detection on the given frame on CPU
        /// </summary>
        /// <param name="image">image where to perform the detection</param>
        /// <param name="threads">number of threads to use</param>
        public int ProcessFrame(IImage2DByteA image)
        {
            if (image.Width > _integralImage.Width || image.Height > _integralImage.Height)
                throw new ArgumentException("Specified image (" + image.Width + " x " + image.Height + ") is too big, maximum size is (" + _integralImage.Width + " x " + _integralImage.Height + ")");

            // create integral image
            _cpuProgram.Integral(image, _integralImage);
            _cpuProgram.IntegralSquare(image, _integral2Image);

            return ProcessFrame();
        }

        /// <summary>
        /// Checks a single window only
        /// </summary>
        /// <param name="rect">window rectangle of image to check</param>
        /// <returns></returns>
        public bool CheckSingleRect(Rectangle rect)
        {
            var factor = System.Math.Min(rect.Width / (float)WindowWidth, rect.Height / (float)WindowHeight);
            SetScale(factor);

            return (Compute(_stageNodes.HostBuffer, _stageCount, _stageNodeCounts.HostBuffer,
               _stageThresholds.HostBuffer, _integralImage, _integral2Image, _invArea,
               rect.Left, rect.Top, rect.Width, rect.Height));
        }

        /// <summary>
        /// Performs object detection on the given frame.
        /// </summary>
        public override int ProcessFrame()
        {
            _resultRectanglesCount = 0;

            int width = _integralImage.Width;
            int height = _integralImage.Height;

            float fstart, fstop, fstep;
            bool inv;

            switch (_scalingMode)
            {
                case ScalingMode.SmallerToLarger:
                    {
                        fstart = 1f;
                        fstop = System.Math.Min(width / (float)WindowWidth, height / (float)WindowHeight);
                        fstep = _scalingFactor;
                        inv = false;
                        break;
                    }
                case ScalingMode.LargerToSmaller:
                    {
                        fstart = System.Math.Min(width / (float)WindowWidth, height / (float)WindowHeight);
                        fstop = 1f;
                        fstep = 1f / _scalingFactor;
                        inv = true;
                        break;
                    }
                default:
                    {
                        fstart = System.Math.Min(width / (float)_minSize.Width, height / (float)_minSize.Height);
                        fstop = fstart + 1;
                        fstep = _scalingFactor;
                        inv = false;
                        break;
                    }
            }

            for (float f = fstart; (inv && f > fstop) || (!inv && f < fstop); f *= fstep)
            {
                // set window scale
                SetScale(f);

                // get scaled window size
                int windowWidth = (int)(WindowWidth * f);
                int windowHeight = (int)(WindowHeight * f);

                // check if the window is lesser than the minimum size
                if (windowWidth < _minSize.Width || windowHeight < _minSize.Height)
                {
                    if (inv) break; else continue;
                }

                // check if the window is bigger than the minimum size
                if (windowWidth > _maxSize.Width || windowHeight > _maxSize.Height)
                {
                    if (inv) continue; else break;
                }

                int stepX = windowWidth >> 3;
                if (stepX <= 0) stepX = 1;
                int stepY = windowHeight >> 3;
                if (stepY <= 0) stepX = 1;

                int endX = width - windowWidth;
                int endY = height - windowHeight;

                int countX = endX / stepX;
                int countY = endY / stepY;

                int threads = Threads;
                ParallelProcessor.For(0, threads, (i) =>
                {
                    // scan the integral image searching for positives
                    for (int y = 0; y < endY; y += stepY)
                    {
                        if (((y / stepY) % threads) != i) continue; // ok, this is another threads task
                        for (int x = 0; x < endX; x += stepX)
                        {
                            // try to detect an object inside the window
                            if (Compute(_stageNodes.HostBuffer, _stageCount, _stageNodeCounts.HostBuffer,
                                _stageThresholds.HostBuffer, _integralImage, _integral2Image, _invArea,
                                x, y, windowWidth, windowHeight))
                            {
                                // an object has been detected
                                lock (_resultRectangles)
                                {
                                    if (_resultRectanglesCount >= _resultRectangles.HostBuffer.Length) return; // maximum amount of rectangles found already
                                    _resultRectangles.HostBuffer[_resultRectanglesCount++] = new Rectangle(x, y, windowWidth, windowHeight);
                                }
                            }
                        }
                    }
                });
            }

            return _resultRectanglesCount;
        }

        /// <summary>
        /// Sets scale of all Haar rectangles in all nodes
        /// </summary>
        /// <param name="scale">scale factor</param>
        protected void SetScale(float scale)
        {
            _invArea = 1 / ((float)WindowWidth * (float)WindowHeight * scale * scale);

            for (int i = 0; i < _totalNodesCount; i++)
                HaarExtensions.SetScaleAndWeight(ref StageNodes.HostBuffer[i], scale, _invArea);
        }

        /// <summary>
        /// Detects the presence of an object in a given window.
        /// </summary>
        protected bool Compute(HaarFeatureNode[] stageNodes,
            int stagesCount,
            int[] stageNodeCounts, float[] stageThresholds,
            IImage2DUIntA integralImage,
            IImage2DUIntA integral2Image,
            float invArea,
            int x, int y, int w, int h)
        {
            float mean = (float)integralImage.GetIntegralSum(x, y, w, h) * invArea;
            float normFactor = (float)integral2Image.GetIntegralSum(x, y, w, h) * invArea
                - (mean * mean);

            normFactor = (normFactor >= 0) ? (float)Math.Sqrt(normFactor) : 1;

            int pos = 0;
            for (int j = 0; j < stagesCount; j++)
            {
                int count = stageNodeCounts[j];
                float threshold = stageThresholds[j];

                float value = 0;
                for (int i = 0; i < count; i++)
                {
                    // evaluate the node's feature
                    float sum = HaarExtensions.GetNodeSum(ref stageNodes[pos], integralImage, x, y);

                    // and increase the value accumulator
                    HaarFeatureNode node = stageNodes[pos++];
                    value += ((sum < node.Threshold * normFactor) ? node.LeftValue : node.RightValue);
                }

                if (value <= threshold) return false; // window has been rejected
            }
            return true;
        }

        /// <summary>
        /// Creates a face detector for CPU processing
        /// </summary>
        /// <param name="imageWidth">image width</param>
        /// <param name="imageHeight">image height</param>
        /// <returns>HaarObjectDetector</returns>
        public static CpuHaarObjectDetector CreateFaceDetector(int imageWidth, int imageHeight)
        {
            CpuHaarObjectDetector res = new CpuHaarObjectDetector();
            res.WindowWidth = 20;
            res.WindowHeight = 20;

            res._integralImage = CpuImage2DUIntA.Create(imageWidth, imageHeight);
            res._integral2Image = CpuImage2DUIntA.Create(imageWidth, imageHeight);
            res._resultRectangles = new CpuBuffer<Rectangle>(MaxResultRectangles);

            var tuple = CreateFaceDetector();
            res._stageNodes = new CpuBuffer<HaarFeatureNode>(tuple.Item1.ToArray());
            res._stageNodeCounts = new CpuBuffer<int>(tuple.Item2.ToArray());
            res._stageThresholds = new CpuBuffer<float>(tuple.Item3.ToArray());
            res._totalNodesCount = tuple.Item1.Count;
            res._stageCount = tuple.Item2.Count;
            return res;
        }
    }
}
