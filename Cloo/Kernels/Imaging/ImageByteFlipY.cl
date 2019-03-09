__kernel void ImageByteFlipY(
					  read_only image2d_t srcImage,
					  write_only image2d_t dstImage
					  )
{                                
	sampler_t smp = CLK_ADDRESS_NONE;
	                                                            
	uint x = get_global_id(0);
	uint y = get_global_id(1);

	uint4 color = read_imageui(srcImage, smp, (int2)(x, y));
	write_imageui(dstImage, (int2)(x, get_global_size(1) - y - 1), color);
}