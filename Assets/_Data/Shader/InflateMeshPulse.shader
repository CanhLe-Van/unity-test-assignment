Shader "Custom/InflateLit"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1)

        _Inflate ("Inflate Amount", Range(0, 0.2)) = 0.02

        _Ambient ("Ambient", Range(0, 1)) = 0.2
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        Pass
        {
            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv              : TEXCOORD0;
                float4 pos             : SV_POSITION;
                float3 worldNormal     : TEXCOORD1;
                float3 worldPos        : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Inflate;
            float _Ambient;

            v2f vert (appdata v)
            {
                v2f o;

                float3 inflatedPos = v.vertex.xyz + v.normal * _Inflate;

                float4 worldPos = mul(unity_ObjectToWorld, float4(inflatedPos, 1.0));
                o.worldPos = worldPos.xyz;

                // normal world
                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                o.pos = UnityWorldToClipPos(worldPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed3 normal = normalize(i.worldNormal);

                fixed3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

                float NdotL = max(0, dot(normal, lightDir));
                float lightAmount = _Ambient + NdotL * _LightColor0.rgb;

                fixed4 tex = tex2D(_MainTex, i.uv) * _Color;
                fixed3 finalColor = tex.rgb * lightAmount;

                return fixed4(finalColor, tex.a);
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}