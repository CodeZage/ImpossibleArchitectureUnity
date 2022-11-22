Shader "Outline/WikiOutline" {
    Properties {
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineThickness ("Outline Thickness", Range(0,.2)) = 0.03
         
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
        
    SubShader {
        // Base pass for object.
        Pass {
            Stencil {
                Ref 1
                Comp Always
                Pass Replace
            }
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _Color;

            struct appdata{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f{
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
           
            v2f vert(appdata v){
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_TARGET{
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;
                return col;
            }
            ENDCG
        }

        Pass {
            Cull Off
            Stencil {
                Ref 1
                Comp NotEqual
                Pass Keep
            }
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            uniform float _OutlineThickness;
            fixed4 _OutlineColor;

            // Slide vertices along the normal by the thickness.
            float4 vert(float4 vertex : POSITION, float3 normal : NORMAL) : SV_POSITION {
                return UnityObjectToClipPos(vertex + normal * _OutlineThickness);
            }

            // Just output the outline color.
            float4 frag(void) : COLOR {
                return _OutlineColor;
            }
            ENDCG
        }
    } 
}