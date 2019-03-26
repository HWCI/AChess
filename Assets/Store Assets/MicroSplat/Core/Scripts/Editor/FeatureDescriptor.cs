//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Text;
using UnityEditor;
using UnityEngine;

namespace JBooth.MicroSplat
{
    public abstract class FeatureDescriptor
    {
        public enum Channel
        {
            R = 0,
            G,
            B,
            A
        }


        public enum V2Cannel
        {
            RG = 0,
            BA
        }

        private static bool drawPertexToggle = true;
        protected static int noPerTexToggleWidth = 20;

        private static readonly GUIContent globalButton = new GUIContent("G",
            "Make property driven by a global variable. Used to integrate with external weathering systems");


        private GUIStyle globalButtonPressedStyle;

        /// <summary>
        ///     All versions must match for module to be active
        /// </summary>
        /// <returns>The version.</returns>
        public abstract string GetVersion();


        // used when you have compiler ordering issues
        public virtual int CompileSortOrder()
        {
            return 0;
        }

        public virtual int DisplaySortOrder()
        {
            return 0;
        }

        public abstract string ModuleName();

        public virtual void OnPostGeneration(StringBuilder sb, string[] features, string name, string baseName = null,
            bool blendable = false)
        {
        }

        /// <summary>
        ///     Requireses the shader model46.
        /// </summary>
        /// <returns><c>true</c>, if shader model46 was requiresed, <c>false</c> otherwise.</returns>
        public virtual bool RequiresShaderModel46()
        {
            return false;
        }

        /// <summary>
        ///     DrawGUI for shader compiler feature options
        /// </summary>
        /// <param name="mat">Mat.</param>
        public abstract void DrawFeatureGUI(Material mat);

        /// <summary>
        ///     Draw the editor for the shaders options
        /// </summary>
        /// <param name="shaderGUI">Shader GU.</param>
        /// <param name="mat">Mat.</param>
        /// <param name="materialEditor">Material editor.</param>
        /// <param name="props">Properties.</param>
        public abstract void DrawShaderGUI(MicroSplatShaderGUI shaderGUI, Material mat, MaterialEditor materialEditor,
            MaterialProperty[] props);


        /// <summary>
        ///     Got per texture properties? Draw the GUI for them here..
        /// </summary>
        /// <param name="index">Index.</param>
        /// <param name="shaderGUI">Shader GU.</param>
        /// <param name="mat">Mat.</param>
        /// <param name="materialEditor">Material editor.</param>
        /// <param name="props">Properties.</param>
        public virtual void DrawPerTextureGUI(int index, Material mat, MicroSplatPropData propData)
        {
        }

        /// <summary>
        ///     Unpack your keywords from the material
        /// </summary>
        /// <param name="keywords">Keywords.</param>
        public abstract void Unpack(string[] keywords);

        /// <summary>
        ///     pack keywords to a string[]
        /// </summary>
        public abstract string[] Pack();

        /// <summary>
        ///     Init yourself
        /// </summary>
        /// <param name="paths">Paths.</param>
        public abstract void InitCompiler(string[] paths);

        /// <summary>
        ///     write property definitions to the shader
        /// </summary>
        /// <param name="features">Features.</param>
        /// <param name="sb">Sb.</param>
        public abstract void WriteProperties(string[] features, StringBuilder sb);

        /// <summary>
        ///     Write the core functions you use to the shader
        /// </summary>
        /// <param name="sb">Sb.</param>
        public abstract void WriteFunctions(StringBuilder sb);

        /// <summary>
        ///     Compute rough cost parameters for your section of the shader
        /// </summary>
        /// <param name="features">
        ///     List of material features.
        ///     <param>
        ///         <param name="arraySampleCount">Array sample count.</param>
        ///         <param name="textureSampleCount">Texture sample count.</param>
        ///         <param name="maxSamples">Max samples.</param>
        ///         <param name="tessellationSamples">Tessellation samples.</param>
        ///         <param name="depTexReadLevel">Dep tex read level.</param>
        public abstract void ComputeSampleCounts(string[] features, ref int arraySampleCount,
            ref int textureSampleCount, ref int maxSamples,
            ref int tessellationSamples, ref int depTexReadLevel);


        public void Pack(Material m)
        {
            var pck = Pack();
            for (var i = 0; i < pck.Length; ++i) m.EnableKeyword(pck[i]);
        }

        private static bool PerTexToggle(Material mat, string keyword)
        {
            if (drawPertexToggle)
            {
                var enabled = mat.IsKeywordEnabled(keyword);
                var newEnabled = EditorGUILayout.Toggle(enabled, GUILayout.Width(20));
                if (enabled != newEnabled)
                {
                    if (newEnabled)
                        mat.EnableKeyword(keyword);
                    else
                        mat.DisableKeyword(keyword);
                }

                return newEnabled;
            }

            EditorGUILayout.LabelField("", GUILayout.Width(noPerTexToggleWidth));
            drawPertexToggle = true;
            return mat.IsKeywordEnabled(keyword);
        }

