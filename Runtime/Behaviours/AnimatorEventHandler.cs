using UnityEngine;

namespace Pretia
{
    /// <summary>
    /// Allows for easy hook up of UnityEvents to setting Animator parameters
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("Pretia/AnimatorEventHandler")]
    [HelpURL(PretiaDocs.COMPONENTS_ROOT + "/animator-event-handler")]
    public class AnimatorEventHandler : MonoBehaviour
    {
        [Tooltip("Paramater to use for SetBool and SetInteger calls")]
        public string defaultParamName = "paramName";
        
        private Animator _animator;
        
        void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        
        public void SetBoolTrue(string name) => _animator.SetBool(name, true);
        public void SetBoolFalse(string name) => _animator.SetBool(name, false);
        
        public void SetBool(bool value) => _animator.SetBool(defaultParamName, value);
        public void SetInteger(int value) => _animator.SetInteger(defaultParamName, value);
    }
}