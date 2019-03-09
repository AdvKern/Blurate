__kernel void ImageFloatSwapChannel(
					  read_only image2d_t srcImage,
					  write_only image2d_t dstImage,
					  const char offsetFrom,
					  const char offsetTo)
{                             
	sampler_t smp = CLK_ADDRESS_NONE;
                                                               
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	union { float4 f4; float a4[4]; } color;
	color.f4 = read_imagef(srcImage, smp, coords);
	float value = color.a4[offsetFrom];
	color.a4[offsetFrom] = color.a4[offsetTo];
	color.a4[offsetTo] = value;
	write_imagef(dstImage, coords, color.f4);
}