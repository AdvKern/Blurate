__kernel void ImageByteSobel(
					  read_only image2d_t srcImage,
					  write_only image2d_t dstImage,
					  sampler_t smp)
{                                                                                            
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	uint4 color1 = read_imageui(srcImage, smp, (int2)(x    , y    ));
	uint4 color2 = read_imageui(srcImage, smp, (int2)(x + 1, y    ));
	uint4 color3 = read_imageui(srcImage, smp, (int2)(x - 1, y    ));
	uint4 color4 = read_imageui(srcImage, smp, (int2)(x    , y + 1));
	uint4 color5 = read_imageui(srcImage, smp, (int2)(x    , y - 1));
	uint4 color6 = read_imageui(srcImage, smp, (int2)(x + 1, y + 1));
	uint4 color7 = read_imageui(srcImage, smp, (int2)(x - 1, y + 1));
	uint4 color8 = read_imageui(srcImage, smp, (int2)(x + 1, y - 1));
	uint4 color9 = read_imageui(srcImage, smp, (int2)(x - 1, y - 1));

	uint4 sum1 = color9 + 2 * color5 + color8 - color7 - 2 * color4 - color6;
	uint4 sum2 = color9 + 2 * color3 + color7 - color8 - 2 * color2 - color6;
	uint4 color = convert_uint4(native_sqrt(convert_float4(sum1 * sum1 + sum2 * sum2)));
	write_imageui(dstImage, coords, color);
}