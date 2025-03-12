Shader "Custom/CloudShadowScrolling"
{
    Properties
    {
        _Speed ("Speed (X, Z)", Vector) = (0.05, 0, 0.05, 0)
        _MainTex ("Cloud Texture 1", 2D) = "white" {}
        _MainTex2 ("Cloud Texture 2", 2D) = "white" {}
        _CloudScale ("Cloud Scale", Float) = 0.5
        _Transparency ("Transparency", Float) = 0.5 // Add transparency property
    }

    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Opaque" }
        Pass
        {
            Name "CloudShadowScrolling"
            Tags { "LightMode" = "Always" }

            // Transparency blend mode
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            Lighting Off

            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            float4 _Speed;
            sampler2D _MainTex;
            sampler2D _MainTex2;
            float _CloudScale;
            float _Transparency; // Use transparency property

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; // Convert to world position
                return o;
            }

            float4 frag (v2f IN) : SV_Target
            {
                // Scrolling texture in world space
                float2 uv1 = IN.worldPos.xz * _CloudScale + _Time.y * _Speed.xz;
                float2 uv2 = IN.worldPos.xz * _CloudScale + _Time.y * _Speed.zw;

                float cloud1 = tex2D(_MainTex, uv1).r;
                float cloud2 = tex2D(_MainTex2, uv2).r;
                float cloudShadow = saturate(cloud1 * cloud2);

                // Apply transparency
                return float4(cloudShadow, cloudShadow, cloudShadow, _Transparency);
            }
            ENDCG
        }
    }
}
