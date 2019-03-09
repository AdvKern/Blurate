#pragma OPENCL EXTENSION cl_khr_global_int32_base_atomics : enable

__kernel void ImageByteRgbHistogramN(
						read_only image2d_t srcImage,
						global uint* histogram,		// size must be N*N*N bytes
						const char n,               // number of bins
						const uint startX,
						const uint startY
						)
{
	sampler_t smp = CLK_ADDRESS_NONE;

    uint x = get_global_id(0) + startX;
    uint y = get_global_id(1) + startY;
	int2 coords = (int2)(x, y);

	uint4 color = read_imageui(srcImage, smp, coords);
	color /= (256 / n);
	atom_inc(&histogram[color.x + color.y * n + color.z * n * n]);
}