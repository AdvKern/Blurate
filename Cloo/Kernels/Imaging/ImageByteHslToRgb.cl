__kernel void ImageByteHslToRgb(
					  read_only image2d_t srcImage,
					  write_only image2d_t dstImage)
{                                    
	sampler_t smp = CLK_ADDRESS_NONE;
	                                                        
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	float4 hsl = convert_float4(read_imageui(srcImage, smp, coords));
	hsl /= 255.0f; // normalize
	
	float4 color;
	float2 temp;
	if (hsl.y != 0)
	{
		if (hsl.z < 0.5f) temp.y = hsl.z * (1.0f + hsl.y); else temp.y = hsl.z + hsl.y - hsl.z * hsl.y;
		temp.x = 2.0f * hsl.z - temp.y;

		float4 tempC;
		tempC.x = hsl.x + 1.0f / 3.0f;
		if (tempC.x < 0) tempC.x += 1;
		if (tempC.x > 1) tempC.x -= 1;
		tempC.y = hsl.x;
		if (tempC.y < 0) tempC.y += 1;
		if (tempC.y > 1) tempC.y -= 1;
		tempC.z = hsl.x - 1.0f / 3.0f;
		if (tempC.z < 0) tempC.z += 1;
		if (tempC.z > 1) tempC.z -= 1;

		// red
		if (6.0f * tempC.x < 1.0f) color.x = temp.x + (temp.y - temp.x) * 6.0f * tempC.x; else
			if (2.0f * tempC.x < 1.0f) color.x = temp.y; else
				if (3.0f * tempC.x < 2.0f) color.x = temp.x + (temp.y - temp.x) * ((2.0f / 3.0f) - tempC.x) * 6.0f; else
					color.x = temp.x;

		// green
		if (6.0f * tempC.y < 1.0f) color.y = temp.x + (temp.y - temp.x) * 6.0f * tempC.y; else
			if (2.0f * tempC.y < 1.0f) color.y = temp.y; else
				if (3.0f * tempC.y < 2.0f) color.y = temp.x + (temp.y - temp.x) * ((2.0f / 3.0f) - tempC.y) * 6.0f; else
					color.y = temp.x;

		// blue
		if (6.0f * tempC.z < 1.0f) color.z = temp.x + (temp.y - temp.x) * 6.0f * tempC.z; else
			if (2.0f * tempC.z < 1.0f) color.z = temp.y; else
				if (3.0f * tempC.z < 2.0f) color.z = temp.x + (temp.y - temp.x) * ((2.0f / 3.0f) - tempC.z) * 6.0f; else
					color.z = temp.x;

	} else color = (float4)(hsl.z);

	color *= 255.0f;
	color.w = hsl.w;

	write_imageui(dstImage, coords, convert_uint4(color));
}