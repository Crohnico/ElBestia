using System;
using UnityEngine;

namespace ElBestia.Combat
{
    public sealed class CombatProjectile : MonoBehaviour
    {
        private Transform target;
        private float speed;
        private Action onImpact;

        public void Launch(Transform impactTarget, float travelSpeed, Action impactCallback)
        {
            target = impactTarget;
            speed = Mathf.Max(0.1f, travelSpeed);
            onImpact = impactCallback;
        }

        private void Update()
        {
            if (target == null)
            {
                Impact();
                return;
            }

            Vector3 targetPosition = target.position + Vector3.up;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.unscaledDeltaTime);
            if ((transform.position - targetPosition).sqrMagnitude <= 0.0025f)
            {
                Impact();
            }
        }

        private void Impact()
        {
            Action callback = onImpact;
            onImpact = null;
            callback?.Invoke();
            Destroy(gameObject);
        }
    }
}
