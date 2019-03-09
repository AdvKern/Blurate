__kernel void CoreFloatSetValue(
					  __global float* dstBuffer,
					  const float value
					  )
{
	uint index = get_global_id(0);
	dstBuffer[index] = value;
}