__kernel void ImageByteHistogram256Parts(
					  read_only image2d_t srcImage,
					  global uint* histogramParts  // size must be width * 256
					  )
{                         
	sampler_t smp = CLK_ADDRESS_CLAMP;
	                                                                   
	uint x = get_global_id(0);
	uint width = get_image_width(srcImage);
	uint height = get_image_height(srcImage);
	
	// clear buffer
	for (uint y = 0; y < 256; y++) 
		histogramParts[y * width + x] = 0;

	// count histogram part values
	for (uint y = 0; y < height; y++) 
	{
		int2 coords = (int2)(x, y);
		int4 color = read_imagei(srcImage, smp, coords);

		uint value = histogramParts[color.w * width + x];
		histogramParts[color.w * width + x] = value + 1;
	}
}