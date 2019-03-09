__kernel void CoreByteSetValue(
					  __global char* dstBuffer,
					  const char value
					  )
{
	uint index = get_global_id(0);
	dstBuffer[index] = value;
}