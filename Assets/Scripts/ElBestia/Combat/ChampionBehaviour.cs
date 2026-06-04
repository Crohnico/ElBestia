using System;
using ElBestia.Champions;
using ElBestia.Combat.Charges;
using ElBestia.Generation;
using ElBestia.Perks;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Combat
{
    public sealed class ChampionBehaviour : MonoBehaviour
    {
        [SerializeField] private ChampionSO champion;
        [SerializeField] private Transform homePosition;
        [SerializeField] private ChampionBehaviour rival;
        [SerializeField] private CombatCrono crono;
        [SerializeField] private float moveSpeed = 4.5f;
        [SerializeField] private float turnSpeedDegrees = 720f;
        [SerializeField] private float minimumVisualRange = 1.5f;
        [SerializeField] private float actionAnimationSeconds = 0.18f;
        [SerializeField] private float criticalDamageMultiplier = 2f;
        [SerializeField] private bool debugCombatFlow = true;
        [SerializeField] private ActiveCharge[] activeCharges = Array.Empty<ActiveCharge>();
        [SerializeField] private float lastRolledActionDelay;

        private int completedActions;
        private int maxHealth;
        private int currentHealth;
        private bool isExecutingActionFlow;
        private bool endTurnChargesConsumed;
        private ChampionChargeController chargeController;
        private ChampionChargeApplication chargeApplication;
        private ChampionMovement movement;
        private ChampionDamageResolver damageResolver;
        private WeaponAvoidanceResolver avoidanceResolver;
        private CounterattackResolver counterattackResolver;
        private ChampionActionFlow actionFlow;
        private SkillActionExecutor skillActionExecutor;
        private ChampionSkillLoadout skillLoadout;
        private ChampionTurnEffects turnEffects;
        private ChampionCombatPresentation presentation;
        private bool isLeftSide;
        private int debugFlowId;
        private int activeDebugFlowId;
        private System.Random rng;

        public ChampionSO Champion => champion;
        public string ChampionId => champion != null ? champion.ChampionId : name;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsAlive => currentHealth > 0;
        public float MovementFloat => movement != null ? movement.MovementFloat : 0f;
        public int ActionSpeedRating => GetActionSpeedRating();
        public float LastRolledActionDelay => lastRolledActionDelay;
        public bool IsResolvingInteraction => isExecutingActionFlow || (counterattackResolver != null && counterattackResolver.IsCounterattacking);
        public string DebugStateSummary => BuildDebugStateSummary();
        public ActiveCharge[] ActiveCharges => activeCharges;
        internal ChampionDamageResolver Damage => damageResolver;
        internal WeaponAvoidanceResolver Avoidance => avoidanceResolver;
        internal CounterattackResolver Counterattacks => counterattackResolver;
        internal ChampionBehaviour RivalForCombat
        {
            get => rival;
            set => rival = value;
        }
        public event Action<ChampionBehaviour, int> SkillExecuted;
        public event Action<ChampionBehaviour, int, int> HealthChanged;
        public event Action<ChampionBehaviour, ActiveCharge[]> ChargesChanged;
        public event Action<ChampionBehaviour> ActionSpeedStatusChanged;

        private void Update()
        {
            movement?.Update();
            if (presentation != null && movement != null)
            {
                float intent = movement.TurnIntent;
                float right = Mathf.Approximately(intent, 0f) ? 0f : presentation.GetTurnValue(intent < 0f);
                presentation.SetLocomotion(movement.MovementFloat, right);
            }
        }

        public void Initialize(ChampionSO championData, Transform home, ChampionBehaviour rivalChampion, CombatCrono combatCrono)
        {
            champion = championData;
            homePosition = home;
            rival = rivalChampion;
            crono = combatCrono;
            isExecutingActionFlow = false;
            endTurnChargesConsumed = false;
            activeDebugFlowId = 0;
            completedActions = 0;
            maxHealth = GetMaxHealth(champion);
            currentHealth = maxHealth;
            activeCharges = Array.Empty<ActiveCharge>();
            chargeController = CreateChargeController();
            chargeApplication = new ChampionChargeApplication(this);
            damageResolver = new ChampionDamageResolver(this);
            avoidanceResolver = new WeaponAvoidanceResolver(this, () => criticalDamageMultiplier);
            counterattackResolver = new CounterattackResolver(this);
            actionFlow = new ChampionActionFlow(this);
            skillActionExecutor = new SkillActionExecutor(this);
            skillLoadout = new ChampionSkillLoadout(this, () => crono);
            turnEffects = new ChampionTurnEffects(this);
            movement = CreateMovement();
            movement.Initialize(homePosition, crono, moveSpeed, turnSpeedDegrees, Mathf.Max(1.5f, minimumVisualRange), actionAnimationSeconds);
            presentation = GetComponent<ChampionCombatPresentation>();
            if (presentation == null)
            {
                presentation = gameObject.AddComponent<ChampionCombatPresentation>();
            }

            presentation.Initialize(this, isLeftSide);
            rng = new System.Random(Mathf.Abs((champion != null ? champion.ChampionId : name).GetHashCode()));
            chargeController.Clear();
            counterattackResolver.Clear();
            skillLoadout.ClearCooldowns();
            turnEffects.ApplyOpeningCharges();
            HealthChanged?.Invoke(this, currentHealth, maxHealth);
        }

        private ChampionMovement CreateMovement()
        {
            return new ChampionMovement(
                transform,
                () => rival,
                () => IsAlive,
                () => counterattackResolver != null && counterattackResolver.IsCounterattacking,
                () => GetChampionLabel(this),
                () => activeDebugFlowId,
                () => debugCombatFlow);
        }

        private ChampionChargeController CreateChargeController()
        {
            return new ChampionChargeController(
                () => activeCharges,
                value => activeCharges = value ?? Array.Empty<ActiveCharge>(),
                () => isExecutingActionFlow,
                () => endTurnChargesConsumed,
                () => completedActions,
                GetActionTimeMultiplier,
                charges => ChargesChanged?.Invoke(this, charges),
                NotifyActionSpeedChangedIfNeeded);
        }

        public void SetRival(ChampionBehaviour rivalChampion)
        {
            rival = rivalChampion;
        }

        public void ConfigureCombatSide(bool leftSide, float cameraYawOffset)
        {
            isLeftSide = leftSide;
            movement?.ConfigureFacing(leftSide, cameraYawOffset);
            presentation?.Initialize(this, isLeftSide);
        }

        public void SetCrono(CombatCrono combatCrono)
        {
            crono = combatCrono;
            movement?.SetCrono(combatCrono);
        }

        public float RollNextActionDelay()
        {
            ChampionStats stats = champion != null ? champion.Stats : null;
            int speedRating = stats != null ? stats.GetActionSpeedRating() : 10;
            float randomWindow = rng != null ? Mathf.Lerp(2.5f, 5.2f, (float)rng.NextDouble()) : UnityEngine.Random.Range(2.5f, 5.2f);
            float delay = randomWindow * 100f / (100f + Mathf.Max(0, speedRating));
            lastRolledActionDelay = Mathf.Max(0.35f, delay / GetOpeningActionSpeedMultiplier());
            return lastRolledActionDelay;
        }

        public float GetActionTimeMultiplier()
        {
            float multiplier = 1f;
            if (HasCharge(ChargeType.Slow))
            {
                multiplier *= 1.5f;
            }

            if (HasCharge(ChargeType.Haste))
            {
                multiplier *= 1f / 1.5f;
            }

            return multiplier;
        }

        public void ExecuteAction(Action onComplete)
        {
            activeDebugFlowId = ++debugFlowId;
            DebugFlow("ExecuteAction", $"Start state={BuildDebugStateSummary()}");
            if (!IsAlive)
            {
                DebugFlow("ExecuteAction", "Champion is dead. Finishing immediately.");
                FinishAction(onComplete);
                return;
            }

            int skillIndex = skillLoadout.PickSkillIndex();
            SkillData skill = skillLoadout.GetSkill(skillIndex);
            if (skill == null)
            {
                DebugFlow("ExecuteAction", $"No skill found for index={skillIndex}. Finishing immediately.");
                FinishAction(onComplete);
                return;
            }

            DebugFlow("ExecuteAction", $"Selected index={skillIndex} skill={GetSkillLabel(skill)} cd={skill.cooldownSeconds:0.00}");
            SkillExecuted?.Invoke(this, skillIndex);
            skillLoadout.SetCooldown(skillIndex, skill);
            StartActionFlow(skill, onComplete);
        }

        private void StartActionFlow(SkillData skill, Action onComplete)
        {
            actionFlow.Start(skill, onComplete);
        }

        public void ReceiveDebugDamage(int amount)
        {
            TakeDamage(Mathf.Max(0, amount), SkillElement.None, null);
        }

        public int TakeDamage(int amount, SkillElement element, ChampionBehaviour source)
        {
            return damageResolver != null ? damageResolver.TakeDamage(amount, element, source) : 0;
        }

        internal int ApplyResolvedDamage(int finalDamage)
        {
            if (finalDamage <= 0)
            {
                return 0;
            }

            currentHealth = Mathf.Max(0, currentHealth - finalDamage);
            HealthChanged?.Invoke(this, currentHealth, maxHealth);
            return finalDamage;
        }

        public void Heal(int amount)
        {
            int heal = Mathf.Max(0, amount);
            if (heal <= 0)
            {
                return;
            }

            currentHealth = Mathf.Min(maxHealth, currentHealth + heal);
            HealthChanged?.Invoke(this, currentHealth, maxHealth);
        }

        public void ForceFinishStuckInteraction()
        {
            DebugFlow("ForceFinishStuck", $"Before clear state={BuildDebugStateSummary()}");
            ForceClearAsyncStep();
            counterattackResolver?.ForceEnd();
            if (rival != null)
            {
                rival.ForceEndCounterattackInteraction();
            }

            counterattackResolver?.ForceEndPendingCounterattackers();

            isExecutingActionFlow = false;
            endTurnChargesConsumed = false;
            StartReturnHome();
            DebugFlow("ForceFinishStuck", $"After clear state={BuildDebugStateSummary()}");
        }

        public void AddCharges(ChargeType charge, int amount)
        {
            AddCharges(charge, amount, this);
        }

        public void AddCharges(ChargeType charge, int amount, ChampionBehaviour source)
        {
            chargeApplication.Add(charge, amount, source);
        }

        public Sprite GetTimelineAvatar()
        {
            return champion != null ? RuntimeSkillIconCache.GetSprite(champion.BaseSkill) : null;
        }

        internal void ExecuteSkillActionsForCombat(SkillAction[] actions, SkillExecutionContext context, bool allowCounterattack)
        {
            skillActionExecutor.ExecuteMany(actions, context, allowCounterattack);
        }

        internal void ExecuteSkillActionForCombat(SkillAction skillAction, SkillExecutionContext context, bool allowCounterattack)
        {
            skillActionExecutor.Execute(skillAction, context, allowCounterattack);
        }

        internal bool ConsumeChargeIfAvailableForCombat(ChargeType chargeType)
        {
            return chargeController != null && chargeController.ConsumeIfAvailable(chargeType);
        }

        internal void ApplyAfterActionPerksForCombat()
        {
            turnEffects.ApplyAfterActionPerks();
        }

        public void ApplyStartTurnEffects()
        {
            turnEffects.ApplyStartTurnEffects();
        }

        internal void ConsumeEndTurnChargesForCombat()
        {
            turnEffects.ConsumeEndTurnCharges();
        }

        internal void ResetEndTurnChargeConsumptionForCombat()
        {
            endTurnChargesConsumed = false;
        }

        internal void MarkEndTurnChargesConsumedForCombat()
        {
            endTurnChargesConsumed = true;
        }

        internal void SetActionFlowActiveForCombat(bool isActive)
        {
            isExecutingActionFlow = isActive;
        }

        internal void ConsumeChargeForCombat(ChargeType chargeType, int amount, bool respectProtection = false)
        {
            chargeController.Consume(chargeType, amount, respectProtection);
        }

        internal void AddRawChargeForCombat(ChargeType chargeType, int amount, ChampionBehaviour source)
        {
            chargeController.Add(chargeType, amount, source);
        }

        internal int GetChargeAmountForCombat(ChargeType chargeType)
        {
            return chargeController != null ? chargeController.GetAmount(chargeType) : 0;
        }

        internal bool HasChargeForCombat(ChargeType chargeType)
        {
            return chargeController != null && chargeController.Has(chargeType);
        }

        private void ConsumeCharge(ChargeType chargeType, int amount, bool respectProtection = false)
        {
            ConsumeChargeForCombat(chargeType, amount, respectProtection);
        }

        private int GetChargeAmount(ChargeType chargeType)
        {
            return GetChargeAmountForCombat(chargeType);
        }

        private bool HasCharge(ChargeType chargeType)
        {
            return HasChargeForCombat(chargeType);
        }

        public static bool TryGetOppositeCharge(ChargeType charge, out ChargeType opposite)
        {
            return ChargeEffectCatalog.TryGetOpposite(charge, out opposite);
        }

        private void NotifyActionSpeedChangedIfNeeded(float previousActionTimeMultiplier)
        {
            if (!Mathf.Approximately(previousActionTimeMultiplier, GetActionTimeMultiplier()))
            {
                ActionSpeedStatusChanged?.Invoke(this);
            }
        }

        private int GetActionSpeedRating()
        {
            return champion != null && champion.Stats != null ? champion.Stats.GetActionSpeedRating() : 0;
        }

        internal float Roll01ForCombat()
        {
            return rng != null ? (float)rng.NextDouble() : UnityEngine.Random.value;
        }

        internal int RollIntForCombat(int minInclusive, int maxExclusive)
        {
            return rng != null ? rng.Next(minInclusive, maxExclusive) : UnityEngine.Random.Range(minInclusive, maxExclusive);
        }

        internal void QueueCounterattackForCombat(ChampionBehaviour counterattacker)
        {
            counterattackResolver?.Queue(counterattacker);
        }

        internal void ApplyCaltropsActionTaxForCombat()
        {
            turnEffects.ApplyCaltropsActionTax();
        }

        private void ForceEndCounterattackInteraction()
        {
            counterattackResolver?.ForceEnd();
        }

        private void MoveIntoSkillRange(SkillData skill, Action onComplete)
        {
            movement.MoveIntoSkillRange(skill, onComplete);
        }

        internal void MoveIntoSkillRangeForCombat(SkillData skill, Action onComplete)
        {
            MoveIntoSkillRange(skill, onComplete);
        }

        internal void CancelReturnHomeForCombat()
        {
            movement?.CancelReturnHome();
        }

        private void PlayActionAnimation(SkillData skill, Action onComplete)
        {
            if (presentation != null)
            {
                presentation.PlayCast(skill, rival, onComplete);
                return;
            }

            movement.PlayActionAnimation(onComplete);
        }

        internal void PlayActionAnimationForCombat(SkillData skill, Action onComplete)
        {
            PlayActionAnimation(skill, onComplete);
        }

        private void ForceClearAsyncStep()
        {
            movement?.ForceClearAsyncStep();
        }

        internal void ForceClearAsyncStepForCombat()
        {
            ForceClearAsyncStep();
        }

        private void StartReturnHome()
        {
            movement?.StartReturnHome();
        }

        internal void StartReturnHomeForCombat()
        {
            StartReturnHome();
        }

        private void FinishAction(Action onComplete)
        {
            DebugFlow("FinishAction", $"Before complete state={BuildDebugStateSummary()}");
            isExecutingActionFlow = false;
            endTurnChargesConsumed = false;
            completedActions++;
            onComplete?.Invoke();
            DebugFlow("FinishAction", $"After complete actions={completedActions}");
        }

        internal void FinishActionForCombat(Action onComplete)
        {
            FinishAction(onComplete);
        }

        private void FaceTarget(ChampionBehaviour target, float deltaTime)
        {
            movement?.FaceTarget(target, deltaTime);
        }

        internal void FaceTargetForCombat(ChampionBehaviour target, float deltaTime)
        {
            FaceTarget(target, deltaTime);
        }

        public void FaceRivalAtTurnStart()
        {
            movement?.BeginFaceTarget(rival);
        }

        internal void PlayImpactReactionForCombat()
        {
            presentation?.PlayImpactReaction(IsAlive);
        }

        private float GetRivalDistance()
        {
            return movement != null ? movement.GetRivalDistance() : -1f;
        }

        private void DebugFlow(string step, string message)
        {
            if (!debugCombatFlow)
            {
                return;
            }

            CombatDebug.Log(nameof(ChampionBehaviour), GetChampionLabel(this), step, message, CombatDebug.ChampionColor, activeDebugFlowId);
        }

        internal void DebugCombatFlow(string step, string message)
        {
            DebugFlow(step, message);
        }

        internal void DebugCombatFlow(string scriptName, string step, string message)
        {
            if (!debugCombatFlow)
            {
                return;
            }

            CombatDebug.Log(scriptName, GetChampionLabel(this), step, message, CombatDebug.ChampionColor, activeDebugFlowId);
        }

        private string BuildDebugStateSummary()
        {
            bool isCounterattacking = counterattackResolver != null && counterattackResolver.IsCounterattacking;
            return $"alive={IsAlive} hp={currentHealth}/{maxHealth} executing={isExecutingActionFlow} counter={isCounterattacking} movement=[{(movement != null ? movement.BuildDebugStateSummary() : "null")}] rival={GetChampionLabel(rival)} crono={(crono != null ? crono.CurrentTime.ToString("0.00") : "null")}";
        }

        private static string GetChampionLabel(ChampionBehaviour behaviour)
        {
            if (behaviour == null)
            {
                return "null";
            }

            if (behaviour.champion != null && !string.IsNullOrEmpty(behaviour.champion.ChampionName))
            {
                return behaviour.champion.ChampionName;
            }

            return behaviour.name;
        }

        private static string GetSkillLabel(SkillData skill)
        {
            return skill != null && !string.IsNullOrEmpty(skill.skillName) ? skill.skillName : "null";
        }

        private float GetOpeningActionSpeedMultiplier()
        {
            if (champion == null || champion.Perks == null)
            {
                return 1f;
            }

            float multiplier = 1f;
            foreach (PerkSO perk in champion.Perks)
            {
                if (perk == null || perk.EffectType != PerkEffectType.OpeningActionSpeed)
                {
                    continue;
                }

                if (completedActions < perk.FlatValue)
                {
                    multiplier = Mathf.Max(multiplier, perk.Multiplier > 0f ? perk.Multiplier : 2f);
                }
            }

            return multiplier;
        }

        private float GetSkillEchoDamageMultiplier()
        {
            if (champion == null || champion.Perks == null)
            {
                return 0f;
            }

            float multiplier = 0f;
            foreach (PerkSO perk in champion.Perks)
            {
                if (perk != null && perk.EffectType == PerkEffectType.SkillEcho)
                {
                    multiplier = Mathf.Max(multiplier, perk.Multiplier > 0f ? perk.Multiplier : 0.5f);
                }
            }

            return multiplier;
        }

        internal float GetSkillEchoDamageMultiplierForCombat()
        {
            return GetSkillEchoDamageMultiplier();
        }

        private static int GetMaxHealth(ChampionSO champion)
        {
            return champion != null && champion.Stats != null ? Mathf.Max(1, champion.Stats.life) : 100;
        }

    }
}
