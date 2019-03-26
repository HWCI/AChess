//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.IO;
using JBooth.MicroSplat;
using UnityEditor;
using UnityEngine;

#if __MICROSPLAT__
public partial class MicroSplatTerrainEditor : Editor
{
    public static void GenerateTerrainBlendData(MicroSplatTerrain bt)
    {
        var t = bt.GetComponent<Terrain>();
        var w = t.terrainData.heightmapWidth;
        var h = t.terrainData.heightmapHeight;

        var data = new Texture2D(w, h, TextureFormat.RGBAHalf, true, true);
        for (var x = 0; x < w; ++x)
        for (var y = 0; y < h; ++y)
        {
            var height = t.terrainData.GetHeight(x, y);
            var normal = t.terrainData.GetInterpolatedNormal((float) x / w, (float) y / h);
            // When you save a texture to EXR format, either in the saving or import stage,
            // some type of gamma curve is applied regardless of the fact that the textures is
            // set to linear. So we pow it here to counteract it, whis is total BS, but works..
            normal.x = normal.x >= 0 ? Mathf.Pow(normal.x, 2.0f) : Mathf.Pow(normal.x, 2) * -1;
            normal.z = normal.z >= 0 ? Mathf.Pow(normal.z, 2.0f) : Mathf.Pow(normal.z, 2) * -1;
            data.SetPixel(x, y, new Color(normal.x, normal.y, normal.z, height));
        }

        data.Apply();

        var path = MicroSplatUtilities.RelativePathFromAsset(t.terrainData);
        path += "/" + t.name + ".exr";
        var bytes = data.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat);
        File.WriteAllBytes(path, bytes);
        DestroyImmediate(data);
        AssetDatabase.Refresh();
        bt.terrainDesc = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        var ai = AssetImporter.GetAtPath(path);
        var ti = ai as TextureImporter;
        var ps = ti.GetDefaultPlatformTextureSettings();

        if (ti.isReadable ||
            ti.wrapMode != TextureWrapMode.Clamp ||
            ps.format != TextureImporterFormat.RGBAHalf ||
            ps.textureCompression != TextureImporterCompression.Uncompressed ||
            ps.overridden != true ||
            ti.filterMode != FilterMode.Bilinear ||
            ti.sRGBTexture)
        {
            ti.sRGBTexture = false;
            ti.filterMode = FilterMode.Bilinear;
            ti.mipmapEnabled = true;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.isReadable = false;
            ps.format = TextureImporterFormat.RGBAHalf;
            ps.textureCompression = TextureImporterCompression.Uncompressed;
            ps.overridden = true;
            ti.SetPlatformTextureSettings(ps);
            ti.SaveAndReimport();
        }

        bt.sTerrainDirty = false;
        EditorUtility.SetDirty(bt);
        MicroSplatTerrain.SyncAll();
    }

    public void DoTerrainDescGUI()
    {
        var bt = target as MicroSplatTerrain;
        var t = bt.GetComponent<Terrain>();
        if (t == null || t.terrainData == null)
        {
            EditorGUILayout.HelpBox("No Terrain found, please add this component to your terrain", MessageType.Error);
            return;
        }

        if (t.materialType != Terrain.MaterialType.Custom || t.materialTemplate == null) return;

        if (!t.materialTemplate.IsKeywordEnabled("_TERRAINBLENDING") &&
            !t.materialTemplate.IsKeywordEnabled("_DYNAMICFLOWS")) return;
        EditorGUILayout.Space();

        if (bt.terrainDesc == null)
            EditorGUILayout.HelpBox("Terrain Descriptor Data is not present, please generate", MessageType.Info);

        if (bt.terrainDesc != null && bt.sTerrainDirty)
            EditorGUILayout.HelpBox("Terrain Descriptor data is out of date, please update", MessageType.Info);
        if (GUILayout.Button(bt.terrainDesc == null
            ? "Generate Terrain Descriptor Data"
            : "Update Terrain Descriptor Data")) GenerateTerrainBlendData(bt);

        if (bt.terrainDesc != null && GUILayout.Button("Clear Terrain Descriptor Data")) bt.terrainDesc = null;

        if (bt.terrainDesc != null)
        {
            EditorGUILayout.BeginHorizontal();
            var mem = bt.terrainDesc.width * bt.terrainDesc.height;
            mem /= 128;
            EditorGUILayout.LabelField("Terrain Descriptor Data Memory: " + mem + "kb");
            EditorGUILayout.EndHorizontal();
        }

        if (bt.blendMat == null && bt.templateMaterial != null &&
            bt.templateMaterial.IsKeywordEnabled("_TERRAINBLENDING"))
        {
            var path = AssetDatabase.GetAssetPath(bt.templateMaterial);
            path = path.Replace(".mat", "_TerrainObjectBlend.mat");
            bt.blendMat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (bt.blendMat == null)
            {
                var shaderPath = path.Replace(".mat", ".shader");
                var shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
                if (shader != null)
                {
                    var mat = new Material(shader);
                    AssetDatabase.CreateAsset(mat, path);
                    AssetDatabase.SaveAssets();
                    MicroSplatTerrain.SyncAll();
                }
            }
        }
    }
}
#endif