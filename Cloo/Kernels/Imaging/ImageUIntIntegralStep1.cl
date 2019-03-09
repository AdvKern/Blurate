__kernel void ImageUIntIntegralStep1(
					  read_only image2d_t srcImage,
					  write_only image2d_t sumImage
					  )
{                                
	sampler_t smp = CLK_ADDRESS_NONE;
	                                                            
	uint y = get_global_id(0);
	uint width = get_image_width(srcImage) - 1;
	uint4 color = 0;

	uint tempVal = 0;
	for (int x = 0; x < width; x++)
	{
		color = read_imageui(srcImage, smp, (int2)(x, y));
		tempVal += color.x;
		color.x = tempVal;
		write_imageui(sumImage, (int2)(x + 1, y + 1), color);
	}
}