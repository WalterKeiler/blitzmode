shader_type spatial;

const int arrayMaxLength = 15;

uniform vec3 feildColor : source_color;
uniform vec3 lineColor : source_color;
uniform vec2 testPos[arrayMaxLength];
uniform int arraylength;
uniform float testfloat;
uniform int TestYard = 5;
uniform int numberIndent = 20;
uniform int yardsLenth = 100;
uniform int yardsWidth = 54;
uniform float lineWidth = .1;

varying vec2 worldPos;

void vertex()
{
	worldPos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xz;
}
float smin(float a, float b, float k)
{
    float h = max(k - abs(a - b), 0.0) / k;
    return min(a, b) - h * h * k * (1.0 / 4.0);
}
float sdBox( in vec2 p, in vec2 b )
{
    vec2 d = abs(p)-b;
    return length(max(d,0.0)) + min(max(d.x,d.y),0.0);
}
float sdSegment( in vec2 p, in vec2 a, in vec2 b )
{
    vec2 pa = p-a, ba = b-a;
    float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
    return length( pa - ba*h );
}
float sdOrientedBox( in vec2 p, in vec2 a, in vec2 b, float th )
{
    float l = length(b-a);
    vec2  d = (b-a)/l;
    vec2  q = (p-(a+b)*0.5);
          q = mat2(vec2(d.x,-d.y),vec2(d.y,d.x))*q;
          q = abs(q)-vec2(l,th)*0.5;
    return length(max(q,0.0)) + min(max(q.x,q.y),0.0);
}
float CreateLines(int interval, out int closestYard)
{
	float d = 1000000000000.0;
	int yardIndex = -1;
	for(int i = 0; i <= yardsLenth / 2; i += interval)
	{
		float nextDist = sdBox(worldPos,vec2(float(i),10000.0));
		d = min(d,abs(nextDist));

		if(d == abs(nextDist) && i < yardsLenth / 2) yardIndex = i;
	}

	closestYard = yardIndex;

	return d;
}

float dot2(vec2 input)
{
	return dot(input,input);
}

float ArrayToSegment(vec2[arrayMaxLength] points, int size, vec2 offset, bool inverted)
{
	float d = 1000000000000.0;
	float invert = inverted ? -1.0 : 1.0;
	for(int i = 0; i < size - 1; i++)
	{
		vec2 pos1 = points[i] * invert;
		vec2 pos2 = points[i+1] * invert;

		float p = sdSegment(worldPos + offset,pos1,pos2);
		d = min(d,p);
	}
	return d;
}

