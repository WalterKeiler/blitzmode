#include "res://Shaders/SDFFunctions.gdshaderinc"

float Zero(vec2 offset, vec2 placementPos, bool inverted)
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
	float x = ArrayToSegment(placementPos,points,9,offset,inverted) - .25;

	float d = x;
	vec3 col = (d<0.0) ? vec3(1,1,1) : vec3(0,0,0);
	return d;
}
float One(vec2 offset, vec2 placementPos, bool inverted)
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

	float x = ArrayToSegment(placementPos,points,5,offset,inverted) - .25;

	return x;
}
float Two(vec2 offset, vec2 placementPos, bool inverted)
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
	float x = ArrayToSegment(placementPos,points,9,offset,inverted) - .25;
	return x;
}
float Three(vec2 offset, vec2 placementPos, bool inverted)
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

	float x = ArrayToSegment(placementPos,points,15,offset,inverted) - .25;
	return x;
}
float Four(vec2 offset, vec2 placementPos, bool inverted)
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
	float x = ArrayToSegment(placementPos,points,7,offset,inverted) - .25;
	return x;
}
float Five(vec2 offset, vec2 placementPos, bool inverted)
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
	float x = ArrayToSegment(placementPos,points,10,offset,inverted) - .25;

	return x;
}
float Six(vec2 offset, vec2 placementPos, bool inverted)
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

	float x = ArrayToSegment(placementPos,points,12,offset,inverted) - .25;


	return x;
}
float Seven(vec2 offset, vec2 placementPos, bool inverted)
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
	float x = ArrayToSegment(placementPos,points,4,offset,inverted) - .25;

	return x;
}
float Eight(vec2 offset, vec2 placementPos, bool inverted)
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
	float x = ArrayToSegment(placementPos,points,15,offset,inverted) - .25;

	float invert = inverted ? -1.0 : 1.0;

	vec2 pos1 = vec2(-.9,-.4) * invert;
	vec2 pos2 = vec2(-.5,0) * invert;

	float p = sdSegment(placementPos + offset,pos1,pos2)- .25;
	x = min(x, p);

	return x;
}
float Nine(vec2 offset, vec2 placementPos, bool inverted)
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
	float x = ArrayToSegment(placementPos,points,12,offset,inverted) - .25;

	return x;
}

float Arrow(vec2 offset, vec2 placementPos, bool inverted)
{
	vec2[arrayMaxLength] points = {
		vec2(0,-.5),
		vec2(.24,0),
		vec2(0,.5),
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
	float x = ArrayToSegment(placementPos,points,3,offset,inverted) - .1;

	return x;
}

float DrawNumbers(float xpos, float ypos, vec2 placementPos, float spacing, int yardsLength)
{
	float[5] b;

	float[5] t;

	spacing /= 1.0;

	int yard = (yardsLength / 2) - int(xpos);
	
	if(yard == 0) return 0.0;
	//yard /= 10;
	int[5] digits;
	int digitNum = 0;
	while (float(yard) > 0.1)
	{
	    int digit = yard % 10;
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
				b[i] = min(Zero(vec2(xpos + space,-ypos),placementPos,false),Zero(vec2(-xpos + space,-ypos),placementPos,false));
				t[i] = min(Zero(vec2(xpos + -space,ypos),placementPos,true),Zero(vec2(-xpos + -space,ypos),placementPos,true));
				break;
			case 1:
				b[i] = min(One(vec2(xpos + space,-ypos),placementPos,false),One(vec2(-xpos + space,-ypos),placementPos,false));
				t[i] = min(One(vec2(xpos + -space,ypos),placementPos,true),One(vec2(-xpos + -space,ypos),placementPos,true));
				break;
			case 2:
				b[i] = min(Two(vec2(xpos + space,-ypos),placementPos,false),Two(vec2(-xpos + space,-ypos),placementPos,false));
				t[i] = min(Two(vec2(xpos + -space,ypos),placementPos,true),Two(vec2(-xpos + -space,ypos),placementPos,true));
				break;
			case 3:
				b[i] = min(Three(vec2(xpos + space,-ypos),placementPos,false),Three(vec2(-xpos + space,-ypos),placementPos,false));
				t[i] = min(Three(vec2(xpos + -space,ypos),placementPos,true),Three(vec2(-xpos + -space,ypos),placementPos,true));
				break;
			case 4:
				b[i] = min(Four(vec2(xpos + space,-ypos),placementPos,false),Four(vec2(-xpos + space,-ypos),placementPos,false));
				t[i] = min(Four(vec2(xpos + -space,ypos),placementPos,true),Four(vec2(-xpos + -space,ypos),placementPos,true));
				break;
			case 5:
				b[i] = min(Five(vec2(xpos + space,-ypos),placementPos,false),Five(vec2(-xpos + space,-ypos),placementPos,false));
				t[i] = min(Five(vec2(xpos + -space,ypos),placementPos,true),Five(vec2(-xpos + -space,ypos),placementPos,true));
				break;
			case 6:
				b[i] = min(Six(vec2(xpos + space,-ypos),placementPos,false),Six(vec2(-xpos + space,-ypos),placementPos,false));
				t[i] = min(Six(vec2(xpos + -space,ypos),placementPos,true),Six(vec2(-xpos + -space,ypos),placementPos,true));
				break;
			case 7:
				b[i] = min(Seven(vec2(xpos + space,-ypos),placementPos,false),Seven(vec2(-xpos + space,-ypos),placementPos,false));
				t[i] = min(Seven(vec2(xpos + -space,ypos),placementPos,true),Seven(vec2(-xpos + -space,ypos),placementPos,true));
				break;
			case 8:
				b[i] = min(Eight(vec2(xpos + space,-ypos),placementPos,false),Eight(vec2(-xpos + space,-ypos),placementPos,false));
				t[i] = min(Eight(vec2(xpos + -space,ypos),placementPos,true),Eight(vec2(-xpos + -space,ypos),placementPos,true));
				break;
			case 9:
				b[i] = min(Nine(vec2(xpos + space,-ypos),placementPos,false),Nine(vec2(-xpos + space,-ypos),placementPos,false));
				t[i] = min(Nine(vec2(xpos + -space,ypos),placementPos,true),Nine(vec2(-xpos + -space,ypos),placementPos,true));
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

	float arrB = min(Arrow(vec2(xpos + (space / 2.0) - .15,-ypos) ,placementPos, true), Arrow(vec2(-xpos - (space / 2.0) - .15, -ypos),placementPos, false));
	float arrT = min(Arrow(vec2(xpos + (space / 2.0) + .15,ypos) ,placementPos, true), Arrow(vec2(-xpos - (space / 2.0) + .15,ypos) ,placementPos, false));
	float arr = min(arrT, arrB);
	if(xpos < 1.0) arr = 1000000.0;
	//float sa = sdBox(vec2(xpos + space,ypos), vec2(05,05));
	//arr = Arrow(vec2(xpos + -space - .25,ypos) , true);

	d = min(d,arr);

	return d;
}