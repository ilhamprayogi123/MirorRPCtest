Shader "Interactor/Standart Metallic Outlines"
{
	Properties
	{
        _Color("Color", Color) = (1,1,1,1)
		[NoScaleOffset]
        _MainTex("Albedo", 2D) = "white" {}
		[NoScaleOffset]
		_MetallicGlossMap("Metallic Gloss", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		[NoScaleOffset]
		[Normal]
		_Normal ("Normal", 2D) = "bump" {}
		_NormalAmount ("Normal Amount", Range(-2,2)) = 1
		[NoScaleOffset]
		_AO ("Ambient Occlusion", 2D) = "white" {}
		_AOAmount ("Ambient Occlusion Amount", Range(0,1)) = 1

		_FirstOutlineColor("First Outline Color", Color) = (1,0,0,0.5)
		_FirstOutlineWidth("First Outlines Width", Range(0.0, 2.0)) = 0.15

		_SecondOutlineColor("Second Outline Color", Color) = (0,0,1,1)
		_SecondOutlineWidth("Second Outlines Width", Range(0.0, 2.0)) = 0.025

		_Angle("Switch shader on angle", Range(0.0, 180.0)) = 89
		[HideInInspector] _Fold("__fld", Float) = 1.0
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	struct appdata 
	{
		float4 vertex : POSITION;
		float4 normal : NORMAL;
	};

	uniform float4 _FirstOutlineColor;
	uniform float _FirstOutlineWidth;
	uniform float4 _SecondOutlineColor;
	uniform float _SecondOutlineWidth;
	uniform float _Angle;

	
	
	uniform float4 _Color;
	uniform sampler2D _MainTex;
	uniform sampler2D _MetallicGlossMap;
	uniform sampler2D _Normal;
	uniform sampler2D _AO;
	half _Glossiness;
	half _Metallic;
	half _NormalAmount;
	half _AOAmount;

	ENDCG

	SubShader
	{
		//First outline
		Pass
		{
			Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Back
			CGPROGRAM

			struct v2f 
			{
				float4 pos : SV_POSITION;
			};

			#pragma vertex vert
			#pragma fragment frag

			v2f vert(appdata v) 
			{
				appdata original = v;

				float3 scaleDir = normalize(v.vertex.xyz - float4(0,0,0,1));
				//This shader consists of 2 ways of generating outline that are dynamically switched based on demiliter angle
				//If vertex normal is pointed away from object origin then custom outline generation is used (based on scaling along the origin-vertex vector)
				//Otherwise the old-school normal vector scaling is used
				//This way prevents weird artifacts from being created when using either of the methods
				if (degrees(acos(dot(scaleDir.xyz, v.normal.xyz))) > _Angle) 
				{
					v.vertex.xyz += normalize(v.normal.xyz) * _FirstOutlineWidth;
				}
				else 
				{
					v.vertex.xyz += scaleDir * _FirstOutlineWidth;
				}

				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				return _FirstOutlineColor;
			}

			ENDCG
		}
		//Second outline
		Pass
		{
			Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Back
			CGPROGRAM

			struct v2f 
			{
				float4 pos : SV_POSITION;
			};

			#pragma vertex vert
			#pragma fragment frag

			v2f vert(appdata v) 
			{
				appdata original = v;
				float3 scaleDir = normalize(v.vertex.xyz - float4(0,0,0,1));
				//This shader consists of 2 ways of generating outline that are dynamically switched based on demiliter angle
				//If vertex normal is pointed away from object origin then custom outline generation is used (based on scaling along the origin-vertex vector)
				//Otherwise the old-school normal vector scaling is used
				//This way prevents weird artifacts from being created when using either of the methods
				if (degrees(acos(dot(scaleDir.xyz, v.normal.xyz))) > _Angle) 
				{
					v.vertex.xyz += normalize(v.normal.xyz) * _SecondOutlineWidth;
				}
				else 
				{
					v.vertex.xyz += scaleDir * _SecondOutlineWidth;
				}

				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				return _SecondOutlineColor;
			}

			ENDCG
		}
		//Surface shader
		Tags{ "Queue" = "Transparent" }

		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard fullforwardshadows

		struct Input 
		{
			float2 uv_MainTex;
			float4 color : COLOR;
		};

		void surf(Input IN, inout SurfaceOutputStandard   o) 
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			fixed4 cSpec = tex2D(_MetallicGlossMap, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Normal = UnpackNormal(tex2D(_Normal, IN.uv_MainTex) * _NormalAmount);
			o.Occlusion = saturate(tex2D(_AO, IN.uv_MainTex).r + (1-_AOAmount));
			o.Metallic = cSpec.rgb * _Metallic; 
            o.Smoothness = cSpec.a * _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "razz.OutlineShaderGUI"
}