Shader "Custom/UI Foil Overlay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _Opacity ("Opacity", Range(0, 1)) = 0
        _FoilPosition ("Foil Position", Vector) = (0.5, 0.5, 0, 0)
        _Brightness ("Brightness", Range(0, 3)) = 1.2
        _BandWidth ("Band Width", Range(0.01, 0.5)) = 0.08
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha One

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;

            float _Opacity;
            float4 _FoilPosition;
            float _Brightness;
            float _BandWidth;

            v2f vert(appdata_t v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // UI Sprite
                fixed4 baseTex = tex2D(_MainTex, uv);

                float diagonal = uv.x + uv.y;
                float pos = _FoilPosition.x + _FoilPosition.y;

                float band = 1.0 - smoothstep(
                    0.0,
                    _BandWidth,
                    abs(diagonal - pos)
                );

                float3 colorA = float3(1.0, 0.85, 0.25);
                float3 colorB = float3(0.45, 0.15, 1.0);

                float3 foilColor = lerp(colorA, colorB, uv.x);

                float alpha = band * _Opacity * baseTex.a;

                return fixed4(
                    foilColor * _Brightness,
                    alpha
                ) * i.color;
            }

            ENDCG
        }
    }
}