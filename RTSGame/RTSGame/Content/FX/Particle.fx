// Camera
float4x4 VP;

// Terrain Color Information
sampler2D Color : register(s0);

// Fog Of War Information
float2 MapSize;
sampler2D FOW : register(s1);

// Always The Same Input
struct VSI {
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
};
struct VSO {
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
    float2 FOWUV : TEXCOORD1;
	float4 Tint : COLOR0;
};

VSO VS(VSI input, float4x4 Instance : POSITION1, float4 Tint : COLOR0) {
    VSO output;

	// Transform World Position
    float4 worldPosition = mul(Instance, input.Position);
	// Project
	output.Position = mul(worldPosition, VP);
    
	// Pass UV/Tint
	output.UV = input.UV;
	output.FOWUV = worldPosition.xz / MapSize;
	output.Tint = Tint;

    return output;
}

float4 PS(VSO input) : COLOR0 {
	// Get Fog Of War
	float fow = tex2D(FOW, input.FOWUV);
	
	// Don't Draw If It Is Less Than 0.1 (Black)
	clip(fow - 0.9);

	// Darken With Fog Of War
    return tex2D(Color, input.UV) * input.Tint * fow;
}

technique Default {
    pass Primary {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}