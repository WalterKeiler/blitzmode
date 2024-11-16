shader_type spatial;

uniform vec3 lineColor : source_color;
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

void fragment()
{
	float d = sdBox(worldPos,vec2(1,10));

	vec3 col = vec3(1,1,1);
	//col *= 1.0 - exp(-6.0*abs(d));
	//col *= 0.8 + 0.2*cos(150.0*d);
	//col = mix( col, vec3(1.0), 1.0-smoothstep(0.0,0.01,abs(d)) );
	col *= step(d - lineWidth,1);
	col *= step(1,d + lineWidth);

	ALBEDO = col * lineColor;
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
