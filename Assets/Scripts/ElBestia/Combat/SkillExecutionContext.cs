using ElBestia.Skills;

namespace ElBestia.Combat
{
    internal sealed class SkillExecutionContext
    {
        public readonly ChampionBehaviour user;
        public readonly ChampionBehaviour enemy;
        public readonly SkillData skill;
        public int damageBonus;
        public float damageMultiplier;

        public SkillExecutionContext(ChampionBehaviour user, ChampionBehaviour enemy, SkillData skill)
        {
            this.user = user;
            this.enemy = enemy;
            this.skill = skill;
            damageMultiplier = 1f;
        }
    }
}
