__kernel void ImageByteBoxBlur(
					  read_only image2d_t srcImage,
					  write_only image2d_t dstImage,
					  sampler_t smp,
					  const int offset)
{                                                                                            
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	uint4 color1 = read_imageui(srcImage, smp, (int2)(x         , y         ));
	uint4 color2 = read_imageui(srcImage, smp, (int2)(x + offset, y         ));
	uint4 color3 = read_imageui(srcImage, smp, (int2)(x - offset, y         ));
	uint4 color4 = read_imageui(srcImage, smp, (int2)(x         , y + offset));
	uint4 color5 = read_imageui(srcImage, smp, (int2)(x         , y - offset));
	uint4 color6 = read_imageui(srcImage, smp, (int2)(x + offset, y + offset));
	uint4 color7 = read_imageui(srcImage, smp, (int2)(x - offset, y + offset));
	uint4 color8 = read_imageui(srcImage, smp, (int2)(x + offset, y - offset));
	uint4 color9 = read_imageui(srcImage, smp, (int2)(x - offset, y - offset));
	uint4 color = (color1 + color2 + color3 + color4 + color5 + color6 + color7 + color8 + color9) / 9;
	write_imageui(dstImage, coords, color);
}