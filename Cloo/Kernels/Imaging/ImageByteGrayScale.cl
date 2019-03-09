__kernel void ImageByteGrayScale(
					  read_only image2d_t srcImage,
					  write_only image2d_t dstImage)
{                         
	sampler_t smp = CLK_ADDRESS_NONE;
	                                                                   
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	uint4 color = read_imageui(srcImage, smp, coords);
	float gray = 0.2989f * (float)color.x + 0.5870f * (float)color.y + 0.1140f * (float)color.z;
	color.x = (uint)gray;
	write_imageui(dstImage, coords, color);
}