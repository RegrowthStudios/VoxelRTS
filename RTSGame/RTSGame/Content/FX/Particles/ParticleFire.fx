// Camera And World
float4x4 WVP;

sampler2D Noise : register(s0);
sampler2D Color : register(s1);
sampler2D Alpha : register(s2);

float time;
float3 rates;
float3 scales;

float2 offset1;
float2 offset2;
float2 offset3;

float distortScale;
float distortBias;

float4 tint;

// Always The Same Input
struct VSI {
    float4 Position : POSITION0;
	float4x4 Instance : POSITION1;
    float2 UV : TEXCOORD0;
	float Time : TEXCOORD1;
};
struct VSO {
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
    float2 NoiseUV1 : TEXCOORD1;
    float2 NoiseUV2 : TEXCOORD2;
    float2 NoiseUV3 : TEXCOORD3;
};

VSO VS(VSI input) {
    VSO output;

	// Project	
	output.Position = mul(input.Position, WVP);
	// Pass UV
	output.UV = input.UV;
	// Calculate Noise UV's
	float pTime = time - input.Time;
    output.NoiseUV1 = (input.UV * scales.x);
    output.NoiseUV1.y = output.NoiseUV1.y + (pTime * rates.x);
    output.NoiseUV2 = (input.UV * scales.y);
    output.NoiseUV2.y = output.NoiseUV2.y + (pTime * rates.y);
    output.NoiseUV3 = (input.UV * scales.z);
    output.NoiseUV3.y = output.NoiseUV3.y + (pTime * rates.z);

    return output;
}

float4 PS(VSO input) : COLOR0 {
    float2 noise1 = tex2D(Noise, frac(input.NoiseUV1)).xy;
	noise1 = mad(noise1, 2, -1);
	noise1 *= offset1;
    float2 noise2 = tex2D(Noise, frac(input.NoiseUV2)).xy;
	noise2 = mad(noise2, 2, -1);
	noise2 *= offset2;
    float2 noise3 = tex2D(Noise, frac(input.NoiseUV3)).xy;
	noise3 = mad(noise3, 2, -1);
	noise3 *= offset3;

	float2 fNoise = noise1 + noise2 + noise3;
	float perturb = ((1 - input.UV.y) * distortScale) + distortBias;
	float2 fUV = (fNoise * perturb) + input.UV;
	fUV = saturate(fUV);
    float4 fireColor = tex2D(Color, fUV);
    float alphaColor = tex2D(Alpha, fUV).r;
	fireColor.a = alphaColor;
	return fireColor * tint;
}

technique Default {
    pass Primary {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}