using System;

namespace ElBestia.Combat
{
    [Serializable]
    public sealed class CharacterIntention
    {
        public string champID;
        public float scheduledAt;
        public float timeStamp;
        public float rolledDelay;
        public float baseTimeStamp;
        public int speedRating;
        public float actionTimeMultiplier = 1f;
    }
}
