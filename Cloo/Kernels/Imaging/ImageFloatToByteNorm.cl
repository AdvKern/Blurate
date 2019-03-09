__kernel void ImageFloatToByteNorm(
					  read_only image2d_t srcImage,
					  write_only image2d_t dstImage)
{                             
	sampler_t smp = CLK_ADDRESS_CLAMP;
	                                                               
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	float4 color = read_imagef(srcImage, smp, coords);
	color *= 255.0f;
	color = clamp(color, 0.0f, 255.0f);
	uint4 val = convert_uint4(color);

	write_imageui(dstImage, coords, val);
}