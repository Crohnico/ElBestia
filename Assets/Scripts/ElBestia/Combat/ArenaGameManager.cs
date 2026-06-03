using ElBestia.Champions;
using ElBestia.UI;
using ElBestia.Visuals;
using UnityEngine;

namespace ElBestia.Combat
{
    public sealed class ArenaGameManager : MonoBehaviour
    {
        [Header("Champions")]
        [SerializeField] private ChampionSO leftChampion;
        [SerializeField] private ChampionSO rightChampion;

        [Header("Scene References")]
        [SerializeField] private GameObject debugChampionPrefab;
        [SerializeField] private Transform spawnLeft;
        [SerializeField] private Transform spawnRight;
        [SerializeField] private GameplayCanvas gameplayCanvas;
        [SerializeField] private CombatCrono crono;
        [SerializeField] private ActionTimeLine actionTimeLine;
        [SerializeField] private float spawnY = 1f;

        [Header("Debug Match")]
        [SerializeField] private float matchDurationSeconds = 60f;
        [SerializeField] private Vector2Int debugDamageRange = new Vector2Int(8, 20);

        [Header("Debug Logs")]
        [SerializeField] private bool paintDebugs = true;
        [SerializeField] private bool debugTimeline = true;
        [SerializeField] private bool debugChampion = true;
        [SerializeField] private bool debugMovement = true;
        [SerializeField] private bool debugActionFlow = true;
        [SerializeField] private bool debugDamage = true;
        [SerializeField] private bool debugCharges = true;

        [Header("Headless Simulation")]
        [SerializeField] private int simulationSeed = 12345;
        [SerializeField] private bool randomizeSimulationSeed;
        [SerializeField] private int simulationRuns = 1;
        [SerializeField] private bool logSimulationEvents;

        private GameObject leftInstance;
        private GameObject rightInstance;
        private float remainingSeconds;
        private ChampionBehaviour leftBehaviour;
        private ChampionBehaviour rightBehaviour;

        private void Awake()
        {
            ApplyDebugSettings();

            if (gameplayCanvas == null)
            {
                gameplayCanvas = FindFirstObjectByType<GameplayCanvas>();
            }

            if (crono == null)
            {
                crono = FindFirstObjectByType<CombatCrono>();
            }

            if (crono == null)
            {
                crono = GetComponent<CombatCrono>();
            }

            if (crono == null)
            {
                crono = gameObject.AddComponent<CombatCrono>();
            }

            if (actionTimeLine == null)
            {
                actionTimeLine = FindFirstObjectByType<ActionTimeLine>();
            }
        }

        private void OnValidate()
        {
            ApplyDebugSettings();
        }

        private void Update()
        {
            if (crono == null || !crono.IsRunning)
            {
                return;
            }

            remainingSeconds = Mathf.Max(0f, remainingSeconds - crono.DeltaTime);
            if (gameplayCanvas != null)
            {
                gameplayCanvas.SetTimer(remainingSeconds);
            }

            if (remainingSeconds <= 0f)
            {
                crono.Pause();
            }
        }

