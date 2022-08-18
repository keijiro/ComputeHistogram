Shader "Histogram Graph"
{
    Properties
    {
        _MainTex("", 2D) = "black" {}
    }

CGINCLUDE

#include "UnityCG.cginc"

StructuredBuffer<uint> _Histogram;
uint _BinCount;
float _VScale;

void Vertex(float4 position : POSITION,
            float2 texCoord : TEXCOORD0,
            out float4 outPosition : SV_Position,
            out float2 outTexCoord : TEXCOORD0)
{
    outPosition = UnityObjectToClipPos(position);
    outTexCoord = texCoord;
}

float4 Fragment(float4 position : SV_Position,
                float2 texCoord : TEXCOORD0) : SV_Target
{
    uint x = texCoord.x * _BinCount;
    return texCoord.y < _Histogram[x] * _VScale;
}

ENDCG

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDCG
        }
    }
}