float Zero(vec2 offset, bool inverted)
{
	vec2[arrayMaxLength] points = {
		vec2(-.9,1),
		vec2(-.9,-1),
		vec2(-.5,-1.4),
		vec2(.5,-1.4),
		vec2(.9,-1),
		vec2(.9,1),
		vec2(.5,1.4),
		vec2(-.5,1.4),
		vec2(-.9,1),
		vec2(10,0),
		vec2(11,0),
		vec2(12,0),
		vec2(13,0),
		vec2(14,0),
		vec2(15,0)
	};
	float x = ArrayToSegment(points,9,offset,inverted) - .25;

	float d = x;
	vec3 col = (d<0.0) ? vec3(1,1,1) : vec3(0,0,0);
	return d;
}
float One(vec2 offset, bool inverted)
{
	vec2[arrayMaxLength] points = {
		vec2(-.6,-1),
		vec2(0,-1.4),
		vec2(0,1.4),
		vec2(-.9,1.4),
		vec2(.9,1.4),
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

	float x = ArrayToSegment(points,5,offset,inverted) - .25;

	float d = x;

	vec3 col = (d<0.0) ? vec3(1,1,1) : vec3(0,0,0);
	return d;
}
float Two(vec2 offset, bool inverted)
{
	vec2[arrayMaxLength] points = {
		vec2(-.9,-.6),
		vec2(-.9,-1),
		vec2(-.5,-1.4),
		vec2(.5,-1.4),
		vec2(.9,-1),
		vec2(.9,-.5),
		vec2(-.9,.8),
		vec2(-.9,1.4),
		vec2(.9,1.4),
		vec2(.9,1.4),
		vec2(.9,1.4),
		vec2(.9,1.4),
		vec2(.9,1.4),
		vec2(.9,1.4),
		vec2(.9,1.4)
	};
	float x = ArrayToSegment(points,9,offset,inverted) - .25;

	float d = x;
	vec3 col = (d<0.0) ? vec3(1,1,1) : vec3(0,0,0);
	return d;
}
float Three(vec2 offset, bool inverted)
{
	vec2[arrayMaxLength] points = {
		vec2(-.9,-.8),
		vec2(-.9,-1),
		vec2(-.5,-1.4),
		vec2(.5,-1.4),
		vec2(.9,-1),
		vec2(.9,-.4),
		vec2(.5,0),
		vec2(.2,0),
		vec2(.5,0),
		vec2(.9,.4),
		vec2(.9,1),
		vec2(.5,1.4),
		vec2(-.5,1.4),
		vec2(-.9,1),
		vec2(-.9,.8)
	};

	float x = ArrayToSegment(points,15,offset,inverted) - .25;

	float d = x;

	vec3 col = (d<0.0) ? vec3(1,1,1) : vec3(0,0,0);
	return d;
}
float Four(vec2 offset, bool inverted)
{
	vec2[arrayMaxLength] points = {
		vec2(.9,.1),
		vec2(-.9,.1),
		vec2(-.9,-.5),
		vec2(.4,-1.4),
		vec2(.4,1.4),
		vec2(-.15,1.4),
		vec2(.9,1.4),
		vec2(8,0),
		vec2(9,0),
		vec2(10,0),
		vec2(11,0),
		vec2(12,0),
		vec2(13,0),
		vec2(14,0),
		vec2(15,0)
	};
	float x = ArrayToSegment(points,7,offset,inverted) - .25;

	float d = x;
	vec3 col = (d<0.0) ? vec3(1,1,1) : vec3(0,0,0);
	return d;
}
float Five(vec2 offset, bool inverted)
{
	vec2[arrayMaxLength] points = {
		vec2(.9,-1.4),
		vec2(-.9,-1.4),
		vec2(-.9,-.8),
		vec2(-.5,-.4),
		vec2(.5,-.4),
		vec2(.9,0),
		vec2(.9,1),
		vec2(.5,1.4),
		vec2(-.5,1.4),
		vec2(-.9,1),
		vec2(11,0),
		vec2(12,0),
		vec2(13,0),
		vec2(14,0),
		vec2(15,0)
	};
	float x = ArrayToSegment(points,10,offset,inverted) - .25;

	float d = x;
	vec3 col = (d<0.0) ? vec3(1,1,1) : vec3(0,0,0);
	return d;
}
float Six(vec2 offset, bool inverted)
{
	vec2[arrayMaxLength] points = {
		vec2(1,0),
		vec2(2,0),
		vec2(3,0),
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
	float box = sdBox(worldPos, vec2(1,1.5));
	float x = ArrayToSegment(testPos,arraylength,offset,inverted) - .1;

	float d = x;
	d *= box;
	return d;
}
float Seven(vec2 offset, bool inverted)
{
	vec2[arrayMaxLength] points = {
		vec2(1,0),
		vec2(2,0),
		vec2(3,0),
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
	float box = sdBox(worldPos, vec2(1,1.5));
	float x = ArrayToSegment(testPos,arraylength,offset,inverted) - .1;

	float d = x;
	d *= box;
	return d;
}
float Eight(vec2 offset, bool inverted)
{
	vec2[arrayMaxLength] points = {
		vec2(1,0),
		vec2(2,0),
		vec2(3,0),
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
	float box = sdBox(worldPos, vec2(1,1.5));
	float x = ArrayToSegment(testPos,arraylength,offset,inverted) - .1;

	float d = x;
	d *= box;
	return d;
}
float Nine(vec2 offset, bool inverted)
{
	vec2[arrayMaxLength] points = {
		vec2(1,0),
		vec2(2,0),
		vec2(3,0),
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
	float box = sdBox(worldPos, vec2(1,1.5));
	float x = ArrayToSegment(testPos,arraylength,offset,inverted) - .1;

	float d = x;
	d *= box;
	vec3 col = (d<0.0) ? vec3(1,1,1) : vec3(0,0,0);
	return d;
}

float DrawNumbers(float xpos, float ypos, float spacing)
{
	float b = min(Two(vec2(xpos + spacing,-ypos),false),Zero(vec2(xpos + -spacing,-ypos),false));
    float b2 = min(Two(vec2(-xpos + spacing,-ypos),false),Zero(vec2(-xpos + -spacing,-ypos),false));
	float b3 = min(b,b2);

	float t = min(Two(vec2(xpos + -spacing,ypos),true),Zero(vec2(xpos + spacing,ypos),true));
    float t2 = min(Two(vec2(-xpos + -spacing,ypos),true),Zero(vec2(-xpos + spacing,ypos),true));
	float t3 = min(t,t2);

	return min(b3,t3);
}

void fragment()
{
	/*
	float d = min(Three(vec2(1.25,0),false),Zero(vec2(-1.25,0),false));

	vec3 col = (d<0.0) ? vec3(1,1,1) : vec3(0,0,0);

	vec3 lineCol = col * lineColor;
	vec3 fieldCol = -col * feildColor;
	ALBEDO = col;
	*/

	float halfWidth = float(yardsWidth) * .5;
	float halfLength = float(yardsLenth) * .5;
	int closestLargeYard;
	int closestSmallYard;

	float fullLines = CreateLines(5, closestLargeYard);
	float partialLines = CreateLines(1, closestSmallYard);

	float edgeDist = sdBox(worldPos,vec2(halfLength,halfWidth));
	float partialLineBox = sdBox(worldPos,vec2(halfLength,halfWidth - halfWidth / 10.0));

	vec3 col = (fullLines>0.0) ? vec3(1,1,1) : vec3(0,0,0);

	col *= step(fullLines,lineWidth);
	col += step(1,partialLineBox);
	col *= step(partialLines,lineWidth);
	col += step(1,edgeDist);

	vec3 onLine = (abs(fullLines) < .5) ? vec3(1,1,1) : vec3(0,0,0);
	float onTenYard = ((closestLargeYard) % 10 == 0) ? 1.0 : 0.0;

	vec3 onEdge = vec3(step(0, partialLineBox + float(numberIndent))).xxx;
	onEdge *= vec3(step((partialLineBox + float(numberIndent)) - 1.0, 0)).xxx;

	float d = DrawNumbers(float(closestLargeYard) * onTenYard,float(numberIndent),1.4);

	col += (d<0.0) ? vec3(1,1,1) : vec3(0,0,0);
	col = clamp(col,0,1);

	vec3 lineCol = col * lineColor;
	vec3 fieldCol = -col * feildColor;

	ALBEDO = lineCol + feildColor;

}