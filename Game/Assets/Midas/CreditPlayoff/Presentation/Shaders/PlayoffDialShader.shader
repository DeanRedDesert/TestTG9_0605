// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Playoff/DialFace"
{
    Properties
    {
        _MainTex("RedTexture", 2D) = "white" {}
        _MainTex2("GreenTexture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _Percentage("Percentage", Range(0.0, 1.000)) = .25
    }
    
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Fog { Mode Off }
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
    
            sampler2D _MainTex;
            sampler2D _MainTex2;
            float _Percentage;
            fixed4 _Color;
            
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };
    
            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color : COLOR;
                half2 texcoord  : TEXCOORD0;
                float3 mpos : DATA;
            };
    
    
            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
    
                // Data for conical gradient
                OUT.mpos = IN.vertex.xyz;
    
                return OUT;
            }
    
            fixed4 frag(v2f IN) : SV_Target
            {
                const float PI = 3.1415926f;
                const float PI_2 = PI * 2;

                fixed4 redTexture = tex2D(_MainTex, float2(IN.texcoord.x, IN.texcoord.y)) * IN.color;
                fixed4 greenTexture = tex2D(_MainTex2, float2(IN.texcoord.x, IN.texcoord.y)) * IN.color;
    
                // This is slightly different than the DialShader. Instead of theta originating from veritical position (12 o'clock),
                // the green area needs to be CENTERED around 12 o'clock.
    
                // Range is (-PI to PI], prefer to work [0, 2 PI) range.
                // Find angle of X, Y coord in [0, 2PI)
                float theta = atan2(IN.mpos.y, IN.mpos.x);
                // Phase shift 3 * PI / 2, makes calcs easier later to calculate around 12 o clock.
                theta += 1.5f * PI;
                theta += step(theta, 0) * PI_2;
                theta -= step(PI_2, theta) * PI_2;

                // Find cutoff points.
                float highAngle = _Percentage * PI;
                float lowAngle = PI_2 - highAngle;

                fixed4 color = redTexture;
                if (theta < highAngle || theta > lowAngle)
                {
                    color = greenTexture;
                }
    
                color.rgb *= color.a;
    
                return color;
            }
            ENDCG
        }
    }
}
