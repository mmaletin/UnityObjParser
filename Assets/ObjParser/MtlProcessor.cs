
namespace Obj
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using UnityEngine;

    public static class MtlProcessor {

        private const NumberStyles floatStyle = NumberStyles.Integer | NumberStyles.AllowDecimalPoint;

        private static readonly Dictionary<string, Texture2D> texturesCache = new Dictionary<string, Texture2D>();

        public static Dictionary<string, Material> ProcessMtl(StreamReader streamReader, Material material, Material transparentMaterial, string directoryName)
        {
            var materials = new Dictionary<string, Material>();

            if (streamReader == null)
            {
                materials.Add("Material", material);
                return materials;
            }

            var split = new List<string>();

            Material current = null;
            bool isTransparent = false;

            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();

                line.BufferSplit(split, ' ');

                if (split.Count == 0) continue;

                var type = split[0].Replace("\t", "");

                switch (type)
                {
                    case "newmtl":
                        current = new Material(material);
                        current.name = split[1];
                        current.SetFloat("_Glossiness", 0);
                        materials.Add(current.name, current);
                        break;
                    case "Kd":
                        AssignColorProperty(current, "_Color", split);
                        break;
                    case "Ks":
                        AssignColorProperty(current, "_SpecColor", split);
                        break;
                    case "map_Kd":
                        AssignTexture(current, "_MainTex", GetTexturePath(line), directoryName);
                        break;
                    case "bump":
                    case "map_bump":
                        AssignTexture(current, "_BumpMap", GetTexturePath(line), directoryName);
                        break;
                    case "map_Ks":
                        AssignTexture(current, "_SpecGlossMap", GetTexturePath(line), directoryName);
                        break;
                    case "Tr":
                        if (!isTransparent)
                        {
                            float transparency = 1 - float.Parse(split[1], floatStyle, CultureInfo.InvariantCulture);
                            ApplyTransparency(transparentMaterial, ref current, ref isTransparent, materials, transparency);
                        }
                        break;
                    case "d":
                        if (!isTransparent)
                        {
                            float transparency = float.Parse(split[1], floatStyle, CultureInfo.InvariantCulture);
                            ApplyTransparency(transparentMaterial, ref current, ref isTransparent, materials, transparency);
                        }
                        break;
                    default:
                        break;
                }
            }

            return materials;
        }

        public static void ClearTexturesCache()
        {
            texturesCache.Clear();
        }

        private static void ApplyTransparency(Material transparentMaterial, ref Material current, ref bool isTransparent, Dictionary<string, Material> materials, float transparency)
        {
            if (transparency < 0.9999f)
            {
                isTransparent = true;
                var prev = current;
                current = new Material(transparentMaterial);
                current.name = prev.name;

                // Copy properties?

                var color = current.GetColor("_Color");
                color = new Color(color.r, color.g, color.b, transparency);
                current.SetColor("_Color", color);

                materials[prev.name] = current;
            }
        }

        private static void AssignColorProperty(Material material, string name, List<string> split)
        {
            if (!material.HasProperty(name)) return;

            var color = new Color(
                float.Parse(split[1], floatStyle, CultureInfo.InvariantCulture),
                float.Parse(split[2], floatStyle, CultureInfo.InvariantCulture),
                float.Parse(split[3], floatStyle, CultureInfo.InvariantCulture),
                material.GetColor(name).a);

            material.SetColor(name, color);
        }

        private static string GetTexturePath(string line)
        {
            var split = new List<string>();
            var ids = new List<int>();

            line.BufferSplitIDs(split, ids, ' ');

            int lastParamId = -1;
            for (int i = 1; i < split.Count; i++)
            {
                if (split[i].StartsWith("-")) lastParamId = i;
            }
            var lastNonNamePart = split[lastParamId + 1];
            int firstNameCharacterId = ids[lastParamId + 1] + lastNonNamePart.Length;

            return (line.Substring(firstNameCharacterId)).Trim();
        }

        private static void AssignTexture(Material material, string name, string texturePath, string directoryName)
        {
            if (!material.HasProperty(name)) return;

            var extention = Path.GetExtension(texturePath);
            if (extention != ".jpg" && extention != ".png")
                return;

            Texture2D tex = null;

            lock (texturesCache)
            {
                if (texturesCache.ContainsKey(texturePath))
                    tex = texturesCache[texturePath];
            }

            if (tex == null)
            {
                tex = new Texture2D(4, 4);
                tex.LoadImage(File.ReadAllBytes(Path.Combine(directoryName, texturePath)));

                lock (texturesCache)
                {
                    texturesCache.Add(texturePath, tex);
                }
            }

            material.SetTexture(name, tex);
        }
    }
}