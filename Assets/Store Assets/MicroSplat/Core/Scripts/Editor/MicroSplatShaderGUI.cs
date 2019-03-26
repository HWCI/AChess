//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Text;
using JBooth.MicroSplat;
using UnityEditor;
using UnityEngine;

public partial class MicroSplatShaderGUI : ShaderGUI
{
    public static readonly string MicroSplatVersion = "2.1";

    private static Material compileMat;
    private static string compileName;
    private static MicroSplatCompiler targetCompiler;
    private readonly StringBuilder builder = new StringBuilder(1024);

    private string cachedTitle;

    private readonly MicroSplatCompiler compiler = new MicroSplatCompiler();
#if UNITY_2018_1_OR_NEWER
    private readonly GUIContent CRenderLoop = new GUIContent("Render Loop",
        "In 2018.1+, Scriptable Render Loops are available. You can select which render loop the shader should be compiled for here");
#endif

    private readonly GUIContent CShaderName = new GUIContent("Name", "Menu path with name for the shader");

    private readonly List<Module> modules = new List<Module>();
    private Vector2 moduleScroll;

    private bool needsCompile;

    private Module openModule;
    private int perTexIndex;
    private GUIContent[] renderLoopNames;

    public MaterialProperty FindProp(string name, MaterialProperty[] props)
    {
        return FindProperty(name, props);
    }

