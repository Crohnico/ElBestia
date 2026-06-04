using UnityEngine;

namespace ElBestia.Combat
{
    public sealed class ActionTimeLine : MonoBehaviour
    {
        [Header("Crono")]
        [SerializeField] private CombatCrono crono;

        [Header("UI References")]
        [SerializeField] private RectTransform leftPlayerInitPos;
        [SerializeField] private RectTransform rightPlayerInitPos;
        [SerializeField] private RectTransform center;
        [SerializeField] private TimelineMarkerUI markerPrefab;

        [Header("Intentions")]
        [SerializeField] private CharacterIntention leftIntention;
        [SerializeField] private CharacterIntention rightIntention;
        [SerializeField] private float actionTimeoutSeconds = 20f;
        [SerializeField] private bool debugTimeline = true;

        private ChampionBehaviour leftChampion;
        private ChampionBehaviour rightChampion;
        private TimelineMarkerUI leftMarker;
        private TimelineMarkerUI rightMarker;
        private bool isWaitingForAction;
        private ChampionBehaviour waitingChampion;
        private float waitingStartedRealtime;
        private readonly ActionTimelineScheduler scheduler = new ActionTimelineScheduler();

        private void Awake()
        {
            if (crono == null)
            {
                crono = FindFirstObjectByType<CombatCrono>();
            }

            AutoBindMissingReferences();
        }

        private void Update()
        {
            if (isWaitingForAction || crono == null || !crono.IsRunning)
            {
                CheckActionTimeout();
                return;
            }

            bool leftReady = leftChampion != null && leftIntention != null && crono.CurrentTime >= leftIntention.timeStamp;
            bool rightReady = rightChampion != null && rightIntention != null && crono.CurrentTime >= rightIntention.timeStamp;

            if (!leftReady && !rightReady)
            {
                return;
            }

            if (leftReady && rightReady)
            {
                float timestampDelta = Mathf.Abs(leftIntention.timeStamp - rightIntention.timeStamp);
                if (timestampDelta <= 0.001f)
                {
                    ExecuteReadyChampion(Random.value < 0.5f ? leftChampion : rightChampion);
                }
                else
                {
                    ExecuteReadyChampion(leftIntention.timeStamp < rightIntention.timeStamp ? leftChampion : rightChampion);
                }
            }
            else
            {
                ExecuteReadyChampion(leftReady ? leftChampion : rightChampion);
            }
        }

        public void SetCrono(CombatCrono combatCrono)
        {
            crono = combatCrono;
        }

        [ContextMenu("Auto Bind Action TimeLine")]
        public void AutoBind()
        {
            leftPlayerInitPos = FindFirstRectTransform("LeftPlayerInitPos", "LeftPlayerPos", "LeftInitPos") ?? leftPlayerInitPos;
            rightPlayerInitPos = FindFirstRectTransform("RightPlayerInitPos", "RightPlayerPos", "RightInitPos") ?? rightPlayerInitPos;
            center = FindFirstRectTransform("Center", "TimelineCenter", "TimeLineCenter") ?? center;
        }

        public void Setup(ChampionBehaviour left, ChampionBehaviour right)
        {
            UnsubscribeFromChampionEvents(leftChampion);
            UnsubscribeFromChampionEvents(rightChampion);

            leftChampion = left;
            rightChampion = right;
            isWaitingForAction = false;
            SubscribeToChampionEvents(leftChampion);
            SubscribeToChampionEvents(rightChampion);

            leftMarker = CreateMarker(leftMarker, leftPlayerInitPos, leftChampion);
            rightMarker = CreateMarker(rightMarker, rightPlayerInitPos, rightChampion);

            Schedule(leftChampion, ref leftIntention, leftMarker, leftPlayerInitPos);
            Schedule(rightChampion, ref rightIntention, rightMarker, rightPlayerInitPos);
        }

        private void ExecuteReadyChampion(ChampionBehaviour champion)
        {
            if (champion == null || crono == null)
            {
                return;
            }

            isWaitingForAction = true;
            waitingChampion = champion;
            waitingStartedRealtime = Time.realtimeSinceStartup;
            DebugTimeline(champion, "ExecuteReady", $"time={crono.CurrentTime:0.00}");
            crono.Pause();
            GetMarker(champion)?.SnapTo(center);
            leftChampion?.FaceRivalAtTurnStart();
            rightChampion?.FaceRivalAtTurnStart();
            champion.ApplyStartTurnEffects();
            if (!champion.IsAlive)
            {
                DebugTimeline(champion, "ExecuteReady", "Champion died from start-turn effects.");
                isWaitingForAction = false;
                waitingChampion = null;
                crono.Pause();
                return;
            }

            champion.ExecuteAction(() => CompleteAction(champion));
        }

        private void CompleteAction(ChampionBehaviour champion)
        {
            DebugTimeline(champion, "CompleteAction", $"Before schedule waiting={isWaitingForAction} state={champion?.DebugStateSummary}");
            if (champion == leftChampion)
            {
                if (leftChampion.IsAlive)
                {
                    Schedule(leftChampion, ref leftIntention, leftMarker, leftPlayerInitPos);
                }
            }
            else if (champion == rightChampion)
            {
                if (rightChampion.IsAlive)
                {
                    Schedule(rightChampion, ref rightIntention, rightMarker, rightPlayerInitPos);
                }
            }

            isWaitingForAction = false;
            waitingChampion = null;
            if (leftChampion != null && rightChampion != null && (!leftChampion.IsAlive || !rightChampion.IsAlive))
            {
                crono?.Pause();
                return;
            }

            crono?.Resume();
            DebugTimeline(champion, "CompleteAction", $"Resume time={crono?.CurrentTime:0.00}");
        }

