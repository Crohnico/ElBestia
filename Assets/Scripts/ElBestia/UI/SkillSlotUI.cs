using ElBestia.Generation;
using ElBestia.Skills;
using UnityEngine;
using UnityEngine.UI;

namespace ElBestia.UI
{
    public sealed class SkillSlotUI : MonoBehaviour
    {
        private const float MinimumVisualCooldownSeconds = 0.2f;

        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image skillImage;
        [SerializeField] private Color backgroundTint = new Color(0.25f, 0.25f, 0.25f, 0.85f);

        private SkillData skill;
        private float cooldownDuration;
        private float cooldownRemaining;

        private void Reset()
        {
            AutoBind();
        }

        private void Awake()
        {
            AutoBindMissingReferences();
            SetReady();
        }

        [ContextMenu("Auto Bind Skill Slot")]
        public void AutoBind()
        {
            backgroundImage = FindImage("BackgroundImage") ?? backgroundImage;
            skillImage = FindImage("SkillImage") ?? skillImage;
        }

        public void SetSkill(SkillData skillData)
        {
            skill = skillData;
            gameObject.SetActive(skill != null);
            if (skill == null)
            {
                return;
            }

            Sprite sprite = RuntimeSkillIconCache.GetSprite(skill);
            if (backgroundImage != null)
            {
                backgroundImage.sprite = sprite;
                backgroundImage.color = backgroundTint;
            }

            if (skillImage != null)
            {
                skillImage.sprite = sprite;
                skillImage.color = Color.white;
                skillImage.fillAmount = 1f;
            }

            SetReady();
        }

        public void StartCooldown()
        {
            StartCooldown(skill != null ? skill.cooldownSeconds : 0f);
        }

        public void StartCooldown(float seconds)
        {
            cooldownDuration = Mathf.Max(MinimumVisualCooldownSeconds, seconds);
            cooldownRemaining = cooldownDuration;

            if (skillImage != null)
            {
                skillImage.fillAmount = 0f;
            }
        }

        public void Tick(float deltaTime)
        {
            if (cooldownRemaining <= 0f)
            {
                return;
            }

            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - deltaTime);
            if (skillImage != null)
            {
                skillImage.fillAmount = cooldownDuration > 0f
                    ? 1f - cooldownRemaining / cooldownDuration
                    : 1f;
            }

            if (cooldownRemaining <= 0f)
            {
                SetReady();
            }
        }

        private void SetReady()
        {
            cooldownDuration = 0f;
            cooldownRemaining = 0f;

            if (skillImage != null)
            {
                skillImage.fillAmount = 1f;
            }
        }

        private void AutoBindMissingReferences()
        {
            if (backgroundImage == null || skillImage == null)
            {
                AutoBind();
            }
        }

        private Image FindImage(string objectName)
        {
            Transform child = FindDeep(transform, objectName);
            return child != null ? child.GetComponent<Image>() : null;
        }

        private static Transform FindDeep(Transform root, string targetName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == targetName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeep(root.GetChild(i), targetName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
