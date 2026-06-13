using System;
using System.Collections;
using ElBestia.Skills;
using ElBestia.Visuals;
using UnityEngine;

namespace ElBestia.Combat
{
    public sealed class ChampionCombatPresentation : MonoBehaviour
    {
        [Serializable]
        public sealed class WeaponAnimation
        {
            public WeaponType weapon;
            public string slashState;
            [Range(0f, 1f)] public float projectileRelease = 0.72f;
            public Transform launchOrigin;
        }

        private static readonly int ForwardId = Animator.StringToHash("FORWARD");
        private static readonly int RightId = Animator.StringToHash("RIGHT");
        private static readonly int HitId = Animator.StringToHash("HIT");
        private static readonly int DeathId = Animator.StringToHash("DEATH");

        [SerializeField] private Animator animator;
        [SerializeField] private CombatProjectileCatalogSO projectileCatalog;
        [SerializeField] private WeaponAnimation[] weaponAnimations =
        {
            new WeaponAnimation { weapon = WeaponType.Fists, slashState = "FIST_SLASH", projectileRelease = 0.65f },
            new WeaponAnimation { weapon = WeaponType.Sword, slashState = "SWORD_SLASH", projectileRelease = 0.72f },
            new WeaponAnimation { weapon = WeaponType.Axe, slashState = "AXE_SLASH", projectileRelease = 0.8f },
            new WeaponAnimation { weapon = WeaponType.Spear, slashState = "SPEAR_SLASH", projectileRelease = 0.8f },
            new WeaponAnimation { weapon = WeaponType.Staff, slashState = "STAFF_SLASH", projectileRelease = 0.75f }
        };
        [Header("Impact Reactions")]
        [SerializeField, Min(0f)] private float reactionBlendSeconds = 0.12f;
        [SerializeField, Min(0.01f)] private float fallbackReleaseSeconds = 0.18f;
        [SerializeField, Min(0.01f)] private float fallbackSphereScale = 0.12f;

        private ChampionBehaviour owner;
        private WeaponSfxOrigin equippedWeaponOrigin;
        private Coroutine castRoutine;
        private Coroutine reactionRoutine;
        private bool isLeftSide;

        public WeaponAnimation[] WeaponAnimations => weaponAnimations;
        public Animator Animator => animator;

        private void Awake()
        {
            EnsureAnimator();
            DisableRootMotion();
        }

        private void OnEnable()
        {
            EnsureAnimator();
            DisableRootMotion();
        }

        private void OnAnimatorMove()
        {
            DisableRootMotion();
        }

        public void Initialize(ChampionBehaviour champion, bool leftSide)
        {
            owner = champion;
            isLeftSide = leftSide;
            EnsureAnimator();
            DisableRootMotion();

            if (projectileCatalog == null)
            {
                projectileCatalog = Resources.Load<CombatProjectileCatalogSO>("CombatProjectileCatalog");
            }

            StickmanBodyConfigurator configurator = GetComponentInChildren<StickmanBodyConfigurator>(true);
            equippedWeaponOrigin = configurator != null ? configurator.EquippedWeaponSfxOrigin : null;
        }

        public void SetLocomotion(float forward, float right)
        {
            if (animator == null)
            {
                return;
            }

            DisableRootMotion();
            animator.SetFloat(ForwardId, Mathf.Clamp01(forward));
            animator.SetFloat(RightId, Mathf.Clamp(right, -1f, 1f));
        }

        public float GetTurnValue(bool towardEnemy)
        {
            return towardEnemy
                ? (isLeftSide ? -1f : 1f)
                : (isLeftSide ? 1f : -1f);
        }

        public void PlayCast(SkillData skill, ChampionBehaviour target, Action onImpact)
        {
            if (castRoutine != null)
            {
                StopCoroutine(castRoutine);
            }

            castRoutine = StartCoroutine(PlayCastRoutine(skill, target, onImpact));
        }

        public void PlayImpactReaction(bool survived, Action onComplete)
        {
            if (!survived)
            {
                PlayDeath();
                onComplete?.Invoke();
                return;
            }

            if (reactionRoutine != null)
            {
                StopCoroutine(reactionRoutine);
            }

            reactionRoutine = StartCoroutine(PlayImpactReactionRoutine(onComplete));
        }

        public void PlayDeath()
        {
            if (reactionRoutine != null)
            {
                StopCoroutine(reactionRoutine);
                reactionRoutine = null;
            }

            if (animator != null)
            {
                PlayBlendedState(DeathId);
            }
        }