        protected static void InitPropData(int pixel, MicroSplatPropData propData, Color defaultValues)
        {
            if (propData == null) return;
            // we reserve the last row of potential values as an initialization bit. 
            if (propData.GetValue(pixel, 15) == new Color(0, 0, 0, 0))
            {
                for (var i = 0; i < 32; ++i) propData.SetValue(i, pixel, defaultValues);
                propData.SetValue(pixel, 15, Color.white);
            }
        }

        protected static bool DrawPerTexFloatSlider(int curIdx, int pixel, string keyword, Material mat,
            MicroSplatPropData propData, Channel channel,
            GUIContent label, float min = 0, float max = 0)
        {
            EditorGUILayout.BeginHorizontal();
            var enabled = PerTexToggle(mat, keyword);
            GUI.enabled = enabled;

            var c = propData.GetValue(curIdx, pixel);
            var v = c[(int) channel];
            var nv = v;
            if (min != max)
                nv = EditorGUILayout.Slider(label, v, min, max);
            else
                nv = EditorGUILayout.FloatField(label, v);
            if (nv != v)
            {
                c[(int) channel] = nv;
                propData.SetValue(curIdx, pixel, c);
            }

            if (GUILayout.Button("All", GUILayout.Width(40)))
                for (var i = 0; i < 16; ++i)
                    propData.SetValue(i, pixel, (int) channel, nv);

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            return enabled;
        }

        protected static bool DrawPerTexVector2(int curIdx, int pixel, string keyword, Material mat,
            MicroSplatPropData propData, V2Cannel channel,
            GUIContent label)
        {
            EditorGUILayout.BeginHorizontal();
            var enabled = PerTexToggle(mat, keyword);
            GUI.enabled = enabled;

            var c = propData.GetValue(curIdx, pixel);
            var v2 = new Vector2(c.r, c.g);
            if (channel == V2Cannel.BA)
            {
                v2.x = c.b;
                v2.y = c.a;
            }

            var nv = v2;

            nv = EditorGUILayout.Vector2Field(label, v2);

            if (nv != v2)
            {
                if (channel == V2Cannel.RG)
                {
                    c.r = nv.x;
                    c.g = nv.y;
                }
                else
                {
                    c.b = nv.x;
                    c.a = nv.y;
                }

                propData.SetValue(curIdx, pixel, c);
            }

            if (GUILayout.Button("All", GUILayout.Width(40)))
                for (var i = 0; i < 16; ++i)
                {
                    // don't erase other pixels..
                    var fv = propData.GetValue(i, pixel);
                    if (channel == V2Cannel.RG)
                    {
                        c.r = nv.x;
                        c.g = nv.y;
                    }
                    else
                    {
                        c.b = nv.x;
                        c.a = nv.y;
                    }

                    propData.SetValue(i, pixel, fv);
                }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            return enabled;
        }