        public void SetUpGame()
        {
            ApplyDebugSettings();

            if (crono != null)
            {
                crono.ResetTime();
                crono.Pause();
            }

            remainingSeconds = Mathf.Max(0f, matchDurationSeconds);

            SpawnDebugChampion(ref leftInstance, spawnLeft, "Left Champion");
            SpawnDebugChampion(ref rightInstance, spawnRight, "Right Champion");
            ConfigureStickman(leftInstance, leftChampion);
            ConfigureStickman(rightInstance, rightChampion);
            leftBehaviour = PrepareBehaviour(leftInstance, leftChampion, spawnLeft, crono);
            rightBehaviour = PrepareBehaviour(rightInstance, rightChampion, spawnRight, crono);

            if (leftBehaviour != null)
            {
                leftBehaviour.SetRival(rightBehaviour);
            }

            if (rightBehaviour != null)
            {
                rightBehaviour.SetRival(leftBehaviour);
            }

            if (leftBehaviour != null)
            {
                leftBehaviour.SkillExecuted -= OnSkillExecuted;
                leftBehaviour.SkillExecuted += OnSkillExecuted;
                leftBehaviour.HealthChanged -= OnHealthChanged;
                leftBehaviour.HealthChanged += OnHealthChanged;
                leftBehaviour.ChargesChanged -= OnChargesChanged;
                leftBehaviour.ChargesChanged += OnChargesChanged;
            }

            if (rightBehaviour != null)
            {
                rightBehaviour.SkillExecuted -= OnSkillExecuted;
                rightBehaviour.SkillExecuted += OnSkillExecuted;
                rightBehaviour.HealthChanged -= OnHealthChanged;
                rightBehaviour.HealthChanged += OnHealthChanged;
                rightBehaviour.ChargesChanged -= OnChargesChanged;
                rightBehaviour.ChargesChanged += OnChargesChanged;
            }

            if (gameplayCanvas != null)
            {
                gameplayCanvas.SetCrono(crono);
                gameplayCanvas.SetChampion(GameplaySide.Left, GetChampionName(leftChampion, "Left Champion"), leftBehaviour != null ? leftBehaviour.CurrentHealth : 100, leftBehaviour != null ? leftBehaviour.MaxHealth : 100);
                gameplayCanvas.SetChampion(GameplaySide.Right, GetChampionName(rightChampion, "Right Champion"), rightBehaviour != null ? rightBehaviour.CurrentHealth : 100, rightBehaviour != null ? rightBehaviour.MaxHealth : 100);
                gameplayCanvas.SetChampionSkills(GameplaySide.Left, leftChampion != null ? leftChampion.BaseSkill : null, leftChampion != null ? leftChampion.Skills : null);
                gameplayCanvas.SetChampionSkills(GameplaySide.Right, rightChampion != null ? rightChampion.BaseSkill : null, rightChampion != null ? rightChampion.Skills : null);
                gameplayCanvas.SetChampionCharges(GameplaySide.Left, leftBehaviour != null ? leftBehaviour.ActiveCharges : null);
                gameplayCanvas.SetChampionCharges(GameplaySide.Right, rightBehaviour != null ? rightBehaviour.ActiveCharges : null);
                gameplayCanvas.SetTimer(remainingSeconds);
            }

            if (actionTimeLine != null)
            {
                actionTimeLine.SetCrono(crono);
                actionTimeLine.Setup(leftBehaviour, rightBehaviour);
            }
        }

        public void StartGame()
        {
            if (leftBehaviour == null || rightBehaviour == null)
            {
                SetUpGame();
            }

            remainingSeconds = Mathf.Max(0f, matchDurationSeconds);
            if (crono != null)
            {
                crono.Resume();
            }

            if (gameplayCanvas != null)
            {
                gameplayCanvas.SetTimer(remainingSeconds);
            }
        }

        public void LeftReciveRandomDamage()
        {
            LeftReceiveRandomDamage();
        }

        public void LeftReceiveRandomDamage()
        {
            EnsureGameReady();
            if (leftBehaviour != null)
            {
                leftBehaviour.ReceiveDebugDamage(RollRandomDamage());
            }
        }

        public void RightReceiveRandomDamage()
        {
            EnsureGameReady();
            if (rightBehaviour != null)
            {
                rightBehaviour.ReceiveDebugDamage(RollRandomDamage());
            }
        }

        public void LeftTriggerSkillCooldown(int skillIndex)
        {
            EnsureGameReady();
            if (gameplayCanvas != null)
            {
                gameplayCanvas.StartSkillCooldown(GameplaySide.Left, skillIndex);
            }
        }

        public void RightTriggerSkillCooldown(int skillIndex)
        {
            EnsureGameReady();
            if (gameplayCanvas != null)
            {
                gameplayCanvas.StartSkillCooldown(GameplaySide.Right, skillIndex);
            }
        }

        [ContextMenu("Run Headless Simulation")]
        public void RunHeadlessSimulation()
        {
            int runs = Mathf.Max(1, simulationRuns);
            int leftWins = 0;
            int rightWins = 0;
            int draws = 0;
            CombatSimulationResult lastResult = null;
            for (int i = 0; i < runs; i++)
            {
                int seed = randomizeSimulationSeed ? Random.Range(int.MinValue, int.MaxValue) : simulationSeed + i;
                lastResult = SimulateCurrentMatch(seed);
                if (lastResult.winner == CombatSimulationSide.Left)
                {
                    leftWins++;
                }
                else if (lastResult.winner == CombatSimulationSide.Right)
                {
                    rightWins++;
                }
                else
                {
                    draws++;
                }
            }

            Debug.Log($"[HeadlessSimulation] runs={runs} leftWins={leftWins} rightWins={rightWins} draws={draws} lastSeed={lastResult?.seed} lastWinner={lastResult?.winner} lastDuration={lastResult?.duration:0.00} leftHp={lastResult?.leftHealth} rightHp={lastResult?.rightHealth}");
            if (logSimulationEvents && lastResult != null)
            {
                foreach (CombatSimulationEvent simulationEvent in lastResult.events)
                {
                    Debug.Log($"[HeadlessSimulation][{simulationEvent.time:0.00}][{simulationEvent.type}] {simulationEvent.actor}->{simulationEvent.target} skill={simulationEvent.skillName} amount={simulationEvent.amount} hp={simulationEvent.targetHealthAfter} charge={simulationEvent.charge} {simulationEvent.message}");
                }
            }
        }

