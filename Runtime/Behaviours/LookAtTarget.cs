using System;
using UnityEngine;

namespace Pretia
{
    /// <summary>
    /// Makes this a target for LookAtControllers.
    /// 
    /// priority value may be used to prioritise certain
    /// LookAtTarget objects over others.
    /// </summary>
    [AddComponentMenu("Pretia/LookAtTarget")]
    [HelpURL(PretiaDocs.COMPONENTS_ROOT + "/look-at-target")]
    public class LookAtTarget : MonoBehaviour
    {
        public static Action<LookAtTarget> OnFinalUpdate = null;

        [Tooltip("Priority of this target (subracted distance in meters from distance checks)")]
        [SerializeField] private float priority = 0;

        [SerializeField] private bool lockRotation = false;

        public float Priority { get => priority; set => priority = value; }

        private void LateUpdate()
        {
            if (lockRotation)
                transform.rotation = Quaternion.identity;
        }

        private void OnPreRender()
        {
            if (OnFinalUpdate != null)
                OnFinalUpdate.Invoke(this);
        }
    }
}