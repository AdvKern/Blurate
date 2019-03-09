__kernel void ImageByteSwapChannel(
					  read_only image2d_t srcImage,
					  write_only image2d_t dstImage,
					  const char offsetFrom,
					  const char offsetTo)
{                             
	sampler_t smp = CLK_ADDRESS_NONE;
                                                               
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	union { uint4 f4; uint a4[4]; } color;
	color.f4 = read_imageui(srcImage, smp, coords);
	uint value = color.a4[offsetFrom];
	color.a4[offsetFrom] = color.a4[offsetTo];
	color.a4[offsetTo] = value;
	write_imageui(dstImage, coords, color.f4);
}