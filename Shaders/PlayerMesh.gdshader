shader_type spatial;
#include "res://Shaders/NumberAndSymbolFunctions.gdshaderinc"
#include "res://Shaders/NoiseFunctions.gdshaderinc"

uniform vec3 MainColor : source_color;
uniform vec3 SecondaryColor : source_color;
uniform vec3 DetailColor : source_color;
uniform int PlayerNumber;
uniform float numberSpacing = 1.4;
uniform vec2 TestPoint1;
uniform vec2 TestPoint2;

varying vec2 objectPos;

void vertex() 
{
	objectPos = VERTEX.xz;
}

float DrawDigits(vec2 uv, vec2 frontPos, vec2 backPos, float spacing, float digitSize, float scale)
{
	int pn = PlayerNumber;
	
	float[5] b;
	float[5] f;
	
	int[5] digits;
	int digitNum = 0;
	while (float(pn) > 0.1)
	{
	    int digit = pn % 10;
	    pn /= 10;
	    digits[digitNum] = digit;
		digitNum++;
	}

	if(digitNum == 1) 
	{
		digits[0] = digits[0];
		digits[1] = 0;
		digitNum++;
	}

	//digit width * digitNum
	float width = float(digitNum) * ((spacing + (1.15 * scale)) * scale) - spacing;
	// added half of a digit to account for the digit being cetered in the middle
	float space = 0.575 - width / 2.0;
	
	
	for(int i = 0; i <= digitNum; i++)
	{
		switch(digits[i])
		{
			case 0:
				//min one side of the field with the other
				f[i] = Zero(uv, vec2(frontPos.x + space,-frontPos.y), false, false, digitSize, scale);
				break;
			case 1:
				f[i] = One(uv, vec2(frontPos.x + space,-frontPos.y), false, false, digitSize, scale);
				break;
			case 2:
				f[i] = Two(uv, vec2(frontPos.x + space,-frontPos.y), false, false, digitSize, scale);
				break;
			case 3:
				f[i] = Three(uv, vec2(frontPos.x + space,-frontPos.y), false, false, digitSize, scale);
				break;
			case 4:
				f[i] = Four(uv, vec2(frontPos.x + space,-frontPos.y), false, false, digitSize, scale);
				break;
			case 5:
				f[i] = Five(uv, vec2(frontPos.x + space,-frontPos.y), false, false, digitSize, scale);
				break;
			case 6:
				f[i] = Six(uv, vec2(frontPos.x + space,-frontPos.y), false, false, digitSize, scale);
				break;
			case 7:
				f[i] = Seven(uv, vec2(frontPos.x + space,-frontPos.y), false, false, digitSize, scale);
				break;
			case 8:
				f[i] = Eight(uv, vec2(frontPos.x + space,-frontPos.y), false, false, digitSize, scale);
				break;
			case 9:
				f[i] = Nine(uv, vec2(frontPos.x + space,-frontPos.y), false, false, digitSize, scale);
				break;
		}
		space += (1.15 * scale) + spacing;
	}

	for(int i = digitNum - 1; i >= 0; i--)
	{
		switch(digits[i])
		{
			case 0:
				//min one side of the field with the other
				b[i] = Zero(uv, vec2(-backPos.x + space,-backPos.y), true, false, digitSize, scale);
				break;
			case 1:
				b[i] = One(uv, vec2(-backPos.x + space,-backPos.y), true, false, digitSize, scale);
				break;
			case 2:
				b[i] = Two(uv, vec2(-backPos.x + space,-backPos.y), true, false, digitSize, scale);
				break;
			case 3:
				b[i] = Three(uv, vec2(-backPos.x + space,-backPos.y), true, false, digitSize, scale);
				break;
			case 4:
				b[i] = Four(uv, vec2(-backPos.x + space,-backPos.y), true, false, digitSize, scale);
				break;
			case 5:
				b[i] = Five(uv, vec2(-backPos.x + space,-backPos.y), true, false, digitSize, scale);
				break;
			case 6:
				b[i] = Six(uv, vec2(-backPos.x + space,-backPos.y), true, false, digitSize, scale);
				break;
			case 7:
				b[i] = Seven(uv, vec2(-backPos.x + space,-backPos.y), true, false, digitSize, scale);
				break;
			case 8:
				b[i] = Eight(uv, vec2(-backPos.x + space,-backPos.y), true, false, digitSize, scale);
				break;
			case 9:
				b[i] = Nine(uv, vec2(-backPos.x + space,-backPos.y), true, false, digitSize, scale);
				break;
		}
		space += (1.15 * scale) + spacing;
	}
	
	float d = 1000000000000.0;
	for(int i = 0; i < digitNum; i++)
	{
		float s = min(b[i], f[i]);
		d = min(d,s);
	}
	
	return d;
}

void fragment() 
{
	float d;
	float t;

	//Lines Torso
	d = sdSegment(UV, vec2(0,.975), vec2(0.4,.975)) -0.01;
	d = min(d,sdSegment(UV, vec2(0,.940), vec2(0.4,.940)) -0.01);
	d = min(d,sdSegment(UV, vec2(0,.73), vec2(0.4,.73)) -0.01);
	d = min(d,sdSegment(UV, vec2(0,.695), vec2(0.4,.695)) -0.01);
	d = min(d,sdSegment(UV2, vec2(0.525,1.01), vec2(0.525,0)) -0.01);
	
	//Lines Legs
	d = min(d,sdSegment(UV2, vec2(0.525,1.01), vec2(0.525,0)) -0.025);
	//t = min(t,sdSegment(UV2, vec2(0.525,1.01), vec2(0.525,0)) -0.025);
	
	//Lines Arms
	d = min(d,sdSegment(UV2, vec2(0.15,1.01), vec2(0.15,0)) -0.05);
	//t = min(t,sdSegment(UV2, vec2(0.15,1.01), vec2(0.15,0)) -0.025);
	
	//Numbers
	d = min(d, DrawDigits(UV,vec2(-1.265,0.83),vec2(1.615,0.515), numberSpacing, .01, .05));
	
	vec3 col = (d<0.0) ? SecondaryColor : MainColor;
	
	ALBEDO = col;
}
