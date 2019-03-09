__kernel void ImageFloatSetChannel(
					  read_only image2d_t srcImage,
					  read_only image2d_t maskImage,
					  write_only image2d_t dstImage,
					  const char offset)
{                             
	sampler_t smp = CLK_ADDRESS_NONE;
                                                               
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	union { float4 f4; float a4[4]; } color;
	color.f4 = read_imagef(srcImage, smp, coords);
	float4 mask = read_imagef(maskImage, smp, coords);
	color.a4[offset] = mask.w;
	write_imagef(dstImage, coords, color.f4);
}