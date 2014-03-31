// The Necessary Parameters To Draw A Model
float4x4 World;
float4x4 VP;

// Textures
sampler2D Model : register(s0);
sampler2D Color : register(s1);
sampler2D Overlay : register(s2);
sampler2D FOW : register(s3);

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
struct VSO {
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
};

VSO VS(VSI input) {
    VSO output;

    float4 worldPosition = mul(input.Position, World);
    output.Position = mul(worldPosition, VP);
    output.UV = input.UV;

    return output;
}
VSO VS_Anim(VSI input, float4x4 InstWorld : POSITION1, float InstAnim : TEXCOORD1) {
    VSO output;

    // Get The UV Coordinates In The Model Texture
    float2 animUV = float2(input.Position.x, input.Position.y + (InstAnim * TexelSize.y * 3.0) + TexelSize.y * 0.5);
    
    // Sample The Model Position
    float x = tex2Dlod(Model, float4(animUV.x, animUV.y, 0, 0)).x;
    float y = tex2Dlod(Model, float4(animUV.x, animUV.y + TexelSize.y, 0, 0)).x;
    float z = tex2Dlod(Model, float4(animUV.x, animUV.y + (TexelSize.y * 2.0), 0, 0)).x;

    float4 worldPosition = mul(InstWorld, float4(x, y, z, 1));
    output.Position = mul(worldPosition, VP);
    output.UV = input.UV;

    return output;
}

float4 PS(VSO input) : COLOR0 {
    return tex2D(Color, input.UV);
}
float4 PS_Swatch(VSO input) : COLOR0 {
    float4 swatch = tex2D(Overlay, input.UV);
    float4 color = tex2D(Color, input.UV);
    float3 sv = swatch.r * CPrimary + swatch.g * CSecondary + swatch.b * CTertiary;
    return swatch.a > 0.5 ? float4(sv, 1) : color;
}

technique Default {
    pass Simple {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
    pass Animation {
        VertexShader = compile vs_3_0 VS_Anim();
        PixelShader = compile ps_3_0 PS_Swatch();
    }
}
