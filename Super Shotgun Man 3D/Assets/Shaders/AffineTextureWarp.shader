Shader "Unlit/AffineTextureWarp"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Lightmap("Texture", 2D) = "white" {}
		_LightLevel("Light Level", Float) = 0.5
		_UnlitColor("Unlit Color", Color) = (0.0, 0.0, 0.0, 1.0)
		_MultColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _HueShift("Hue Shift", Range(0.0, 1.0)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

			sampler2D _Lightmap;
			float4 _Lightmap_ST;
			float _LightLevel;
			float4 _UnlitColor;
			float4 _MultColor;
            float _HueShift;

			half3 ObjectScale() {
				return half3(
					length(unity_ObjectToWorld._m00_m10_m20),
					length(unity_ObjectToWorld._m01_m11_m21),
					length(unity_ObjectToWorld._m02_m12_m22)
					);
			}

			half Average(half3 scale) {
				return (scale.x + scale.y + scale.z) / 3.0;
			}

            float3 Hue2RGB(float hue){
                hue = frac(hue);
                float r = abs(hue * 6.0 - 3.0) - 1.0;
                float g = 2 - abs(hue * 6.0 - 2.0);
                float b = 2 - abs(hue * 6.0 - 4.0);
                float3 rgb = float3(r, g, b);
                rgb = saturate(rgb);
                return rgb;
            }

            float3 HSV2RGB(float3 hsv){
                float3 rgb = Hue2RGB(hsv.x);
                rgb = lerp(1, rgb, hsv.y);
                rgb = rgb * hsv.z;
                return rgb;
            }

            float3 RGB2HSV(float3 rgb){
                float max_comp = max(rgb.r, max(rgb.g, rgb.b));
                float min_comp = min(rgb.r, min(rgb.g, rgb.b));
                float diff = max_comp - min_comp;
                float hue = 0.0;

                if(max_comp == rgb.r){
                    hue = 0.0 + (rgb.g - rgb.b) / diff;
                }
                else if(max_comp == rgb.g)
                {
                    hue = 2.0 + (rgb.b - rgb.r) / diff;
                }
                else
                {
                    hue = 4.0 + (rgb.r - rgb.g) / diff;
                }

                hue = frac(hue / 6.0);
                float sat = diff / max_comp;
                float val = max_comp;
                return float3(hue, sat, val);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv = TRANSFORM_TEX(v.uv, _Lightmap);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture and lighting
				fixed4 col = tex2D(_MainTex, i.uv * Average(ObjectScale()) * _MainTex_ST.xy + _MainTex_ST.zw);
				fixed4 unlitcol = col * _UnlitColor;
				fixed4 lightmapcol = tex2D(_Lightmap, i.uv * Average(ObjectScale()) * _Lightmap_ST.xy + _Lightmap_ST.zw);
				lightmapcol = lerp(col, lightmapcol, lightmapcol.a);

				//interpolate between lit values by Light
				col = lerp(unlitcol, col, clamp(_LightLevel * 2.0, 0.0, 1.0));
				col *= _MultColor;
				col = lerp(col, lightmapcol, clamp((_LightLevel - 0.5) * 2.0, 0.0, 1.0));

                //warp the colorspace by converting it to HSV colorspace, translating it, and converting it back to RGB
                fixed3 col_hsv = RGB2HSV(col);
                col_hsv.x += _HueShift;
                fixed3 col_rgb = HSV2RGB(col_hsv);
                col = fixed4(col_rgb.x, col_rgb.y, col_rgb.z, col.a);


                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
