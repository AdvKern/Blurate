__kernel void ImageByteAToByteRgbA(
					  read_only image2d_t srcImage,
					  write_only image2d_t dstImage)
{
	sampler_t smp = CLK_ADDRESS_NONE;
                                                               
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	uint4 sColor = read_imageui(srcImage, smp, coords);
	uint4 dColor = sColor.x;
	dColor.x = 255;
	write_imageui(dstImage, coords, dColor);
}