    private bool DrawRenderLoopGUI(Material targetMat)
    {
#if UNITY_2018_1_OR_NEWER
        // init render loop name list
        if (renderLoopNames == null || renderLoopNames.Length != availableRenderLoops.Count)
        {
            var rln = new List<GUIContent>();
            for (var i = 0; i < availableRenderLoops.Count; ++i)
                rln.Add(new GUIContent(availableRenderLoops[i].GetDisplayName()));
            renderLoopNames = rln.ToArray();
        }

        if (renderLoopNames.Length == 1) return false;

        var keywords = targetMat.shaderKeywords;
        var curRenderLoopIndex = 0;
        for (var i = 0; i < keywords.Length; ++i)
        {
            var s = keywords[i];
            for (var j = 0; j < availableRenderLoops.Count; ++j)
                if (s == availableRenderLoops[j].GetRenderLoopKeyword())
                {
                    curRenderLoopIndex = j;
                    compiler.renderLoop = availableRenderLoops[j];
                    break;
                }
        }

        var oldIdx = curRenderLoopIndex;
        curRenderLoopIndex = EditorGUILayout.Popup(CRenderLoop, curRenderLoopIndex, renderLoopNames);
        if (oldIdx != curRenderLoopIndex && curRenderLoopIndex >= 0 && curRenderLoopIndex < availableRenderLoops.Count)
        {
            if (compiler.renderLoop != null) targetMat.DisableKeyword(compiler.renderLoop.GetRenderLoopKeyword());
            compiler.renderLoop = availableRenderLoops[curRenderLoopIndex];
            targetMat.EnableKeyword(compiler.renderLoop.GetRenderLoopKeyword());
            return true;
        }
#endif

#if UNITY_2018_3_OR_NEWER
        if (targetMat != null && !targetMat.enableInstancing)
        {
            EditorUtility.SetDirty(targetMat);
            targetMat.enableInstancing = true;
        }
#endif
        return false;
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        if (cachedTitle == null) cachedTitle = "Shader Generator        v:" + MicroSplatVersion;
        if (GUI.enabled == false)
        {
            EditorGUILayout.HelpBox("You must edit the template material, not the instance being used",
                MessageType.Info);
            return;
        }

        EditorGUI.BeginChangeCheck(); // sync materials
        var targetMat = materialEditor.target as Material;
        var diff = targetMat.GetTexture("_Diffuse") as Texture2DArray;


        compiler.Init();
        // must unpack everything before the generator draws- otherwise we get IMGUI errors
        for (var i = 0; i < compiler.extensions.Count; ++i)
        {
            var ext = compiler.extensions[i];
            ext.Unpack(targetMat.shaderKeywords);
        }

        var shaderName = targetMat.shader.name;
        DrawModules();

        EditorGUI.BeginChangeCheck(); // needs compile

        if (MicroSplatUtilities.DrawRollup(cachedTitle))
        {
            shaderName = EditorGUILayout.DelayedTextField(CShaderName, shaderName);

            if (DrawRenderLoopGUI(targetMat)) needsCompile = true;

            for (var i = 0; i < compiler.extensions.Count; ++i)
            {
                var e = compiler.extensions[i];
                if (e.GetVersion() == MicroSplatVersion)
                {
                    e.DrawFeatureGUI(targetMat);
                }
                else
                    EditorGUILayout.HelpBox(
                        "Extension : " + e + " is version " + e.GetVersion() + " and MicroSplat is version " +
                        MicroSplatVersion + ", please update", MessageType.Error);
            }

            for (var i = 0; i < availableRenderLoops.Count; ++i)
            {
                var rl = availableRenderLoops[i];
                if (rl.GetVersion() != MicroSplatVersion)
                    EditorGUILayout.HelpBox(
                        "Render Loop : " + rl.GetDisplayName() + " is version " + rl.GetVersion() +
                        " and MicroSplat is version " + MicroSplatVersion + ", please update", MessageType.Error);
            }
        }

        needsCompile = needsCompile || EditorGUI.EndChangeCheck();

        var featureCount = targetMat.shaderKeywords.Length;
        // note, ideally we wouldn't draw the GUI for the rest of stuff if we need to compile.
        // But we can't really do that without causing IMGUI to split warnings about
        // mismatched GUILayout blocks
        if (!needsCompile)
        {
            for (var i = 0; i < compiler.extensions.Count; ++i)
            {
                var ext = compiler.extensions[i];
                if (ext.GetVersion() == MicroSplatVersion)
                    ext.DrawShaderGUI(this, targetMat, materialEditor, props);
                else
                    EditorGUILayout.HelpBox(
                        "Extension : " + ext + " is version " + ext.GetVersion() + " and MicroSplat is version " +
                        MicroSplatVersion + ", please update so that all modules are using the same version.",
                        MessageType.Error);
            }


            if (diff != null && MicroSplatUtilities.DrawRollup("Per Texture Properties"))
            {
                var propTex = FindOrCreatePropTex(targetMat);
                perTexIndex = MicroSplatUtilities.DrawTextureSelector(perTexIndex, diff);
                for (var i = 0; i < compiler.extensions.Count; ++i)
                {
                    var ext = compiler.extensions[i];
                    if (ext.GetVersion() == MicroSplatVersion) ext.DrawPerTextureGUI(perTexIndex, targetMat, propTex);
                }
            }
        }

        if (!needsCompile)
            if (featureCount != targetMat.shaderKeywords.Length)
                needsCompile = true;


        var arraySampleCount = 0;
        var textureSampleCount = 0;
        var maxSamples = 0;
        var tessSamples = 0;
        var depTexReadLevel = 0;
        builder.Length = 0;
        for (var i = 0; i < compiler.extensions.Count; ++i)
        {
            var ext = compiler.extensions[i];
            if (ext.GetVersion() == MicroSplatVersion)
                ext.ComputeSampleCounts(targetMat.shaderKeywords, ref arraySampleCount, ref textureSampleCount,
                    ref maxSamples, ref tessSamples, ref depTexReadLevel);
        }

        if (MicroSplatUtilities.DrawRollup("Debug"))
        {
            var shaderModel = compiler.GetShaderModel(targetMat.shaderKeywords);
            builder.Append("Shader Model : ");
            builder.AppendLine(shaderModel);
            if (maxSamples != arraySampleCount)
            {
                builder.Append("Texture Array Samples : ");
                builder.AppendLine(arraySampleCount.ToString());

                builder.Append("Regular Samples : ");
                builder.AppendLine(textureSampleCount.ToString());
            }
            else
            {
                builder.Append("Texture Array Samples : ");
                builder.AppendLine(arraySampleCount.ToString());
                builder.Append("Regular Samples : ");
                builder.AppendLine(textureSampleCount.ToString());
            }

            if (tessSamples > 0)
            {
                builder.Append("Tessellation Samples : ");
                builder.AppendLine(tessSamples.ToString());
            }

            if (depTexReadLevel > 0)
            {
                builder.Append(depTexReadLevel.ToString());
                builder.AppendLine(" areas with dependent texture reads");
            }

            EditorGUILayout.HelpBox(builder.ToString(), MessageType.Info);
        }

        if (EditorGUI.EndChangeCheck() && !needsCompile)
        {
            MicroSplatTerrain.SyncAll();
#if __MICROSPLAT_MESH__
         MicroSplatMesh.SyncAll();
#endif
        }

        if (needsCompile)
        {
            needsCompile = false;
            targetMat.shaderKeywords = null;
            for (var i = 0; i < compiler.extensions.Count; ++i) compiler.extensions[i].Pack(targetMat);
            if (compiler.renderLoop != null) targetMat.EnableKeyword(compiler.renderLoop.GetRenderLoopKeyword());

            // horrible workaround to GUI warning issues
            compileMat = targetMat;
            compileName = shaderName;
            targetCompiler = compiler;
            EditorApplication.delayCall += TriggerCompile;
        }
    }

