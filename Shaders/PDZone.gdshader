shader_type spatial;
#include "res://Shaders/SDFFunctions.gdshaderinc"

uniform float OutlineSize = .1;
uniform float Scale = 1;

uniform vec3 Color : source_color;

uniform float Circle;


void vertex() {
}

void fragment() 
{
	float circle = opOnion(sdCircle(UV + vec2(-.5,-.5), .49), OutlineSize / Scale);
	float d = (circle * Circle);
	d = step(d,0);
	vec3 col = Color;
	ALBEDO = col;
	ALPHA = d;
}