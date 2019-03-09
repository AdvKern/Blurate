__kernel void ImageFloatMultiplyValue(
					  read_only image2d_t srcImage,
					  write_only image2d_t dstImage,
					  const float factor
					  )
{                         
	sampler_t smp = CLK_ADDRESS_NONE;
                                                                   
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	float4 color = read_imagef(srcImage, smp, coords);
	color *= factor;
	write_imagef(dstImage, coords, color);
}