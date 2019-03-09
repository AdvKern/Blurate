__kernel void ImageByteFlipX(
					  read_only image2d_t srcImage,
					  write_only image2d_t dstImage
					  )
{                                
	sampler_t smp = CLK_ADDRESS_NONE;
	                                                            
	uint x = get_global_id(0);
	uint y = get_global_id(1);

	uint4 color = read_imageui(srcImage, smp, (int2)(x, y));
	write_imageui(dstImage, (int2)(get_global_size(0) - x - 1, y), color);
}