Shader "Hidden/Daydream/Gizmo"
{
  Properties
  {
    _MainTex("Texture", 2D) = "white" {}
    _Color("Color", Color) = (1.0, 0.0, 0.0, 1.0)
  }
    SubShader
    {
      Tags { "ForceSupported" = "True" "Queue" = "Transparent" }
          Blend SrcAlpha OneMinusSrcAlpha
          ZWrite Off Cull Off Fog { Mode Off }
      Pass
      {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        // make fog work
        #pragma multi_compile_fog

        #include "UnityCG.cginc"

        struct appdata {
          float4 vertex : POSITION;
          float2 uv : TEXCOORD0;
        };

        struct v2f {
          float2 uv : TEXCOORD0;
          float2 fade : TEXCOORD1;
          float4 vertex : SV_POSITION;
        };

        half4 _Color;
        sampler2D _MainTex;
        float4 _MainTex_ST;
        float4x4 _world;

        v2f vert(appdata v) {
          v2f o;
          float4x4 mv = mul(UNITY_MATRIX_V, _world);
          float4 viewPos = mul(mv, v.vertex);
          o.vertex = mul(UNITY_MATRIX_P, viewPos);
          o.uv = TRANSFORM_TEX(v.uv, _MainTex);
          o.fade = clamp(smoothstep(_ProjectionParams.y, 2.0, -viewPos.z), 0.0, 1.0);
          return o;
        }

        fixed4 frag(v2f i) : SV_Target
        {
          fixed4 col = tex2D(_MainTex, i.uv);
          col.a = col.a*i.fade.x;
          return col*_Color;
        }
        ENDCG
      }
    }
}
