shader_type spatial;

uniform vec3 feildColor : source_color;
uniform vec3 lineColor : source_color;
uniform int yardsLenth;
uniform int yardsWidth;
uniform float lineWidth;

varying vec2 worldPos;

void vertex()
{
	worldPos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xz;
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

float CreateLines(int interval)
{
	float d = 1000000000000.0;
	
	for(int i = 0; i <= yardsLenth; i += interval)
	{
		float nextDist = sdBox(worldPos,vec2(float(i),10000.0));
		d = min(d,abs(nextDist));
	}
	return d;
}

void fragment()
{
	float fullLines = CreateLines(5);
	float partialLines = CreateLines(1);
	float edgeDist = sdBox(worldPos,vec2(float(yardsLenth),float(yardsWidth)));
	float partialLineBox = sdBox(worldPos,vec2(float(yardsLenth),float(yardsWidth - 5)));
	
	vec3 col = (fullLines>0.0) ? vec3(1,1,1) : vec3(0,0,0);
	
	col *= step(fullLines,lineWidth);
	col += step(1,partialLineBox);
	col *= step(partialLines,lineWidth);
	col += step(1,edgeDist);
	col = clamp(col,0,1);

	vec3 lineCol = col * lineColor;
	vec3 fieldCol = col * feildColor;

	ALBEDO = lineCol + feildColor;
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
