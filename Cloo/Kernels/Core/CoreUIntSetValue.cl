__kernel void CoreUIntSetValue(
					  __global uint* dstBuffer,
					  const uint value
					  )
{
	uint index = get_global_id(0);
	dstBuffer[index] = value;
}