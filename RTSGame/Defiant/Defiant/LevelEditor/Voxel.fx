// Camera
float4x4 VP;
float4x4 World;

// Terrain Color Information
sampler2D VoxMap : register(s0);
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

    return output;
}

float4 PS(VSO input) : COLOR0 {
	float2 uv = fmod(input.UV, 1);
	uv = input.UVRect.xy + input.UVRect.zw * uv;
    return tex2D(VoxMap, uv) * input.Tint;
}

technique Default {
    pass Primary {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}