// Translation And Then Scaling Of A Map
float3 Translation;
float3 Scaling;

// Camera
float4x4 VP;

// Terrain Color Information
texture TexTerrain;
sampler2D Terrain = sampler_state {
    Texture = <TexTerrain>;
    Magfilter = LINEAR;
    Minfilter = LINEAR;
    Mipfilter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

// Fog Of War Information
texture TexFOW;
sampler2D FOW = sampler_state {
    Texture = <TexFOW>;
    Magfilter = LINEAR;
    Minfilter = LINEAR;
    Mipfilter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

// Always The Same Input
struct VSI {
    float3 Position : POSITION0;
    float2 UV : TEXCOORD0;
};
struct VSO {
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
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

float4 PS(VSO input) : COLOR0 {
	// Only Give Terrain
    return tex2D(Terrain, input.UV);
}
float4 PSFOW(VSO input) : COLOR0 {
	// Get Fog Of War
	float fow = tex2D(FOW, input.UV).x;
	
	// Don't Draw If It Is Less Than 0.1 (Black)
	clip(fow - 0.1);

	// Darken With Fog Of War
    return tex2D(Terrain, input.UV) * fow;
}

technique Default {
    pass Simple {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
    pass FOW {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PSFOW();
    }
}
