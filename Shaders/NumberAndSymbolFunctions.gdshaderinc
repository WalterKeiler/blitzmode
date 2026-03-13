#include "res://Shaders/SDFFunctions.gdshaderinc"

float Zero(vec2 inputPos, vec2 offset, bool invertedX, bool invertedY, float digitSize, float scale)
{
	float invX = invertedX ? -1.0 : 1.0;
	float invY = invertedY ? -1.0 : 1.0;
	
	vec2 s = vec2(invX * scale, invY * scale);
	
	vec2[arrayMaxLength] points = {
		vec2(-.9,1) * s,
		vec2(-.9,-1) * s,
		vec2(-.5,-1.4) * s,
		vec2(.5,-1.4) * s,
		vec2(.9,-1) * s,
		vec2(.9,1) * s,
		vec2(.5,1.4) * s,
		vec2(-.5,1.4) * s,
		vec2(-.9,1) * s,
		vec2(10,0),
		vec2(11,0),
		vec2(12,0),
		vec2(13,0),
		vec2(14,0),
		vec2(15,0)
	};
	float x = ArrayToSegment(inputPos,points,9,offset) - digitSize;

	float d = x;
	vec3 col = (d<0.0) ? vec3(1,1,1) : vec3(0,0,0);
	return d;
}
float One(vec2 inputPos, vec2 offset, bool invertedX, bool invertedY, float digitSize, float scale)
{
	float invX = invertedX ? -1.0 : 1.0;
	float invY = invertedY ? -1.0 : 1.0;
	
	vec2 s = vec2(invX * scale, invY * scale);
	
	vec2[arrayMaxLength] points = {
		vec2(-.6,-1) * s,
		vec2(0,-1.4) * s,
		vec2(0,1.4) * s,
		vec2(-.9,1.4) * s,
		vec2(.9,1.4) * s,
		vec2(6,0),
		vec2(7,0),
		vec2(8,0),
		vec2(9,0),
		vec2(10,0),
		vec2(11,0),
		vec2(12,0),
		vec2(13,0),
		vec2(14,0),
		vec2(15,0)
	};

	float x = ArrayToSegment(inputPos,points,5,offset) - digitSize;

	return x;
}
float Two(vec2 inputPos, vec2 offset, bool invertedX, bool invertedY, float digitSize, float scale)
{
	float invX = invertedX ? -1.0 : 1.0;
	float invY = invertedY ? -1.0 : 1.0;
	
	vec2 s = vec2(invX * scale, invY * scale);
	
	vec2[arrayMaxLength] points = {
		vec2(-.9,-.6) * s,
		vec2(-.9,-1) * s,
		vec2(-.5,-1.4) * s,
		vec2(.5,-1.4) * s,
		vec2(.9,-1) * s,
		vec2(.9,-.5) * s,
		vec2(-.9,.8) * s,
		vec2(-.9,1.4) * s,
		vec2(.9,1.4) * s,
		vec2(.9,1.4),
		vec2(.9,1.4),
		vec2(.9,1.4),
		vec2(.9,1.4),
		vec2(.9,1.4),
		vec2(.9,1.4)
	};
	float x = ArrayToSegment(inputPos,points,9,offset) - digitSize;
	return x;
}
float Three(vec2 inputPos, vec2 offset, bool invertedX, bool invertedY, float digitSize, float scale)
{
	float invX = invertedX ? -1.0 : 1.0;
	float invY = invertedY ? -1.0 : 1.0;
	
	vec2 s = vec2(invX * scale, invY * scale);
	
	vec2[arrayMaxLength] points = {
		vec2(-.9,-.8) * s,
		vec2(-.9,-1) * s,
		vec2(-.5,-1.4) * s,
		vec2(.5,-1.4) * s,
		vec2(.9,-1) * s,
		vec2(.9,-.4) * s,
		vec2(.5,0) * s,
		vec2(.2,0) * s,
		vec2(.5,0) * s,
		vec2(.9,.4) * s,
		vec2(.9,1) * s,
		vec2(.5,1.4) * s,
		vec2(-.5,1.4) * s,
		vec2(-.9,1) * s,
		vec2(-.9,.8) * s
	};

	float x = ArrayToSegment(inputPos,points,15,offset) - digitSize;
	return x;
}
float Four(vec2 inputPos, vec2 offset, bool invertedX, bool invertedY, float digitSize, float scale)
{
	float invX = invertedX ? -1.0 : 1.0;
	float invY = invertedY ? -1.0 : 1.0;
	
	vec2 s = vec2(invX * scale, invY * scale);
	
	vec2[arrayMaxLength] points = {
		vec2(.9,.1) * s,
		vec2(-.9,.1) * s,
		vec2(-.9,-.5) * s,
		vec2(.4,-1.4) * s,
		vec2(.4,1.4) * s,
		vec2(-.15,1.4) * s,
		vec2(.9,1.4) * s,
		vec2(8,0),
		vec2(9,0),
		vec2(10,0),
		vec2(11,0),
		vec2(12,0),
		vec2(13,0),
		vec2(14,0),
		vec2(15,0)
	};
	float x = ArrayToSegment(inputPos,points,7,offset) - digitSize;
	return x;
}
float Five(vec2 inputPos, vec2 offset, bool invertedX, bool invertedY, float digitSize, float scale)
{
	float invX = invertedX ? -1.0 : 1.0;
	float invY = invertedY ? -1.0 : 1.0;
	
	vec2 s = vec2(invX * scale, invY * scale);
	
	vec2[arrayMaxLength] points = {
		vec2(.9,-1.4) * s,
		vec2(-.9,-1.4) * s,
		vec2(-.9,-.8) * s,
		vec2(-.5,-.4) * s,
		vec2(.5,-.4) * s,
		vec2(.9,0) * s,
		vec2(.9,1) * s,
		vec2(.5,1.4) * s,
		vec2(-.5,1.4) * s,
		vec2(-.9,1) * s,
		vec2(11,0),
		vec2(12,0),
		vec2(13,0),
		vec2(14,0),
		vec2(15,0)
	};
	float x = ArrayToSegment(inputPos,points,10,offset) - digitSize;

	return x;
}
float Six(vec2 inputPos, vec2 offset, bool invertedX, bool invertedY, float digitSize, float scale)
{
	float invX = invertedX ? -1.0 : 1.0;
	float invY = invertedY ? -1.0 : 1.0;
	
	vec2 s = vec2(invX * scale, invY * scale);
	
	vec2[arrayMaxLength] points = {
		vec2(0.9,-1) * s,
		vec2(.5,-1.4) * s,
		vec2(-.5,-1.4) * s,
		vec2(-0.9,-1) * s,
		vec2(-0.9,1) * s,
		vec2(-0.5,1.4) * s,
		vec2(0.5,1.4) * s,
		vec2(0.9,1) * s,
		vec2(.9,0.4) * s,
		vec2(0.5,0) * s,
		vec2(-0.5,0) * s,
		vec2(-0.9,0.4) * s,
		vec2(13,0),
		vec2(14,0),
		vec2(15,0)
	};

	float x = ArrayToSegment(inputPos,points,12,offset) - digitSize;


	return x;
}
float Seven(vec2 inputPos, vec2 offset, bool invertedX, bool invertedY, float digitSize, float scale)
{
	float invX = invertedX ? -1.0 : 1.0;
	float invY = invertedY ? -1.0 : 1.0;
	
	vec2 s = vec2(invX * scale, invY * scale);
	
	vec2[arrayMaxLength] points = {
		vec2(-.9,-1) * s,
		vec2(-.9,-1.4) * s,
		vec2(.9,-1.4) * s,
		vec2(-0.5,1.4) * s,
		vec2(5,0),
		vec2(6,0),
		vec2(7,0),
		vec2(8,0),
		vec2(9,0),
		vec2(10,0),
		vec2(11,0),
		vec2(12,0),
		vec2(13,0),
		vec2(14,0),
		vec2(15,0)
	};
	float x = ArrayToSegment(inputPos,points,4,offset) - digitSize;

	return x;
}
float Eight(vec2 inputPos, vec2 offset, bool invertedX, bool invertedY, float digitSize, float scale)
{
	float invX = invertedX ? -1.0 : 1.0;
	float invY = invertedY ? -1.0 : 1.0;
	
	vec2 s = vec2(invX * scale, invY * scale);
	
	vec2[arrayMaxLength] points = {
		vec2(.5,0) * s,
		vec2(-.5,0) * s,
		vec2(-.9,.4) * s,
		vec2(-.9,1) * s,
		vec2(-.5,1.4) * s,
		vec2(.5,1.4) * s,
		vec2(.9,1) * s,
		vec2(.9,.4) * s,
		vec2(.5,0) * s,
		vec2(.9,-.4) * s,
		vec2(.9,-1) * s,
		vec2(.5,-1.4) * s,
		vec2(-.5,-1.4) * s,
		vec2(-.9,-1) * s,
		vec2(-.9,-.4) * s
	};
	float x = ArrayToSegment(inputPos,points,15,offset) - digitSize;

	vec2 pos1 = vec2(-.9,-.4) * s;
	vec2 pos2 = vec2(-.5,0) * s;

	float p = sdSegment(inputPos + offset,pos1,pos2)- digitSize;
	x = min(x, p);

	return x;
}
float Nine(vec2 inputPos, vec2 offset, bool invertedX, bool invertedY, float digitSize, float scale)
{
	float invX = invertedX ? -1.0 : 1.0;
	float invY = invertedY ? -1.0 : 1.0;
	
	vec2 s = vec2(invX * scale, invY * scale);
	
	vec2[arrayMaxLength] points = {
		vec2(-.9,1) * s,
		vec2(-.5,1.4) * s,
		vec2(.5,1.4) * s,
		vec2(.9,1) * s,
		vec2(.9,-1) * s,
		vec2(.5,-1.4) * s,
		vec2(-.5,-1.4) * s,
		vec2(-.9,-1) * s,
		vec2(-.9,-.4) * s,
		vec2(-.5,0) * s,
		vec2(.5,0) * s,
		vec2(.9,-.4) * s,
		vec2(13,0),
		vec2(14,0),
		vec2(15,0)
	};
	float x = ArrayToSegment(inputPos,points,12,offset) - digitSize;

	return x;
}

float Arrow(vec2 inputPos, vec2 offset, bool invertedX, bool invertedY, float digitSize, float scale)
{
	float invX = invertedX ? -1.0 : 1.0;
	float invY = invertedY ? -1.0 : 1.0;
	
	vec2 s = vec2(invX * scale, invY * scale);
	
	vec2[arrayMaxLength] points = {
		vec2(0,-.5) * s,
		vec2(.24,0) * s,
		vec2(0,.5) * s,
		vec2(4,0),
		vec2(5,0),
		vec2(6,0),
		vec2(7,0),
		vec2(8,0),
		vec2(9,0),
		vec2(10,0),
		vec2(11,0),
		vec2(12,0),
		vec2(13,0),
		vec2(14,0),
		vec2(15,0)
	};
	float x = ArrayToSegment(inputPos,points,3,offset) - digitSize;

	return x;
}