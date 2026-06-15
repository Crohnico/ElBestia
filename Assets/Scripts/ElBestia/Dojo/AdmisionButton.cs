using ElBestia.Champions;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElBestia.Dojo
{
    public sealed class AdmisionButton : MonoBehaviour
    {
        public Button button;
        public Graphic background;
        public TMP_Text championNameText;
        [NonSerialized]
        public ChampionData championData;

        public void Bind(ChampionData champion, Action<AdmisionButton> onClick)
        {
            championData = champion;
            championNameText.text = champion.ChampionName;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick(this));
        }

        public void SetColor(Color color)
        {
            background.color = color;
        }
    }
}
