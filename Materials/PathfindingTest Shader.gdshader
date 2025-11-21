shader_type spatial;
#include "res://Shaders/SDFFunctions.gdshaderinc"

uniform vec3 Target;
uniform vec3 P1;
uniform vec3 P2;
uniform vec3 P3;


varying vec2 fieldPos;

void vertex() 
{
	fieldPos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xz;
}

void fragment() 
{
	float Ball = sdCircle(Target.xz + fieldPos, 2);
	
	float p1 = sdCircle(P1.xz + fieldPos, 1.0);
	float p2 = sdCircle(P2.xz + fieldPos, 1.0);
	float p3 = sdCircle(P3.xz + fieldPos, 1.0);
	
	float p = min(p1,min(p2,p3));
	
	 //p = step(p, 0.0);
	
	//Ball = step(0, Ball);
	
	//Ball *= 10000.0;
	
	float d = p;
	
	if(Ball > 0.0) d = (Ball + .1) * 10.0;
	
	
	vec3 col = vec3(1) * d / 100.0;
	
	ALBEDO = col;
}

