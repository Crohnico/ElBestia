using ElBestia.Skills;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElBestia.UI
{
    public sealed class BuffUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text amountText;

        private ChargeType charge;

        public ChargeType Charge => charge;

        private void Reset()
        {
            AutoBind();
        }

        private void Awake()
        {
            AutoBindMissingReferences();
        }

        [ContextMenu("Auto Bind Buff UI")]
        public void AutoBind()
        {
            iconImage = GetComponentInChildren<Image>(true) ?? iconImage;
            amountText = GetComponentInChildren<TMP_Text>(true) ?? amountText;
        }

        public void Set(ChargeType chargeType, int amount, Sprite icon)
        {
            charge = chargeType;
            gameObject.SetActive(amount > 0 && chargeType != ChargeType.None);

            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = icon != null;
            }

            if (amountText != null)
            {
                amountText.text = amount.ToString();
            }
        }

        public void Clear()
        {
            charge = ChargeType.None;
            gameObject.SetActive(false);
        }

        private void AutoBindMissingReferences()
        {
            if (iconImage == null || amountText == null)
            {
                AutoBind();
            }
        }
    }
}