        protected static bool DrawPerTexVector2Vector2(int curIdx, int pixel, string keyword, Material mat,
            MicroSplatPropData propData,
            GUIContent label, GUIContent label2)
        {
            EditorGUILayout.BeginHorizontal();
            var enabled = PerTexToggle(mat, keyword);
            GUI.enabled = enabled;

            var c = propData.GetValue(curIdx, pixel);
            var v1 = new Vector2(c.r, c.g);
            var v2 = new Vector2(c.b, c.a);
            var nv1 = v1;
            var nv2 = v2;
            EditorGUILayout.BeginVertical();
            nv1 = EditorGUILayout.Vector2Field(label, v1);
            nv2 = EditorGUILayout.Vector2Field(label2, v2);
            EditorGUILayout.EndVertical();

            if (nv1 != v1 || nv2 != v2)
            {
                c.r = nv1.x;
                c.g = nv1.y;
                c.b = nv2.x;
                c.a = nv2.y;
                propData.SetValue(curIdx, pixel, c);
            }

            if (GUILayout.Button("All", GUILayout.Width(40)))
            {
                c.r = nv1.x;
                c.g = nv1.y;
                c.b = nv2.x;
                c.a = nv2.y;
                for (var i = 0; i < 16; ++i) propData.SetValue(i, pixel, c);
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            return enabled;
        }

        protected bool DrawPerTexColor(int curIdx, int pixel, string keyword, Material mat, MicroSplatPropData propData,
            GUIContent label, bool hasAlpha)
        {
            EditorGUILayout.BeginHorizontal();
            var enabled = PerTexToggle(mat, keyword);
            GUI.enabled = enabled;
            var c = propData.GetValue(curIdx, pixel);
            var nv = EditorGUILayout.ColorField(label, c);
            if (nv != c)
            {
                if (!hasAlpha) nv.a = c.a;
                propData.SetValue(curIdx, pixel, nv);
            }

            if (GUILayout.Button("All", GUILayout.Width(40)))
                for (var i = 0; i < 16; ++i)
                {
                    if (!hasAlpha) nv.a = propData.GetValue(i, pixel).a;
                    propData.SetValue(i, pixel, nv);
                }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            return enabled;
        }

        protected static bool DrawPerTexPopUp(int curIdx, int pixel, string keyword, Material mat,
            MicroSplatPropData propData, Channel channel,
            GUIContent label, GUIContent[] options, float[] values)
        {
            EditorGUILayout.BeginHorizontal();
            var enabled = PerTexToggle(mat, keyword);
            GUI.enabled = enabled;
            var c = propData.GetValue(curIdx, pixel);
            var v = c[(int) channel];

            var selected = -1;

            if (values.Length == 0 || options.Length == 0)
            {
                selected = -1;
            }
            else if (options.Length == 1 || values.Length == 1 || values[0] >= v)
            {
                selected = 0;
            }
            else if (values[values.Length - 1] < v)
            {
                selected = values.Length - 1;
            }
            else
            {
                var length = options.Length < values.Length ? options.Length : values.Length;
                var dist = -1f;

                for (var i = 0; i < length; i++)
                    if (values[i] == v)
                    {
                        selected = i;
                        break;
                    }
                    else
                    {
                        var diff = Mathf.Abs(values[i] - v);
                        if (dist < 0)
                        {
                            dist = diff;
                            selected = i;
                        }
                        else if (diff < dist)
                        {
                            dist = diff;
                            selected = i;
                        }
                    }
            }

            selected = EditorGUILayout.Popup(label, selected, options);
            v = selected >= 0 ? values[selected] : 0;
            c[(int) channel] = v;
            propData.SetValue(curIdx, pixel, c);

            if (GUILayout.Button("All", GUILayout.Width(40)))
                for (var i = 0; i < 16; ++i)
                {
                    var nv = propData.GetValue(i, pixel);
                    nv[(int) channel] = v;
                    propData.SetValue(i, pixel, nv);
                }

            GUI.enabled = true;
            drawPertexToggle = true;
            EditorGUILayout.EndHorizontal();

            return enabled;
        }


        protected static void DrawPerTexVector2NoToggle(int curIdx, int pixel, string keyword, Material mat,
            MicroSplatPropData propData, V2Cannel channel,
            GUIContent label)
        {
            drawPertexToggle = false;
            DrawPerTexVector2(curIdx, pixel, keyword, mat, propData, channel, label);
        }

        protected static void DrawPerTexVector2Vector2NoToggle(int curIdx, int pixel, string keyword, Material mat,
            MicroSplatPropData propData,
            GUIContent label, GUIContent label2)
        {
            drawPertexToggle = false;
            DrawPerTexVector2Vector2(curIdx, pixel, keyword, mat, propData, label, label2);
        }

        protected static void DrawPerTexFloatSliderNoToggle(int curIdx, int pixel, string keyword, Material mat,
            MicroSplatPropData propData, Channel channel,
            GUIContent label, float min = 0, float max = 0)
        {
            drawPertexToggle = false;
            DrawPerTexFloatSlider(curIdx, pixel, keyword, mat, propData, channel, label, min, max);
        }

        protected static void DrawPerTexColorNoToggle(int curIdx, int pixel, MicroSplatPropData propData,
            GUIContent label)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(20));
            var c = propData.GetValue(curIdx, pixel);
            var nv = EditorGUILayout.ColorField(label, c);
            if (nv != c) propData.SetValue(curIdx, pixel, nv);

            if (GUILayout.Button("All", GUILayout.Width(40)))
                for (var i = 0; i < 16; ++i)
                    propData.SetValue(i, pixel, nv);

            EditorGUILayout.EndHorizontal();
            drawPertexToggle = true;
        }

        protected static void DrawPerTexPopUpNoToggle(int curIdx, int pixel, string keyword, Material mat,
            MicroSplatPropData propData, Channel channel,
            GUIContent label, GUIContent[] options, float[] values)
        {
            drawPertexToggle = false;
            DrawPerTexPopUp(curIdx, pixel, keyword, mat, propData, channel, label, options, values);
        }

        protected bool DrawGlobalToggle(string keyword, Material mat)
        {
            var b = mat.IsKeywordEnabled(keyword);
            if (globalButtonPressedStyle == null)
            {
                globalButtonPressedStyle = new GUIStyle(GUI.skin.label);
                globalButtonPressedStyle.normal.background = new Texture2D(1, 1);
                globalButtonPressedStyle.normal.background.SetPixel(0, 0, Color.yellow);
                globalButtonPressedStyle.normal.background.Apply();
                globalButtonPressedStyle.normal.textColor = Color.black;
            }

            var pressed = GUILayout.Button(globalButton, b ? globalButtonPressedStyle : GUI.skin.label,
                GUILayout.Width(14));


            if (pressed)
            {
                if (b)
                    mat.DisableKeyword(keyword);
                else
                    mat.EnableKeyword(keyword);
                b = !b;
                EditorUtility.SetDirty(mat);
            }

            return b;
        }
    }
}