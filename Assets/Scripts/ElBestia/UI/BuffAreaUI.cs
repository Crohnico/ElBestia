using System;
using System.Collections.Generic;
using ElBestia.Combat;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.UI
{
    public sealed class BuffAreaUI : MonoBehaviour
    {
        [SerializeField] private BuffUI buffPrefab;
        [SerializeField] private ChargeIconCatalogSO iconCatalog;

        private readonly List<BuffUI> pool = new List<BuffUI>();

        private void Reset()
        {
            AutoBind();
        }

        private void Awake()
        {
            AutoBindMissingReferences();
        }

        [ContextMenu("Auto Bind Buff Area")]
        public void AutoBind()
        {
            buffPrefab = GetComponentInChildren<BuffUI>(true) ?? buffPrefab;
            if (buffPrefab == null && transform.childCount > 0)
            {
                buffPrefab = GetOrAddBuffUI(transform.GetChild(0));
            }

            iconCatalog = iconCatalog != null ? iconCatalog : ChargeIconCatalogSO.LoadDefault();
            if (buffPrefab != null && !pool.Contains(buffPrefab))
            {
                pool.Add(buffPrefab);
                buffPrefab.Clear();
            }
        }

        public void SetIconCatalog(ChargeIconCatalogSO catalog)
        {
            iconCatalog = catalog != null ? catalog : iconCatalog;
        }

        public void SetCharges(ActiveCharge[] charges)
        {
            AutoBindMissingReferences();
            HidePool();

            if (charges == null)
            {
                return;
            }

            int visibleIndex = 0;
            foreach (ActiveCharge charge in charges)
            {
                if (charge == null || charge.charge == ChargeType.None || charge.amount <= 0)
                {
                    continue;
                }

                BuffUI buff = GetPooledBuff(visibleIndex);
                buff.Set(charge.charge, charge.amount, iconCatalog != null ? iconCatalog.GetIcon(charge.charge) : null);
                visibleIndex++;
            }
        }

        public void Clear()
        {
            HidePool();
        }

        private BuffUI GetPooledBuff(int index)
        {
            while (pool.Count <= index)
            {
                pool.Add(CreateBuff());
            }

            return pool[index];
        }

        private BuffUI CreateBuff()
        {
            BuffUI source = buffPrefab;
            if (source != null)
            {
                BuffUI instance = Instantiate(source, transform);
                instance.AutoBind();
                instance.Clear();
                return instance;
            }

            var buffObject = new GameObject("Buff", typeof(RectTransform), typeof(BuffUI));
            buffObject.transform.SetParent(transform, false);
            BuffUI buff = buffObject.GetComponent<BuffUI>();
            buff.Clear();
            return buff;
        }

        private void HidePool()
        {
            foreach (BuffUI buff in pool)
            {
                if (buff != null)
                {
                    buff.Clear();
                }
            }
        }

        private void AutoBindMissingReferences()
        {
            if (buffPrefab == null)
            {
                AutoBind();
            }
        }

        private static BuffUI GetOrAddBuffUI(Transform root)
        {
            BuffUI buff = root.GetComponent<BuffUI>();
            if (buff == null)
            {
                buff = root.gameObject.AddComponent<BuffUI>();
            }

            buff.AutoBind();
            return buff;
        }
    }
}
