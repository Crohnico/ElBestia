using System;
using ElBestia.Champions;

namespace ElBestia.Skills
{
    [Serializable]
    public sealed class StatScaling
    {
        public ChampionStatType stat;
        public GrowthGrade scaling;
        public bool usesGradeScaling = true;
    }

    [Serializable]
    public sealed class SkillAction
    {
        public SkillTarget target;
        public SkillActionType action;
        public ChargeType charge;
        public int amount;
    }

    [Serializable]
    public sealed class SkillData
    {
        public string skillName;
        public string skillNameId;
        public string description;
        public bool isBaseSkill;
        public GrowthGrade quality;
        public int level = 1;
        public float cooldownSeconds;
        public SkillElement element;
        public int energyCost;
        public float range;
        public WeaponType[] weapons;
        public StatScaling[] statScaling;
        public SkillAction[] preCast;
        public SkillAction[] cast;
        public SkillAction[] postCast;
        public int iconSeed;
    }
}
