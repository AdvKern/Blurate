__kernel void ImageFloatSetValue(
					  write_only image2d_t dstImage,
					  const float value
					  )
{                                
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	write_imagef(dstImage, coords, value);
}