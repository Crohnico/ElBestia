using System.Collections.Generic;
using ElBestia.Generation;
using ElBestia.Skills;
using UnityEditor;
using UnityEngine;

namespace ElBestia.Editor
{
    [InitializeOnLoad]
    public static class SkillIconCache
    {
        private static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();

        static SkillIconCache()
        {
            AssemblyReloadEvents.beforeAssemblyReload += Clear;
            EditorApplication.quitting += Clear;
        }

        public static Texture2D GetIcon(SkillData skill)
        {
            if (skill == null)
            {
                return Texture2D.grayTexture;
            }

            string key = GetKey(skill);
            if (Cache.TryGetValue(key, out Texture2D cached) && cached != null)
            {
                return cached;
            }

            Texture2D texture = SkillIconGenerator.CreateTexture(skill);
            texture.hideFlags = HideFlags.HideAndDontSave;
            Cache[key] = texture;
            return texture;
        }

        private static string GetKey(SkillData skill)
        {
            string scaling = string.Empty;
            if (skill.statScaling != null)
            {
                for (int i = 0; i < skill.statScaling.Length; i++)
                {
                    if (skill.statScaling[i] == null)
                    {
                        continue;
                    }

                    scaling += $"{skill.statScaling[i].stat}:{skill.statScaling[i].scaling};";
                }
            }

            return $"{skill.iconSeed}|{skill.quality}|{skill.element}|{scaling}";
        }

        private static void Clear()
        {
            foreach (Texture2D texture in Cache.Values)
            {
                if (texture != null)
                {
                    Object.DestroyImmediate(texture);
                }
            }

            Cache.Clear();
        }
    }
}
