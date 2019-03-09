﻿__kernel void ImageByteSetChannel(
					  read_only image2d_t srcImage,
					  read_only image2d_t maskImage,
					  write_only image2d_t dstImage,
					  const char offset)
{                             
	sampler_t smp = CLK_ADDRESS_NONE;
                                                               
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	union { uint4 f4; uint a4[4]; } color;
	color.f4 = read_imageui(srcImage, smp, coords);
	uint4 mask = read_imageui(maskImage, smp, coords);
	color.a4[offset] = mask.x;
	write_imageui(dstImage, coords, color.f4);
}