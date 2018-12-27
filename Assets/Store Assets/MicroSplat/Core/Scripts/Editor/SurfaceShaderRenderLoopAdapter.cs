//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace JBooth.MicroSplat
{
    public class SurfaceShaderRenderLoopAdapter : IRenderLoopAdapter
    {
        private const string declareTerrain =
            "      #pragma surface surf Standard vertex:vert fullforwardshadows addshadow";

        private const string declareTerrainDebug = "      #pragma surface surf Unlit vertex:vert nofog";

        private const string declareTerrainTess =
            "      #pragma surface surf Standard vertex:disp tessellate:TessDistance fullforwardshadows addshadow";

        private const string declareBlend =
            "      #pragma surface blendSurf TerrainBlendable fullforwardshadows addshadow decal:blend";

        private const string declareMeshBlend =
            "      #pragma surface surf Standard fullforwardshadows addshadow decal:blend";

        private static TextAsset vertexFunc;
        private static TextAsset fragmentFunc;
        private static TextAsset terrainBlendBody;
        private static TextAsset terrainBody;
        private static TextAsset sharedInc;
        private static TextAsset meshBlendBody;

        public string GetDisplayName()
        {
            return "Surface Shader";
        }

        public string GetRenderLoopKeyword()
        {
            return "_MSRENDERLOOP_SURFACESHADER";
        }

        public int GetNumPasses()
        {
            return 1;
        }

        public void WriteShaderHeader(string[] features, StringBuilder sb,
            MicroSplatShaderGUI.MicroSplatCompiler compiler, bool blend)
        {
            sb.AppendLine();
            sb.AppendLine("   CGINCLUDE");

            if (features.Contains("_BDRF1") || features.Contains("_BDRF2") || features.Contains("_BDRF3"))
            {
                if (features.Contains("_BDRF1"))
                    sb.AppendLine("      #define UNITY_BRDF_PBS BRDF1_Unity_PBS");
                else if (features.Contains("_BDRF2"))
                    sb.AppendLine("      #define UNITY_BRDF_PBS BRDF2_Unity_PBS");
                else if (features.Contains("_BDRF3")) sb.AppendLine("      #define UNITY_BRDF_PBS BRDF3_Unity_PBS");
            }

            sb.AppendLine("   ENDCG");
            sb.AppendLine();

            sb.AppendLine("   SubShader {");

            sb.AppendLine("      Tags{ \"RenderType\" = \"Opaque\"  \"Queue\" = \"Geometry+100\" }");
            sb.AppendLine("      Cull Back");
            sb.AppendLine("      ZTest LEqual");
            if (blend) sb.AppendLine("      BLEND ONE ONE");
            sb.AppendLine("      CGPROGRAM");
        }

        public void WritePassHeader(string[] features, StringBuilder sb,
            MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend)
        {
            sb.AppendLine("      #pragma exclude_renderers d3d9");

            sb.AppendLine("      #include \"UnityCG.cginc\"");
            sb.AppendLine("      #include \"AutoLight.cginc\"");
            sb.AppendLine("      #include \"Lighting.cginc\"");
            sb.AppendLine("      #include \"UnityPBSLighting.cginc\"");
            sb.AppendLine("      #include \"UnityStandardBRDF.cginc\"");
            sb.AppendLine();

            var pragma = "";
            if (blend)
            {
                if (features.Contains("_MESHOVERLAYSPLATS"))
                    pragma = declareMeshBlend;
                else
                    pragma = declareBlend;
            }
            else if (!features.Contains("_TESSDISTANCE"))
            {
                if (MicroSplatShaderGUI.MicroSplatCompiler.HasDebugFeature(features))
                {
                    pragma = declareTerrainDebug;
                    if (features.Contains("_ALPHAHOLE") || features.Contains("_ALPHABELOWHEIGHT"))
                        pragma += " addshadow";
                }
                else
                {
                    pragma = declareTerrain;
                }
            }
            else
            {
                pragma = declareTerrainTess;
            }

            if (features.Contains("_BDRFLAMBERT")) pragma = pragma.Replace("Standard", "Lambert");
            sb.Append(pragma);

            if (!blend)
                if (features.Contains("_BDRF1") || features.Contains("_BDRF2") || features.Contains("_BDRF3"))
                    sb.Append(" exclude_path:deferred");

            // don't remove
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            // for 2018..
#if UNITY_2018_3_OR_NEWER
            sb.AppendLine(
                "     #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd");
#endif

            sb.AppendLine("      #pragma target " + compiler.GetShaderModel(features));
        }


        public void WriteVertexFunction(string[] features, StringBuilder sb,
            MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend)
        {
            sb.AppendLine(vertexFunc.text);
        }

        public void WriteFragmentFunction(string[] features, StringBuilder sb,
            MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend)
        {
            sb.AppendLine(fragmentFunc.text);
            if (blend)
            {
                if (features.Contains("_MESHOVERLAYSPLATS") && meshBlendBody != null)
                    sb.AppendLine(meshBlendBody.text);
                else if (features.Contains("_TERRAINBLENDING") && terrainBlendBody != null)
                    sb.AppendLine(terrainBlendBody.text);
            }

            sb.AppendLine("ENDCG\n\n   }");
        }


        public void WriteShaderFooter(string[] features, StringBuilder sb,
            MicroSplatShaderGUI.MicroSplatCompiler compiler, bool blend, string baseName)
        {
            if (blend)
            {
                sb.AppendLine("   CustomEditor \"MicroSplatBlendableMaterialEditor\"");
            }
            else if (baseName != null)
            {
                sb.AppendLine("   Dependency \"AddPassShader\" = \"Hidden/MicroSplat/AddPass\"");
                sb.AppendLine("   Dependency \"BaseMapShader\" = \"" + baseName + "\"");
                sb.AppendLine("   CustomEditor \"MicroSplatShaderGUI\"");
            }

            sb.AppendLine("   Fallback \"Nature/Terrain/Diffuse\"");
            sb.Append("}");
        }

        public void Init(string[] paths)
        {
            for (var i = 0; i < paths.Length; ++i)
            {
                var p = paths[i];
                if (p.EndsWith("microsplat_terrain_surface_vertex.txt"))
                    vertexFunc = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                if (p.EndsWith("microsplat_terrain_surface_fragment.txt"))
                    fragmentFunc = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                if (p.EndsWith("microsplat_terrainblend_body.txt"))
                    terrainBlendBody = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                if (p.EndsWith("microsplat_meshoverlay_body.txt"))
                    meshBlendBody = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                if (p.EndsWith("microsplat_terrain_body.txt"))
                    terrainBody = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                if (p.EndsWith("microsplat_shared.txt")) sharedInc = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
        }

        public void PostProcessShader(string[] features, StringBuilder sb,
            MicroSplatShaderGUI.MicroSplatCompiler compiler, bool blend)
        {
        }

        public void WriteSharedCode(string[] features, StringBuilder sb,
            MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend)
        {
            sb.AppendLine(sharedInc.text);
        }

        public void WriteTerrainBody(string[] features, StringBuilder sb,
            MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend)
        {
            sb.AppendLine(terrainBody.text);
        }

        public MicroSplatShaderGUI.PassType GetPassType(int i)
        {
            return MicroSplatShaderGUI.PassType.Surface;
        }

        public string GetVersion()
        {
            return MicroSplatShaderGUI.MicroSplatVersion;
        }
    }
}