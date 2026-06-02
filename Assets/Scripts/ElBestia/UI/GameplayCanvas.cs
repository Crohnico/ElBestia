using System;
using ElBestia.Combat;
using ElBestia.Skills;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElBestia.UI
{
    public enum GameplaySide
    {
        Left,
        Right
    }

    public sealed class GameplayCanvas : MonoBehaviour
    {
        [SerializeField] private ChampionHud leftChampion;
        [SerializeField] private ChampionHud rightChampion;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private CombatCrono crono;
        [SerializeField] private ChargeIconCatalogSO chargeIconCatalog;

        [Header("Damage Bar")]
        [SerializeField] private float damageHoldSeconds = 0.02f;
        [SerializeField] private float damageCatchupSpeed = 5.5f;

        private int lastDisplayedTimerSeconds = -1;

        private void Reset()
        {
            AutoBind();
        }

        private void Awake()
        {
            AutoBindMissingReferences();
            chargeIconCatalog = chargeIconCatalog != null ? chargeIconCatalog : ChargeIconCatalogSO.LoadDefault();
            leftChampion.SetChargeIconCatalog(chargeIconCatalog);
            rightChampion.SetChargeIconCatalog(chargeIconCatalog);
            leftChampion.Initialize(damageHoldSeconds);
            rightChampion.Initialize(damageHoldSeconds);
        }

        private void Update()
        {
            float cronoDeltaTime = crono != null ? crono.DeltaTime : Time.deltaTime;
            float visualDeltaTime = Time.deltaTime;
            leftChampion.Tick(cronoDeltaTime, visualDeltaTime, damageCatchupSpeed);
            rightChampion.Tick(cronoDeltaTime, visualDeltaTime, damageCatchupSpeed);
        }

        [ContextMenu("Auto Bind Gameplay Canvas")]
        public void AutoBind()
        {
            leftChampion = ChampionHud.AutoBind(transform.Find("LeftChampion"));
            rightChampion = ChampionHud.AutoBind(transform.Find("RightChampion"));

            Transform timer = transform.Find("Timer");
            timerText = timer != null ? timer.GetComponentInChildren<TMP_Text>(true) : GetComponentInChildren<TMP_Text>(true);
        }

        public void SetCrono(CombatCrono combatCrono)
        {
            crono = combatCrono;
        }

        public void SetChargeIconCatalog(ChargeIconCatalogSO catalog)
        {
            chargeIconCatalog = catalog;
            leftChampion.SetChargeIconCatalog(chargeIconCatalog);
            rightChampion.SetChargeIconCatalog(chargeIconCatalog);
        }

        public void SetChampion(GameplaySide side, string championName, int currentHealth, int maxHealth)
        {
            if (side == GameplaySide.Left)
            {
                leftChampion.SetName(championName);
                leftChampion.SetHealth(currentHealth, maxHealth, true, damageHoldSeconds);
            }
            else
            {
                rightChampion.SetName(championName);
                rightChampion.SetHealth(currentHealth, maxHealth, true, damageHoldSeconds);
            }
        }

        public void SetChampionHealth(GameplaySide side, int currentHealth, int maxHealth)
        {
            if (side == GameplaySide.Left)
            {
                leftChampion.SetHealth(currentHealth, maxHealth, false, damageHoldSeconds);
            }
            else
            {
                rightChampion.SetHealth(currentHealth, maxHealth, false, damageHoldSeconds);
            }
        }

        public void SetChampionSkills(GameplaySide side, SkillData baseSkill, SkillData[] skills)
        {
            if (side == GameplaySide.Left)
            {
                leftChampion.SetSkills(baseSkill, skills);
            }
            else
            {
                rightChampion.SetSkills(baseSkill, skills);
            }
        }

        public void SetChampionCharges(GameplaySide side, ActiveCharge[] charges)
        {
            if (side == GameplaySide.Left)
            {
                leftChampion.SetCharges(charges);
            }
            else
            {
                rightChampion.SetCharges(charges);
            }
        }

        public void StartSkillCooldown(GameplaySide side, int skillIndex)
        {
            if (side == GameplaySide.Left)
            {
                leftChampion.StartSkillCooldown(skillIndex);
            }
            else
            {
                rightChampion.StartSkillCooldown(skillIndex);
            }
        }

        public void StartSkillCooldown(GameplaySide side, int skillIndex, float cooldownSeconds)
        {
            if (side == GameplaySide.Left)
            {
                leftChampion.StartSkillCooldown(skillIndex, cooldownSeconds);
            }
            else
            {
                rightChampion.StartSkillCooldown(skillIndex, cooldownSeconds);
            }
        }

        public void SetTimer(float seconds)
        {
            if (timerText == null)
            {
                return;
            }

            float safeSeconds = Mathf.Max(0f, seconds);
            int displaySeconds = Mathf.CeilToInt(safeSeconds);
            if (displaySeconds == lastDisplayedTimerSeconds)
            {
                return;
            }

            lastDisplayedTimerSeconds = displaySeconds;
            timerText.text = displaySeconds.ToString();
        }

        private void AutoBindMissingReferences()
        {
            if (leftChampion.HasMissingReference() || rightChampion.HasMissingReference() || timerText == null)
            {
                AutoBind();
            }
        }
    }

    [Serializable]
    public struct SkillHolderUI
    {
        [SerializeField] private SkillSlotUI baseSkill;
        [SerializeField] private SkillSlotUI skill1;
        [SerializeField] private SkillSlotUI skill2;
        [SerializeField] private SkillSlotUI skill3;

        public static SkillHolderUI AutoBind(Transform root)
        {
            var holder = new SkillHolderUI();
            if (root == null)
            {
                return holder;
            }

            holder.baseSkill = FindSlot(root, "BaseSkill") ?? GetSlotByIndex(root, 0);
            holder.skill1 = FindSlot(root, "Skill 1") ?? GetSlotByIndex(root, 1);
            holder.skill2 = FindSlot(root, "Skill 2") ?? GetSlotByIndex(root, 2);
            holder.skill3 = FindSlot(root, "Skill 3") ?? GetSlotByIndex(root, 3);
            return holder;
        }

        public void SetSkills(SkillData championBaseSkill, SkillData[] skills)
        {
            SetSlot(baseSkill, championBaseSkill);
            SetSlot(skill1, GetSkill(skills, 0));
            SetSlot(skill2, GetSkill(skills, 1));
            SetSlot(skill3, GetSkill(skills, 2));
        }

        public bool HasAnySlot()
        {
            return baseSkill != null || skill1 != null || skill2 != null || skill3 != null;
        }

        public void StartCooldown(int skillIndex)
        {
            SkillSlotUI slot = GetSlot(skillIndex);
            if (slot != null)
            {
                slot.StartCooldown();
            }
        }

        public void StartCooldown(int skillIndex, float cooldownSeconds)
        {
            SkillSlotUI slot = GetSlot(skillIndex);
            if (slot != null)
            {
                slot.StartCooldown(cooldownSeconds);
            }
        }

        public void Tick(float deltaTime)
        {
            TickSlot(baseSkill, deltaTime);
            TickSlot(skill1, deltaTime);
            TickSlot(skill2, deltaTime);
            TickSlot(skill3, deltaTime);
        }

        private SkillSlotUI GetSlot(int skillIndex)
        {
            switch (skillIndex)
            {
                case 0:
                    return baseSkill;
                case 1:
                    return skill1;
                case 2:
                    return skill2;
                case 3:
                    return skill3;
                default:
                    return null;
            }
        }

        private static void SetSlot(SkillSlotUI slot, SkillData skill)
        {
            if (slot != null)
            {
                slot.SetSkill(skill);
            }
        }

        private static SkillData GetSkill(SkillData[] skills, int index)
        {
            return skills != null && index >= 0 && index < skills.Length ? skills[index] : null;
        }

        private static void TickSlot(SkillSlotUI slot, float deltaTime)
        {
            if (slot != null)
            {
                slot.Tick(deltaTime);
            }
        }

        private static SkillSlotUI GetOrAddSlot(Transform slotRoot)
        {
            SkillSlotUI slot = slotRoot.GetComponent<SkillSlotUI>();
            if (slot == null)
            {
                slot = slotRoot.gameObject.AddComponent<SkillSlotUI>();
            }

            slot.AutoBind();
            return slot;
        }

        private static SkillSlotUI FindSlot(Transform root, string slotName)
        {
            Transform slotRoot = FindDeep(root, slotName);
            return slotRoot != null ? GetOrAddSlot(slotRoot) : null;
        }

        private static SkillSlotUI GetSlotByIndex(Transform root, int index)
        {
            return root.childCount > index ? GetOrAddSlot(root.GetChild(index)) : null;
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

    [Serializable]
    public struct ChampionHud
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Image healthFill;
        [SerializeField] private Image damageFill;
        [SerializeField] private SkillHolderUI skillHolder;
        [SerializeField] private BuffAreaUI buffArea;

        private float healthAmount;
        private float displayedHealthAmount;
        private float damageAmount;
        private float damageHoldTimer;

        public static ChampionHud AutoBind(Transform root)
        {
            var hud = new ChampionHud();
            if (root == null)
            {
                return hud;
            }

            Transform health = FindDeep(root, "Health");
            Transform damage = FindDeep(root, "Damage");

            hud.nameText = root.GetComponentInChildren<TMP_Text>(true);
            hud.healthFill = health != null ? health.GetComponent<Image>() : null;
            hud.damageFill = damage != null ? damage.GetComponent<Image>() : null;
            hud.skillHolder = SkillHolderUI.AutoBind(FindDeep(root, "SkillHolder", true));
            hud.buffArea = GetOrAddBuffArea(FindDeep(root, "BuffArea", true));
            return hud;
        }

        public bool HasMissingReference()
        {
            return nameText == null || healthFill == null || damageFill == null || !skillHolder.HasAnySlot();
        }

        public void Initialize(float damageHoldSeconds)
        {
            healthAmount = healthFill != null ? healthFill.fillAmount : 1f;
            displayedHealthAmount = healthAmount;
            damageAmount = damageFill != null ? damageFill.fillAmount : healthAmount;
            damageHoldTimer = damageHoldSeconds;
            buffArea?.Clear();
        }

        public void SetName(string championName)
        {
            if (nameText == null)
            {
                return;
            }

            nameText.text = string.IsNullOrEmpty(championName) ? "-" : championName;
        }

        public void SetSkills(SkillData baseSkill, SkillData[] skills)
        {
            skillHolder.SetSkills(baseSkill, skills);
        }

        public void SetChargeIconCatalog(ChargeIconCatalogSO catalog)
        {
            if (buffArea != null)
            {
                buffArea.SetIconCatalog(catalog);
            }
        }

        public void SetCharges(ActiveCharge[] charges)
        {
            if (buffArea != null)
            {
                buffArea.SetCharges(charges);
            }
        }

        public void StartSkillCooldown(int skillIndex)
        {
            skillHolder.StartCooldown(skillIndex);
        }

        public void StartSkillCooldown(int skillIndex, float cooldownSeconds)
        {
            skillHolder.StartCooldown(skillIndex, cooldownSeconds);
        }

        public void SetHealth(int currentHealth, int maxHealth, bool instant, float damageHoldSeconds)
        {
            float target = maxHealth > 0 ? Mathf.Clamp01(currentHealth / (float)maxHealth) : 0f;
            bool isHealing = target > healthAmount;
            healthAmount = target;
            if (instant || !isHealing)
            {
                displayedHealthAmount = healthAmount;
                if (healthFill != null)
                {
                    healthFill.fillAmount = displayedHealthAmount;
                }
            }

            if (instant || target > damageAmount)
            {
                damageAmount = target;
                if (damageFill != null)
                {
                    damageFill.fillAmount = damageAmount;
                }
            }

            damageHoldTimer = damageHoldSeconds;
        }

        public void Tick(float cronoDeltaTime, float visualDeltaTime, float catchupSpeed)
        {
            skillHolder.Tick(cronoDeltaTime);
            if (healthFill != null && displayedHealthAmount < healthAmount)
            {
                displayedHealthAmount = Mathf.MoveTowards(displayedHealthAmount, healthAmount, catchupSpeed * visualDeltaTime);
                healthFill.fillAmount = displayedHealthAmount;
            }

            if (damageFill == null)
            {
                return;
            }

            if (damageAmount <= healthAmount)
            {
                damageAmount = healthAmount;
                damageFill.fillAmount = damageAmount;
                return;
            }

            damageHoldTimer -= visualDeltaTime;
            if (damageHoldTimer > 0f)
            {
                return;
            }

            damageAmount = Mathf.MoveTowards(damageAmount, healthAmount, catchupSpeed * visualDeltaTime);
            damageFill.fillAmount = damageAmount;
        }

        private static Transform FindDeep(Transform root, string targetName, bool allowUnityDuplicateSuffix = false)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == targetName || (allowUnityDuplicateSuffix && root.name.StartsWith(targetName + " (", StringComparison.Ordinal)))
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeep(root.GetChild(i), targetName, allowUnityDuplicateSuffix);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static BuffAreaUI GetOrAddBuffArea(Transform root)
        {
            if (root == null)
            {
                return null;
            }

            BuffAreaUI buffArea = root.GetComponent<BuffAreaUI>();
            if (buffArea == null)
            {
                buffArea = root.gameObject.AddComponent<BuffAreaUI>();
            }

            buffArea.AutoBind();
            return buffArea;
        }
    }
}
