__kernel void ImageByteSetValueA(
					  write_only image2d_t dstImage,
					  const uint value
					  )
{                                
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	write_imageui(dstImage, coords, value);
}