shader_type spatial;
#include "res://Shaders/SDFFunctions.gdshaderinc"

const float OutlineSize = .1;

uniform bool isSelected;
uniform vec3 UnselectedCol : source_color;
uniform vec3 SelectedCol : source_color;

uniform float Circle;
uniform float Square;
uniform float Triangle;


void vertex() {
}

void fragment() 
{
	float square = opOnion(sdBox(UV + vec2(-.5,-.5), vec2(.375,.375)),OutlineSize);
	float trangle = opOnion(sdEquilateralTriangle(UV + vec2(-.5,-.425), .4),OutlineSize);
	float circle = opOnion(sdCircle(UV + vec2(-.5,-.5), .4), OutlineSize);
	float d = (square * Square) + (trangle * Triangle) + (circle * Circle);
	d = step(d,0);
	vec3 col = isSelected ? SelectedCol : UnselectedCol;
	ALBEDO = col;
	ALPHA = d;
}