using TMPro;
using UnityEngine;

namespace ElBestia.UI
{
    public sealed class SkillDataInfoContainer : MonoBehaviour
    {
        public TMP_Text title;
        public TMP_Text description;

        public void SetTitle(string value)
        {
            title.text = value;
        }

        public void SetDescription(string value)
        {
            description.text = value;
        }
    }
}
