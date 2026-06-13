using System;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Combat
{
    internal sealed class ChampionMoveToRange
    {
        private const float RangeCompletionTolerance = 0.05f;
        private const float SnapDistance = 0.03f;
        private const float MinimumMovementDeltaTime = 0.001f;

        private readonly Transform ownerTransform;
        private readonly Func<ChampionBehaviour> getRival;
        private readonly Func<bool> isOwnerAlive;
        private readonly Func<string> getOwnerLabel;
        private readonly Func<int> getFlowId;
        private readonly Func<bool> isDebugEnabled;
        private float lockedHeight;
        private float moveSpeed;
        private float turnSpeedDegrees;
        private float minimumVisualRange;
        private float towardEnemyYaw;
        private Action pendingComplete;
        private SkillData pendingSkill;
        private float nextPendingStateLogAt;

        public ChampionMoveToRange(
            Transform ownerTransform,
            Func<ChampionBehaviour> getRival,
            Func<bool> isOwnerAlive,
            Func<string> getOwnerLabel,
            Func<int> getFlowId,
            Func<bool> isDebugEnabled)
        {
            this.ownerTransform = ownerTransform;
            this.getRival = getRival;
            this.isOwnerAlive = isOwnerAlive;
            this.getOwnerLabel = getOwnerLabel;
            this.getFlowId = getFlowId;
            this.isDebugEnabled = isDebugEnabled;
        }

        public bool HasPending => pendingComplete != null;
        public bool IsTurning { get; private set; }
        public float MovementFloat { get; private set; }
        public string PendingLabel => pendingSkill != null ? "MoveToRange" : string.Empty;
        public string PendingSkillName => SkillLabel(pendingSkill);

        public void Initialize(float lockedHeight, float moveSpeed, float turnSpeedDegrees, float minimumVisualRange)
        {
            this.lockedHeight = lockedHeight;
            this.moveSpeed = moveSpeed;
            this.turnSpeedDegrees = turnSpeedDegrees;
            this.minimumVisualRange = minimumVisualRange;
            ForceClear();
        }

        public void ConfigureFacing(float enemyYaw)
        {
            towardEnemyYaw = enemyYaw;
        }

        public void Begin(SkillData skill, Action onComplete)
        {
            nextPendingStateLogAt = Time.unscaledTime + 1f;
            Debug("MoveToRange", $"Start skill={SkillLabel(skill)} range={GetExecutionRange(skill):0.00} distance={GetRivalDistance():0.00} rival={ChampionLabel(getRival())}");
            pendingSkill = skill;
            pendingComplete = onComplete;
            Update();
        }

        public void Update()
        {
            if (pendingComplete == null || pendingSkill == null)
            {
                return;
            }

            ChampionBehaviour rival = getRival();
            if (rival == null || !isOwnerAlive() || !rival.IsAlive)
            {
                Debug("MoveToRange", $"Complete skill={SkillLabel(pendingSkill)} distance={GetRivalDistance():0.00} range={GetExecutionRange(pendingSkill):0.00}");
                Complete();
                return;
            }

            if (IsWithinSkillRange(pendingSkill))
            {
                FaceEnemy(Mathf.Max(Time.unscaledDeltaTime, MinimumMovementDeltaTime));
                IsTurning = !IsFacingEnemy();
                MovementFloat = 0f;
                if (!IsTurning)
                {
                    Debug("MoveToRange", $"Complete skill={SkillLabel(pendingSkill)} distance={GetRivalDistance():0.00} range={GetExecutionRange(pendingSkill):0.00}");
                    Complete();
                }

                return;
            }

            if (Time.unscaledTime >= nextPendingStateLogAt)
            {
                nextPendingStateLogAt = Time.unscaledTime + 1f;
                Debug("MoveToRange", $"Waiting distance={GetRivalDistance():0.00} range={GetExecutionRange(pendingSkill):0.00} delta={Time.unscaledDeltaTime:0.000} pos={ownerTransform.position} rivalPos={rival.transform.position}");
            }

            Vector3 targetPosition = GetSkillExecutionPosition(pendingSkill);
            float distanceToTarget = FlatDistance(ownerTransform.position, targetPosition);
            if (distanceToTarget <= SnapDistance)
            {
                SnapTo(targetPosition);
                Debug("MoveToRange", $"SnapComplete distanceToTarget={distanceToTarget:0.000} distance={GetRivalDistance():0.00} range={GetExecutionRange(pendingSkill):0.00}");
                return;
            }

            MoveTowards(targetPosition, Mathf.Max(Time.unscaledDeltaTime, MinimumMovementDeltaTime));
        }

        public void ForceClear()
        {
            pendingComplete = null;
            pendingSkill = null;
            nextPendingStateLogAt = 0f;
            MovementFloat = 0f;
            IsTurning = false;
        }

        public float GetRivalDistance()
        {
            ChampionBehaviour rival = getRival();
            return rival != null ? FlatDistance(ownerTransform.position, rival.transform.position) : -1f;
        }

        private void Complete()
        {
            Action complete = pendingComplete;
            ForceClear();
            Debug("Async", "Complete move-to-range step");
            complete?.Invoke();
        }

        private bool IsWithinSkillRange(SkillData skill)
        {
            ChampionBehaviour rival = getRival();
            return rival == null || FlatDistance(ownerTransform.position, rival.transform.position) <= GetExecutionRange(skill) + RangeCompletionTolerance;
        }

        private Vector3 GetSkillExecutionPosition(SkillData skill)
        {
            ChampionBehaviour rival = getRival();
            if (rival == null)
            {
                return ownerTransform.position;
            }

            Vector3 selfPosition = ownerTransform.position;
            Vector3 rivalPosition = rival.transform.position;
            Vector3 awayFromRival = selfPosition - rivalPosition;
            awayFromRival.y = 0f;
            if (awayFromRival.sqrMagnitude <= 0.000001f)
            {
                awayFromRival = -ownerTransform.forward;
                awayFromRival.y = 0f;
            }

            Vector3 targetPosition = rivalPosition + awayFromRival.normalized * GetExecutionRange(skill);
            targetPosition.y = lockedHeight;
            return targetPosition;
        }

        private void MoveTowards(Vector3 targetPosition, float deltaTime)
        {
            Vector3 current = ownerTransform.position;
            targetPosition.y = lockedHeight;
            FaceEnemy(deltaTime);
            IsTurning = !IsFacingEnemy();
            if (IsTurning)
            {
                MovementFloat = 0f;
                return;
            }

            Vector3 next = Vector3.MoveTowards(current, targetPosition, moveSpeed * deltaTime);
            next.y = lockedHeight;
            ownerTransform.position = next;
            MovementFloat = (next - current).sqrMagnitude > 0.000001f ? 1f : 0f;
        }

        private void SnapTo(Vector3 targetPosition)
        {
            targetPosition.y = lockedHeight;
            ownerTransform.position = targetPosition;
            MovementFloat = 0f;
            IsTurning = false;
        }

        private void FaceEnemy(float deltaTime)
        {
            Quaternion targetRotation = Quaternion.Euler(0f, towardEnemyYaw, 0f);
            Quaternion nextRotation = Quaternion.RotateTowards(ownerTransform.rotation, targetRotation, turnSpeedDegrees * deltaTime);
            ownerTransform.rotation = Quaternion.Euler(0f, nextRotation.eulerAngles.y, 0f);
        }

        private bool IsFacingEnemy()
        {
            return Mathf.Abs(Mathf.DeltaAngle(ownerTransform.eulerAngles.y, towardEnemyYaw)) <= 3f;
        }

        private float GetExecutionRange(SkillData skill)
        {
            return Mathf.Max(minimumVisualRange, skill != null ? skill.range : minimumVisualRange);
        }

        private void Debug(string step, string message)
        {
            if (!isDebugEnabled())
            {
                return;
            }

            CombatDebug.Log(nameof(ChampionMoveToRange), getOwnerLabel(), step, message, CombatDebug.ChampionColor, getFlowId());
        }

        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }

        private static string ChampionLabel(ChampionBehaviour behaviour)
        {
            if (behaviour == null)
            {
                return "null";
            }

            return behaviour.Champion != null && !string.IsNullOrEmpty(behaviour.Champion.ChampionName)
                ? behaviour.Champion.ChampionName
                : behaviour.name;
        }

        private static string SkillLabel(SkillData skill)
        {
            return skill != null && !string.IsNullOrEmpty(skill.skillName) ? skill.skillName : "null";
        }
    }
}
