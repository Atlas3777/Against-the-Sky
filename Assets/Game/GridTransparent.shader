Shader "Custom/HexagonalGrid"
{
    Properties
    {
        _LineColor ("Line Color", Color) = (1,1,1,1)
        _LineWidth ("Line Width", Range(0.001, 0.5)) = 0.05
        _GridSize ("Grid Size", Range(0.1, 10)) = 2.0
        _Smoothness ("Smoothness", Range(0.001, 0.1)) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _LineColor;
            float _LineWidth;
            float _GridSize;
            float _Smoothness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * 10;
                return o;
            }

            // Функция для вычисления расстояния до края шестиугольника
            float hexDistance(float2 p, float radius)
            {
                p = abs(p);
                return max(dot(p, float2(0.866025, 0.5)), p.y);
            }

            float hexGrid(float2 uv, float size)
            {
                // Размеры шестиугольника
                float hexWidth = size * 2.0;
                float hexHeight = size * sqrt(3.0);
                
                // Базовые координаты сетки
                float2 coord = float2(
                    uv.x / (hexWidth * 0.75),  // 0.75 для правильного шага
                    uv.y / hexHeight
                );
                
                // Определяем базовую ячейку
                float2 grid = floor(coord);
                
                // Смещение для четных/нечетных колонок
                float offsetY = (fmod(grid.x, 2.0) == 0.0) ? 0.0 : 0.5;
                float2 center = float2(
                    grid.x * (hexWidth * 0.75),
                    (grid.y + offsetY) * hexHeight
                );
                
                // Находим ближайшие соседние центры
                float2 centers[3];
                centers[0] = center;
                
                // Соседи сверху и снизу в той же колонке
                centers[1] = center + float2(0, hexHeight);
                centers[2] = center - float2(0, hexHeight);
                
                // Проверяем диагональных соседей
                if (fmod(grid.x, 2.0) == 0.0) {
                    // Для четных колонок
                    centers[1] = center + float2(hexWidth * 0.75, hexHeight * 0.5);
                    centers[2] = center + float2(hexWidth * 0.75, -hexHeight * 0.5);
                } else {
                    // Для нечетных колонок
                    centers[1] = center + float2(hexWidth * 0.75, -hexHeight * 0.5);
                    centers[2] = center + float2(hexWidth * 0.75, hexHeight * 0.5);
                }
                
                // Находим минимальное расстояние
                float minDist = hexDistance(uv - centers[0], size);
                for (int i = 1; i < 3; i++) {
                    float dist = hexDistance(uv - centers[i], size);
                    minDist = min(minDist, dist);
                }
                
                return minDist;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = (i.uv - float2(5, 5)) * 2;
                float gridValue = hexGrid(uv, _GridSize);
                
                // Создаем линии
                float lineE = smoothstep(_GridSize - _LineWidth - _Smoothness, 
                                      _GridSize - _LineWidth, 
                                      gridValue);
                lineE *= (1.0 - smoothstep(_GridSize - _LineWidth, 
                                        _GridSize - _LineWidth + _Smoothness, 
                                        gridValue));
                
                fixed4 col = _LineColor;
                col.a *= lineE;
                return col;
            }
            ENDCG
        }
    }
}