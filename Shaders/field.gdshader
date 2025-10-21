shader_type spatial;
#include "res://Shaders/SDFFunctions.gdshaderinc"

//const int arrayMaxLength = 15;

uniform bool useWorldSpace;
uniform vec3 fieldColor : source_color;
uniform vec3 lineColor : source_color;
uniform vec3 lineOfScrimmageColor : source_color;
uniform vec3 firstDownColor : source_color;
uniform float lineOfScrimmage = 20;
uniform float firstDownLine = 20;
uniform int numberIndent = 20;
uniform float numberSpacing = 1.4;
uniform int yardsLength = 100;
uniform int yardsWidth = 54;
uniform float lineWidth = .1;

varying vec2 fieldPos;

void vertex()
{
    float a,b,c,d,e,f,g,h;
    a = 0.0;
    b = 0.0;
    c = 1.0;
    d = 1.0;
    e = float(-yardsLength) *0.5;
    f = float(-yardsWidth) *0.5;
    g = float(yardsLength) *0.5;
    h = float(yardsWidth) *0.5;

	fieldPos = vec2(
        e + (UV.x - a) * (g - e) / (c - a),
        f + (UV.y - b) * (h - f) / (d - b)
    );

	vec2 worldPos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xz;
	fieldPos = useWorldSpace? worldPos : fieldPos;
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
	float x = ArrayToSegment(fieldPos,points,9,offset,inverted) - .25;

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

	float x = ArrayToSegment(fieldPos,points,5,offset,inverted) - .25;

	return x;
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
	float x = ArrayToSegment(fieldPos,points,9,offset,inverted) - .25;
	return x;
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

	float x = ArrayToSegment(fieldPos,points,15,offset,inverted) - .25;
	return x;
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
	float x = ArrayToSegment(fieldPos,points,7,offset,inverted) - .25;
	return x;
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
	float x = ArrayToSegment(fieldPos,points,10,offset,inverted) - .25;

	return x;
}
float Six(vec2 offset, bool inverted)
{
	vec2[arrayMaxLength] points = {
		vec2(0.9,-1),
		vec2(.5,-1.4),
		vec2(-.5,-1.4),
		vec2(-0.9,-1),
		vec2(-0.9,1),
		vec2(-0.5,1.4),
		vec2(0.5,1.4),
		vec2(0.9,1),
		vec2(.9,0.4),
		vec2(0.5,0),
		vec2(-0.5,0),
		vec2(-0.9,0.4),
		vec2(13,0),
		vec2(14,0),
		vec2(15,0)
	};

	float x = ArrayToSegment(fieldPos,points,12,offset,inverted) - .25;


	return x;
}
float Seven(vec2 offset, bool inverted)
{
	vec2[arrayMaxLength] points = {
		vec2(-.9,-1),
		vec2(-.9,-1.4),
		vec2(.9,-1.4),
		vec2(-0.5,1.4),
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
	float x = ArrayToSegment(fieldPos,points,4,offset,inverted) - .25;

	return x;
}
float Eight(vec2 offset, bool inverted)
{
	vec2[arrayMaxLength] points = {
		vec2(.5,0),
		vec2(-.5,0),
		vec2(-.9,.4),
		vec2(-.9,1),
		vec2(-.5,1.4),
		vec2(.5,1.4),
		vec2(.9,1),
		vec2(.9,.4),
		vec2(.5,0),
		vec2(.9,-.4),
		vec2(.9,-1),
		vec2(.5,-1.4),
		vec2(-.5,-1.4),
		vec2(-.9,-1),
		vec2(-.9,-.4)
	};
	float x = ArrayToSegment(fieldPos,points,15,offset,inverted) - .25;

	float invert = inverted ? -1.0 : 1.0;

	vec2 pos1 = vec2(-.9,-.4) * invert;
	vec2 pos2 = vec2(-.5,0) * invert;

	float p = sdSegment(fieldPos + offset,pos1,pos2)- .25;
	x = min(x, p);

	return x;
}
float Nine(vec2 offset, bool inverted)
{
	vec2[arrayMaxLength] points = {
		vec2(-.9,1),
		vec2(-.5,1.4),
		vec2(.5,1.4),
		vec2(.9,1),
		vec2(.9,-1),
		vec2(.5,-1.4),
		vec2(-.5,-1.4),
		vec2(-.9,-1),
		vec2(-.9,-.4),
		vec2(-.5,0),
		vec2(.5,0),
		vec2(.9,-.4),
		vec2(13,0),
		vec2(14,0),
		vec2(15,0)
	};
	float x = ArrayToSegment(fieldPos,points,12,offset,inverted) - .25;

	return x;
}

float DrawNumbers(float xpos, float ypos, float spacing)
{
	float[5] b;

	float[5] t;

	int yard = (yardsLength / 2) - int(xpos);
	//yard /= 10;
	int[5] digits;
	int digitNum = 0;
	while (float(yard) > 0.1)
	{
	    int digit = yard%10;
	    yard /= 10;
	    digits[digitNum] = digit;
		digitNum++;
	}

	//digit width * digitNum
	float width = float(digitNum) * (spacing + 1.15) - spacing;
	// added half of a digit to account for the digit being cetered in the middle
	float space = 0.575 - width / 2.0;
	for(int i = 0; i <= digitNum; i++)
	{
		switch(digits[i])
		{
			case 0:
				//min one side of the field with the other
				b[i] = min(Zero(vec2(xpos + space,-ypos),false),Zero(vec2(-xpos + space,-ypos),false));
				t[i] = min(Zero(vec2(xpos + -space,ypos),true),Zero(vec2(-xpos + -space,ypos),true));
				break;
			case 1:
				b[i] = min(One(vec2(xpos + space,-ypos),false),One(vec2(-xpos + space,-ypos),false));
				t[i] = min(One(vec2(xpos + -space,ypos),true),One(vec2(-xpos + -space,ypos),true));
				break;
			case 2:
				b[i] = min(Two(vec2(xpos + space,-ypos),false),Two(vec2(-xpos + space,-ypos),false));
				t[i] = min(Two(vec2(xpos + -space,ypos),true),Two(vec2(-xpos + -space,ypos),true));
				break;
			case 3:
				b[i] = min(Three(vec2(xpos + space,-ypos),false),Three(vec2(-xpos + space,-ypos),false));
				t[i] = min(Three(vec2(xpos + -space,ypos),true),Three(vec2(-xpos + -space,ypos),true));
				break;
			case 4:
				b[i] = min(Four(vec2(xpos + space,-ypos),false),Four(vec2(-xpos + space,-ypos),false));
				t[i] = min(Four(vec2(xpos + -space,ypos),true),Four(vec2(-xpos + -space,ypos),true));
				break;
			case 5:
				b[i] = min(Five(vec2(xpos + space,-ypos),false),Five(vec2(-xpos + space,-ypos),false));
				t[i] = min(Five(vec2(xpos + -space,ypos),true),Five(vec2(-xpos + -space,ypos),true));
				break;
			case 6:
				b[i] = min(Six(vec2(xpos + space,-ypos),false),Six(vec2(-xpos + space,-ypos),false));
				t[i] = min(Six(vec2(xpos + -space,ypos),true),Six(vec2(-xpos + -space,ypos),true));
				break;
			case 7:
				b[i] = min(Seven(vec2(xpos + space,-ypos),false),Seven(vec2(-xpos + space,-ypos),false));
				t[i] = min(Seven(vec2(xpos + -space,ypos),true),Seven(vec2(-xpos + -space,ypos),true));
				break;
			case 8:
				b[i] = min(Eight(vec2(xpos + space,-ypos),false),Eight(vec2(-xpos + space,-ypos),false));
				t[i] = min(Eight(vec2(xpos + -space,ypos),true),Eight(vec2(-xpos + -space,ypos),true));
				break;
			case 9:
				b[i] = min(Nine(vec2(xpos + space,-ypos),false),Nine(vec2(-xpos + space,-ypos),false));
				t[i] = min(Nine(vec2(xpos + -space,ypos),true),Nine(vec2(-xpos + -space,ypos),true));
				break;
		}
		space += 1.15 + spacing;
	}

	float d = 1000000000000.0;
	for(int i = 0; i < digitNum; i++)
	{
		float s = min(b[i],t[i]);
		d = min(d,s);
	}

	return d;
}

void fragment()
{
	float halfWidth = float(yardsWidth) * .5;
	float halfLength = float(yardsLength) * .5;
	int closestLargeYard;
	int closestSmallYard;

	float fullLines = CreateLines(fieldPos,yardsLength,yardsWidth,5, closestLargeYard);
	float partialLines = CreateLines(fieldPos,yardsLength,yardsWidth,1, closestSmallYard);

	float edgeDist = sdBox(fieldPos,vec2(halfLength,halfWidth));
	float partialLineBox = opXor(sdBox(fieldPos,vec2(halfLength,halfWidth - halfWidth / 10.0)),
	opXor(sdBox(fieldPos,vec2(halfLength,halfWidth / 5.0)),sdBox(fieldPos,vec2(halfLength,halfWidth / 10.0))));

	float los = sdSegment(fieldPos, vec2(lineOfScrimmage, float(yardsWidth)), vec2(lineOfScrimmage, -float(yardsWidth)))-0.10;
	float fd = sdSegment(fieldPos, vec2(firstDownLine, float(yardsWidth)), vec2(firstDownLine, -float(yardsWidth)))-0.10;

	vec3 losVec = (los<0.0) ? vec3(1,1,1) : vec3(0,0,0);
	losVec -= step(1,edgeDist);

	vec3 fdVec = (fd<0.0) ? vec3(1,1,1) : vec3(0,0,0);
	fdVec -= step(1,edgeDist);

	vec3 col = (fullLines>0.0) ? vec3(1,1,1) : vec3(0,0,0);

	col *= step(fullLines,lineWidth);
	col += step(1,partialLineBox);
	col *= step(partialLines,lineWidth);
	col += step(1,edgeDist);

	vec3 onLine = (abs(fullLines) < .5) ? vec3(1,1,1) : vec3(0,0,0);
	float onTenYard = ((closestLargeYard) % 10 == 0) ? 1.0 : 0.0;

	vec3 onEdge = vec3(step(0, partialLineBox + float(numberIndent))).xxx;
	onEdge *= vec3(step((partialLineBox + float(numberIndent)) - 1.0, 0)).xxx;

	float d = DrawNumbers(float(closestLargeYard) * onTenYard,halfWidth - float(numberIndent),numberSpacing);

	col += (d<0.0) ? vec3(1,1,1) : vec3(0,0,0);

	losVec = clamp(losVec,0,1);
	fdVec = clamp(fdVec,0,1);
	col = clamp(col,0,1);

	col -= losVec;
	col -= fdVec;

	col = clamp(col,0,1);

	vec3 lineCol = col * lineColor;
	vec3 fieldCol = -col * fieldColor;
	vec3 losCol = losVec * lineOfScrimmageColor;
	vec3 fdCol = fdVec * firstDownColor;

	vec3 finalCol = lineCol + fieldColor + losCol + fdCol;

	finalCol = clamp(finalCol,0,1);

	ALBEDO = finalCol;
}
