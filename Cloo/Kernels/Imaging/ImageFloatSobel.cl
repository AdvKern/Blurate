__kernel void ImageFloatSobel(
					  read_only image2d_t srcImage,
					  write_only image2d_t dstImage,
					  sampler_t smp)
{                                                                                            
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	float4 color1 = read_imagef(srcImage, smp, (int2)(x    , y    ));
	float4 color2 = read_imagef(srcImage, smp, (int2)(x + 1, y    ));
	float4 color3 = read_imagef(srcImage, smp, (int2)(x - 1, y    ));
	float4 color4 = read_imagef(srcImage, smp, (int2)(x    , y + 1));
	float4 color5 = read_imagef(srcImage, smp, (int2)(x    , y - 1));
	float4 color6 = read_imagef(srcImage, smp, (int2)(x + 1, y + 1));
	float4 color7 = read_imagef(srcImage, smp, (int2)(x - 1, y + 1));
	float4 color8 = read_imagef(srcImage, smp, (int2)(x + 1, y - 1));
	float4 color9 = read_imagef(srcImage, smp, (int2)(x - 1, y - 1));

	float4 sum1 = color9 + 2.0f * color5 + color8 - color7 - 2.0f * color4 - color6;
	float4 sum2 = color9 + 2.0f * color3 + color7 - color8 - 2.0f * color2 - color6;
	float4 color = native_sqrt(sum1 * sum1 + sum2 * sum2);
	write_imagef(dstImage, coords, color);
}