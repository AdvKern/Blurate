__kernel void ImageFloatBoxBlur(
					  read_only image2d_t srcImage,
					  write_only image2d_t dstImage,
					  sampler_t smp,
					  const int offset)
{                                                                                            
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	float4 color1 = read_imagef(srcImage, smp, (int2)(x         , y         ));
	float4 color2 = read_imagef(srcImage, smp, (int2)(x + offset, y         ));
	float4 color3 = read_imagef(srcImage, smp, (int2)(x - offset, y         ));
	float4 color4 = read_imagef(srcImage, smp, (int2)(x         , y + offset));
	float4 color5 = read_imagef(srcImage, smp, (int2)(x         , y - offset));
	float4 color6 = read_imagef(srcImage, smp, (int2)(x + offset, y + offset));
	float4 color7 = read_imagef(srcImage, smp, (int2)(x - offset, y + offset));
	float4 color8 = read_imagef(srcImage, smp, (int2)(x + offset, y - offset));
	float4 color9 = read_imagef(srcImage, smp, (int2)(x - offset, y - offset));
	float4 color = (color1 + color2 + color3 + color4 + color5 + color6 + color7 + color8 + color9) * 1.0f/9.0f;
	write_imagef(dstImage, coords, color);
}