        public void PreviewSlash(WeaponType weapon, float normalizedTime)
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            WeaponAnimation settings = FindSettings(weapon);
            if (animator != null && settings != null && !string.IsNullOrWhiteSpace(settings.slashState))
            {
                animator.Play(settings.slashState, 0, Mathf.Clamp01(normalizedTime));
                animator.Update(0f);
            }
        }

        private IEnumerator PlayCastRoutine(SkillData skill, ChampionBehaviour target, Action onImpact)
        {
            WeaponType weapon = owner != null && owner.Champion != null ? owner.Champion.EquippedWeapon : WeaponType.Fists;
            WeaponAnimation settings = FindSettings(weapon);
            bool playedState = animator != null && settings != null && !string.IsNullOrWhiteSpace(settings.slashState);
            if (playedState)
            {
                animator.Play(settings.slashState, 0, 0f);
                yield return null;
                int stateHash = Animator.StringToHash(settings.slashState);
                float timeoutAt = Time.unscaledTime + 5f;
                bool enteredState = false;
                while (animator != null && Time.unscaledTime < timeoutAt)
                {
                    AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                    if (state.shortNameHash == stateHash)
                    {
                        enteredState = true;
                    }

                    if ((enteredState && state.shortNameHash != stateHash)
                        || (state.shortNameHash == stateHash && state.normalizedTime >= settings.projectileRelease))
                    {
                        break;
                    }

                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSecondsRealtime(fallbackReleaseSeconds);
            }

            castRoutine = null;
            if (target == null || !SkillTargetsEnemy(skill))
            {
                onImpact?.Invoke();
                yield break;
            }

            LaunchProjectile(skill, weapon, settings, target, onImpact);
        }

        private void LaunchProjectile(SkillData skill, WeaponType weapon, WeaponAnimation settings, ChampionBehaviour target, Action onImpact)
        {
            Transform origin = settings != null && settings.launchOrigin != null
                ? settings.launchOrigin
                : equippedWeaponOrigin != null ? equippedWeaponOrigin.LaunchPoint : transform;

            GameObject prefab = null;
            float speed = 12f;
            projectileCatalog?.TryGet(weapon, skill != null ? skill.element : SkillElement.None, out prefab, out speed);
            GameObject projectileObject = prefab != null
                ? Instantiate(prefab, origin.position, Quaternion.identity)
                : CreateFallbackProjectile(origin.position);

            CombatProjectile projectile = projectileObject.GetComponent<CombatProjectile>();
            if (projectile == null)
            {
                projectile = projectileObject.AddComponent<CombatProjectile>();
            }

            projectile.Launch(target.transform, speed, () =>
            {
                target.FaceRivalBeforeImpactForCombat(onImpact);
            });
        }

        private IEnumerator PlayImpactReactionRoutine(Action onComplete)
        {
            if (animator == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            int stateHash = HitId;
            PlayBlendedState(stateHash);
            yield return null;

            float timeoutAt = Time.unscaledTime + 5f;
            bool enteredState = false;
            while (animator != null && Time.unscaledTime < timeoutAt)
            {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                if (state.shortNameHash == stateHash)
                {
                    enteredState = true;
                }
                else if (enteredState)
                {
                    break;
                }

                yield return null;
            }

            reactionRoutine = null;
            onComplete?.Invoke();
        }

        private void PlayBlendedState(int stateHash)
        {
            DisableRootMotion();
            if (reactionBlendSeconds <= 0f)
            {
                animator.Play(stateHash, 0, 0f);
                return;
            }

            animator.CrossFadeInFixedTime(stateHash, reactionBlendSeconds, 0, 0f);
        }

        private GameObject CreateFallbackProjectile(Vector3 position)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Fallback Combat Projectile";
            sphere.transform.position = position;
            sphere.transform.localScale = Vector3.one * fallbackSphereScale;
            Collider collider = sphere.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            return sphere;
        }

        private WeaponAnimation FindSettings(WeaponType weapon)
        {
            if (weaponAnimations != null)
            {
                foreach (WeaponAnimation settings in weaponAnimations)
                {
                    if (settings != null && settings.weapon == weapon)
                    {
                        return settings;
                    }
                }
            }

            return null;
        }

        private static bool PhaseTargetsEnemy(SkillAction[] actions)
        {
            if (actions == null)
            {
                return false;
            }

            foreach (SkillAction action in actions)
            {
                if (action != null && action.target == SkillTarget.Enemy)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool SkillTargetsEnemy(SkillData skill)
        {
            return skill != null && (PhaseTargetsEnemy(skill.cast) || PhaseTargetsEnemy(skill.postCast));
        }

        private void EnsureAnimator()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }
        }

        private void DisableRootMotion()
        {
            if (animator != null && animator.applyRootMotion)
            {
                animator.applyRootMotion = false;
            }
        }
    }
}
