// clears all result rectangles
__kernel void ImageViolaJonesClear(
					  __global Rectangle* resRectangles        // result rectangles
					  )
{                                
	int x = get_global_id(0);

	__global Rectangle* rect = &resRectangles[x];
	rect->x = 0;
	rect->y = 0;
	rect->width = 0;
	rect->height = 0;
}
