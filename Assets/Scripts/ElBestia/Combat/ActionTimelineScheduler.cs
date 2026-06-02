using UnityEngine;

namespace ElBestia.Combat
{
    internal sealed class ActionTimelineScheduler
    {
        public void Schedule(ChampionBehaviour champion, CharacterIntention intention, float startTime)
        {
            float delay = champion.RollNextActionDelay();
            intention.champID = champion.ChampionId;
            intention.scheduledAt = startTime;
            intention.rolledDelay = delay;
            intention.baseTimeStamp = startTime + delay;
            intention.speedRating = champion.ActionSpeedRating;
            ApplyActionTimeMultiplier(intention, champion, startTime, false);
        }

        public void Retarget(CharacterIntention intention, ChampionBehaviour champion, float currentTime)
        {
            ApplyActionTimeMultiplier(intention, champion, currentTime, true);
        }

        private static void ApplyActionTimeMultiplier(CharacterIntention intention, ChampionBehaviour champion, float currentTime, bool clampToVisibleFuture)
        {
            float multiplier = Mathf.Max(0.001f, champion.GetActionTimeMultiplier());
            float baseDuration = Mathf.Max(0.001f, intention.baseTimeStamp - intention.scheduledAt);
            float recalculatedTimeStamp = intention.scheduledAt + baseDuration * multiplier;
            if (clampToVisibleFuture && recalculatedTimeStamp < currentTime)
            {
                recalculatedTimeStamp = currentTime + 0.2f;
            }

            intention.actionTimeMultiplier = multiplier;
            intention.timeStamp = recalculatedTimeStamp;
        }
    }
}
