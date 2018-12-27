//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JBooth.MicroSplat
{
    public class MicroSplatUtilities
    {
        private static Dictionary<string, Texture2D> autoTextures;


        private static readonly Dictionary<string, bool> rolloutStates = new Dictionary<string, bool>();
        private static GUIStyle rolloutStyle;

        private static readonly List<TextureArrayPreviewCache> previewCache = new List<TextureArrayPreviewCache>(32);


        // REPLACEMENT FOR SELECTION GRID cause it's crap
        private static Texture2D selectedTex;
        private static Texture2D labelBackgroundTex;

        private static readonly Dictionary<int, Texture2D> cachedSelectionImages = new Dictionary<int, Texture2D>();

        public static string MakeRelativePath(string path)
        {
            path = path.Replace(Application.dataPath, "Assets/");
            path = path.Replace("\\", "/");
            path = path.Replace("//", "/");
            return path;
        }

        public static string MakeAbsolutePath(string path)
        {
            if (!path.StartsWith(Application.dataPath))
                if (path.StartsWith("Assets"))
                {
                    path = path.Substring(6);
                    path = Application.dataPath + path;
                }

            return path;
        }

        public static void EnforceDefaultTexture(MaterialProperty texProp, string autoTextureName)
        {
            if (texProp.textureValue == null)
            {
                var def = GetAutoTexture(autoTextureName);
                if (def != null) texProp.textureValue = def;
            }
        }

        public static Texture2D GetAutoTexture(string name)
        {
            if (autoTextures == null)
            {
                autoTextures = new Dictionary<string, Texture2D>();
                var guids = AssetDatabase.FindAssets("microsplat_def_ t:texture2D");
                for (var i = 0; i < guids.Length; ++i)
                {
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guids[i]));
                    autoTextures.Add(tex.name, tex);
                }
            }

            Texture2D ret;
            if (autoTextures.TryGetValue(name, out ret)) return ret;
            return null;
        }

        public static void DrawTextureField(MicroSplatTerrain t, GUIContent content, ref Texture2D tex, string keword,
            string keyword2 = null)
        {
            if (t.templateMaterial.IsKeywordEnabled(keword))
            {
                if (keyword2 != null && !t.templateMaterial.IsKeywordEnabled(keyword2)) return;
                EditorGUI.BeginChangeCheck();

                tex = EditorGUILayout.ObjectField(content, tex, typeof(Texture2D), false) as Texture2D;

                if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(t);
            }
        }

        public static bool DrawRollup(string text, bool defaultState = true, bool inset = false)
        {
            if (rolloutStyle == null)
            {
                rolloutStyle = GUI.skin.box;
                rolloutStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            }

            var oldColor = GUI.contentColor;
            GUI.contentColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            if (inset)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.GetControlRect(GUILayout.Width(40));
            }

            if (!rolloutStates.ContainsKey(text)) rolloutStates[text] = defaultState;
            if (GUILayout.Button(text, rolloutStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20)))
                rolloutStates[text] = !rolloutStates[text];
            if (inset)
            {
                EditorGUILayout.GetControlRect(GUILayout.Width(40));
                EditorGUILayout.EndHorizontal();
            }

            GUI.contentColor = oldColor;
            return rolloutStates[text];
        }

        public static void DrawSeparator()
        {
            EditorGUILayout.Separator();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Separator();
        }

        private static Texture2D FindInPreviewCache(int hash)
        {
            for (var i = 0; i < previewCache.Count; ++i)
                if (previewCache[i].hash == hash)
                    return previewCache[i].texture;
            return null;
        }

        public static void ClearPreviewCache()
        {
            for (var i = 0; i < previewCache.Count; ++i)
                if (previewCache[i].texture != null)
                    Object.DestroyImmediate(previewCache[i].texture);
            previewCache.Clear();
        }

        // workaround for unity's editor bug w/ linear. Blit texture into linear render buffer,
        // readback int linear texture, write into PNG, read back from PNG. Cause Unity..
        public static void FixUnityEditorLinearBug(ref Texture2D tex, int width = 128, int height = 128)
        {
            var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear);
            Graphics.Blit(tex, rt);
            Object.DestroyImmediate(tex);
            var tempTex = new Texture2D(width, height, TextureFormat.RGBA32, true, true);
            RenderTexture.active = rt;
            tempTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            RenderTexture.active = null;
            tex = new Texture2D(2, 2, TextureFormat.RGBA32, true, true);
            tex.LoadImage(tempTex.EncodeToPNG());
            Object.DestroyImmediate(tempTex);
        }

        public static int DrawTextureSelector(int textureIndex, Texture2DArray ta, bool compact = false)
        {
            if (ta == null)
                return textureIndex;
            var count = ta.depth;
            if (count > 32)
                count = 32;
            var disp = Texture2D.blackTexture;
            if (ta != null)
            {
                var hash = ta.GetHashCode() * (textureIndex + 7);
                var hashed = FindInPreviewCache(hash);
                if (hashed == null)
                {
                    hashed = new Texture2D(ta.width, ta.height, ta.format, false);
                    Graphics.CopyTexture(ta, textureIndex, 0, hashed, 0, 0);
                    hashed.Apply(false, false);
                    //FixUnityEditorLinearBug(ref hashed);
                    var hd = new TextureArrayPreviewCache();
                    hd.hash = hash;
                    hd.texture = hashed;
                    previewCache.Add(hd);
                    if (previewCache.Count > 20)
                    {
                        hd = previewCache[0];
                        previewCache.RemoveAt(0);
                        if (hd.texture != null) Object.DestroyImmediate(hd.texture);
                    }
                }

                disp = hashed;
            }

            if (compact)
            {
                EditorGUILayout.BeginVertical();
                EditorGUI.DrawPreviewTexture(EditorGUILayout.GetControlRect(GUILayout.Width(110), GUILayout.Height(96)),
                    disp);
                textureIndex = EditorGUILayout.IntSlider(textureIndex, 0, count - 1, GUILayout.Width(120));
                EditorGUILayout.EndVertical();
            }
            else
            {
                textureIndex = EditorGUILayout.IntSlider("index", textureIndex, 0, count - 1);
                EditorGUI.DrawPreviewTexture(
                    EditorGUILayout.GetControlRect(GUILayout.Width(128), GUILayout.Height(128)), disp);
            }

            return textureIndex;
        }

        public static void WarnLinear(Texture2D tex)
        {
            if (tex != null)
            {
                var ai = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
                if (ai != null)
                {
                    var ti = ai as TextureImporter;
                    if (ti.sRGBTexture)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.HelpBox("Texture should be linear", MessageType.Error);
                        if (GUILayout.Button("Fix"))
                        {
                            ti.sRGBTexture = false;
                            ti.SaveAndReimport();
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }

        public static string RelativePathFromAsset(Object o)
        {
            string path = null;
            if (o != null) path = AssetDatabase.GetAssetPath(o);
            if (string.IsNullOrEmpty(path))
            {
                var selectionPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (!string.IsNullOrEmpty(selectionPath)) path = selectionPath;
            }

            if (string.IsNullOrEmpty(path)) path = SceneManager.GetActiveScene().path;

            if (string.IsNullOrEmpty(path)) path = "Assets";

            path = path.Replace("\\", "/");
            if (path.Contains("/")) path = path.Substring(0, path.LastIndexOf("/"));
            path += "/MicroSplatData";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return path;
        }

        private static void SetupSelectionGrid()
        {
            if (selectedTex == null)
            {
                selectedTex = new Texture2D(128, 128, TextureFormat.ARGB32, false);
                for (var x = 0; x < 128; ++x)
                for (var y = 0; y < 128; ++y)
                    if (x < 1 || x > 126 || y < 1 || y > 126)
                        selectedTex.SetPixel(x, y, new Color(0, 0, 128));
                    else
                        selectedTex.SetPixel(x, y, new Color(0, 0, 0, 0));
                selectedTex.Apply();
            }

            if (labelBackgroundTex == null)
            {
                labelBackgroundTex = new Texture2D(1, 1);
                labelBackgroundTex.SetPixel(0, 0, new Color(0.0f, 0.0f, 0.0f, 0.5f));
                labelBackgroundTex.Apply();
            }
        }

        private static int DrawSelectionElement(Rect r, int i, int index, Texture2D image, string label, int elemSize)
        {
            if (GUI.Button(r, "", GUI.skin.box)) index = i;
            GUI.DrawTexture(r, image != null ? image : Texture2D.blackTexture, ScaleMode.ScaleToFit, false);
            if (i == index) GUI.DrawTexture(r, selectedTex, ScaleMode.ScaleToFit, true);

            if (!string.IsNullOrEmpty(label))
            {
                r.height = 18;
                var v = r.center;
                v.y += elemSize - 18;
                r.center = v;

                var contentColor = GUI.contentColor;
                GUI.DrawTexture(r, labelBackgroundTex, ScaleMode.StretchToFill);
                GUI.contentColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                GUI.Box(r, label);
                GUI.contentColor = contentColor;
            }

            return index;
        }

        private static int DrawSelectionElement(Rect r, int i, int index, Texture2DArray texArray, string label,
            int elemSize)
        {
            if (GUI.Button(r, "", GUI.skin.box)) index = i;

            var hash = texArray.GetHashCode() * 7 + i * 31;
            Texture2D image = null;
            var found = cachedSelectionImages.TryGetValue(hash, out image);

            if (!found || image == null)
            {
                var tmp = new Texture2D(texArray.width, texArray.height, texArray.format, false, false);
                Graphics.CopyTexture(texArray, i, 0, tmp, 0, 0);
                tmp.Apply();
                var rt = RenderTexture.GetTemporary(128, 128, 0, RenderTextureFormat.ARGB32,
                    RenderTextureReadWrite.sRGB);

                Graphics.Blit(tmp, rt);
                RenderTexture.active = rt;

                image = new Texture2D(128, 128, TextureFormat.RGBA32, true, false);
                image.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
                image.Apply();
                cachedSelectionImages[hash] = image;

                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(rt);
            }

            GUI.DrawTexture(r, image, ScaleMode.ScaleToFit, false);
            if (i == index) GUI.DrawTexture(r, selectedTex, ScaleMode.ScaleToFit, true);
            if (!string.IsNullOrEmpty(label))
            {
                r.height = 18;
                var v = r.center;
                v.y += elemSize - 18;
                r.center = v;

                var contentColor = GUI.contentColor;
                GUI.DrawTexture(r, labelBackgroundTex, ScaleMode.StretchToFill);
                GUI.contentColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                GUI.Box(r, label);
                GUI.contentColor = contentColor;
            }

            return index;
        }

        public static int SelectionGrid(int index, Texture2D[] contents, int elemSize)
        {
            SetupSelectionGrid();
            var width = Mathf.Max(1, (int) EditorGUIUtility.currentViewWidth / (elemSize + 3));
            EditorGUILayout.BeginVertical(GUILayout.Height((contents.Length / width + 1) * elemSize + 5));
            var w = 0;
            EditorGUILayout.BeginHorizontal();
            for (var i = 0; i < contents.Length; ++i)
            {
                var r = EditorGUILayout.GetControlRect(GUILayout.Width(elemSize), GUILayout.Height(elemSize));
                index = DrawSelectionElement(r, i, index, contents[i], contents[i].name, elemSize);

                w++;
                if (w >= width)
                {
                    EditorGUILayout.EndHorizontal();
                    w = 0;
                    EditorGUILayout.BeginHorizontal();
                }
            }

            var e = Event.current;
            if (e.isKey && e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.LeftArrow)
                {
                    index--;
                    e.Use();
                }

                if (e.keyCode == KeyCode.RightArrow)
                {
                    index++;
                    e.Use();
                }

                if (e.keyCode == KeyCode.DownArrow)
                {
                    index += width;
                    e.Use();
                }

                if (e.keyCode == KeyCode.UpArrow)
                {
                    index -= width;
                    e.Use();
                }
            }

            index = Mathf.Clamp(index, 0, contents.Length - 1);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            return index;
        }

        public static int SelectionGrid(int index, Texture2DArray contents, int elemSize)
        {
            SetupSelectionGrid();
            var width = Mathf.Max(1, (int) EditorGUIUtility.currentViewWidth / (elemSize + 5));
            EditorGUILayout.BeginVertical(GUILayout.Height((contents.depth / width + 1) * elemSize + 10));
            var w = 0;
            EditorGUILayout.BeginHorizontal();
            for (var i = 0; i < contents.depth; ++i)
            {
                var r = EditorGUILayout.GetControlRect(GUILayout.Width(elemSize), GUILayout.Height(elemSize));
                index = DrawSelectionElement(r, i, index, contents, "", elemSize);

                w++;
                if (w >= width)
                {
                    EditorGUILayout.EndHorizontal();
                    w = 0;
                    EditorGUILayout.BeginHorizontal();
                }
            }

            var e = Event.current;
            if (e.isKey && e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.LeftArrow)
                {
                    index--;
                    e.Use();
                }

                if (e.keyCode == KeyCode.RightArrow)
                {
                    index++;
                    e.Use();
                }

                if (e.keyCode == KeyCode.DownArrow)
                {
                    index += width;
                    e.Use();
                }

                if (e.keyCode == KeyCode.UpArrow)
                {
                    index -= width;
                    e.Use();
                }
            }

            index = Mathf.Clamp(index, 0, contents.depth - 1);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            return index;
        }


        // for caching previews
        public class TextureArrayPreviewCache
        {
            public int hash;
            public Texture2D texture;
        }

        //
    }
}