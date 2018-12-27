//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


using UnityEditor;
using UnityEditor.Callbacks;

namespace JBooth.MicroSplat
{
    [InitializeOnLoad]
    public class MicroSplatDefines
    {
        private const string sMicroSplatDefine = "__MICROSPLAT__";

        static MicroSplatDefines()
        {
            InitDefine(sMicroSplatDefine);
        }

        public static bool HasDefine(string def)
        {
            var target = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
            return defines.Contains(def);
        }

        public static void InitDefine(string def)
        {
            var target = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
            if (!defines.Contains(def))
            {
                if (string.IsNullOrEmpty(defines))
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(target, def);
                }
                else
                {
                    if (!defines[defines.Length - 1].Equals(';')) defines += ';';
                    defines += def;
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(target, defines);
                }
            }
        }

        [PostProcessScene(0)]
        public static void OnPostprocessScene()
        {
            InitDefine(sMicroSplatDefine);
        }
    }
}