// Pcx - Point cloud importer & renderer for Unity
// https://github.com/keijiro/Pcx

// This file uses some code from [BA_PointCloud](https://github.com/SFraissTU/BA_PointCloud).
// 
// QuadGeoScreenSizeShader.shader
// https://github.com/SFraissTU/BA_PointCloud/blob/830a66bac5347b97a0619dcb878befaebca78539/PointCloudRenderer/Assets/Resources/Shaders/QuadGeoScreenSizeShader.shader
// 
// License
// https://github.com/SFraissTU/BA_PointCloud/blob/830a66bac5347b97a0619dcb878befaebca78539/LICENSE
// 
// BSD 2-Clause License
// 
// Copyright (c) 2017-2018, Simon Maximilian Fraiss
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

Shader "Point Cloud/Custom Point"
{
    Properties
    {
        //[HDR] _Tint("Tint", Color) = (0.5, 0.5, 0.5, 1)
        _ScreenWidth("Screen Width", Int) = 0
        _ScreenHeight("Screen Height", Int) = 0
        _PointSize("Point Size", Float) = 5.0
        _Intensity("Intensity", Float) = 0.0
        _NoiseStrength("Noise Strength", Float) = 0.0
        _NoiseScale("Noise Scale", Float) = 1.0
        _NoiseSpeed("Noise Speed", Float) = 1.0
        _GridStrength("Grid Strength", Float) = 0.0
        _GridScale("Grid Scale", Float) = 1.0
        _GridSpeed("Grid Speed", Float) = 1.0
        _GridHueShift("Grid Hue Shift", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 200

        Pass
        {
            Cull front

            CGPROGRAM

            #pragma vertex Vertex
            #pragma geometry Geometry
            #pragma fragment Fragment

            #pragma multi_compile_fog
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA

            #include "UnityCG.cginc"
            #include "Packages/jp.keijiro.pcx/Runtime/Shaders/Common.cginc"
            #include "Packages/jp.keijiro.noiseshader/Shader/ClassicNoise2D.hlsl"
            #include "Packages/jp.keijiro.noiseshader/Shader/ClassicNoise3D.hlsl"
            #include "Packages/jp.keijiro.noiseshader/Shader/SimplexNoise2D.hlsl"
            #include "Packages/jp.keijiro.noiseshader/Shader/SimplexNoise3D.hlsl"
            #include "Assets/Material/ColorspaceConversion.hlsl"

            struct VertexInput
            {
                float4 position : POSITION;
                half4 color : COLOR;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexMiddle
            {
                float4 position : SV_Position;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {
                float4 position : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            //half4 _Tint;
            int _ScreenWidth;
            int _ScreenHeight;

            half _PointSize;
            float _Intensity;
            float _NoiseStrength;
            float _NoiseScale;
            float _NoiseSpeed;
            float _GridStrength;
            float _GridScale;
            float _GridSpeed;
            float _GridHueShift;

            float3 rgb2hsv(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
            }

            VertexMiddle Vertex(VertexInput input)
            {
                float4 pos = input.position;
                half4 col = input.color;

                float4 worldPos = mul(unity_ObjectToWorld, pos);
                float worldDist= distance(_WorldSpaceCameraPos, worldPos);

            #ifdef UNITY_COLORSPACE_GAMMA
                //col *= _Tint.rgb * 2;
            #else
                //col.rgb *= LinearToGammaSpace(_Tint.rgb) * 2;
                //col.rgb = GammaToLinearSpace(col);
            #endif

                float3 noisePosA = pos.xyz * _NoiseScale + float3(_Time.y * _NoiseSpeed, 0.0, 0.0);
                pos += SimplexNoiseGrad(noisePosA) * _NoiseStrength * 0.1;

                float3 noisePosB = pos.xyz * _NoiseScale * 5 + float3(-_Time.y * _NoiseSpeed, 0.0, 0.0);
                pos += SimplexNoiseGrad(noisePosB) * _NoiseStrength * 0.1;

                VertexMiddle o;
                
                UNITY_INITIALIZE_OUTPUT(VertexMiddle, o); // set all values in the v2g o to 0.0
                UNITY_SETUP_INSTANCE_ID(input);           // setup the instanced id to be accessed
                UNITY_TRANSFER_INSTANCE_ID(input, o);     // copy instance id in the appdata input to the v2g o

                o.position = UnityObjectToClipPos(pos);

                float3 hsv = rgb2hsv(col.rgb);
                hsv.g *= 3.0; // colorfully
                hsv.b *= _Intensity;

                float flicker = lerp(0.8, 1.0, SimplexNoise(float2(0.0, _Time.y)));
                o.color.rgb = hsv2rgb(hsv) * flicker;

                float fogDist = 60.0;
                o.color.a = col.a * (1.0 - saturate(worldDist / fogDist));

                float3 gridColor = _GridStrength * 5.0;
                gridColor = rgb2hsv(gridColor);
                gridColor.r += _GridHueShift;
                gridColor.g = 0.5f;
                gridColor = hsv2rgb(gridColor);

                o.color.rgb = lerp(o.color.rgb, gridColor,
                    saturate(
                        step(frac(pos.x * _GridScale), 0.01) +
                        step(frac(pos.y * _GridScale + _Time.y * _GridSpeed), 0.01)
                    ) * (1.0 - step(_GridStrength, 0.0)
                ));

                return o;
            }

            [maxvertexcount(4)]
            void Geometry(point VertexMiddle input[1], inout TriangleStream<VertexOutput> outputStream) {
                float xsize = _PointSize / _ScreenWidth;
                float ysize = _PointSize / _ScreenHeight;

                VertexOutput out1;
                UNITY_INITIALIZE_OUTPUT(VertexOutput, out1); // set all values in the g2f o to 0.0
                UNITY_SETUP_INSTANCE_ID(input[0]);           // setup the instanced id to be accessed
                UNITY_TRANSFER_INSTANCE_ID(input[0], out1);  // copy instance id in the v2f i[0] to the g2f o
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(out1);

                out1.position = input[0].position;
                out1.color = input[0].color;
                out1.uv = float2(-1.0f, 1.0f);
                out1.position.x -= out1.position.w * xsize;
                out1.position.y += out1.position.w * ysize;

                VertexOutput out2;                           // set all values in the g2f o to 0.0
                UNITY_INITIALIZE_OUTPUT(VertexOutput, out2); // setup the instanced id to be accessed
                UNITY_SETUP_INSTANCE_ID(input[0]);           // copy instance id in the v2f i[0] to the g2f o
                UNITY_TRANSFER_INSTANCE_ID(input[0], out2);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(out2);

                out2.position = input[0].position;
                out2.color = input[0].color;
                out2.uv = float2(1.0f, 1.0f);
                out2.position.x += out2.position.w * xsize;
                out2.position.y += out2.position.w * ysize;

                VertexOutput out3;                           // set all values in the g2f o to 0.0
                UNITY_INITIALIZE_OUTPUT(VertexOutput, out3); // setup the instanced id to be accessed
                UNITY_SETUP_INSTANCE_ID(input[0]);           // copy instance id in the v2f i[0] to the g2f o
                UNITY_TRANSFER_INSTANCE_ID(input[0], out3);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(out3);

                out3.position = input[0].position;
                out3.color = input[0].color;
                out3.uv = float2(1.0f, -1.0f);
                out3.position.x += out3.position.w * xsize;
                out3.position.y -= out3.position.w * ysize;

                VertexOutput out4;                           // set all values in the g2f o to 0.0
                UNITY_INITIALIZE_OUTPUT(VertexOutput, out4); // setup the instanced id to be accessed
                UNITY_SETUP_INSTANCE_ID(input[0]);           // copy instance id in the v2f i[0] to the g2f o
                UNITY_TRANSFER_INSTANCE_ID(input[0], out4);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(out4);

                out4.position = input[0].position;
                out4.color = input[0].color;
                out4.uv = float2(-1.0f, -1.0f);
                out4.position.x -= out4.position.w * xsize;
                out4.position.y -= out4.position.w * ysize;

                outputStream.Append(out1);
                outputStream.Append(out2);
                outputStream.Append(out4);
                outputStream.Append(out3);
            }

            half4 Fragment(VertexOutput input) : SV_Target
            {
                if (input.uv.x * input.uv.x + input.uv.y * input.uv.y > 1) {
                    discard;
                }
                return half4(input.color);
            }

            ENDCG
        }
    }
}
