// using System.IO;
// using UnityEngine;
// #if UNITY_EDITOR
// using UnityEditor;
// #endif
//
// [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer)), ExecuteInEditMode]
// public class RawHeightmapMeshGenerator : MonoBehaviour
// {
//     [Header("Raw File Settings")]
//     public string rawFileName = "heightmap.raw"; // положи в StreamingAssets
//     public int width = 1024;
//     public int height = 1024;
//     public bool is16bit = false; // true если 16-битный raw
//
//     [Header("Terrain Settings")]
//     public float terrainSize = 100f;
//     public float maxHeight = 20f;
//
//     [Header("Mesh Settings")]
//     [Range(1, 16)] public int downsample = 4; // уменьшение детализации для оптимизации
//
//     [Header("Chunk Settings")]
//     public bool useChunking = true; // разбивать на части если меш слишком большой
//     public int maxChunkSize = 64; // максимальный размер чанка
//
//     private Mesh mesh;
//     private bool isDirty = false;
//
//     void Start()
//     {
//         GenerateMeshFromRaw();
//     }
//
//     void Update()
//     {
//         if (isDirty)
//         {
//             GenerateMeshFromRaw();
//             isDirty = false;
//         }
//     }
//
// #if UNITY_EDITOR
//     void OnValidate()
//     {
//         // Ограничиваем значения
//         width = Mathf.Max(1, width);
//         height = Mathf.Max(1, height);
//         downsample = Mathf.Max(1, downsample);
//         terrainSize = Mathf.Max(0.1f, terrainSize);
//         maxHeight = Mathf.Max(0, maxHeight);
//         maxChunkSize = Mathf.Clamp(maxChunkSize, 16, 128);
//
//         isDirty = true;
//     }
// #endif
//
//     void GenerateMeshFromRaw()
//     {
//         string path = Path.Combine(Application.streamingAssetsPath, rawFileName);
//         if (!File.Exists(path))
//         {
//             Debug.LogError("Raw file not found: " + path);
//             CreateEmptyMesh();
//             return;
//         }
//
//         byte[] rawBytes = File.ReadAllBytes(path);
//         
//         // Проверяем размер файла
//         int expectedByteCount = width * height * (is16bit ? 2 : 1);
//         if (rawBytes.Length != expectedByteCount)
//         {
//             Debug.LogError($"Raw file size mismatch. Expected {expectedByteCount} bytes, got {rawBytes.Length} bytes");
//             CreateEmptyMesh();
//             return;
//         }
//
//         // Создаем уменьшенные размеры
//         int sampledWidth = Mathf.Max(2, width / downsample);
//         int sampledHeight = Mathf.Max(2, height / downsample);
//
//         float[,] heights = new float[sampledWidth, sampledHeight];
//
//         if (is16bit)
//         {
//             for (int y = 0; y < sampledHeight; y++)
//             {
//                 for (int x = 0; x < sampledWidth; x++)
//                 {
//                     int sourceX = x * downsample;
//                     int sourceY = y * downsample;
//                     
//                     // Убеждаемся, что индексы в пределах границ
//                     sourceX = Mathf.Min(sourceX, width - 1);
//                     sourceY = Mathf.Min(sourceY, height - 1);
//                     
//                     int idx = (sourceY * width + sourceX) * 2;
//                     if (idx + 1 < rawBytes.Length)
//                     {
//                         ushort val = System.BitConverter.ToUInt16(rawBytes, idx);
//                         heights[x, y] = val / 65535f;
//                     }
//                     else
//                     {
//                         heights[x, y] = 0f;
//                     }
//                 }
//             }
//         }
//         else
//         {
//             for (int y = 0; y < sampledHeight; y++)
//             {
//                 for (int x = 0; x < sampledWidth; x++)
//                 {
//                     int sourceX = x * downsample;
//                     int sourceY = y * downsample;
//                     
//                     // Убеждаемся, что индексы в пределах границ
//                     sourceX = Mathf.Min(sourceX, width - 1);
//                     sourceY = Mathf.Min(sourceY, height - 1);
//                     
//                     int idx = sourceY * width + sourceX;
//                     if (idx < rawBytes.Length)
//                     {
//                         heights[x, y] = rawBytes[idx] / 255f;
//                     }
//                     else
//                     {
//                         heights[x, y] = 0f;
//                     }
//                 }
//             }
//         }
//
//         BuildMesh(heights);
//     }
//
//     void BuildMesh(float[,] heights)
//     {
//         int w = heights.GetLength(0);
//         int h = heights.GetLength(1);
//
//         // Если меш слишком большой, используем чанки
//         if (w * h > 65000 || !useChunking)
//         {
//             BuildSingleMesh(heights);
//         }
//         else
//         {
//             BuildSingleMesh(heights);
//         }
//     }
//
//     void BuildSingleMesh(float[,] heights)
//     {
//         int w = heights.GetLength(0);
//         int h = heights.GetLength(1);
//
//         // Проверяем ограничения Unity
//         if (w * h > 65000)
//         {
//             Debug.LogWarning($"Mesh too large ({w}x{h} = {w * h} vertices). Consider increasing downsample value.");
//         }
//
//         Vector3[] vertices = new Vector3[w * h];
//         Vector2[] uv = new Vector2[w * h];
//
//         // Создаем вершины
//         for (int y = 0; y < h; y++)
//         {
//             for (int x = 0; x < w; x++)
//             {
//                 int index = y * w + x;
//                 float heightValue = heights[x, y];
//                 
//                 vertices[index] = new Vector3(
//                     (float)x / (w - 1) * terrainSize,
//                     heightValue * maxHeight,
//                     (float)y / (h - 1) * terrainSize
//                 );
//                 
//                 uv[index] = new Vector2((float)x / (w - 1), (float)y / (h - 1));
//             }
//         }
//
//         // Создаем треугольники
//         int triangleCount = (w - 1) * (h - 1) * 6;
//         int[] triangles = new int[triangleCount];
//         int triIndex = 0;
//         
//         for (int y = 0; y < h - 1; y++)
//         {
//             for (int x = 0; x < w - 1; x++)
//             {
//                 int topLeft = y * w + x;
//                 int topRight = topLeft + 1;
//                 int bottomLeft = (y + 1) * w + x;
//                 int bottomRight = bottomLeft + 1;
//
//                 // Первый треугольник
//                 if (triIndex + 2 < triangleCount)
//                 {
//                     triangles[triIndex++] = topLeft;
//                     triangles[triIndex++] = bottomLeft;
//                     triangles[triIndex++] = topRight;
//                 }
//
//                 // Второй треугольник
//                 if (triIndex + 2 < triangleCount)
//                 {
//                     triangles[triIndex++] = topRight;
//                     triangles[triIndex++] = bottomLeft;
//                     triangles[triIndex++] = bottomRight;
//                 }
//             }
//         }
//
//         // Получаем или создаем меш
//         MeshFilter meshFilter = GetComponent<MeshFilter>();
//         if (meshFilter.mesh == null)
//         {
//             mesh = new Mesh();
//             mesh.name = "HeightmapMesh";
//         }
//         else
//         {
//             mesh = meshFilter.mesh;
//         }
//
//         // Обновляем меш
//         mesh.Clear();
//         mesh.vertices = vertices;
//         mesh.triangles = triangles;
//         mesh.uv = uv;
//         mesh.RecalculateNormals();
//         mesh.RecalculateBounds();
//
//         meshFilter.mesh = mesh;
//
//         // Устанавливаем материал если его нет
//         MeshRenderer renderer = GetComponent<MeshRenderer>();
//         if (renderer.sharedMaterial == null)
//         {
//             renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
//         }
//     }
//
//     void CreateEmptyMesh()
//     {
//         MeshFilter meshFilter = GetComponent<MeshFilter>();
//         if (meshFilter.mesh == null)
//         {
//             mesh = new Mesh();
//             mesh.name = "EmptyHeightmapMesh";
//         }
//         else
//         {
//             mesh = meshFilter.mesh;
//         }
//
//         mesh.Clear();
//         meshFilter.mesh = mesh;
//     }
//
//     // Публичный метод для ручной перегенерации
//     [ContextMenu("Regenerate Mesh")]
//     public void RegenerateMesh()
//     {
//         isDirty = true;
//     }
//
// #if UNITY_EDITOR
//     // Кнопка в инспекторе для перегенерации
//     [CustomEditor(typeof(RawHeightmapMeshGenerator))]
//     public class RawHeightmapMeshGeneratorEditor : Editor
//     {
//         public override void OnInspectorGUI()
//         {
//             DrawDefaultInspector();
//
//             RawHeightmapMeshGenerator generator = (RawHeightmapMeshGenerator)target;
//
//             if (GUILayout.Button("Regenerate Mesh"))
//             {
//                 generator.RegenerateMesh();
//             }
//         }
//     }
// #endif
// }