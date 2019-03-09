__kernel void ImageFloatGrayScale(
					  read_only image2d_t srcImage,
					  write_only image2d_t dstImage)
{                         
	sampler_t smp = CLK_ADDRESS_NONE;
	                                                                   
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	float4 color = read_imagef(srcImage, smp, coords);
	float gray = 0.2989f * color.x + 0.5870f * color.y + 0.1140f * color.z;
	color.x = gray;
	write_imagef(dstImage, coords, color);
}