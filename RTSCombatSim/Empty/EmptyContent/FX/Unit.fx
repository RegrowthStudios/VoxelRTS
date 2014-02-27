float4x4 VP;

struct VSI {
    float4      ModelPosition   :   POSITION0;
	float4      ModelColor      :   COLOR0;
	float4x4    InstWorld       :   POSITION1;
	float4      InstColor       :   COLOR1;
};
struct VSO {
    float4      Position        :   POSITION0;
	float4      WorldPosition   :   TEXCOORD0;
	float4      Color           :   COLOR0;
};

VSO VS(VSI input) {
    VSO output;
	output.WorldPosition = mul(input.InstWorld, input.ModelPosition);
    output.Position = mul(output.WorldPosition, VP);
    output.Color = input.ModelColor * input.InstColor;
    return output;
}
float4 PS(VSO input) : COLOR0 {
    return input.Color;
}

technique Default {
    pass Main {
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS();
    }
}
