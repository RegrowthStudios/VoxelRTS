// The Screen Size Transform
float2 IHS;

// The Glyph Map
texture Texture;
sampler TexSampler = sampler_state {
	texture = <Texture>;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
};

// Vertex Input Structure
struct VSI {
	float2 Position : POSITION0;
	float2 UV       : TEXCOORD0;
	float4 Tint     : COLOR0;
};
// Vertex Output Structure
struct VSO {
	float4 Position  : POSITION0;
	float2 UV        : TEXCOORD0;
	float4 Tint      : COLOR0;
};

// Vertex Shader
VSO VS(VSI input) {
	VSO output;

	output.Position = float4((input.Position.x * IHS.x) - 1, 1 - (input.Position.y * IHS.y), 0, 1);
	output.UV = input.UV;
	output.Tint = input.Tint;

	return output;
}

// Pixel Shader
float4 PS(VSO input) : COLOR0 {
	return tex2D(TexSampler, input.UV) * input.Tint;
}

// Technique
technique Default {
	pass p0 {
		VertexShader = compile vs_2_0 VS();
		PixelShader  = compile ps_2_0 PS();
	}
}