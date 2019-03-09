__kernel void ImageFloatRgbToHsl(
					  read_only image2d_t srcImage,
					  write_only image2d_t dstImage)
{                                     
	sampler_t smp = CLK_ADDRESS_NONE;
	                                                       
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	float4 color = read_imagef(srcImage, smp, coords);
	color /= 255.0f; // normalize

	// find minimum
	float min = color.x;
	if (color.y < min) min = color.y;
	if (color.z < min) min = color.z;

	// find maximum
	float max = color.x;
	if (color.y > max) max = color.y;
	if (color.z > max) max = color.z;

	float4 hsl;
	hsl.z = (max + min) / 2.0f;
	if (max != min) 
	{
		if (hsl.z < 0.5f) hsl.y = (max - min) / (max + min);
			else hsl.y = (max - min) / (2.0f - max - min);

		if (color.x == max) hsl.x = (color.y - color.z) / (max - min); else
			if (color.y == max) hsl.x = 2.0f + (color.z - color.x) / (max - min); else
				if (color.z == max) hsl.x = 4.0f + (color.x - color.y) / (max - min);

		hsl.x /= 6.0f;
		if (hsl.x < 0) hsl.x += 1.0f;
	}

	hsl *= 255.0f;
	hsl.w = color.w;

	write_imagef(dstImage, coords, hsl);
}