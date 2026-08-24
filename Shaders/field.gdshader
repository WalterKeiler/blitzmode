shader_type spatial;
#include "res://Shaders/SDFFunctions.gdshaderinc"
#include "res://Shaders/NoiseFunctions.gdshaderinc"
#include "res://Shaders/NumberFunctions.gdshaderinc"

//const int arrayMaxLength = 15;

uniform bool useWorldSpace;
uniform vec3 fieldColor : source_color;
uniform vec3 lineColor : source_color;
uniform vec3 lineOfScrimmageColor : source_color;
uniform vec3 firstDownColor : source_color;
uniform float noiseEffect = 1;
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


	vec3 onEdge = vec3(step(0, partialLineBox + float(numberIndent))).xxx;
	onEdge *= vec3(step((partialLineBox + float(numberIndent)) - 1.0, 0)).xxx;

	float yard = round(float(closestSmallYard) / 10.0) * 10.0;
	
	if(int(yard) - closestSmallYard == 4) yard = float(closestLargeYard + 5);

	float d = DrawNumbers(yard, halfWidth - float(numberIndent), fieldPos, numberSpacing, yardsLength);

	col += (d<0.0) ? vec3(1,1,1) : vec3(0,0,0);

	losVec = clamp(losVec,0,1);
	fdVec = clamp(fdVec,0,1);
	col = clamp(col,0,1);

	col -= losVec;
	col -= fdVec;

	col = clamp(col,0,1);

	float grassNoise = noise(round(fieldPos * 10.0)) * noiseEffect;

	grassNoise = smoothstep(2.5,.0,grassNoise);

	vec3 lineCol = col * lineColor;
	vec3 fieldCol = -col * fieldColor;
	vec3 losCol = losVec * lineOfScrimmageColor;
	vec3 fdCol = fdVec * firstDownColor;

	vec3 finalCol = clamp(lineCol * grassNoise, 0, 1) + clamp(fieldColor * grassNoise, 0, 1) + losCol + fdCol;

	finalCol = clamp(finalCol,0,1);

	ALBEDO = finalCol;
	//ALBEDO = vec3(grassNoise);
}
