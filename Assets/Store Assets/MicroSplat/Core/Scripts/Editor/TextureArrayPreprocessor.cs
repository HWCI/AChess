//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;

namespace JBooth.MicroSplat
{
    internal class TextureArrayPreProcessor : AssetPostprocessor
    {
        private static int GetNewHash(TextureArrayConfig cfg)
        {
            unchecked
            {
                var settings =
                    TextureArrayConfigEditor.GetSettingsGroup(cfg, EditorUserBuildSettings.activeBuildTarget);
                var h = 17;

                h = h * TextureArrayConfigEditor.GetTextureFormat(cfg, settings.diffuseSettings.compression)
                        .GetHashCode() * 7;
                h = h * TextureArrayConfigEditor.GetTextureFormat(cfg, settings.normalSettings.compression)
                        .GetHashCode() * 13;
                h = h * TextureArrayConfigEditor.GetTextureFormat(cfg, settings.emissiveSettings.compression)
                        .GetHashCode() * 17;
                h = h * TextureArrayConfigEditor.GetTextureFormat(cfg, settings.antiTileSettings.compression)
                        .GetHashCode() * 31;
                h = h * TextureArrayConfigEditor.GetTextureFormat(cfg, settings.smoothSettings.compression)
                        .GetHashCode() * 37;
                h = h * Application.unityVersion.GetHashCode() * 43;
#if UNITY_EDITOR
                h = h * EditorUserBuildSettings.activeBuildTarget.GetHashCode() * 47;
#endif
                return h;
            }
        }


        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            var cfgs = AssetDatabase.FindAssets("t:TextureArrayConfig");
            for (var i = 0; i < cfgs.Length; ++i)
            {
                var asset = AssetDatabase.GUIDToAssetPath(cfgs[i]);
                var cfg = AssetDatabase.LoadAssetAtPath<TextureArrayConfig>(asset);
                if (cfg != null)
                {
                    var hash = GetNewHash(cfg);
                    if (hash != cfg.hash)
                    {
                        cfg.hash = hash;
                        TextureArrayConfigEditor.CompileConfig(cfg);
                        EditorUtility.SetDirty(cfg);
                    }
                }
            }
        }
    }
}