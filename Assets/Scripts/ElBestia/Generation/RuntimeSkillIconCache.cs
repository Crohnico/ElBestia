using System.Collections.Generic;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Generation
{
    public static class RuntimeSkillIconCache
    {
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        public static Sprite GetSprite(SkillData skill)
        {
            if (skill == null)
            {
                return null;
            }

            string key = GetKey(skill);
            if (Sprites.TryGetValue(key, out Sprite cached) && cached != null)
            {
                return cached;
            }

            Texture2D texture = SkillIconGenerator.CreateTexture(skill);
            texture.hideFlags = HideFlags.HideAndDontSave;

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                SkillIconGenerator.IconSize);
            sprite.hideFlags = HideFlags.HideAndDontSave;
            Sprites[key] = sprite;
            return sprite;
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
    }
}
