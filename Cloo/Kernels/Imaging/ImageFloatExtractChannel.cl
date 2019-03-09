__kernel void ImageFloatExtractChannel(
					  read_only image2d_t srcImage,
					  write_only image2d_t dstImage,
					  const char offset)
{
	sampler_t smp = CLK_ADDRESS_NONE;

	uint x = get_global_id(0);
	uint y = get_global_id(1);
	int2 coords = (int2)(x, y);

	union { float4 f4; float a4[4]; } source;
	source.f4 = read_imagef(srcImage, smp, coords);
	float4 dest;
	dest.x = source.a4[offset];
	write_imagef(dstImage, coords, dest);
}