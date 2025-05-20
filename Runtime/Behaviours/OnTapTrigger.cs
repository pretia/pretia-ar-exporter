using UnityEngine;
using UnityEngine.Events;

namespace Pretia
{
    /// <summary>
    /// Pretia AR Behaviour to trigger an action when the object is tapped
    /// </summary>
    [AddComponentMenu("Pretia/OnTapTrigger")]
    [HelpURL(PretiaDocs.COMPONENTS_ROOT + "/on-tap-trigger")]
    public class OnTapTrigger : MonoBehaviour
    {
        public UnityEvent<bool> OnTap;
        public UnityEvent<Vector3> OnTapPosition;

        private bool _active = false;

        private void OnMouseDown()
        {
            _active = !_active;
            OnTap?.Invoke(_active);

            if (OnTapPosition != null && OnTapPosition.GetPersistentEventCount() > 0)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
                {
                    OnTapPosition.Invoke(hit.point);
                }
            }
        }
    }
}