using System.Collections.Generic;
using ElBestia.Champions;
using ElBestia.UI;
using UnityEngine;

namespace ElBestia.Dojo
{
    public sealed class AdmisionScrollView : MonoBehaviour
    {
        private static readonly Color DefaultUnselectedColor = new Color32(0x41, 0x44, 0x4A, 0xFF);
        private static readonly Color DefaultSelectedColor = new Color32(0x72, 0x77, 0x80, 0xFF);

        public AdmisionButton admisionButton;
        public Transform content;
        public UIScrollRectResetToTop scrollReset;
        public ChampionShowcase championShowcase;
        public int initialSelectedIndex;
        public Color unselectedColor = DefaultUnselectedColor;
        public Color selectedColor = DefaultSelectedColor;

        private readonly List<AdmisionButton> spawnedButtons = new List<AdmisionButton>();
        private AdmisionButton selectedButton;

        private void Awake()
        {
            admisionButton.gameObject.SetActive(false);
        }

        private void Start()
        {
            Populate();
        }

        public void Populate()
        {
            ClearButtons();

            List<ChampionData> champions = ChampionAdmisionList.Instance.champions;
            for (int i = 0; i < champions.Count; i++)
            {
                AdmisionButton button = Instantiate(admisionButton, content);
                button.gameObject.SetActive(true);
                button.Bind(champions[i], SelectButton);
                button.SetColor(unselectedColor);
                spawnedButtons.Add(button);
            }

            SelectInitialButton();
            scrollReset.ResetToTopNextFrame();
        }

        public void ClearButtons()
        {
            selectedButton = null;

            for (int i = spawnedButtons.Count - 1; i >= 0; i--)
            {
                DestroyButton(spawnedButtons[i]);
            }

            spawnedButtons.Clear();
        }

        public void SelectButton(AdmisionButton button)
        {
            if (selectedButton != null)
            {
                selectedButton.SetColor(unselectedColor);
            }

            selectedButton = button;
            selectedButton.SetColor(selectedColor);
            championShowcase.Show(selectedButton.championData);
        }

        private void SelectInitialButton()
        {
            if (spawnedButtons.Count == 0)
            {
                return;
            }

            int index = Mathf.Clamp(initialSelectedIndex, 0, spawnedButtons.Count - 1);
            SelectButton(spawnedButtons[index]);
        }

        private static void DestroyButton(AdmisionButton button)
        {
            if (Application.isPlaying)
            {
                Destroy(button.gameObject);
                return;
            }

            DestroyImmediate(button.gameObject);
        }
    }
}