    protected void TriggerCompile()
    {
        targetCompiler.Compile(compileMat, compileName);
    }

    private void InitModules()
    {
        if (modules.Count == 0)
        {
            //
#if !__MICROSPLAT_GLOBALTEXTURE__
            modules.Add(new Module("https://www.assetstore.unity3d.com/#!/content/96482?aid=1011l37NJ&pubref=1011l37NJ",
                "microsplat_module_globaltexture"));
#endif
#if !__MICROSPLAT_SNOW__
            modules.Add(new Module("https://www.assetstore.unity3d.com/#!/content/96486?aid=1011l37NJ&pubref=1011l37NJ",
                "microsplat_module_snow"));
#endif
#if !__MICROSPLAT_TESSELLATION__
            modules.Add(new Module("https://www.assetstore.unity3d.com/#!/content/96484?aid=1011l37NJ&pubref=1011l37NJ",
                "microsplat_module_tessellation"));
#endif
#if !__MICROSPLAT_DETAILRESAMPLE__
            modules.Add(new Module("https://www.assetstore.unity3d.com/#!/content/96480?aid=1011l37NJ&pubref=1011l37NJ",
                "microsplat_module_detailresample"));
#endif
#if !__MICROSPLAT_TERRAINBLEND__
            modules.Add(new Module("https://www.assetstore.unity3d.com/#!/content/97364?aid=1011l37NJ&pubref=1011l37NJ",
                "microsplat_module_terrainblending"));
#endif
#if !__MICROSPLAT_STREAMS__
            modules.Add(new Module("https://www.assetstore.unity3d.com/#!/content/97993?aid=1011l37NJ&pubref=1011l37NJ",
                "microsplat_module_streams"));
#endif
#if !__MICROSPLAT_ALPHAHOLE__
            modules.Add(new Module("https://www.assetstore.unity3d.com/#!/content/97495?aid=1011l37NJ&pubref=1011l37NJ",
                "microsplat_module_alphahole"));
#endif
#if !__MICROSPLAT_TRIPLANAR__
            modules.Add(new Module("https://www.assetstore.unity3d.com/#!/content/96777?aid=1011l37NJ&pubref=1011l37NJ",
                "microsplat_module_triplanaruvs"));
#endif
#if !__MICROSPLAT_TEXTURECLUSTERS__
            modules.Add(new Module(
                "https://www.assetstore.unity3d.com/#!/content/104223?aid=1011l37NJ&pubref=1011l37NJ",
                "microsplat_module_textureclusters"));
#endif
#if !__MICROSPLAT_WINDGLITTER__
            modules.Add(new Module(
                "https://www.assetstore.unity3d.com/#!/content/105627?aid=1011l37NJ&pubref=1011l37NJ",
                "microsplat_module_windglitter"));
#endif
#if !__MICROSPLAT_ADVANCED_DETAIL__
            modules.Add(new Module(
                "https://www.assetstore.unity3d.com/#!/content/108321?aid=1011l37NJ&pubref=1011l37NJ",
                "microsplat_module_advanceddetails"));
#endif
//#if !CASCADE
//         modules.Add(new Module("https://assetstore.unity.com/packages/tools/terrain/cascade-rivers-lakes-waterfalls-and-more-106072?aid=1011l37NJ&pubref=1011l37NJ", "microsplat_cascade"));
//#endif

            var n = modules.Count;
            if (n > 1)
            {
                var rnd = new System.Random((int) (Random.value * 1000));
                while (n > 1)
                {
                    n--;
                    var k = rnd.Next(n + 1);
                    var value = modules[k];
                    modules[k] = modules[n];
                    modules[n] = value;
                }
            }
        }
    }

    private void DrawModule(Module m)
    {
        if (GUILayout.Button(m.texture, GUI.skin.box, GUILayout.Width(128), GUILayout.Height(128)))
            Application.OpenURL(m.assetStore);
    }

    private void DrawModules()
    {
        InitModules();
        if (modules.Count == 0) return;

        EditorGUILayout.LabelField("Want more features? Add them here..");

        moduleScroll = EditorGUILayout.BeginScrollView(moduleScroll, GUILayout.Height(156));
        GUILayout.BeginHorizontal();
        for (var i = 0; i < modules.Count; ++i) DrawModule(modules[i]);
        GUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }


    private class Module
    {
        public readonly string assetStore;
        public readonly Texture2D texture;

        public Module(string url, string img)
        {
            assetStore = url;
            texture = Resources.Load<Texture2D>(img);
        }
    }
}