// Camera
float4x4 VP;
float4x4 World;

// Translation And Then Scaling Of A Map
float2 TexelSize;
float2 MapSize;

// Terrain Color Information
sampler2D VoxMap : register(s0);
sampler2D FOW : register(s1);
// Always The Same Input
struct VSI {
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
    float4 UVRect : TEXCOORD1;
	float4 Tint : COLOR0;
};
struct VSO {
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
    float4 UVRect : TEXCOORD1;
	float4 Tint : COLOR0;
	float2 FOWUV : TEXCOORD2;
};

VSO VS(VSI input) {
    VSO output;

	// Project	
    float4 worldPosition = mul(input.Position, World);
	output.Position = mul(worldPosition, VP);
    
	// Pass UV
	output.UV = input.UV;
	output.UVRect = input.UVRect;
	output.Tint = input.Tint;
	output.FOWUV = worldPosition.xz / MapSize.xy;
    return output;
}

float SampleFOW(float2 uv) {
	float2 rt = uv / TexelSize;
	float mx = fmod(rt.x, 1) - 0.5;
	mx = trunc(mx * 3);
	float my = fmod(rt.y, 1) - 0.5;
	my = trunc(my * 3);
	float fow = tex2D(FOW, uv) + 
		tex2D(FOW, uv + float2(mx * TexelSize.x, 0)) +
		tex2D(FOW, uv + float2(0, my * TexelSize.y)) +
		tex2D(FOW, uv + float2(mx * TexelSize.x, my * TexelSize.y));
	return fow * 0.25;
}

float4 PS(VSO input) : COLOR0 {
	// Get Fog Of War
	float fow = SampleFOW(input.FOWUV);
	
	// Don't Draw If It Is Less Than 0.1 (Black)
	clip(fow - 0.1);

	float2 uv = fmod(input.UV, 1);
	uv = input.UVRect.xy + input.UVRect.zw * uv;
    return tex2D(VoxMap, uv) * input.Tint * fow;
}

technique Default {
    pass Primary {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}