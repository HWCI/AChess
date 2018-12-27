﻿//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


using System.Collections.Generic;
using JBooth.MicroSplat;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MicroSplatTerrain))]
[CanEditMultipleObjects]
public partial class MicroSplatTerrainEditor : Editor
{
#if __MICROSPLAT__
    private static readonly GUIContent geoTexOverride = new GUIContent("Geo Texture Override",
        "If you want each terrain object to have it's own geo texture instead of the one defined in the material, add it here");

    private static readonly GUIContent geoTintOverride = new GUIContent("Tint Texture Override",
        "If you want each terrain object to have it's own global tint instead of the one defined in the material, add it here");

    private static readonly GUIContent geoNormalOverride = new GUIContent("Global Normal Override",
        "If you want each terrain object to have it's own global normal instead of the one defined in the material, add it here");

    private static readonly GUIContent CTemplateMaterial =
        new GUIContent("Template Material", "Material to use for this terrain");

    private static readonly GUIContent CVSGrassMap = new GUIContent("Grass Map", "Grass Map from Vegetation Studio");

    private static readonly GUIContent CVSShadowMap =
        new GUIContent("Shadow Map", "Shadow map texture from Vegetation Studio");

    private static readonly GUIContent
        CBlendMat = new GUIContent("Blend Mat", "Blending material for terrain blending");

    private static readonly GUIContent CCustomSplat0 =
        new GUIContent("Custom Splat 0", "Custom splat map for textures 0-3");

    private static readonly GUIContent CCustomSplat1 =
        new GUIContent("Custom Splat 1", "Custom splat map for textures 4-7");

    private static readonly GUIContent CCustomSplat2 =
        new GUIContent("Custom Splat 2", "Custom splat map for textures 8-11");

    private static readonly GUIContent CCustomSplat3 =
        new GUIContent("Custom Splat 3", "Custom splat map for textures 12-15");

    private static readonly GUIContent CCustomSplat4 =
        new GUIContent("Custom Splat 4", "Custom splat map for textures 16-19");

    private static readonly GUIContent CCustomSplat5 =
        new GUIContent("Custom Splat 5", "Custom splat map for textures 20-23");

    private static readonly GUIContent CCustomSplat6 =
        new GUIContent("Custom Splat 6", "Custom splat map for textures 24-27");

    private static readonly GUIContent CCustomSplat7 =
        new GUIContent("Custom Splat 7", "Custom splat map for textures 28-31");

