float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;

Texture xTexture;
sampler CubeTextureSampler = sampler_state 
{
	texture = <xTexture>; 
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter=LINEAR; 
	AddressU = mirror; 
	AddressV = mirror;
};

//------- Technique: SkyPlane --------
struct SkyPlaneVertexToPixel
{
    float4 Position   	: POSITION;    
    float3 Pos3D		: TEXCOORD0;
};

SkyPlaneVertexToPixel SkyPlaneVS( float4 inPos : POSITION)
{	
	SkyPlaneVertexToPixel Output = (SkyPlaneVertexToPixel)0;
	float4x4 preViewProjection = mul (xView, xProjection);
	float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
	
	Output.Position = mul(inPos, preWorldViewProjection);
	Output.Pos3D = inPos;
    
	return Output;    
}

struct SkyPlanePixelToFrame
{
    float4 Color : COLOR0;
};

SkyPlanePixelToFrame SkyPlanePS(SkyPlaneVertexToPixel PSIn) 
{
	SkyPlanePixelToFrame Output = (SkyPlanePixelToFrame)0;		
	
	Output.Color = texCUBE(CubeTextureSampler, PSIn.Pos3D);

	return Output;
}

technique SkyPlane
{
	pass Pass0
    {   
    	VertexShader = compile vs_1_1 SkyPlaneVS();
        PixelShader  = compile ps_1_1 SkyPlanePS();
    }
}

