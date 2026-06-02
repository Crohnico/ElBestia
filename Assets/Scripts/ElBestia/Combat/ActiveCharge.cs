using System;
using ElBestia.Skills;

namespace ElBestia.Combat
{
    [Serializable]
    public sealed class ActiveCharge
    {
        public ChargeType charge;
        public int amount;
        public ChampionBehaviour source;
        public int protectedAmount;
        public int protectedUntilCompletedActions;
    }
}
