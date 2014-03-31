// Translation And Then Scaling Of A Map
float3 Translation;
float3 Scaling;
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
    float3 worldPosition = (input.Position + Translation) * Scaling;
    
	// Project	
	output.Position = mul(float4(worldPosition, 1), VP);
    
	// Pass UV
	output.UV = input.UV;

    return output;
}
VSO_M VSMap(VSI input) {
    VSO_M output;

	// Transform World Position
    float3 worldPosition = (input.Position + Translation) * Scaling;
    
	// Project	
	output.Position = mul(float4(worldPosition, 1), VP);
    
	// Pass UV
	output.UV = input.UV;
	output.FOWUV = float2(worldPosition.x / MapSize.x, worldPosition.z / MapSize.y);

    return output;
}

float4 PS(VSO input) : COLOR0 {
	// Get Fog Of War
	float fow = tex2D(FOW, input.UV);
	
	// Don't Draw If It Is Less Than 0.1 (Black)
	clip(fow - 0.1);

	// Darken With Fog Of War
    return tex2D(Terrain, input.UV) * fow;
}
float4 PSMap(VSO_M input) : COLOR0 {
	// Get Fog Of War
	float fow = tex2D(FOW, input.FOWUV).x;
	
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