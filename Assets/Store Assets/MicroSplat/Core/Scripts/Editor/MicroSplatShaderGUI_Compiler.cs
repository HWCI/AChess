//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JBooth.MicroSplat;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public partial class MicroSplatShaderGUI : ShaderGUI
{
    public enum PassType
    {
        Surface = 0,
        Color,
        Meta,
        Depth,
        Shadow
    }

    // hacky, but prevents having to change the module api..
    public static PassType passType = PassType.Surface;

    private static readonly List<IRenderLoopAdapter> availableRenderLoops = new List<IRenderLoopAdapter>();


    [MenuItem("Assets/Create/Shader/MicroSplat Shader")]
    private static void NewShader2()
    {
        NewShader();
    }

    [MenuItem("Assets/Create/MicroSplat/MicroSplat Shader")]
    public static Shader NewShader()
    {
        var path = "Assets";
        foreach (var obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (File.Exists(path)) path = Path.GetDirectoryName(path);
            break;
        }

        path = path.Replace("\\", "/");
        path = AssetDatabase.GenerateUniqueAssetPath(path + "/MicroSplat.shader");
        var name = path.Substring(path.LastIndexOf("/"));
        name = name.Substring(0, name.IndexOf("."));
        var compiler = new MicroSplatCompiler();
        compiler.Init();
        var ret = compiler.Compile(new string[1] {"_MSRENDERLOOP_SURFACESHADER"}, name, name);
        File.WriteAllText(path, ret);
        AssetDatabase.Refresh();
        return AssetDatabase.LoadAssetAtPath<Shader>(path);
    }

    public static Material NewShaderAndMaterial(string path, string name, string[] keywords = null)
    {
        var shaderPath = AssetDatabase.GenerateUniqueAssetPath(path + "/MicroSplat.shader");
        var shaderBasePath = shaderPath.Replace(".shader", "_Base.shader");
        var matPath = AssetDatabase.GenerateUniqueAssetPath(path + "/MicroSplat.mat");

        var compiler = new MicroSplatCompiler();
        compiler.Init();

        if (keywords == null) keywords = new string[0];

        var baseName = "Hidden/MicroSplat/" + name + "_Base";

        var baseShader = compiler.Compile(keywords, baseName);
        var regularShader = compiler.Compile(keywords, name, baseName);

        File.WriteAllText(shaderPath, regularShader);
        File.WriteAllText(shaderBasePath, baseShader);


        if (keywords.Contains("_MESHOVERLAYSPLATS"))
        {
            var meshOverlayShader = compiler.Compile(keywords, name, null, true);
            File.WriteAllText(shaderPath.Replace(".shader", "_MeshOverlay.shader"), meshOverlayShader);
        }

        AssetDatabase.Refresh();
        var s = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);

        var m = new Material(s);
        m.shaderKeywords = keywords;
        AssetDatabase.CreateAsset(m, matPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath<Material>(matPath);
    }

    public static Material NewShaderAndMaterial(Terrain t)
    {
        var path = MicroSplatUtilities.RelativePathFromAsset(t.terrainData);
        return NewShaderAndMaterial(path, t.name);
    }

    public class MicroSplatCompiler
    {
        private static readonly StringBuilder sBuilder = new StringBuilder(256000);
        public List<FeatureDescriptor> extensions = new List<FeatureDescriptor>();

        public IRenderLoopAdapter renderLoop;

        public string GetShaderModel(string[] features)
        {
            var minModel = "3.5";
            for (var i = 0; i < extensions.Count; ++i)
                if (extensions[i].RequiresShaderModel46())
                    minModel = "4.6";
            if (features.Contains("_FORCEMODEL46")) minModel = "4.6";
            if (features.Contains("_FORCEMODEL50")) minModel = "5.0";

            return minModel;
        }

        public void Init()
        {
            if (extensions.Count == 0)
            {
                var paths = AssetDatabase.FindAssets("microsplat_ t:TextAsset");
                for (var i = 0; i < paths.Length; ++i) paths[i] = AssetDatabase.GUIDToAssetPath(paths[i]);


                // init extensions
                var types = Assembly.GetExecutingAssembly().GetTypes();
                var possible = (from Type type in types
                    where type.IsSubclassOf(typeof(FeatureDescriptor))
                    select type).ToArray();

                for (var i = 0; i < possible.Length; ++i)
                {
                    var typ = possible[i];
                    var ext = Activator.CreateInstance(typ) as FeatureDescriptor;
                    ext.InitCompiler(paths);
                    extensions.Add(ext);
                }

                extensions.Sort(delegate(FeatureDescriptor p1, FeatureDescriptor p2)
                {
                    if (p1.DisplaySortOrder() != 0 || p2.DisplaySortOrder() != 0)
                        return p1.DisplaySortOrder().CompareTo(p2.DisplaySortOrder());
                    return p1.GetType().Name.CompareTo(p2.GetType().Name);
                });


                var adapters = (from Type type in types
                    where type.GetInterfaces().Contains(typeof(IRenderLoopAdapter))
                    select type).ToArray();

                availableRenderLoops.Clear();
                for (var i = 0; i < adapters.Length; ++i)
                {
                    var typ = adapters[i];
                    var adapter = Activator.CreateInstance(typ) as IRenderLoopAdapter;
                    adapter.Init(paths);
                    availableRenderLoops.Add(adapter);
                }
            }
        }


        private void WriteFeatures(string[] features, StringBuilder sb)
        {
            sb.AppendLine();
            for (var i = 0; i < features.Length; ++i) sb.AppendLine("      #define " + features[i] + " 1");

            sb.AppendLine();
        }

        private void WriteExtensions(string[] features, StringBuilder sb)
        {
            // sort for compile order
            extensions.Sort(delegate(FeatureDescriptor p1, FeatureDescriptor p2)
            {
                if (p1.CompileSortOrder() != p2.CompileSortOrder())
                    return p1.CompileSortOrder() < p2.CompileSortOrder() ? -1 : 1;
                return p1.GetType().Name.CompareTo(p2.GetType().Name);
            });

            for (var i = 0; i < extensions.Count; ++i)
            {
                var ext = extensions[i];
                if (ext.GetVersion() == MicroSplatVersion) extensions[i].WriteFunctions(sb);
            }

            // sort by name, then display order..
            extensions.Sort(delegate(FeatureDescriptor p1, FeatureDescriptor p2)
            {
                if (p1.DisplaySortOrder() != 0 || p2.DisplaySortOrder() != 0)
                    return p1.DisplaySortOrder().CompareTo(p2.DisplaySortOrder());
                return p1.GetType().Name.CompareTo(p2.GetType().Name);
            });
        }


        private void WriteProperties(string[] features, StringBuilder sb, bool blendable)
        {
            sb.AppendLine("   Properties {");

            var max4 = features.Contains("_MAX4TEXTURES");
            var max8 = features.Contains("_MAX8TEXTURES");
            var max12 = features.Contains("_MAX12TEXTURES");
            var max20 = features.Contains("_MAX20TEXTURES");
            var max24 = features.Contains("_MAX24TEXTURES");
            var max28 = features.Contains("_MAX28TEXTURES");
            var max32 = features.Contains("_MAX32TEXTURES");

            // always have this for UVs
            sb.AppendLine("      [HideInInspector] _Control0 (\"Control0\", 2D) = \"red\" {}");


            var custom = features.Contains("_CUSTOMSPLATTEXTURES");
            var controlName = "_Control";
            if (custom) controlName = "_CustomControl";


            if (custom) sb.AppendLine("      [HideInInspector] _CustomControl0 (\"Control0\", 2D) = \"red\" {}");

            if (!max4) sb.AppendLine("      [HideInInspector] " + controlName + "1 (\"Control1\", 2D) = \"black\" {}");
            if (!max4 && !max8)
                sb.AppendLine("      [HideInInspector] " + controlName + "2 (\"Control2\", 2D) = \"black\" {}");
            if (!max4 && !max8 && !max12)
                sb.AppendLine("      [HideInInspector] " + controlName + "3 (\"Control3\", 2D) = \"black\" {}");
            if (max20 || max24 || max28 || max32)
                sb.AppendLine("      [HideInInspector] " + controlName + "4 (\"Control4\", 2D) = \"black\" {}");
            if (max24 || max28 || max32)
                sb.AppendLine("      [HideInInspector] " + controlName + "5 (\"Control5\", 2D) = \"black\" {}");
            if (max28 || max32)
                sb.AppendLine("      [HideInInspector] " + controlName + "6 (\"Control6\", 2D) = \"black\" {}");
            if (max32) sb.AppendLine("      [HideInInspector] " + controlName + "7 (\"Control7\", 2D) = \"black\" {}");

            for (var i = 0; i < extensions.Count; ++i)
            {
                var ext = extensions[i];
                if (ext.GetVersion() == MicroSplatVersion) ext.WriteProperties(features, sb);
                sb.AppendLine("");
            }

            sb.AppendLine("   }");
        }

        public static bool HasDebugFeature(string[] features)
        {
            return features.Contains("_DEBUG_OUTPUT_ALBEDO") ||
                   features.Contains("_DEBUG_OUTPUT_NORMAL") ||
                   features.Contains("_DEBUG_OUTPUT_HEIGHT") ||
                   features.Contains("_DEBUG_OUTPUT_METAL") ||
                   features.Contains("_DEBUG_OUTPUT_SMOOTHNESS") ||
                   features.Contains("_DEBUG_OUTPUT_AO") ||
                   features.Contains("_DEBUG_OUTPUT_EMISSION");
        }

        public string Compile(string[] features, string name, string baseName = null, bool blendable = false)
        {
            Init();

            // get default render loop if it doesn't exist
            if (renderLoop == null)
                for (var i = 0; i < availableRenderLoops.Count; ++i)
                    if (availableRenderLoops[i].GetType() == typeof(SurfaceShaderRenderLoopAdapter))
                        renderLoop = availableRenderLoops[i];

            for (var i = 0; i < extensions.Count; ++i)
            {
                var ext = extensions[i];
                ext.Unpack(features);
            }

            sBuilder.Length = 0;
            var sb = sBuilder;
            sb.AppendLine("//////////////////////////////////////////////////////");
            sb.AppendLine("// MicroSplat");
            sb.AppendLine("// Copyright (c) Jason Booth");
            sb.AppendLine("//");
            sb.AppendLine("// Auto-generated shader code, don't hand edit!");
            sb.AppendLine("//   Compiled with MicroSplat " + MicroSplatVersion);
            sb.AppendLine("//   Unity : " + Application.unityVersion);
            sb.AppendLine("//   Platform : " + Application.platform);
            if (renderLoop != null) sb.AppendLine("//   RenderLoop : " + renderLoop.GetDisplayName());
            sb.AppendLine("//////////////////////////////////////////////////////");
            sb.AppendLine();

            if (!blendable && baseName == null)
                sb.Append("Shader \"Hidden/MicroSplat/");
            else
                sb.Append("Shader \"MicroSplat/");
            while (name.Contains("/")) name = name.Substring(name.IndexOf("/") + 1);
            sb.Append(name);
            if (blendable)
            {
                if (features.Contains("_MESHOVERLAYSPLATS"))
                    sb.Append("_MeshOverlay");
                else
                    sb.Append("_BlendWithTerrain");
            }

            sb.AppendLine("\" {");


            // props
            WriteProperties(features, sb, blendable);
            renderLoop.WriteShaderHeader(features, sb, this, blendable);


            for (var pass = 0; pass < renderLoop.GetNumPasses(); ++pass)
            {
                renderLoop.WritePassHeader(features, sb, this, pass, blendable);

                // don't remove
                sb.AppendLine();
                sb.AppendLine();

                WriteFeatures(features, sb);
                if (renderLoop == null)
                    sb.AppendLine("      #define _MSRENDERLOOP_SURFACESHADER 1");
                else
                    sb.AppendLine("      #define " + renderLoop.GetRenderLoopKeyword() + " 1");

                if (blendable)
                {
                    if (features.Contains("_MESHOVERLAYSPLATS"))
                        sb.AppendLine("      #define _MESHOVERLAYSPLATSSHADER 1");
                    else
                        sb.AppendLine("      #define _TERRAINBLENDABLESHADER 1");
                }

                renderLoop.WriteSharedCode(features, sb, this, pass, blendable);
                renderLoop.WriteVertexFunction(features, sb, this, pass, blendable);

                passType = renderLoop.GetPassType(pass);
                WriteExtensions(features, sb);


                renderLoop.WriteTerrainBody(features, sb, this, pass, blendable);

                renderLoop.WriteFragmentFunction(features, sb, this, pass, blendable);
            }


            renderLoop.WriteShaderFooter(features, sb, this, blendable, baseName);

            for (var i = 0; i < extensions.Count; ++i)
            {
                var ext = extensions[i];
                ext.OnPostGeneration(sb, features, name, baseName, blendable);
            }

            sb.AppendLine("");
            renderLoop.PostProcessShader(features, sb, this, blendable);
            var output = sb.ToString();

            // fix newline mixing warnings..
            output = Regex.Replace(output, "\r\n?|\n", Environment.NewLine);
            return output;
        }

        public void Compile(Material m, string shaderName = null)
        {
            var hash = 0;

            for (var i = 0; i < m.shaderKeywords.Length; ++i) hash += 31 + m.shaderKeywords[i].GetHashCode();
            var path = AssetDatabase.GetAssetPath(m.shader);
            var nm = m.shader.name;
            if (!string.IsNullOrEmpty(shaderName)) nm = shaderName;
            var baseName = "Hidden/" + nm + "_Base" + hash;

            var terrainShader = Compile(m.shaderKeywords, nm, baseName);
            if (renderLoop != null) m.EnableKeyword(renderLoop.GetRenderLoopKeyword());
            string blendShader = null;

            // strip extra feature from terrain blending to make it cheaper
            if (m.IsKeywordEnabled("_TERRAINBLENDING"))
            {
                var blendKeywords = new List<string>(m.shaderKeywords);
                if (m.IsKeywordEnabled("_TBDISABLE_DETAILNOISE") && blendKeywords.Contains("_DETAILNOISE"))
                    blendKeywords.Remove("_DETAILNOISE");
                if (m.IsKeywordEnabled("_TBDISABLE_DETAILNOISE") && blendKeywords.Contains("_ANTITILEARRAYDETAIL"))
                    blendKeywords.Remove("_ANTITILEARRAYDETAIL");
                if (m.IsKeywordEnabled("_TBDISABLE_DISTANCENOISE") && blendKeywords.Contains("_DISTANCENOISE"))
                    blendKeywords.Remove("_DISTANCENOISE");
                if (m.IsKeywordEnabled("_TBDISABLE_DISTANCENOISE") && blendKeywords.Contains("_ANTITILEARRAYDISTANCE"))
                    blendKeywords.Remove("_ANTITILEARRAYDISTANCE");
                if (m.IsKeywordEnabled("_TBDISABLE_DISTANCERESAMPLE") && blendKeywords.Contains("_DISTANCERESAMPLE"))
                    blendKeywords.Remove("_DISTANCERESAMPLE");

                blendShader = Compile(blendKeywords.ToArray(), nm, null, true);
            }


            string meshBlendShader = null;
            if (m.IsKeywordEnabled("_MESHOVERLAYSPLATS"))
            {
                var blendKeywords = new List<string>(m.shaderKeywords);
                if (blendKeywords.Contains("_TESSDISTANCE")) blendKeywords.Remove("_TESSDISTANCE");
                meshBlendShader = Compile(blendKeywords.ToArray(), nm, null, true);
            }

            File.WriteAllText(path, terrainShader);

            if (!m.IsKeywordEnabled("_MICROMESH"))
            {
                // generate fallback
                var oldKeywords = new string[m.shaderKeywords.Length];
                Array.Copy(m.shaderKeywords, oldKeywords, m.shaderKeywords.Length);
                m.DisableKeyword("_TESSDISTANCE");
                m.DisableKeyword("_PARALLAX");
                m.DisableKeyword("_DETAILNOISE");


                var fallback = Compile(m.shaderKeywords, baseName);
                m.shaderKeywords = oldKeywords;
                var fallbackPath = path.Replace(".shader", "_Base.shader");
                File.WriteAllText(fallbackPath, fallback);
            }


            var terrainBlendPath = path.Replace(".shader", "_TerrainObjectBlend.shader");
            var meshBlendPath = path.Replace(".shader", "_MeshOverlay.shader");

            if (blendShader != null) File.WriteAllText(terrainBlendPath, blendShader);
            if (meshBlendShader != null) File.WriteAllText(meshBlendPath, meshBlendShader);

            EditorUtility.SetDirty(m);
            AssetDatabase.Refresh();
            MicroSplatTerrain.SyncAll();
        }
    }
}