        private void CheckActionTimeout()
        {
            if (!isWaitingForAction || waitingChampion == null || actionTimeoutSeconds <= 0f)
            {
                return;
            }

            if (Time.realtimeSinceStartup - waitingStartedRealtime < actionTimeoutSeconds)
            {
                float waited = Time.realtimeSinceStartup - waitingStartedRealtime;
                if (Mathf.Abs(waited - Mathf.Round(waited)) < Time.unscaledDeltaTime)
                {
                    DebugTimeline(waitingChampion, "Waiting", $"waited={waited:0.00}/{actionTimeoutSeconds:0.00} state={waitingChampion.DebugStateSummary}");
                }

                return;
            }

            DebugTimeline(waitingChampion, "Timeout", $"state={waitingChampion.DebugStateSummary}");
            waitingChampion.ForceFinishStuckInteraction();
            CompleteAction(waitingChampion);
        }

        private void Schedule(ChampionBehaviour champion, ref CharacterIntention intention, TimelineMarkerUI marker, RectTransform initPos)
        {
            if (champion == null || crono == null)
            {
                return;
            }

            if (intention == null)
            {
                intention = new CharacterIntention();
            }

            float startTime = crono.CurrentTime;
            scheduler.Schedule(champion, intention, startTime);
            DebugTimeline(champion, "Schedule", $"start={startTime:0.00} baseArrival={intention.baseTimeStamp:0.00} arrival={intention.timeStamp:0.00} delay={intention.rolledDelay:0.00} multiplier={intention.actionTimeMultiplier:0.00}");
            if (marker != null)
            {
                marker.Travel(initPos, center, startTime, intention.timeStamp, crono);
            }
        }

        private void OnChampionActionSpeedStatusChanged(ChampionBehaviour champion)
        {
            if (champion == null || crono == null)
            {
                return;
            }

            if (champion == leftChampion)
            {
                RetargetIntention(leftIntention, leftMarker, champion);
            }
            else if (champion == rightChampion)
            {
                RetargetIntention(rightIntention, rightMarker, champion);
            }
        }

        private void RetargetIntention(CharacterIntention intention, TimelineMarkerUI marker, ChampionBehaviour champion)
        {
            if (intention == null || champion == null || crono == null)
            {
                return;
            }

            if (isWaitingForAction && champion == waitingChampion)
            {
                DebugTimeline(champion, "RetargetBlocked", $"time={crono.CurrentTime:0.00} state={champion.DebugStateSummary}");
                return;
            }

            float previousArrival = intention.timeStamp;
            float previousMultiplier = intention.actionTimeMultiplier;
            scheduler.Retarget(intention, champion, crono.CurrentTime);
            DebugTimeline(champion, "Retarget", $"time={crono.CurrentTime:0.00} oldArrival={previousArrival:0.00} newArrival={intention.timeStamp:0.00} baseStart={intention.scheduledAt:0.00} baseArrival={intention.baseTimeStamp:0.00} oldMultiplier={previousMultiplier:0.00} newMultiplier={intention.actionTimeMultiplier:0.00}");
            if (marker != null)
            {
                marker.Retarget(center, crono.CurrentTime, intention.timeStamp);
            }
        }

        private void DebugTimeline(ChampionBehaviour champion, string step, string message)
        {
            if (!debugTimeline)
            {
                return;
            }

            string championName = champion != null && champion.Champion != null && !string.IsNullOrEmpty(champion.Champion.ChampionName)
                ? champion.Champion.ChampionName
                : champion != null ? champion.name : "null";
            CombatDebug.Log(nameof(ActionTimeLine), championName, step, message, CombatDebug.TimelineColor);
        }

        private TimelineMarkerUI CreateMarker(TimelineMarkerUI current, RectTransform parent, ChampionBehaviour champion)
        {
            if (current != null)
            {
                DestroyMarker(current.gameObject);
            }

            if (parent == null)
            {
                return null;
            }

            TimelineMarkerUI marker;
            if (markerPrefab != null)
            {
                marker = Instantiate(markerPrefab, parent);
            }
            else
            {
                var markerObject = new GameObject("TimelineMarker", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(TimelineMarkerUI));
                markerObject.transform.SetParent(parent, false);
                marker = markerObject.GetComponent<TimelineMarkerUI>();
            }

            marker.SetAvatar(champion != null ? champion.GetTimelineAvatar() : null);
            marker.SnapTo(parent);
            return marker;
        }

        private TimelineMarkerUI GetMarker(ChampionBehaviour champion)
        {
            if (champion == leftChampion)
            {
                return leftMarker;
            }

            return champion == rightChampion ? rightMarker : null;
        }

        private static void DestroyMarker(GameObject marker)
        {
            if (Application.isPlaying)
            {
                Destroy(marker);
            }
            else
            {
                DestroyImmediate(marker);
            }
        }

        private void SubscribeToChampionEvents(ChampionBehaviour champion)
        {
            if (champion != null)
            {
                champion.ActionSpeedStatusChanged += OnChampionActionSpeedStatusChanged;
            }
        }

        private void UnsubscribeFromChampionEvents(ChampionBehaviour champion)
        {
            if (champion != null)
            {
                champion.ActionSpeedStatusChanged -= OnChampionActionSpeedStatusChanged;
            }
        }

        private void AutoBindMissingReferences()
        {
            if (leftPlayerInitPos == null || rightPlayerInitPos == null || center == null)
            {
                AutoBind();
            }
        }

        private static RectTransform FindFirstRectTransform(params string[] objectNames)
        {
            foreach (string objectName in objectNames)
            {
                GameObject found = GameObject.Find(objectName);
                RectTransform rectTransform = found != null ? found.transform as RectTransform : null;
                if (rectTransform != null)
                {
                    return rectTransform;
                }
            }

            return null;
        }
    }
}
