__kernel void ImageByteSetValueRgbA(
					  write_only image2d_t dstImage,
					  const uint r,
					  const uint g,
					  const uint b,
					  const uint a
					  )
{                                
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	uint4 value;
	value.x = r;
	value.y = g;
	value.z = b;
	value.w = a;
	write_imageui(dstImage, coords, value);
}