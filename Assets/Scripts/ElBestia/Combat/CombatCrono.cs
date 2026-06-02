using UnityEngine;

namespace ElBestia.Combat
{
    public sealed class CombatCrono : MonoBehaviour
    {
        [SerializeField] private bool runOnAwake;

        public float CurrentTime { get; private set; }
        public float DeltaTime { get; private set; }
        public bool IsRunning { get; private set; }

        private void Awake()
        {
            IsRunning = runOnAwake;
        }

        private void Update()
        {
            if (!IsRunning)
            {
                DeltaTime = 0f;
                return;
            }

            DeltaTime = Time.deltaTime;
            CurrentTime += DeltaTime;
        }

        public void ResetTime()
        {
            CurrentTime = 0f;
            DeltaTime = 0f;
        }

        public void Pause()
        {
            IsRunning = false;
            DeltaTime = 0f;
        }

        public void Resume()
        {
            IsRunning = true;
        }
    }
}
