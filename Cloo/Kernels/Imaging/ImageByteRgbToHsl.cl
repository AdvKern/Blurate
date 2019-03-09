__kernel void ImageByteRgbToHsl(
					  read_only image2d_t srcImage,
					  write_only image2d_t dstImage)
{                                     
	sampler_t smp = CLK_ADDRESS_NONE;
	                                                       
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	uint4 color = read_imageui(srcImage, smp, coords);

	// find minimum
	uint min = color.x;
	if (color.y < min) min = color.y;
	if (color.z < min) min = color.z;

	// find maximum
	uint max = color.x;
	if (color.y > max) max = color.y;
	if (color.z > max) max = color.z;

	uint4 hsl;
	hsl.z = (max + min) / 2;
	if (max != min) 
	{
		if (hsl.z < 128) hsl.y = 255 * (max - min) / (max + min);
			else hsl.y = 255 * (max - min) / (510 - max - min);

		float h;
		if (color.x == max) h = (float)(color.y - color.z) / (float)(max - min); else
			if (color.y == max) h = 2.0f + (float)(color.z - color.x) / (float)(max - min); else
				if (color.z == max) h = 4.0f + (float)(color.x - color.y) / (float)(max - min);

		hsl.x = 42.666667f * h;
		if ((int)hsl.x < 0) hsl.x += 255;
	}

	hsl.w = color.w;

	write_imageui(dstImage, coords, hsl);
}
