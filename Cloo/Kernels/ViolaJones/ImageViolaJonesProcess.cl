// set scale for Haar node rectangles and weight
__kernel void ImageViolaJonesProcess(
					  __global HaarFeatureNode* stageNodes,
					  int stagesCount,
					  __global int* stageNodeCounts,
					  __global float* stageThresholds,
					  read_only image2d_t integralImage,        // integral image
					  read_only image2d_t integral2Image,       // squared integral image
					  __global Rectangle* resRectangles,        // result rectangles
                      int maxResRectangles,
					  float scale,
					  int stepX,
					  int stepY,
					  int windowWidth,
					  int windowHeight)
{
	int x = get_global_id(0) * stepX;
	int y = get_global_id(1) * stepY;
	int imageWidth = get_image_width(integralImage);

	// try to detect an object inside the window
    bool res = compute(stageNodes, stagesCount, stageNodeCounts,
        stageThresholds, integralImage, integral2Image, scale,
        x, y, windowWidth, windowHeight);
	if (res)
    {
        // an object has been detected
		__global Rectangle* rect = &resRectangles[(get_global_id(0) * get_global_id(1) ^ imageWidth ^ stepX ^ windowWidth) % maxResRectangles];
		rect->x = x;
		rect->y = y;
		rect->width = windowWidth;
		rect->height = windowHeight;
    }
}
