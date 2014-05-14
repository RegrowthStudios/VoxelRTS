// Camera Information
float4x4 VP;

// Time Information
float Time;

// Fog Of War Information
float2 MapSize;
sampler2D FOW : register(s0);

// Color Information
sampler2D Color : register(s1);
sampler2D Noise : register(s2);
sampler2D Alpha : register(s3);

// Lightning Shader Variables
float Splits;

// Fire Shader Variables
float3 rates;
float3 scales;
float2 offset1;
float2 offset2;
float2 offset3;
float distortScale;
float distortBias;

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

	// Transform World Position And Project
    float4 worldPosition = mul(Instance, input.Position);
	output.Position = mul(worldPosition, VP);
    
	// Pass UV/Tint
	output.UV = input.UV;
	output.FOWUV = worldPosition.xz / MapSize;
	output.Tint = Tint;

    return output;
}
float4 PS(VSO input) : COLOR0 {
	// Don't Draw In Fog Of War
	float fow = tex2D(FOW, input.FOWUV);
	clip(fow - 0.9);

    return tex2D(Color, input.UV) * input.Tint;
}


struct VSILightning {
    float4 Position : POSITION0;
	float4x4 Instance : POSITION1;
    float2 UV : TEXCOORD0;
	float2 TimeType : TEXCOORD1;
	float4 Tint : COLOR0;
};
struct VSOLightning {
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
    float2 FOWUV : TEXCOORD1;
	float4 Tint : COLOR0;
};
VSOLightning VSLightning(VSILightning input) {
	VSOLightning output;

	// Transform World Position And Project
    float4 worldPosition = mul(input.Instance, input.Position);
	output.Position = mul(worldPosition, VP);
    
	// Pass UV/Tint
	output.UV = float2((input.TimeType.y + input.UV.x) / Splits, input.UV.y);
	output.FOWUV = worldPosition.xz / MapSize;
	output.Tint = input.Tint;

    return output;
}
float4 PSLightning(VSOLightning input) : COLOR0 {
	// Don't Draw In Fog Of War
	float fow = tex2D(FOW, input.FOWUV);
	clip(fow - 0.9);

	// Don't Draw Transparency
	float4 col = tex2D(Color, input.UV);
	clip(col.a - 0.1);

	return col * input.Tint;
}


struct VSIFire {
    float4 Position : POSITION0;
	float4x4 Instance : POSITION1;
    float2 UV : TEXCOORD0;
	float Time : TEXCOORD1;
	float4 Tint : COLOR0;
};
struct VSOFire {
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
    float2 FOWUV : TEXCOORD1;
    float2 NoiseUV1 : TEXCOORD2;
    float2 NoiseUV2 : TEXCOORD3;
    float2 NoiseUV3 : TEXCOORD4;
	float4 Tint : COLOR0;
};
VSOFire VSFire(VSIFire input) {
    VSOFire output;

	// Transform World Position And Project
    float4 worldPosition = mul(input.Instance, input.Position);
	output.Position = mul(worldPosition, VP);

	// Pass UV / Tint
	output.UV = input.UV;
	output.FOWUV = worldPosition.xz / MapSize;
	output.Tint = input.Tint;

	// Calculate Noise UV's
	float pTime = Time - input.Time;
    output.NoiseUV1 = (input.UV * scales.x);
    output.NoiseUV1.y = output.NoiseUV1.y + (pTime * rates.x);
    output.NoiseUV2 = (input.UV * scales.y);
    output.NoiseUV2.y = output.NoiseUV2.y + (pTime * rates.y);
    output.NoiseUV3 = (input.UV * scales.z);
    output.NoiseUV3.y = output.NoiseUV3.y + (pTime * rates.z);

    return output;
}
float4 PSFire(VSOFire input) : COLOR0 {
	// Don't Draw In Fog Of War
	float fow = tex2D(FOW, input.FOWUV);
	clip(fow - 0.9);

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
	return fireColor * input.Tint;
}


struct VSIAlert {
    float4 Position : POSITION0;
	float3 Origin : POSITION1;
	float3 Target : POSITION2;
    float4 DS : POSITION3;
    float2 UV : TEXCOORD0;
	float4 Tint1 : COLOR0;
	float4 Tint2 : COLOR1;
};
struct VSOAlert {
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
	float4 Tint : COLOR0;
};
VSOAlert VSAlert(VSIAlert input) {
	VSOAlert output;

	// Transform World Position And Project
    float4 worldPosition = input.Position;
	float tp = saturate((Time - input.DS.x) / input.DS.y);
	worldPosition.xyz *= lerp(input.DS.z, input.DS.w, tp);
	worldPosition.xyz += lerp(input.Origin, input.Target, tp);
	output.Position = mul(worldPosition, VP);
    
	// Pass UV/Tint
	output.UV = input.UV;
	output.Tint = lerp(input.Tint1, input.Tint2, tp);

    return output;
}
float4 PSAlert(VSOAlert input) : COLOR0 {
	return tex2D(Color, input.UV) * input.Tint;
}


technique Default {
    pass Simple {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
	pass Lightning {
        VertexShader = compile vs_3_0 VSLightning();
        PixelShader = compile ps_3_0 PSLightning();
    }
	pass Fire {
        VertexShader = compile vs_3_0 VSFire();
        PixelShader = compile ps_3_0 PSFire();
    }
	pass Alert {
        VertexShader = compile vs_3_0 VSAlert();
        PixelShader = compile ps_3_0 PSAlert();
    }
}