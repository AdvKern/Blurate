__kernel void ImageFloatIntegralSquare(
		read_only image2d_t sumImage,
		write_only image2d_t dstImage,
		const int height)
{                                
	sampler_t smp = CLK_ADDRESS_NONE;
	                                                            
	uint x = get_global_id(0);
	float tempVal = 0;
	for (int y = 1; y < height; y++)
	{
		float4 color = read_imagef(sumImage, smp, (int2)(x + 1, y));
		tempVal += color.x;
		color.x = tempVal;
 		write_imagef(dstImage, (int2)(x + 1, y), color);
	}
}