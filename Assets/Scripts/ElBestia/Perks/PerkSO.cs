using ElBestia.Champions;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Perks
{
    [CreateAssetMenu(menuName = "El Bestia/Perks/Perk", fileName = "Perk")]
    public sealed class PerkSO : ScriptableObject
    {
        [SerializeField] private string perkId;
        [SerializeField] private string displayName;
        [SerializeField] private PerkRarity rarity;
        [TextArea(2, 4)]
        [SerializeField] private string description;
        [SerializeField] private PerkEffectType effectType;
        [SerializeField] private PerkCombatStatType combatStat;
        [SerializeField] private ChampionStatType stat;
        [SerializeField] private WeaponType weapon;
        [SerializeField] private SkillElement element;
        [SerializeField] private ChargeType charge;
        [SerializeField] private int flatValue;
        [SerializeField] private float multiplier = 1f;
        [SerializeField] private float chance;
        [SerializeField] private bool isUnique;
        [SerializeField] private string uniqueGroup;

        public string PerkId => perkId;
        public string DisplayName => displayName;
        public PerkRarity Rarity => rarity;
        public string Description => description;
        public PerkEffectType EffectType => effectType;
        public PerkCombatStatType CombatStat => combatStat;
        public ChampionStatType Stat => stat;
        public WeaponType Weapon => weapon;
        public SkillElement Element => element;
        public ChargeType Charge => charge;
        public int FlatValue => flatValue;
        public float Multiplier => multiplier;
        public float Chance => chance;
        public bool IsUnique => isUnique;
        public string UniqueGroup => uniqueGroup;

        public void Configure(
            string id,
            string perkName,
            PerkRarity perkRarity,
            string perkDescription,
            PerkEffectType perkEffectType,
            PerkCombatStatType affectedCombatStat = PerkCombatStatType.None,
            ChampionStatType affectedStat = ChampionStatType.Strength,
            WeaponType affectedWeapon = WeaponType.None,
            SkillElement affectedElement = SkillElement.None,
            ChargeType affectedCharge = ChargeType.None,
            int value = 0,
            float effectMultiplier = 1f,
            float effectChance = 0f,
            bool unique = false,
            string uniquePerkGroup = "")
        {
            perkId = id;
            displayName = perkName;
            rarity = perkRarity;
            description = perkDescription;
            effectType = perkEffectType;
            combatStat = affectedCombatStat;
            stat = affectedStat;
            weapon = affectedWeapon;
            element = affectedElement;
            charge = affectedCharge;
            flatValue = value;
            multiplier = effectMultiplier;
            chance = effectChance;
            isUnique = unique;
            uniqueGroup = uniquePerkGroup;
        }
    }
}
