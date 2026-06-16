using ElBestia.Generation;
using ElBestia.Menu;
using ElBestia.Skills;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ElBestia.UI
{
    public sealed class SkillDataVisualizer : MonoBehaviour, IInteractable, IPointerEnterHandler, IPointerExitHandler
    {
        public bool isHover;
        public Image image;

        private SkillData skill;
        private SkillDataInfoContainer dataContainer;

        public bool IsHover => isHover;

        public void SetUp(SkillData skillData, SkillDataInfoContainer skillDataInfoContainer)
        {
            skill = skillData;
            dataContainer = skillDataInfoContainer;

            if (image != null)
            {
                image.sprite = RuntimeSkillIconCache.GetSprite(skill);
                image.enabled = skill != null;
            }

            isHover = false;
        }

        public void OnHover(bool isHovering)
        {
            isHover = isHovering;

            if (!isHover || skill == null || dataContainer == null)
            {
                return;
            }

            dataContainer.gameObject.SetActive(true);
            dataContainer.SetTitle(skill.skillName);
            dataContainer.SetDescription(skill.description);
        }

        public void OnClick()
        {
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHover(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnHover(false);
        }
    }
}
