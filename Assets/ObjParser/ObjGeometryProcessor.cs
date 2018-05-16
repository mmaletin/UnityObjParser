
namespace Obj
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using UnityEngine;

    public class ObjGeometryProcessor {

        private const NumberStyles floatStyle = NumberStyles.Integer | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent;

        private string name;
        private string group;

        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector3> uvs = new List<Vector3>();
        private List<Vector3> normals = new List<Vector3>();

        private Dictionary<string, List<int>> triangles = new Dictionary<string, List<int>>();
        private string materialName = "Material";

        private Dictionary<ObjParserVertexData, int> splitVertices = new Dictionary<ObjParserVertexData, int>();
        private List<ObjParserVertexData> triangulationBuffer = new List<ObjParserVertexData>();

        private List<string> vertexSplit = new List<string>();

        private bool processingFaces = false;

        public static ModelData ProcessStream(StreamReader streamReader, float scale, CancellationToken? ct = null)
        {
            var geometryProcessor = new ObjGeometryProcessor();
            return geometryProcessor.GetMeshes(streamReader, scale, ct);
        }

        public ModelData GetMeshes(StreamReader streamReader, float scale, CancellationToken? ct = null)
        {
            var modelData = new ModelData();

            ClearAll();

            var split = new List<string>();

            bool hasMaterials = false;

            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();

                line.BufferSplit(split, ' ');

                if (split.Count == 0) continue;

                var type = split[0];

                switch (type)
                {
                    case "o":
                        name = split[1];
                        break;
                    case "g":
                        group = split[1];
                        break;
                    case "v":
                        if (processingFaces)
                        {
                            FinishMeshProcessing(modelData.meshes);
                            hasMaterials = false;
                        }

                        vertices.Add(new Vector3(
                            -float.Parse(split[1], floatStyle, CultureInfo.InvariantCulture) * scale,
                            float.Parse(split[2], floatStyle, CultureInfo.InvariantCulture) * scale,
                            float.Parse(split[3], floatStyle, CultureInfo.InvariantCulture) * scale));
                        break;
                    case "vt":
                        uvs.Add(new Vector3(
                            float.Parse(split[1], floatStyle, CultureInfo.InvariantCulture),
                            float.Parse(split[2], floatStyle, CultureInfo.InvariantCulture),
                            split.Count >= 4 ? float.Parse(split[3], floatStyle, CultureInfo.InvariantCulture) : 0));
                        break;
                    case "vn":
                        normals.Add(new Vector3(
                            -float.Parse(split[1], floatStyle, CultureInfo.InvariantCulture),
                            float.Parse(split[2], floatStyle, CultureInfo.InvariantCulture),
                            float.Parse(split[3], floatStyle, CultureInfo.InvariantCulture)));
                        break;
                    case "f":
                        processingFaces = true;
                        if (!hasMaterials) // For meshes with no materials
                        {
                            triangles.Add(materialName, new List<int>());
                            hasMaterials = true;
                        }
                        AddFace(split);
                        break;
                    case "usemtl":
                        materialName = split[1];
                        if (!triangles.ContainsKey(materialName))
                            triangles.Add(materialName, new List<int>());
                        hasMaterials = true;
                        break;
                    case "mtllib":
                        modelData.materialsLibraryName = line.Substring(split[0].Length).TrimStart();
                        break;
                    default:
                        break;
                }

                if (ct?.IsCancellationRequested ?? false)
                {
                    streamReader.Close();
                    ct?.ThrowIfCancellationRequested();
                }
            }

            FinishMeshProcessing(modelData.meshes);

            streamReader.Close();

            ct?.ThrowIfCancellationRequested();

            return modelData;
        }

        private void AddFace(List<string> split)
        {
            triangulationBuffer.Clear();

            for (int i = 1; i < split.Count; i++)
            {
                split[i].BufferSplit(vertexSplit, '/', true);

                int vertex, uv, normal;

                if (!int.TryParse(vertexSplit[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out vertex))
                {
                    continue;
                }
                if (vertexSplit.Count < 2 || !int.TryParse(vertexSplit[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out uv)) uv = 1;
                if (vertexSplit.Count < 3 || !int.TryParse(vertexSplit[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out normal)) normal = 1;

                if (vertex < 0) vertex += vertices.Count + 1;
                if (uv < 0) uv += uvs.Count + 1;
                if (normal < 0) normal += normals.Count + 1;

                var vertexData = new ObjParserVertexData(vertex - 1, uv - 1, normal - 1);

                triangulationBuffer.Add(vertexData);

                if (!splitVertices.ContainsKey(vertexData))
                {
                    splitVertices.Add(vertexData, splitVertices.Count);
                }
            }

            // Triangulation

            if (triangulationBuffer.Count == 3)
            {
                triangles[materialName].Add(splitVertices[triangulationBuffer[2]]);
                triangles[materialName].Add(splitVertices[triangulationBuffer[1]]);
                triangles[materialName].Add(splitVertices[triangulationBuffer[0]]);
            }
            else if (triangulationBuffer.Count > 3) // Assuming convex poly
            {
                int trialglesCount = triangulationBuffer.Count - 2;

                for (int i = 0; i < trialglesCount; i++)
                {
                    triangles[materialName].Add(splitVertices[triangulationBuffer[2 + i]]);
                    triangles[materialName].Add(splitVertices[triangulationBuffer[1 + i]]);
                    triangles[materialName].Add(splitVertices[triangulationBuffer[0]]);
                }
            }
        }

        private void FinishMeshProcessing(List<MeshData> resultsAppendList)
        {
            if (vertices.Count == 0) return;

            var meshData = new MeshData() { name = name, group = group };

            int count = splitVertices.Count;

            meshData.vertices = new Vector3[count];
            meshData.uvs = new Vector2[count];
            meshData.normals = new Vector3[count];

            int id = 0;

            meshData.hasUVs = uvs.Count > 0;
            meshData.hasNormals = normals.Count > 0;

            foreach (var vertexData in splitVertices.Keys)
            {
                meshData.vertices[id] = vertices[vertexData.vertex];
                if (meshData.hasUVs) meshData.uvs[id] = uvs[vertexData.uv];
                if (meshData.hasNormals) meshData.normals[id] = normals[vertexData.normal];

                id++;
            }

            meshData.triangles = new List<List<int>>();
            meshData.materialNames = new List<string>();

            foreach (var pair in triangles)
            {
                meshData.materialNames.Add(pair.Key);
                meshData.triangles.Add(pair.Value);
            }

            triangles.Clear();
            splitVertices.Clear();

            processingFaces = false;

            resultsAppendList.Add(meshData);
        }

        private void ClearAll()
        {
            vertices.Clear();
            uvs.Clear();
            normals.Clear();
            triangles.Clear();
            splitVertices.Clear();
            processingFaces = false;
        }
    }
}