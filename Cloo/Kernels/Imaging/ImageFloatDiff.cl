__kernel void ImageFloatDiff(
					  read_only image2d_t srcImage1,
					  read_only image2d_t srcImage2,
					  write_only image2d_t dstImage
					  )
{                                
	sampler_t smp = CLK_ADDRESS_NONE;
	                                                            
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	float4 color1 = read_imagef(srcImage1, smp, coords);
	float4 color2 = read_imagef(srcImage2, smp, coords);
	color1 -= color2;
	write_imagef(dstImage, coords, color1);
}