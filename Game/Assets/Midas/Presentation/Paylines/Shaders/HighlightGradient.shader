// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//-----------------------------------------------------------------------
// <copyright file = "HighlightGradientPayline.shader" company = "IGT">
//     Copyright (c) IGT 2014.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

Shader "Paylines/HighlightGradient" {
    Properties {
    	_Color ("Color", Color) = (1,1,1,1) 
        _LineWidth ("Line Width", Float) = 0
        _BlurThreshold ("Blur Threshold", Float) = 0.8
    }
    SubShader 
    {
        Tags {"Queue" = "Transparent+20" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

             struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 color : COLOR;
            };

			uniform float4 _Color;
            uniform float _LineWidth;
            v2f vert(appdata_full v)
            {
                v2f output;
                float4 lineVert = v.vertex;
                lineVert.xyz += (v.normal * _LineWidth);
                output.pos = UnityObjectToClipPos (lineVert);
                output.uv = float4(v.normal, 0.0f);
                output.uv2 = v.texcoord;
                output.color = v.color * _Color;
                return output;
            }

            uniform float _BlurThreshold;
            float BlurEdge( float rho )
            {
                if( rho < _BlurThreshold )
                    return 1.0f;
                else
                {
                    float normrho = (rho - _BlurThreshold) * 1 / (1 - _BlurThreshold);
                    return 1 - normrho;
                }
            }

            float4 frag (v2f i) : COLOR
            {
                float3 scaledColor;
                if(i.uv2.y >= 0.5)
                {
                    scaledColor = i.color.xyz * (1-(i.uv2.y * 2.1 - 1.3));
                }
                else
                {
                    scaledColor = i.color.xyz * ((i.uv2.y * 2));
                }
                return float4(scaledColor, i.color.w * BlurEdge(abs(i.uv2.y * 2 - 1)));
            }

            ENDCG 
        }
    } 
    FallBack "Diffuse"
}
