﻿//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections.Generic;
using JBooth.MicroSplat;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Terrain))]
[DisallowMultipleComponent]
public class MicroSplatTerrain : MicroSplatObject
{
    [HideInInspector] // for build include
    public Shader addPass;

    public delegate void MaterialSyncAll();

    public delegate void MaterialSync(Material m);

    public static event MaterialSyncAll OnMaterialSyncAll;
    public event MaterialSync OnMaterialSync;

    private static readonly List<MicroSplatTerrain> sInstances = new List<MicroSplatTerrain>();

    public Terrain terrain;

#if UNITY_EDITOR
    [HideInInspector] public int oldBaseMapSize;
    [HideInInspector] public float oldBaseMapDistance;
#endif

    // LOTS of hacking around Unity bugs. In some versions of Unity, accessing any data on the terrain too early can cause a crash, either
    // in editor, or in playmode, but not in the same way. Really just want to call Sync on Enabled, and Cleanup on Disabled, but triggers
    // too many issues, so we dodge the bullet in various cases to hack around it- so ugly.

    private void Awake()
    {
        terrain = GetComponent<Terrain>();
#if UNITY_EDITOR
        Sync();
#endif
    }

    private void OnEnable()
    {
        terrain = GetComponent<Terrain>();
        sInstances.Add(this);
#if UNITY_EDITOR
        Sync();
#else
      if (reenabled)
      {
         Sync();
      }
#endif
    }

#if !UNITY_EDITOR
   void Start()
   {
      Sync();
   }
#endif

    [HideInInspector] public bool reenabled;

    private void OnDisable()
    {
        sInstances.Remove(this);
        Cleanup();
        reenabled = true;
    }

#if UNITY_EDITOR
    public bool sTerrainDirty;
    private void OnTerrainChanged(int f)
    {
        var flags = (TerrainChangedFlags) f;
        if ((flags & TerrainChangedFlags.Heightmap) != 0) sTerrainDirty = true;

        if ((flags & TerrainChangedFlags.DelayedHeightmapUpdate) != 0) sTerrainDirty = true;
    }
#endif

    private void Cleanup()
    {
        if (matInstance != null && matInstance != templateMaterial)
        {
            DestroyImmediate(matInstance);
            terrain.materialTemplate = null;
        }
#if UNITY_EDITOR
        terrain.basemapDistance = oldBaseMapDistance;
        if (terrain.terrainData != null) terrain.terrainData.baseMapResolution = Mathf.Max(16, oldBaseMapSize);
#endif
    }

    public void Sync()
    {
        if (templateMaterial == null)
            return;

#if UNITY_EDITOR
        if (terrain.materialType != Terrain.MaterialType.Custom || terrain.materialTemplate == null)
        {
            oldBaseMapSize = terrain.terrainData.baseMapResolution;
            oldBaseMapDistance = terrain.basemapDistance;
        }

        if (addPass == null) addPass = Shader.Find("Hidden/MicroSpat/AddPass");
#endif
        Material m = null;


        if (terrain.materialTemplate == matInstance && matInstance != null)
        {
            terrain.materialTemplate.CopyPropertiesFromMaterial(templateMaterial);
            m = terrain.materialTemplate;
        }
        else
        {
            m = new Material(templateMaterial);
        }

        terrain.materialType = Terrain.MaterialType.Custom;


        m.hideFlags = HideFlags.HideAndDontSave;
        terrain.materialTemplate = m;
        matInstance = m;

        ApplyMaps(m);

        if (m.IsKeywordEnabled("_CUSTOMSPLATTEXTURES"))
        {
            m.SetTexture("_CustomControl0", customControl0 != null ? customControl0 : Texture2D.blackTexture);
            m.SetTexture("_CustomControl1", customControl1 != null ? customControl1 : Texture2D.blackTexture);
            m.SetTexture("_CustomControl2", customControl2 != null ? customControl2 : Texture2D.blackTexture);
            m.SetTexture("_CustomControl3", customControl3 != null ? customControl3 : Texture2D.blackTexture);
            m.SetTexture("_CustomControl4", customControl4 != null ? customControl4 : Texture2D.blackTexture);
            m.SetTexture("_CustomControl5", customControl5 != null ? customControl5 : Texture2D.blackTexture);
            m.SetTexture("_CustomControl6", customControl6 != null ? customControl6 : Texture2D.blackTexture);
            m.SetTexture("_CustomControl7", customControl7 != null ? customControl7 : Texture2D.blackTexture);
        }
        else
        {
            var controls = terrain.terrainData.alphamapTextures;
            ApplyControlTextures(controls, m);
        }

        // set base map distance to max of fancy features
        // base map does not use the "base map" texture, so slam to 16
        // only do this stuff editor time to avoid runtime cost
#if UNITY_EDITOR
        if (terrain.terrainData.baseMapResolution != 16) terrain.terrainData.baseMapResolution = 16;
        float basemapDistance = 0;

        if (m.HasProperty("_TessData2"))
        {
            var d = m.GetVector("_TessData2").y;
            if (d > basemapDistance)
                basemapDistance = d;
        }

        if (m.HasProperty("_ParallaxParams"))
        {
            var v = m.GetVector("_ParallaxParams");
            var d = v.y + v.z;
            if (d > basemapDistance)
                basemapDistance = d;
        }

        if (m.HasProperty("_DetailNoiseScaleStrengthFade"))
        {
            var d = m.GetVector("_DetailNoiseScaleStrengthFade").z;
            if (d > basemapDistance)
                basemapDistance = d;
        }

        terrain.basemapDistance = basemapDistance;
#endif

        if (OnMaterialSync != null) OnMaterialSync(m);

        ApplyBlendMap();


#if UNITY_EDITOR
        RestorePrototypes();
#endif
    }