        public CombatSimulationResult SimulateCurrentMatch(int seed)
        {
            return CombatSimulator.Run(leftChampion, rightChampion, new CombatSimulationOptions
            {
                seed = seed,
                maxDurationSeconds = matchDurationSeconds
            });
        }

        private void EnsureGameReady()
        {
            if (leftBehaviour == null || rightBehaviour == null)
            {
                SetUpGame();
            }
        }

        private int RollRandomDamage()
        {
            int min = Mathf.Min(debugDamageRange.x, debugDamageRange.y);
            int max = Mathf.Max(debugDamageRange.x, debugDamageRange.y);
            return Random.Range(min, max + 1);
        }

        private void ApplyDebugSettings()
        {
            CombatDebug.Configure(
                paintDebugs,
                debugTimeline,
                debugChampion,
                debugMovement,
                debugActionFlow,
                debugDamage,
                debugCharges);
        }

        private void SpawnDebugChampion(ref GameObject instance, Transform spawnPoint, string fallbackName)
        {
            if (spawnPoint == null)
            {
                return;
            }

            if (instance != null)
            {
                DestroyInstance(instance);
            }

            Vector3 position = spawnPoint.position;
            position.y = spawnY;
            instance = debugChampionPrefab != null
                ? Instantiate(debugChampionPrefab, position, spawnPoint.rotation)
                : GameObject.CreatePrimitive(PrimitiveType.Capsule);
            instance.transform.SetPositionAndRotation(position, spawnPoint.rotation);
            instance.name = fallbackName;
        }

        private static ChampionBehaviour PrepareBehaviour(GameObject instance, ChampionSO champion, Transform home, CombatCrono crono)
        {
            if (instance == null)
            {
                return null;
            }

            ChampionBehaviour behaviour = instance.GetComponent<ChampionBehaviour>();
            if (behaviour == null)
            {
                behaviour = instance.AddComponent<ChampionBehaviour>();
            }

            behaviour.Initialize(champion, home, null, crono);
            return behaviour;
        }

        private static void ConfigureStickman(GameObject instance, ChampionSO champion)
        {
            if (instance == null)
            {
                return;
            }

            StickmanBodyConfigurator configurator = instance.GetComponentInChildren<StickmanBodyConfigurator>(true);
            if (configurator != null)
            {
                configurator.ConfigureFromChampion(champion);
            }
        }

        private void OnSkillExecuted(ChampionBehaviour behaviour, int skillIndex)
        {
            if (gameplayCanvas == null)
            {
                return;
            }

            if (behaviour == leftBehaviour)
            {
                gameplayCanvas.StartSkillCooldown(GameplaySide.Left, skillIndex);
            }
            else if (behaviour == rightBehaviour)
            {
                gameplayCanvas.StartSkillCooldown(GameplaySide.Right, skillIndex);
            }
        }

        private void OnHealthChanged(ChampionBehaviour behaviour, int currentHealth, int maxHealth)
        {
            if (gameplayCanvas == null)
            {
                return;
            }

            if (behaviour == leftBehaviour)
            {
                gameplayCanvas.SetChampionHealth(GameplaySide.Left, currentHealth, maxHealth);
            }
            else if (behaviour == rightBehaviour)
            {
                gameplayCanvas.SetChampionHealth(GameplaySide.Right, currentHealth, maxHealth);
            }

            if (currentHealth <= 0)
            {
                crono?.Pause();
            }
        }

        private void OnChargesChanged(ChampionBehaviour behaviour, ActiveCharge[] charges)
        {
            if (gameplayCanvas == null)
            {
                return;
            }

            if (behaviour == leftBehaviour)
            {
                gameplayCanvas.SetChampionCharges(GameplaySide.Left, charges);
            }
            else if (behaviour == rightBehaviour)
            {
                gameplayCanvas.SetChampionCharges(GameplaySide.Right, charges);
            }
        }

        private static void DestroyInstance(GameObject instance)
        {
            if (Application.isPlaying)
            {
                Destroy(instance);
            }
            else
            {
                DestroyImmediate(instance);
            }
        }

        private static string GetChampionName(ChampionSO champion, string fallbackName)
        {
            if (champion != null && !string.IsNullOrEmpty(champion.ChampionName))
            {
                return champion.ChampionName;
            }

            return fallbackName;
        }
    }
}
