shader_type spatial;
#include "res://Shaders/SDFFunctions.gdshaderinc"

//const int arrayMaxLength = 15;

uniform bool useWorldSpace;
uniform vec3 ballColor : source_color;
uniform vec3 lineColor : source_color;


varying vec2 fieldPos;

void vertex()
{
}


void fragment()
{
	float d, dstep;
	vec2 start, end;
	const int lace_count = 7;

	start = vec2(0.5,0.3);
	end = vec2(0.5, .7);
	d = sdSegment(UV, start, end) -0.01;
	
	for (int i = 0; i < lace_count; i++)
	{
		d = min(
			d,
			sdSegment(
				UV,
				start + vec2(-0.025, float(i)/((float(lace_count)-1.0)/(end.y - start.y))),
				start + vec2(0.025, float(i)/((float(lace_count)-1.0)/(end.y - start.y)))
			)-0.01
		);
	}
	
	ALBEDO = (d<0.0) ? lineColor : ballColor;
}