    public override void OnInspectorGUI()
    {
        var t = target as MicroSplatTerrain;
        if (t == null)
        {
            EditorGUILayout.HelpBox("No Terrain Present, please put this component on a terrain", MessageType.Error);
            return;
        }

        EditorGUI.BeginChangeCheck();
        t.templateMaterial =
            EditorGUILayout.ObjectField(CTemplateMaterial, t.templateMaterial, typeof(Material), false) as Material;


        if (t.templateMaterial == null)
        {
            if (GUILayout.Button("Convert to MicroSplat"))
            {
                // get all terrains in selection, not just this one, and treat as one giant terrain
                var objs = Selection.gameObjects;
                var terrains = new List<Terrain>();
                for (var i = 0; i < objs.Length; ++i)
                {
                    var ter = objs[i].GetComponent<Terrain>();
                    if (ter != null) terrains.Add(ter);
                    var trs = objs[i].GetComponentsInChildren<Terrain>();
                    for (var x = 0; x < trs.Length; ++x)
                        if (!terrains.Contains(trs[x]))
                            terrains.Add(trs[x]);
                }

                // setup this terrain
                var terrain = t.GetComponent<Terrain>();
                t.templateMaterial = MicroSplatShaderGUI.NewShaderAndMaterial(terrain);
                var config = TextureArrayConfigEditor.CreateConfig(terrain);
                t.templateMaterial.SetTexture("_Diffuse", config.diffuseArray);
                t.templateMaterial.SetTexture("_NormalSAO", config.normalSAOArray);

                t.propData = MicroSplatShaderGUI.FindOrCreatePropTex(t.templateMaterial);
#if UNITY_2018_3_OR_NEWER
                if (terrain.terrainData.terrainLayers.Length > 0)
                {
                    var uvScale = terrain.terrainData.terrainLayers[0].tileSize;
                    var uvOffset = terrain.terrainData.terrainLayers[0].tileOffset;

                    uvScale = MicroSplatRuntimeUtil.UnityUVScaleToUVScale(uvScale, terrain);
                    uvOffset.x = uvScale.x / terrain.terrainData.size.x * 0.5f * uvOffset.x;
                    uvOffset.y = uvScale.y / terrain.terrainData.size.x * 0.5f * uvOffset.y;
                    var scaleOffset = new Vector4(uvScale.x, uvScale.y, uvOffset.x, uvOffset.y);
                    t.templateMaterial.SetVector("_UVScale", scaleOffset);
                }
#else
            if (terrain.terrainData.splatPrototypes.Length > 0)
            {
               var uvScale = terrain.terrainData.splatPrototypes[0].tileSize;
               var uvOffset = terrain.terrainData.splatPrototypes[0].tileOffset;

               uvScale = MicroSplatRuntimeUtil.UnityUVScaleToUVScale(uvScale, terrain);
               uvOffset.x = uvScale.x / terrain.terrainData.size.x * 0.5f * uvOffset.x;
               uvOffset.y = uvScale.y / terrain.terrainData.size.x * 0.5f * uvOffset.y;
               Vector4 scaleOffset = new Vector4(uvScale.x, uvScale.y, uvOffset.x, uvOffset.y);
               t.templateMaterial.SetVector("_UVScale", scaleOffset);

            }
#endif

                // now make sure others all have the same settings as well.
                for (var i = 0; i < terrains.Count; ++i)
                {
                    var nt = terrains[i];
                    var mgr = nt.GetComponent<MicroSplatTerrain>();
                    if (mgr == null) mgr = nt.gameObject.AddComponent<MicroSplatTerrain>();
                    mgr.templateMaterial = t.templateMaterial;

                    if (mgr.propData == null)
                        mgr.propData = MicroSplatShaderGUI.FindOrCreatePropTex(mgr.templateMaterial);
                }

                Selection.SetActiveObjectWithContext(config, config);
            }

            MicroSplatTerrain.SyncAll();
            return;
        }

        if (t.propData == null) t.propData = MicroSplatShaderGUI.FindOrCreatePropTex(t.templateMaterial);

#if __MICROSPLAT_TERRAINBLEND__ || __MICROSPLAT_STREAMS__
      DoTerrainDescGUI();
#endif

        // could move this to some type of interfaced component created by the module if this becomes a thing,
        // but I think this will be most of the cases?

        MicroSplatUtilities.DrawTextureField(t, CCustomSplat0, ref t.customControl0, "_CUSTOMSPLATTEXTURES");

        MicroSplatUtilities.DrawTextureField(t, CCustomSplat1, ref t.customControl1, "_CUSTOMSPLATTEXTURES");
        MicroSplatUtilities.DrawTextureField(t, CCustomSplat2, ref t.customControl2, "_CUSTOMSPLATTEXTURES");
        MicroSplatUtilities.DrawTextureField(t, CCustomSplat3, ref t.customControl3, "_CUSTOMSPLATTEXTURES");
        MicroSplatUtilities.DrawTextureField(t, CCustomSplat4, ref t.customControl4, "_CUSTOMSPLATTEXTURES");
        MicroSplatUtilities.DrawTextureField(t, CCustomSplat5, ref t.customControl5, "_CUSTOMSPLATTEXTURES");
        MicroSplatUtilities.DrawTextureField(t, CCustomSplat6, ref t.customControl6, "_CUSTOMSPLATTEXTURES");
        MicroSplatUtilities.DrawTextureField(t, CCustomSplat7, ref t.customControl7, "_CUSTOMSPLATTEXTURES");

        MicroSplatUtilities.DrawTextureField(t, geoTexOverride, ref t.geoTextureOverride, "_GEOMAP");

        MicroSplatUtilities.DrawTextureField(t, geoTintOverride, ref t.tintMapOverride, "_GLOBALTINT");

        MicroSplatUtilities.DrawTextureField(t, geoNormalOverride, ref t.globalNormalOverride, "_GLOBALNORMALS");

#if __MICROSPLAT_ADVANCED_DETAIL__
      DrawAdvancedModuleDetailGUI(t);      
#endif

        if (t.templateMaterial.IsKeywordEnabled("_VSGRASSMAP"))
        {
            EditorGUI.BeginChangeCheck();

            t.vsGrassMap =
                EditorGUILayout.ObjectField(CVSGrassMap, t.vsGrassMap, typeof(Texture2D), false) as Texture2D;

            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(t);
        }

        if (t.templateMaterial.IsKeywordEnabled("_VSSHADOWMAP"))
        {
            EditorGUI.BeginChangeCheck();


            t.vsShadowMap =
                EditorGUILayout.ObjectField(CVSShadowMap, t.vsShadowMap, typeof(Texture2D), false) as Texture2D;

            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(t);
        }


        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Sync"))
        {
            var mgr = target as MicroSplatTerrain;
            mgr.Sync();
        }

        if (GUILayout.Button("Sync All")) MicroSplatTerrain.SyncAll();
        EditorGUILayout.EndHorizontal();

        BakingGUI(t);
        WeightLimitingGUI(t);

#if __MICROSPLAT_ADVANCED_DETAIL__
      DrawAdvancedModuleDetailTooset(t);
#endif

        if (MicroSplatUtilities.DrawRollup("Debug", false, true))
        {
            EditorGUI.indentLevel += 2;
            EditorGUILayout.HelpBox(
                "These should not need to be edited unless something funky has happened. They are automatically managed by MicroSplat.",
                MessageType.Info);
            t.propData =
                EditorGUILayout.ObjectField("Per Texture Data", t.propData, typeof(MicroSplatPropData), false) as
                    MicroSplatPropData;
            t.blendMat = EditorGUILayout.ObjectField(CBlendMat, t.blendMat, typeof(Material), false) as Material;
            t.terrainDesc =
                EditorGUILayout.ObjectField("Terrain Descriptor", t.terrainDesc, typeof(Texture2D), false) as Texture2D;
            t.addPass = EditorGUILayout.ObjectField("Add Pass", t.addPass, typeof(Shader), false) as Shader;
            EditorGUI.indentLevel -= 2;
        }

        if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(t);
    }

    partial void DrawAdvancedModuleDetailGUI(MicroSplatTerrain t);
    partial void DrawAdvancedModuleDetailTooset(MicroSplatTerrain t);

#endif
}