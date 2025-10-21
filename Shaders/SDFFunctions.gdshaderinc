const int arrayMaxLength = 15;

float opXor( float a, float b )
{
    return max( min(a,b), -max(a,b) );
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
    float CreateLines(vec2 pos,int totalLength, int totalWidth,int interval, out int closestYard)
    {
        float d = 1000000000000.0;
        int yardIndex = -1;
        for(int i = 0; i <= totalLength / 2; i += interval)
        {
            float nextDist = sdBox(pos,vec2(float(i),float(totalWidth)));
            d = min(d,abs(nextDist));

            if(d == abs(nextDist) && i < totalLength / 2) yardIndex = i;
        }

        closestYard = yardIndex;

        return d;
    }

    float dot2(vec2 input)
    {
        return dot(input,input);
    }

    float ArrayToSegment(vec2 pos, vec2[arrayMaxLength] points, int size, vec2 offset, bool inverted)
    {
        float d = 1000000000000.0;
        float invert = inverted ? -1.0 : 1.0;
        for(int i = 0; i < size - 1; i++)
        {
            vec2 pos1 = points[i] * invert;
            vec2 pos2 = points[i+1] * invert;

            float p = sdSegment(pos + offset,pos1,pos2);
            d = min(d,p);
        }
        return d;
    }