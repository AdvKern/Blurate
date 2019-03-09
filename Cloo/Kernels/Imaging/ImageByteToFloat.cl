__kernel void ImageByteToFloat(
					  read_only image2d_t srcImage,
					  write_only image2d_t dstImage)
{                             
	sampler_t smp = CLK_ADDRESS_NONE;
                                                               
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	uint4 color = read_imageui(srcImage, smp, coords);
	float4 val = convert_float4(color);

	write_imagef(dstImage, coords, val);
}