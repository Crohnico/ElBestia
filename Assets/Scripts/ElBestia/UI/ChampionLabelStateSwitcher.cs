using UnityEngine;
using UnityEngine.UI;

namespace ElBestia.UI
{
    public enum ChampionLabelState
    {
        Base,
        Info,
        Bio
    }

    public sealed class ChampionLabelStateSwitcher : MonoBehaviour
    {
        public GameObject baseObject;
        public GameObject infoObject;
        public GameObject bioObject;
        public Image baseHeader;
        public Image infoHeader;
        public Image bioHeader;
        public Color selectedColor = Color.white;
        public Color unselectedColor = Color.gray;
        public ChampionLabelState currentState;

        private void Awake()
        {
            SetState(ChampionLabelState.Base);
        }

        public void ButtonAction(ChampionLabelState state)
        {
            SetState(state);
        }

        public void SetBase()
        {
            SetState(ChampionLabelState.Base);
        }

        public void SetInfo()
        {
            SetState(ChampionLabelState.Info);
        }

        public void SetBio()
        {
            SetState(ChampionLabelState.Bio);
        }

        public void SetState(ChampionLabelState state)
        {
            currentState = state;

            SetActive(baseObject, state == ChampionLabelState.Base);
            SetActive(infoObject, state == ChampionLabelState.Info);
            SetActive(bioObject, state == ChampionLabelState.Bio);
            SetHeaderColor(baseHeader, state == ChampionLabelState.Base);
            SetHeaderColor(infoHeader, state == ChampionLabelState.Info);
            SetHeaderColor(bioHeader, state == ChampionLabelState.Bio);
        }

        private static void SetActive(GameObject target, bool isActive)
        {
            if (target != null)
            {
                target.SetActive(isActive);
            }
        }

        private void SetHeaderColor(Image header, bool isSelected)
        {
            if (header != null)
            {
                header.color = isSelected ? selectedColor : unselectedColor;
            }
        }
    }
}
