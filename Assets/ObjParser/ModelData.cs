
namespace Obj
{
    using System.Collections.Generic;
    using UnityEngine;

    public class ModelData {

        public List<MeshData> meshes = new List<MeshData>();
        public Dictionary<string, Material> materials = new Dictionary<string, Material>();

        public string materialsLibraryName;
    }
}