    public override Bounds GetBounds()
    {
        return terrain.terrainData.bounds;
    }

#if UNITY_EDITOR


    private void RestorePrototypes()
    {
        if (templateMaterial != null)
        {
            var diffuseArray = templateMaterial.GetTexture("_Diffuse") as Texture2DArray;
            if (diffuseArray != null)
            {
                var cfg = TextureArrayConfig.FindConfig(diffuseArray);
                if (cfg != null)
                {
                    var count = cfg.sourceTextures.Count;
                    if (count > 32)
                        count = 32;
#if UNITY_2018_3_OR_NEWER
                    var protos = terrain.terrainData.terrainLayers;
                    var needsRefresh = false;
                    if (protos.Length != count) needsRefresh = true;
                    if (!needsRefresh)
                        for (var i = 0; i < protos.Length; ++i)
                            if (protos[i].diffuseTexture != cfg.sourceTextures[i].diffuse)
                                needsRefresh = true;
                    if (needsRefresh)
                    {
                        var v4 = templateMaterial.GetVector("_UVScale");

                        var uvScales = new Vector2(v4.x, v4.y);
                        uvScales = MicroSplatRuntimeUtil.UVScaleToUnityUVScale(uvScales, terrain);

                        protos = new TerrainLayer[count];
                        for (var i = 0; i < count; ++i)
                        {
                            var sp = new TerrainLayer();
                            sp.diffuseTexture = cfg.sourceTextures[i].diffuse;
                            sp.tileSize = uvScales;
                            if (cfg.sourceTextures[i].normal != null)
                                sp.normalMapTexture = cfg.sourceTextures[i].normal;

                            protos[i] = sp;
                        }

                        terrain.terrainData.terrainLayers = protos;
                        EditorUtility.SetDirty(terrain);
                        EditorUtility.SetDirty(terrain.terrainData);
                    }
#else
               var protos = terrain.terrainData.splatPrototypes;
               bool needsRefresh = false;
               if (protos.Length != count)
               {
                  needsRefresh = true;
               }
               if (!needsRefresh)
               {
                  for (int i = 0; i < protos.Length; ++i)
                  {
                     if (protos[i].texture != cfg.sourceTextures[i].diffuse)
                     {
                        needsRefresh = true;
                     }
                  }
               }
               if (needsRefresh)
               {
                  Vector4 v4 = templateMaterial.GetVector("_UVScale");

                  Vector2 uvScales = new Vector2(v4.x, v4.y);
                  uvScales = MicroSplatRuntimeUtil.UVScaleToUnityUVScale(uvScales, terrain);

                  protos = new SplatPrototype[count];
                  for (int i = 0; i < count; ++i)
                  {
                     SplatPrototype sp = new SplatPrototype();
                     sp.texture = cfg.sourceTextures[i].diffuse;
                     sp.tileSize = uvScales;
                     if (cfg.sourceTextures[i].normal != null)
                     {
                        sp.normalMap = cfg.sourceTextures[i].normal;
                     }
                     protos[i] = sp;
                  }

                  terrain.terrainData.splatPrototypes = protos;
                  UnityEditor.EditorUtility.SetDirty(terrain);
                  UnityEditor.EditorUtility.SetDirty(terrain.terrainData);
               }
#endif
                }
            }
        }
    }
#endif

    public static void SyncAll()
    {
        for (var i = 0; i < sInstances.Count; ++i) sInstances[i].Sync();
        if (OnMaterialSyncAll != null) OnMaterialSyncAll();
    }
}