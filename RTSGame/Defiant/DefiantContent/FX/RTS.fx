// The Necessary Parameters To Draw A Model
float4x4 World;
float4x4 VP;

// Textures
sampler2D Model : register(s0);
sampler2D Color : register(s1);
sampler2D Overlay : register(s2);
sampler2D FOW : register(s3);
sampler2D BuildNoise : register(s3);

// Used For RGB Palette Color Calculation
float3 CPrimary;
float3 CSecondary;
float3 CTertiary;

// Used For Animation To Figure Out The Actual Position And Normal
float2 TexelSize;

// To Determine It's Fog Of War
float2 MapSize;

// Always The Same Input
struct VSI {
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
};
struct VSOBuilding {
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
	float BuildAmount : TEXCOORD1;
};
VSOBuilding VSBuilding(VSI input, float4x4 InstWorld : POSITION1, float BuildAmount : TEXCOORD1) {
    VSOBuilding output;

    float4 worldPosition = mul(InstWorld, input.Position);
    output.Position = mul(worldPosition, VP);
    output.UV = input.UV;
	output.BuildAmount = BuildAmount;

    return output;
}
float4 PSBuilding(VSOBuilding input) : COLOR0 {
	float alpha = input.BuildAmount - tex2D(BuildNoise, input.UV).x;
	clip(alpha + 0.3);
	
	float4 swatch = tex2D(Overlay, input.UV);
	float4 color = tex2D(Color, input.UV);
    float3 sv = swatch.r * CPrimary + swatch.g * CSecondary + swatch.b * CTertiary;
    float3 cNorm = lerp(color.xyz, sv, swatch.a);
	return float4(lerp(CPrimary, cNorm, saturate(input.BuildAmount + alpha * 6)), 1);
}

struct VSO {
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
};
VSO VSUnit(VSI input, float4x4 InstWorld : POSITION1, float InstAnim : TEXCOORD1) {
    VSO output;

    // Get The UV Coordinates In The Model Texture
    float2 animUV = float2(input.Position.x, (InstAnim * TexelSize.y * 3.0) + TexelSize.y * 0.5);
    
    // Sample The Model Position
    float x = tex2Dlod(Model, float4(animUV.x, animUV.y, 0, 0));
    float y = tex2Dlod(Model, float4(animUV.x, animUV.y + TexelSize.y, 0, 0));
    float z = tex2Dlod(Model, float4(animUV.x, animUV.y + TexelSize.y + TexelSize.y, 0, 0));

    float4 worldPosition = mul(InstWorld, float4(x, y, z, 1));
    output.Position = mul(worldPosition, VP);
    output.UV = input.UV;

    return output;
}
float4 PSUnit(VSO input) : COLOR0 {
    float4 swatch = tex2D(Overlay, input.UV);
    float4 color = tex2D(Color, input.UV);
    float3 sv = swatch.r * CPrimary + swatch.g * CSecondary + swatch.b * CTertiary;
    return lerp(color, float4(sv, 1), swatch.a);
}

struct VSIHealth {
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
    float3 InstPosition : POSITION1;
    float4 DRH : TEXCOORD1;
	float4 Tint : COLOR0;
};
struct VSOHealth {
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
	float4 Tint : COLOR0;
	float3 DH : TEXCOORD1;
};
VSOHealth VSHealth(VSIHealth input) {
	VSOHealth output;

	float3 uDir = float3(input.DRH.y, 0, -input.DRH.x);
	float3 vDir = float3(input.DRH.x, 0, input.DRH.y);
	float4 worldPosition = float4(
		input.Position.x * uDir * input.DRH.z +
		input.Position.z * vDir * input.DRH.z,
		1
		);
	worldPosition.xyz = worldPosition.xyz + input.InstPosition;

    output.Position = mul(worldPosition, VP);
    output.UV = input.UV;
	output.DH = input.DRH.xyw;
	output.Tint = input.Tint;

	return output;
}
float4 PSHealth(VSOHealth input) : COLOR0 {
	float2 sc = input.UV;
	sc.x = sc.x * 2 - 1;
	sc.y = 1 - sc.y * 2;
	sc = normalize(sc);

	float dsc = dot(sc, float2(0, -1));
	dsc = (dsc + 1) * 0.5;
	clip(input.DH.z - dsc);

	return tex2D(Model, input.UV) * input.Tint;
	return input.Tint;
}

technique Entity {
    pass Building {
        VertexShader = compile vs_3_0 VSBuilding();
        PixelShader = compile ps_3_0 PSBuilding();
    }
    pass Unit {
        VertexShader = compile vs_3_0 VSUnit();
        PixelShader = compile ps_3_0 PSUnit();
    }
}

technique Health {
	pass Main {
		VertexShader = compile vs_3_0 VSHealth();
        PixelShader = compile ps_3_0 PSHealth();
	}
}