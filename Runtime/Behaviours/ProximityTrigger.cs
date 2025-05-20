using System;
using UnityEngine;
using UnityEngine.Events;

namespace Pretia
{
    /// <summary>
    /// Trigger callbacks based on proximity of the MainCamera to this transform,
    /// as well as optional required distance of this transform from a ray cast
    /// from the camera.
    /// </summary>
    [AddComponentMenu("Pretia/ProximityTrigger")]
    [HelpURL(PretiaDocs.COMPONENTS_ROOT + "/proximity-trigger")]
    public class ProximityTrigger : MonoBehaviour
    {
        [Tooltip("Distance (in world space units / meters) from the camera at which OnEnter will be triggered")]
        public float enterProximity = 2.0f;
        [Tooltip("Distance (in world space units / meters) from the camera at which OnExit will be triggered after an OnEnter event")]
        public float exitProximity = 2.5f;
        [Tooltip("If greater than zero then this is the required distance (in world space units / meters) " +
                 "that this transform must be from the camera ray for an OnEnter to trigger. " +
                 "Set to 0 or less to remove this requirement.")]
        public float requiredLookRayDistance = 0.0f;

        [Tooltip("Time in seconds between proximity checks")]
        public float checkFrequency = 0.1f;

        [Tooltip("Called when the camera is within <enterProximity> meters of this transform, and requiredLookRayDistance is also satisfied")]
        public UnityEvent OnEnter;
        [Tooltip("Called after an OnEnter when the camera moves more than <exitProximity> meters from this transform")]
        public UnityEvent OnExit;
        
        private float _lastTick = 0.0f;
        private bool _inTriggerArea = false;

        private void LateUpdate()
        {
            if (Time.timeScale > 0 && Time.unscaledTime - _lastTick > checkFrequency)
            {
                _lastTick = Time.unscaledTime;
                CheckProximity();
            }
        }

        private bool CheckLookRay(Camera cam)
        {
            if (requiredLookRayDistance > 0)
            {
                // Find closest distance from camera ray to this transform
                Ray ray = cam.ScreenPointToRay(new Vector2(Screen.width / 2.0f, Screen.height / 2.0f));
                Vector3 point = transform.position;

                Vector3 rayDirection = ray.direction.normalized;
                Vector3 pointToRayOrigin = point - ray.origin;

                float dotProduct = Vector3.Dot(pointToRayOrigin, rayDirection);
                Vector3 closestPointOnRay = ray.origin + rayDirection * dotProduct;

                float dist = Vector3.Distance(point, closestPointOnRay);

                return dist < requiredLookRayDistance;
            }

            // If there requiredLookRayDistance is not set, then always pass the check
            return true;
        }

        private void CheckProximity()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                if (!_inTriggerArea)
                {
                    if (Vector3.Distance(transform.position, cam.transform.position) < enterProximity
                        && CheckLookRay(cam))
                    {
                        _inTriggerArea = true;
                        OnEnter?.Invoke();
                    }
                }
                else
                {
                    if (Vector3.Distance(transform.position, cam.transform.position) > exitProximity)
                    {
                        _inTriggerArea = false;
                        OnExit?.Invoke();
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, 0.9F);
            Gizmos.DrawWireSphere(transform.position, enterProximity);
            
            Gizmos.color = new Color(1, 1, 0, 0.5F);
            Gizmos.DrawWireSphere(transform.position, exitProximity);
        }
    }
}
