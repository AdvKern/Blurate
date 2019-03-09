// simple rectangle
typedef struct
{
    int x;
    int y;
    int width;
    int height;
} Rectangle;

// Haar rectangle
typedef struct
{
	int x;
	int y;
	int width;
	int height;
    float weight;
    int scaledX;
    int scaledY;
    int scaledWidth;
    int scaledHeight;
    float scaledWeight;
} HaarRectangle;

// Haar feature node
typedef struct
{
    float threshold;
    float leftValue;
    float rightValue;
    int rectCount;
    HaarRectangle rect[3];
} HaarFeatureNode;

// calculate the integral of a rectangle
inline uint getIntegralSum(read_only image2d_t integralImage, int x, int y, int width, int height)
{
    sampler_t smp = CLK_ADDRESS_CLAMP;
	uint4 color1 = read_imageui(integralImage, smp, (int2)(x, y));
	uint4 color2 = read_imageui(integralImage, smp, (int2)(x + width, y + height));
	uint4 color3 = read_imageui(integralImage, smp, (int2)(x, y + height));
	uint4 color4 = read_imageui(integralImage, smp, (int2)(x + width, y));

    return color1.x + color2.x - color3.x - color4.x;
}

// calculate feature node sum (= sum of all rectangles in node)
float getFeatureNodeSum(__global HaarFeatureNode* node,
	read_only image2d_t integralImage, float scale,	float invArea, int x, int y)
{
	float4 rects[3];
	float weights[3];

	// scale first rectangle
	rects[0].x = node->rect[0].x * scale;
	rects[0].y = node->rect[0].y * scale;
	rects[0].z = node->rect[0].width * scale;
	rects[0].w = node->rect[0].height * scale;

	// scale and weight second rectangle
	rects[1].x = node->rect[1].x * scale;
	rects[1].y = node->rect[1].y * scale;
	rects[1].z = node->rect[1].width * scale;
	rects[1].w = node->rect[1].height * scale;
	weights[1] = node->rect[1].weight * invArea;

	float sum;
	switch (node->rectCount)
	{
		case 2:
			weights[0] = -(rects[1].z * rects[1].w * weights[1]) / (rects[0].z * rects[0].w);
			sum = getIntegralSum(integralImage, x + rects[0].x, y + rects[0].y, 
				rects[0].z, rects[0].w) * weights[0];
			sum += getIntegralSum(integralImage, x + rects[1].x, y + rects[1].y, 
				rects[1].z, rects[1].w) * weights[1];
			return sum;
		case 3:
			// scale and weight third rectangle
			rects[2].x = node->rect[2].x * scale;
			rects[2].y = node->rect[2].y * scale;
			rects[2].z = node->rect[2].width * scale;
			rects[2].w = node->rect[2].height * scale;
			weights[2] = node->rect[2].weight * invArea;

			weights[0] = -(rects[1].z * rects[1].w * weights[1] + rects[2].z * rects[2].w * weights[2]) / (rects[0].z * rects[0].w);

			sum = getIntegralSum(integralImage, x + rects[0].x, y + rects[0].y, 
				rects[0].z, rects[0].w) * weights[0];
			sum += getIntegralSum(integralImage, x + rects[1].x, y + rects[1].y, 
				rects[1].z, rects[1].w) * weights[1];
			sum += getIntegralSum(integralImage, x + rects[2].x, y + rects[2].y, 
				rects[2].z, rects[2].w) * weights[2];
			return sum;
	}
	return 0;
}

// detects the presence of an object in a given window
bool compute(__global HaarFeatureNode* stageNodes, int stagesCount,
	__global int* stageNodeCounts, __global float* stageThresholds, 
	read_only image2d_t integralImage, read_only image2d_t integral2Image,
	float scale, int x, int y, int w, int h)
{
    float invArea = 1 / ((float)w * (float)h);

	float mean = getIntegralSum(integralImage, x, y, w, h) * invArea;
    float normFactor = getIntegralSum(integral2Image, x, y, w, h) * invArea 
		- (mean * mean);

    normFactor = (normFactor >= 0) ? sqrt(normFactor) : 1;

	int pos = 0;
	for (int j = 0; j < stagesCount; j++)
    {
		int count = stageNodeCounts[j];
		float threshold = stageThresholds[j];

        float value = 0;
		for (int i = 0; i < count; i++)
		{
			__global HaarFeatureNode* node = &stageNodes[pos++];

			// evaluate the node's feature
			float sum = getFeatureNodeSum(node, integralImage, scale, invArea, x, y);

			// and increase the value accumulator
			value += ((sum < node->threshold * normFactor) ? node->leftValue : node->rightValue);
		}

		if (value <= threshold) return false; // window has been rejected
    }
    return true;
}
