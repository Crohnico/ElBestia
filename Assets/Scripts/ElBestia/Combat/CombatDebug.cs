using UnityEngine;

namespace ElBestia.Combat
{
    internal static class CombatDebug
    {
        public const string ChampionColor = "7FD7FF";
        public const string TimelineColor = "FFD166";

        public static bool Enabled { get; private set; } = true;
        public static bool TimelineEnabled { get; private set; } = true;
        public static bool ChampionEnabled { get; private set; } = true;
        public static bool MovementEnabled { get; private set; } = true;
        public static bool ActionFlowEnabled { get; private set; } = true;
        public static bool DamageEnabled { get; private set; } = true;
        public static bool ChargesEnabled { get; private set; } = true;

        public static void Configure(
            bool enabled,
            bool timeline,
            bool champion,
            bool movement,
            bool actionFlow,
            bool damage,
            bool charges)
        {
            Enabled = enabled;
            TimelineEnabled = timeline;
            ChampionEnabled = champion;
            MovementEnabled = movement;
            ActionFlowEnabled = actionFlow;
            DamageEnabled = damage;
            ChargesEnabled = charges;
        }

        public static void Log(string scriptName, string championName, string step, string message, string colorHex, int flowId = 0)
        {
            if (!Enabled || !IsScriptEnabled(scriptName))
            {
                return;
            }

            string flow = flowId > 0 ? $"[Flow#{flowId}]" : string.Empty;
            Debug.Log($"<color=#{colorHex}>[{scriptName}][{championName}]{flow}[{step}] {message}</color>");
        }

        private static bool IsScriptEnabled(string scriptName)
        {
            switch (scriptName)
            {
                case nameof(ActionTimeLine):
                    return TimelineEnabled;
                case nameof(ChampionMovement):
                case nameof(ChampionMoveToRange):
                case nameof(ChampionReturnHomeMovement):
                    return MovementEnabled;
                case nameof(ChampionActionFlow):
                case nameof(SkillPhaseExecutor):
                case nameof(SkillActionExecutor):
                case nameof(CounterattackResolver):
                    return ActionFlowEnabled;
                case nameof(ChampionDamageResolver):
                    return DamageEnabled;
                case nameof(ChampionChargeApplication):
                    return ChargesEnabled;
                case nameof(ChampionBehaviour):
                    return ChampionEnabled;
                default:
                    return true;
            }
        }
    }
}
