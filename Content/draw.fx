float4x4 World;
float4x4 View;
float4x4 Projection;

float4x4 WorldInverseTranspose;

float3 DiffuseLightDirection = float3(1, 0, 1);
float4 DiffuseColor = float4(1, 0.5, 0, 1);
float DiffuseIntensity = 1.0;
float3 FillLightDirection = float3(0,1,1);
float4 FillColor = float4(0.5,0.5,1,0.5);
float FillIntensity = 1.0;
float4 FogColor = float4(0,0,0,0);

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
	float FogFactor : TEXCOORD1;
	float3 Normal : TEXCOORD2;
};


VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    input.Position.w = 1.0f;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	output.Color = input.Color;

	float4 cameraPosition;
    
	//We first calculate the Z coordinate of the vertex in view space.
	//We then use that with the fog end and start position in the fog factor equation to produce a fog factor 
	//that we send into the pixel shader.

    // Calculate the camera position.
    cameraPosition = mul(input.Position, World);
    cameraPosition = mul(cameraPosition, View);

    // Calculate linear fog.    
    output.FogFactor = saturate((30.0f + cameraPosition.z) / (20.0f));

	output.Normal = input.Normal;
    return output;
}

struct PixelShaderOutput
{
    float4 Color : COLOR0;
};

PixelShaderOutput PSColor(VertexShaderOutput input) : COLOR0
{
    PixelShaderOutput output;

	float4 normal = mul(input.Normal, WorldInverseTranspose);
	float4 texColor = input.Color;//tex2D(diffuseSampler, input.TexCoord);
    float lightIntensity = dot(normal, DiffuseLightDirection);
    output.Color = saturate(
		(DiffuseColor * DiffuseIntensity * lightIntensity * texColor)
		+ (FillColor * FillIntensity * dot(normal, FillLightDirection) * texColor));
	output.Color += texColor * 0.1;
    //output.Color = lerp(output.Color, FogColor, 1.0f - input.FogFactor);
	output.Color.a = input.Color.a;
    return output;
}

technique DrawColor
{
    pass Pass1
    {
		AlphaBlendEnable = true;
		BlendOp = Add;
		SrcBlend = One;
		DestBlend = InvSrcAlpha;
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		CullMode = None;
		
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PSColor();
    }
}

