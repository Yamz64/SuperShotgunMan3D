Shader "Unlit/AffineTextureWarp"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Lightmap("Texture", 2D) = "white" {}
		_LightLevel("Light Level", Float) = 0.5
		_UnlitColor("Unlit Color", Color) = (0.0, 0.0, 0.0, 1.0)
		_MultColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _OutlineColor("Outline Overlay Color", Color) = (0.2, 0.07843135, 0.07843135, 1.0)
        _ColorEpsilon("Outline Epsilon", Float) = .01
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
                float4 world_normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 world_normal : TEXCOORD1;
                float3 world_pos : TEXCOORD2;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

            sampler2D _MainTex;
            float4 _MainTex_ST;

			sampler2D _Lightmap;
			float4 _Lightmap_ST;
			float _LightLevel;
			float4 _UnlitColor;
			float4 _MultColor;
            float4 _OutlineColor;
            float _ColorEpsilon;
            float _HueShift;
            float4 _CastingObjects[256];

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

            bool CompareColor(float4 a, float4 b){
                return distance(a, b) > _ColorEpsilon;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv = TRANSFORM_TEX(v.uv, _Lightmap);
                o.world_normal = UnityObjectToWorldNormal(v.world_normal);
                o.world_pos = mul(unity_ObjectToWorld, v.vertex);
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

                //calculate a light modifier based off from the normal's dot product with the up vector and north vector
                float light_mod_1 = (dot(i.world_normal, float3(0.0, 1.0, 0.0)) + 1.0) / 1.25;
                light_mod_1 = clamp(light_mod_1, 0.25, 1.0);

                float light_mod_2 = (dot(i.world_normal, float3(0.0, 0.0, 1.0)) + 1.0) / 1.25;
                light_mod_2 = clamp(light_mod_2, 0.25, 1.0);

                //further modify the light value again by comparing distance of pixel with a shadowcasting object's POSITION
                float light_mod_3 = 1.0;

                for(int n=0; n<256; n++){
                    float dist = distance(i.world_pos, _CastingObjects[n].xyz);
                    if(dist < _CastingObjects[n].w){
                        light_mod_3 = 0.5f;
                        break;
                    }
                }

				//interpolate between lit values by Light
				col = lerp(unlitcol, col, clamp(_LightLevel * 2.0 * light_mod_1 * light_mod_2 * light_mod_3, 0.0, 1.0));
				col *= _MultColor;
				col = lerp(col, lightmapcol, clamp((_LightLevel * light_mod_3 - 0.5) * 2.0, 0.0, 1.0));


                //warp the colorspace by converting it to HSV colorspace, translating it, and converting it back to RGB
                if(CompareColor(col, _OutlineColor))
                {
                    fixed3 col_hsv = RGB2HSV(col);
                    col_hsv.x += _HueShift;
                    fixed3 col_rgb = HSV2RGB(col_hsv);
                    col = fixed4(col_rgb.x, col_rgb.y, col_rgb.z, col.a);
                }

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
