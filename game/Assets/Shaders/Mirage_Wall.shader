Shader "Mirage/Wall" {
	Properties {
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
		[MainColor] _Color ("Color", Color) = (1,1,1,1)
		_Glossiness ("Smoothness", Range(0, 1)) = 0.5
		_Metallic ("Metallic", Range(0, 1)) = 0
		_FalloffOffset ("Falloff offset", Range(0, 100)) = 20
		_FalloffSpeed ("Falloff speed", Range(0, 10)) = 1
		_MinimumHeight ("Minimum Height", Range(0, 1)) = 0.2
	}
	SubShader {
		LOD 200
		Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Opaque" }
		Pass {
			Name "FORWARD"
			LOD 200
			//Tags { "LIGHTMODE" = "FORWARDBASE" "RenderType" = "Wall" "SHADOWSUPPORT" = "true" }
            Tags
            {
                "LightMode" = "UniversalForwardOnly"
            }
			GpuProgramID 27690
			
			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			//#include "UnityInput.hlsl"
			//#include "UnityShaderVariables.cginc"
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float3 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
				float4 texcoord5 : TEXCOORD5;
				float4 texcoord6 : TEXCOORD6;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			
			CBUFFER_START(UnityPerMaterial)
			// $Globals ConstantBuffers for Vertex Shader
			float4 _CameraTarget;
			float _FalloffOffset;
			float _FalloffSpeed;
			float _MinimumHeight;
			float4 _MainTex_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _LightColor0;
			float _Glossiness;
			float _Metallic;
			float4 _Color;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _BaseMap;
			CBUFFER_END
			
			// Keywords: DIRECTIONAL
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0 = v.vertex - _CameraTarget;
                tmp0.x = dot(tmp0, tmp0);
                tmp0.x = sqrt(tmp0.x);
                tmp0.x = tmp0.x * _FalloffSpeed + -_FalloffOffset;
                tmp0.x = max(tmp0.x, _MinimumHeight);
                tmp0.x = min(tmp0.x, 1.0);
                tmp0.y = tmp0.x * v.vertex.y;
                tmp0.x = 1.0 - tmp0.x;
                tmp1.y = saturate(v.texcoord.y - tmp0.x);
                tmp0 = tmp0.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp2 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                o.texcoord2.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                tmp0 = tmp2.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp0 = unity_MatrixVP._m00_m10_m20_m30 * tmp2.xxxx + tmp0;
                tmp0 = unity_MatrixVP._m02_m12_m22_m32 * tmp2.zzzz + tmp0;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp2.wwww + tmp0;
                tmp1.x = v.texcoord.x;
                o.texcoord.xy = tmp1.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                tmp0.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp0.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp0.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                o.texcoord1.xyz = tmp0.www * tmp0.xyz;
                o.texcoord5 = float4(0.0, 0.0, 0.0, 0.0);
                o.texcoord6 = float4(0.0, 0.0, 0.0, 0.0);
                return o;
			}
			// Keywords: DIRECTIONAL
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                float4 tmp5;
                float4 tmp6;
                float4 tmp7;
                float4 tmp8;
                float4 tmp9;
                float4 tmp10;
                float4 tmp11;
                tmp0.xyz = _WorldSpaceCameraPos - inp.texcoord2.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp0.xyz;
                tmp2 = tex2D(_BaseMap, inp.texcoord.xy);
                tmp3.xyz = tmp2.xyz * _Color.xyz;
                tmp1.w = unity_ProbeVolumeParams.x == 1.0;
                if (tmp1.w) {
                    tmp1.w = unity_ProbeVolumeParams.y == 1.0;
                    tmp4.xyz = inp.texcoord2.yyy * unity_ProbeVolumeWorldToObject._m01_m11_m21;
                    tmp4.xyz = unity_ProbeVolumeWorldToObject._m00_m10_m20 * inp.texcoord2.xxx + tmp4.xyz;
                    tmp4.xyz = unity_ProbeVolumeWorldToObject._m02_m12_m22 * inp.texcoord2.zzz + tmp4.xyz;
                    tmp4.xyz = tmp4.xyz + unity_ProbeVolumeWorldToObject._m03_m13_m23;
                    tmp4.xyz = tmp1.www ? tmp4.xyz : inp.texcoord2.xyz;
                    tmp4.xyz = tmp4.xyz - unity_ProbeVolumeMin;
                    tmp4.yzw = tmp4.xyz * unity_ProbeVolumeSizeInv;
                    tmp1.w = tmp4.y * 0.25 + 0.75;
                    tmp2.w = unity_ProbeVolumeParams.z * 0.5 + 0.75;
                    tmp4.x = max(tmp1.w, tmp2.w);
                    tmp4 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp4.xzw);
                } else {
                    tmp4 = float4(1.0, 1.0, 1.0, 1.0);
                }
                tmp1.w = saturate(dot(tmp4, unity_OcclusionMaskSelector));
                tmp2.w = 1.0 - _Glossiness;
                tmp3.w = dot(-tmp1.xyz, inp.texcoord1.xyz);
                tmp3.w = tmp3.w + tmp3.w;
                tmp4.xyz = inp.texcoord1.xyz * -tmp3.www + -tmp1.xyz;
                tmp5.xyz = tmp1.www * _LightColor0.xyz;
                tmp1.w = unity_SpecCube0_ProbePosition.w > 0.0;
                if (tmp1.w) {
                    tmp1.w = dot(tmp4.xyz, tmp4.xyz);
                    tmp1.w = rsqrt(tmp1.w);
                    tmp6.xyz = tmp1.www * tmp4.xyz;
                    tmp7.xyz = unity_SpecCube0_BoxMax.xyz - inp.texcoord2.xyz;
                    tmp7.xyz = tmp7.xyz / tmp6.xyz;
                    tmp8.xyz = unity_SpecCube0_BoxMin.xyz - inp.texcoord2.xyz;
                    tmp8.xyz = tmp8.xyz / tmp6.xyz;
                    tmp9.xyz = tmp6.xyz > float3(0.0, 0.0, 0.0);
                    tmp7.xyz = tmp9.xyz ? tmp7.xyz : tmp8.xyz;
                    tmp1.w = min(tmp7.y, tmp7.x);
                    tmp1.w = min(tmp7.z, tmp1.w);
                    tmp7.xyz = inp.texcoord2.xyz - unity_SpecCube0_ProbePosition.xyz;
                    tmp6.xyz = tmp6.xyz * tmp1.www + tmp7.xyz;
                } else {
                    tmp6.xyz = tmp4.xyz;
                }
                tmp1.w = -tmp2.w * 0.7 + 1.7;
                tmp1.w = tmp1.w * tmp2.w;
                tmp1.w = tmp1.w * 6.0;
                tmp6 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp6.xyz, tmp1.w));
                tmp3.w = tmp6.w - 1.0;
                tmp3.w = unity_SpecCube0_HDR.w * tmp3.w + 1.0;
                tmp3.w = tmp3.w * unity_SpecCube0_HDR.x;
                tmp7.xyz = tmp6.xyz * tmp3.www;
                tmp4.w = unity_SpecCube0_BoxMin.w < 0.99999;
                if (tmp4.w) {
                    tmp4.w = unity_SpecCube1_ProbePosition.w > 0.0;
                    if (tmp4.w) {
                        tmp4.w = dot(tmp4.xyz, tmp4.xyz);
                        tmp4.w = rsqrt(tmp4.w);
                        tmp8.xyz = tmp4.www * tmp4.xyz;
                        tmp9.xyz = unity_SpecCube1_BoxMax.xyz - inp.texcoord2.xyz;
                        tmp9.xyz = tmp9.xyz / tmp8.xyz;
                        tmp10.xyz = unity_SpecCube1_BoxMin.xyz - inp.texcoord2.xyz;
                        tmp10.xyz = tmp10.xyz / tmp8.xyz;
                        tmp11.xyz = tmp8.xyz > float3(0.0, 0.0, 0.0);
                        tmp9.xyz = tmp11.xyz ? tmp9.xyz : tmp10.xyz;
                        tmp4.w = min(tmp9.y, tmp9.x);
                        tmp4.w = min(tmp9.z, tmp4.w);
                        tmp9.xyz = inp.texcoord2.xyz - unity_SpecCube1_ProbePosition.xyz;
                        tmp4.xyz = tmp8.xyz * tmp4.www + tmp9.xyz;
                    }
                    tmp4 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp4.xyz, tmp1.w));
                    tmp1.w = tmp4.w - 1.0;
                    tmp1.w = unity_SpecCube1_HDR.w * tmp1.w + 1.0;
                    tmp1.w = tmp1.w * unity_SpecCube1_HDR.x;
                    tmp4.xyz = tmp4.xyz * tmp1.www;
                    tmp6.xyz = tmp3.www * tmp6.xyz + -tmp4.xyz;
                    tmp7.xyz = unity_SpecCube0_BoxMin.www * tmp6.xyz + tmp4.xyz;
                }
                tmp1.w = dot(inp.texcoord1.xyz, inp.texcoord1.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp4.xyz = tmp1.www * inp.texcoord1.xyz;
                tmp2.xyz = tmp2.xyz * _Color.xyz + float3(-0.2209163, -0.2209163, -0.2209163);
                tmp2.xyz = _Metallic.xxx * tmp2.xyz + float3(0.2209163, 0.2209163, 0.2209163);
                tmp1.w = -_Metallic * 0.7790837 + 0.7790837;
                tmp3.xyz = tmp1.www * tmp3.xyz;
                tmp0.xyz = tmp0.xyz * tmp0.www + _WorldSpaceLightPos0.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = max(tmp0.w, 0.001);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp0.w = dot(tmp4.xyz, tmp1.xyz);
                tmp1.x = saturate(dot(tmp4.xyz, _WorldSpaceLightPos0.xyz));
                tmp1.y = saturate(dot(tmp4.xyz, tmp0.xyz));
                tmp0.x = saturate(dot(_WorldSpaceLightPos0.xyz, tmp0.xyz));
                tmp0.y = tmp0.x * tmp0.x;
                tmp0.y = dot(tmp0.xy, tmp2.xy);
                tmp0.y = tmp0.y - 0.5;
                tmp0.z = 1.0 - tmp1.x;
                tmp1.z = tmp0.z * tmp0.z;
                tmp1.z = tmp1.z * tmp1.z;
                tmp0.z = tmp0.z * tmp1.z;
                tmp0.z = tmp0.y * tmp0.z + 1.0;
                tmp1.z = 1.0 - abs(tmp0.w);
                tmp3.w = tmp1.z * tmp1.z;
                tmp3.w = tmp3.w * tmp3.w;
                tmp1.z = tmp1.z * tmp3.w;
                tmp0.y = tmp0.y * tmp1.z + 1.0;
                tmp0.y = tmp0.y * tmp0.z;
                tmp0.z = tmp2.w * tmp2.w;
                tmp0.z = max(tmp0.z, 0.002);
                tmp3.w = 1.0 - tmp0.z;
                tmp4.x = abs(tmp0.w) * tmp3.w + tmp0.z;
                tmp3.w = tmp1.x * tmp3.w + tmp0.z;
                tmp0.w = abs(tmp0.w) * tmp3.w;
                tmp0.w = tmp1.x * tmp4.x + tmp0.w;
                tmp0.w = tmp0.w + 0.00001;
                tmp0.w = 0.5 / tmp0.w;
                tmp3.w = tmp0.z * tmp0.z;
                tmp4.x = tmp1.y * tmp3.w + -tmp1.y;
                tmp1.y = tmp4.x * tmp1.y + 1.0;
                tmp3.w = tmp3.w * 0.3183099;
                tmp1.y = tmp1.y * tmp1.y + 0.0000001;
                tmp1.y = tmp3.w / tmp1.y;
                tmp0.w = tmp0.w * tmp1.y;
                tmp0.zw = tmp0.zw * float2(0.28, 3.141593);
                tmp0.w = max(tmp0.w, 0.0001);
                tmp0.w = sqrt(tmp0.w);
                tmp0.yw = tmp1.xx * tmp0.yw;
                tmp0.z = -tmp0.z * tmp2.w + 1.0;
                tmp1.x = dot(tmp2.xyz, tmp2.xyz);
                tmp1.x = tmp1.x != 0.0;
                tmp1.x = tmp1.x ? 1.0 : 0.0;
                tmp0.w = tmp0.w * tmp1.x;
                tmp1.x = _Glossiness - tmp1.w;
                tmp1.x = saturate(tmp1.x + 1.0);
                tmp4.xyz = tmp0.yyy * tmp5.xyz;
                tmp5.xyz = tmp5.xyz * tmp0.www;
                tmp0.x = 1.0 - tmp0.x;
                tmp0.y = tmp0.x * tmp0.x;
                tmp0.y = tmp0.y * tmp0.y;
                tmp0.x = tmp0.x * tmp0.y;
                tmp6.xyz = float3(1.0, 1.0, 1.0) - tmp2.xyz;
                tmp0.xyw = tmp6.xyz * tmp0.xxx + tmp2.xyz;
                tmp0.xyw = tmp0.xyw * tmp5.xyz;
                tmp0.xyw = tmp3.xyz * tmp4.xyz + tmp0.xyw;
                tmp3.xyz = tmp7.xyz * tmp0.zzz;
                tmp1.xyw = tmp1.xxx - tmp2.xyz;
                tmp1.xyz = tmp1.zzz * tmp1.xyw + tmp2.xyz;
                o.sv_target.xyz = tmp3.xyz * tmp1.xyz + tmp0.xyw;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDHLSL
		}
	}
	Fallback "Diffuse"
}