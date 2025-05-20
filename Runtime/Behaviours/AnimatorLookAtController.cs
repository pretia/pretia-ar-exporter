using UnityEngine;
using UnityEngine.Events;

namespace Pretia
{
    /// <summary>
    /// 
    /// Handles LookAt via Animator.
    /// 
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("Pretia/AnimatorLookAtController")]
    [HelpURL(PretiaDocs.COMPONENTS_ROOT + "/animator-look-at-controller")]
    public class AnimatorLookAtController : MonoBehaviour
    {
        [Tooltip("(0-1) the global weight of the LookAt, multiplier for other parameters.")]
        [Range(0.0f, 1.0f)]
        public float weight = 1.0f;
        
        [Tooltip("(0-1) determines how much the body is involved in the LookAt.")]
        [Range(0.0f, 1.0f)]
        public float bodyWeight = 0.2f;
        
        [Tooltip("(0-1) determines how much the head is involved in the LookAt.")]
        [Range(0.0f, 1.0f)]
        public float headWeight = 0.95f;
        
        [Tooltip("(0-1) determines how much the eyes are involved in the LookAt.")]
        [Range(0.0f, 1.0f)]
        public float eyesWeight = 1.0f;
        
        [Tooltip("(0-1) 0.0 means the character is unrestrained in motion. 1.0 means the character is clamped (look at becomes impossible). 0.5 means the character is able to move on half of the possible range (180 degrees).")]
        [Range(0.0f, 1.0f)]
        public float clampWeight = 0.5f;

        [Tooltip("Position offset to look at, relative to the target's actual location, in world space")]
        public Vector3 positionOffset = new Vector3(0.0f, 0.0f, 0.0f);

        [Tooltip("Automatically target the highest priority LookAtTarget object if no target is set")]
        public bool autoTarget = true;

        [Tooltip("Transform to look at.")]
        public Transform target;

        [Tooltip("Maximum distance at which headlook will be enabled")]
        public float lookRange = 40.0f;
        
        [Tooltip("Speed at which character blends to / from looking")]
        public float blendSpeed = 2.0f;

        [Tooltip("Called when the transform starts looking at a target " +
                 "(after it was previously not looking at any target)")]
        public UnityEvent OnHeadLookStart;
        
        [Tooltip("Called when the transform is no longer looking at any target")]
        public UnityEvent OnHeadLookStop;

        private Animator _animator;
        private float _blendAmt = 0.0f;
        private float _lastTargetUpdateTime = 0.0f;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            
            UpdateTarget();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (target != null)
            {
                _animator.SetLookAtWeight(_blendAmt * weight, bodyWeight, headWeight, eyesWeight, clampWeight);
                _animator.SetLookAtPosition(target.position + positionOffset);
            }
        }
        
        private void LateUpdate()
        {
            UpdateTarget();

            Vector3 offsetVector = target != null ? target.position - transform.position : Vector3.zero;

            if (// Check we have a target
                target != null &&
                // Check we are within range for headlook
                Vector3.Distance(transform.position, target.position) < lookRange &&
                // Check we are roughly still looking at the target
                Vector3.Angle(offsetVector, transform.forward) < (clampWeight + 0.25f) * 180.0f)
            {
                if (_blendAmt == 0)
                    OnHeadLookStart?.Invoke();

                // Blend current blendAmt toward maxBlendAmt
                _blendAmt = Mathf.MoveTowards(_blendAmt, 1.0f, Time.deltaTime * blendSpeed);
            }
            else if (_blendAmt > 0.0f)
            {
                _blendAmt = Mathf.MoveTowards(_blendAmt, 0, Time.deltaTime * blendSpeed);

                if (_blendAmt <= 0)
                    OnHeadLookStop?.Invoke();
            }
        }

        private void UpdateTarget()
        {
            if ((target == null || Time.time - _lastTargetUpdateTime > 1.0f) && autoTarget)
            {
                _lastTargetUpdateTime = Time.time;

                LookAtTarget[] targetObjs = GameObject.FindObjectsOfType<LookAtTarget>();

                target = null;
                
                float closestDistance = lookRange + 2.0f;
                // Find the closest object tagged as "LookTarget"
                foreach (LookAtTarget targetObj in targetObjs)
                {
                    Vector3 offsetVector = targetObj.transform.position - transform.position;

                    // We subtract the targetObjs priority to prefer higher priority targets
                    float d = offsetVector.magnitude - targetObj.Priority;

                    if (d <= closestDistance)
                    {
                        target = targetObj.transform;
                        closestDistance = d;
                    }
                }
            }
        }
    }
}