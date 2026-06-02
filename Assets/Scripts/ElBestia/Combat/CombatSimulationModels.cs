using System;
using System.Collections.Generic;
using ElBestia.Skills;

namespace ElBestia.Combat
{
    public enum CombatSimulationSide
    {
        None,
        Left,
        Right,
        Draw
    }

    public enum CombatSimulationEventType
    {
        MatchStart,
        TurnScheduled,
        TurnStart,
        SkillSelected,
        DirectDamage,
        DotDamage,
        Heal,
        ChargeAdded,
        ChargeConsumed,
        Dodged,
        Blocked,
        Critical,
        MatchEnd
    }

    [Serializable]
    public sealed class CombatSimulationOptions
    {
        public int seed = 12345;
        public float maxDurationSeconds = 60f;
        public int maxEvents = 10000;
        public float directDamageMinMultiplier = 0.85f;
        public float directDamageMaxMultiplier = 1f;
    }

    [Serializable]
    public sealed class CombatSimulationEvent
    {
        public float time;
        public CombatSimulationEventType type;
        public CombatSimulationSide actor;
        public CombatSimulationSide target;
        public string skillName;
        public SkillElement element;
        public ChargeType charge;
        public int amount;
        public int targetHealthAfter;
        public string message;
    }

    [Serializable]
    public sealed class CombatSimulationResult
    {
        public int seed;
        public bool completed;
        public float duration;
        public CombatSimulationSide winner;
        public int leftHealth;
        public int rightHealth;
        public readonly List<CombatSimulationEvent> events = new List<CombatSimulationEvent>();
    }
}
