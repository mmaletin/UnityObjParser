
namespace Obj
{
    using System.Collections.Generic;
    using UnityEngine;

    public class MeshData {

        public string name;
        public string group;

        public Vector3[] vertices;
        public Vector2[] uvs;
        public Vector3[] normals;

        public bool hasUVs;
        public bool hasNormals;

        public List<List<int>> triangles;

        public List<string> materialNames;

        public Mesh ToMesh(bool needsTangents = true)
        {
            var result = new Mesh()
            {
                name = name ?? group,
                indexFormat = vertices.Length >= 65536 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16
            };

            result.vertices = vertices;
            if (hasUVs) result.uv = uvs;
            if (hasNormals) result.normals = normals;

            result.subMeshCount = triangles.Count;

            for (int i = 0; i < triangles.Count; i++)
            {
                result.SetTriangles(triangles[i], i, false);
            }

            result.RecalculateBounds();

            if (!hasNormals) result.RecalculateNormals();
            if (hasUVs && needsTangents) result.RecalculateTangents();

            return result;
        }
    }
}