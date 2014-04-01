// Translation And Then Scaling Of A Map
float2 TexelSize;
float2 MapSize;

// Camera
float4x4 VP;

// Terrain Color Information
sampler2D Terrain : register(s0);
// Fog Of War Information
sampler2D FOW : register(s1);

// Always The Same Input
struct VSI {
    float3 Position : POSITION0;
    float2 UV : TEXCOORD0;
};
struct VSO {
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
};
struct VSO_M {
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
    float2 FOWUV : TEXCOORD1;
};

VSO VS(VSI input) {
    VSO output;

	// Transform World Position
    float3 worldPosition = input.Position;
    
	// Project	
	output.Position = mul(float4(worldPosition, 1), VP);
    
	// Pass UV
	output.UV = input.UV;

    return output;
}
VSO_M VSMap(VSI input) {
    VSO_M output;

	// Transform World Position
    float3 worldPosition = input.Position;
    
	// Project	
	output.Position = mul(float4(worldPosition, 1), VP);
    
	// Pass UV
	output.UV = input.UV;
	output.FOWUV = float2(worldPosition.x / MapSize.x, worldPosition.z / MapSize.y);

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
	float fow = SampleFOW(input.UV);
	
	// Don't Draw If It Is Less Than 0.1 (Black)
	clip(fow - 0.1);

	// Darken With Fog Of War
    return tex2D(Terrain, input.UV) * fow;
}
float4 PSMap(VSO_M input) : COLOR0 {
	// Get Fog Of War
	float fow = SampleFOW(input.FOWUV);
	
	// Don't Draw If It Is Less Than 0.1 (Black)
	clip(fow - 0.1);

	// Darken With Fog Of War
    return tex2D(Terrain, input.UV) * fow;
}

technique Default {
    pass Primary {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
	pass Secondary {
		VertexShader = compile vs_3_0 VSMap();
        PixelShader = compile ps_3_0 PSMap();
	}
}