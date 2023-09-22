Shader "ShaderTest/HeatMap"
{
    Properties
    {
        [NoScaleOffset]
        _HeatMapTex("Texture",2D) = "white"{}
        _Diffuse ("Diffuse", Color) = (1, 1, 1, 1)
        _Specular ("Specular", Color) = (1, 1, 1, 1)
        _Gloss ("Gloss", Range(8.0, 256)) = 20
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        Cull back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ BEGIN_RENDER
            #include "Lighting.cginc"

            fixed4 _Diffuse;
            fixed4 _Specular;
            float _Gloss;

            sampler2D _HeatMapTex;
            half _Alpha;
            uniform int _FactorCount = 0;
            uniform float3 _Factors[100]; //采样点的坐标
            uniform float3 _FactorsProperties[400]; //对应的属性 半径、敏感度、温度影响度

            struct a2v
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : POSITION;
                fixed3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD0;
            };

            v2f vert(a2v input)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(input.vertex);
                o.worldNormal = mul(input.normal, (float3x3)unity_WorldToObject);
                o.worldPos = mul(unity_ObjectToWorld, input.vertex).xyz;
                return o;
            }

            half4 frag(v2f input):SV_Target
            {
                #if BEGIN_RENDER
                    half heat = 0;
				    for( int i = 0 ; i < _FactorCount;i++ )
				    {
					    half dis = distance(input.worldPos,_Factors[i].xyz); //计算两者的距离
					    float radius = _FactorsProperties[i].x;
					    float intensity = _FactorsProperties[i].y;
					    float temperatureFacator = _FactorsProperties[i].z;
					    half ratio = 1 - saturate(dis/radius);
					    heat += /*intensity * */ratio * temperatureFacator;
					    heat = clamp(heat,0,1);
				    }
				    half4 color = tex2D(_HeatMapTex,fixed2(heat,0.5));
				    return color;
                #else
                    // Get ambient term
                    fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;

                    fixed3 worldNormal = normalize(input.worldNormal);
                    fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);

                    // Compute diffuse term
                    fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * max(0, dot(worldNormal, worldLightDir));

                    // Get the view direction in world space
                    fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - input.worldPos.xyz);
                    // Get the half direction in world space
                    fixed3 halfDir = normalize(worldLightDir + viewDir);
                    // Compute specular term
                    fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(max(0, dot(worldNormal, halfDir)), _Gloss);

                    return fixed4(ambient + diffuse + specular, 1.0);
                #endif
            }
            ENDCG
        }
    }
}