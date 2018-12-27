//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;
using JBooth.MicroSplat;
using UnityEditor;
using UnityEngine;

public partial class MicroSplatTerrainEditor : Editor
{
    public enum BakingPasses
    {
        Albedo = 1,
        Height = 2,
        Normal = 4,
        Metallic = 8,
        Smoothness = 16,
        AO = 32,
        Emissive = 64
    }

    public enum BakingResolutions
    {
        k256 = 256,
        k512 = 512,
        k1024 = 1024,
        k2048 = 2048,
        k4096 = 4096,
        k8192 = 8192
    }

    private bool needsBake;

    public BakingPasses passes = 0;
    public BakingResolutions res = BakingResolutions.k1024;

    public void BakingGUI(MicroSplatTerrain t)
    {
        if (needsBake && Event.current.type == EventType.Repaint)
        {
            needsBake = false;
            Bake(t);
        }

        if (MicroSplatUtilities.DrawRollup("Render Baking", false))
        {
            res = (BakingResolutions) EditorGUILayout.EnumPopup(new GUIContent("Resolution"), res);

#if UNITY_2017_3_OR_NEWER
            passes = (BakingPasses) EditorGUILayout.EnumFlagsField(new GUIContent("Features"), passes);
#else
            passes = (BakingPasses)EditorGUILayout.EnumMaskPopup(new GUIContent("Features"), passes);
#endif

            if (GUILayout.Button("Export Selected")) needsBake = true;
        }
    }


    private bool IsEnabled(BakingPasses p)
    {
        return ((int) passes & (int) p) == (int) p;
    }


    private static MicroSplatBaseFeatures.DefineFeature FeatureFromOutput(MicroSplatBaseFeatures.DebugOutput p)
    {
        if (p == MicroSplatBaseFeatures.DebugOutput.Albedo)
            return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_ALBEDO;
        if (p == MicroSplatBaseFeatures.DebugOutput.AO)
            return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_AO;
        if (p == MicroSplatBaseFeatures.DebugOutput.Emission)
            return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_EMISSION;
        if (p == MicroSplatBaseFeatures.DebugOutput.Height)
            return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_HEIGHT;
        if (p == MicroSplatBaseFeatures.DebugOutput.Metallic)
            return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_METAL;
        if (p == MicroSplatBaseFeatures.DebugOutput.Normal)
            return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_NORMAL;
        if (p == MicroSplatBaseFeatures.DebugOutput.Smoothness)
            return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_SMOOTHNESS;
        return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_ALBEDO;
    }

    private static MicroSplatBaseFeatures.DebugOutput OutputFromPass(BakingPasses p)
    {
        if (p == BakingPasses.Albedo)
            return MicroSplatBaseFeatures.DebugOutput.Albedo;
        if (p == BakingPasses.AO)
            return MicroSplatBaseFeatures.DebugOutput.AO;
        if (p == BakingPasses.Emissive)
            return MicroSplatBaseFeatures.DebugOutput.Emission;
        if (p == BakingPasses.Height)
            return MicroSplatBaseFeatures.DebugOutput.Height;
        if (p == BakingPasses.Metallic)
            return MicroSplatBaseFeatures.DebugOutput.Metallic;
        if (p == BakingPasses.Normal)
            return MicroSplatBaseFeatures.DebugOutput.Normal;
        if (p == BakingPasses.Smoothness) return MicroSplatBaseFeatures.DebugOutput.Smoothness;
        return MicroSplatBaseFeatures.DebugOutput.Albedo;
    }

    private static void RemoveKeyword(List<string> keywords, string keyword)
    {
        if (keywords.Contains(keyword)) keywords.Remove(keyword);
    }

    private static Material SetupMaterial(Material mat, MicroSplatBaseFeatures.DebugOutput debugOutput)
    {
        var comp = new MicroSplatShaderGUI.MicroSplatCompiler();
        var keywords = new List<string>(mat.shaderKeywords);

        RemoveKeyword(keywords, "_SNOW");
        RemoveKeyword(keywords, "_TESSDISTANCE");
        RemoveKeyword(keywords, "_WINDPARTICULATE");
        RemoveKeyword(keywords, "_SNOWPARTICULATE");
        RemoveKeyword(keywords, "_GLITTER");
        RemoveKeyword(keywords, "_SNOWGLITTER");

        keywords.Add(FeatureFromOutput(debugOutput).ToString());

        var shader = comp.Compile(keywords.ToArray(), "RenderBake_" + debugOutput);
        var s = ShaderUtil.CreateShaderAsset(shader);
        var renderMat = new Material(mat);
        renderMat.shader = s;
        return renderMat;
    }


    public static Texture2D Bake(MicroSplatTerrain mst, BakingPasses p, int resolution)
    {
        var cam = new GameObject("cam").AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 0.5f;
        cam.transform.position = new Vector3(0.5f, 10000.5f, -1);
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 2.0f;
        cam.enabled = false;
        cam.depthTextureMode = DepthTextureMode.None;
        cam.clearFlags = CameraClearFlags.Color;
        cam.backgroundColor = Color.grey;

        var debugOutput = OutputFromPass(p);
        var readWrite =
            debugOutput == MicroSplatBaseFeatures.DebugOutput.Albedo ||
            debugOutput == MicroSplatBaseFeatures.DebugOutput.Emission
                ? RenderTextureReadWrite.sRGB
                : RenderTextureReadWrite.Linear;

        var rt = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.ARGB32, readWrite);
        RenderTexture.active = rt;
        cam.targetTexture = rt;

        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.transform.position = new Vector3(0, 10000, 0);
        cam.transform.position = new Vector3(0, 10000, -1);
        var renderMat = SetupMaterial(mst.matInstance, debugOutput);
        go.GetComponent<MeshRenderer>().sharedMaterial = renderMat;
        var fog = RenderSettings.fog;
        if (p == BakingPasses.Normal)
            cam.backgroundColor = Color.blue;
        else
            cam.backgroundColor = Color.gray;
        var ambInt = RenderSettings.ambientIntensity;
        var reflectInt = RenderSettings.reflectionIntensity;
        RenderSettings.ambientIntensity = 0;
        RenderSettings.reflectionIntensity = 0;
        Unsupported.SetRenderSettingsUseFogNoDirty(false);
        cam.Render();
        Unsupported.SetRenderSettingsUseFogNoDirty(fog);

        RenderSettings.ambientIntensity = ambInt;
        RenderSettings.reflectionIntensity = reflectInt;
        var tex = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        tex.Apply();


        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null)
            if (mr.sharedMaterial != null)
            {
                if (mr.sharedMaterial.shader != null)
                    DestroyImmediate(mr.sharedMaterial.shader);
                DestroyImmediate(mr.sharedMaterial);
            }

        DestroyImmediate(go);
        DestroyImmediate(cam.gameObject);
        return tex;
    }

    private void Bake(MicroSplatTerrain mst)
    {
        // for each pass
        var pass = 1;
        while (pass <= (int) BakingPasses.Emissive)
        {
            var p = (BakingPasses) pass;
            pass *= 2;
            if (!IsEnabled(p)) continue;
            var debugOutput = OutputFromPass(p);
            var tex = Bake(mst, p, (int) res);
            var bytes = tex.EncodeToPNG();

            var texPath = MicroSplatUtilities.RelativePathFromAsset(mst.terrain) + "/" + mst.terrain.name + "_" +
                          debugOutput;
            File.WriteAllBytes(texPath + ".png", bytes);
        }

        AssetDatabase.Refresh();
    }
}