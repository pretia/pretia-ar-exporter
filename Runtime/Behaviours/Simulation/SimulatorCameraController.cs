using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pretia
{
    public class SimulatorCameraController : MonoBehaviour
    {
        public Vector2 yRotationLimits = new Vector2(-88.0f, 88.0f);
        public Vector2 zoomLimits = new Vector2(0, 0);

        public float rotationSpeed = 0.2f;
        public float moveSpeed = 1.2f;
        public float momentum = 10.0f;
        public float rotationBlendSpeed = 10.0f;

        private Vector3 focalPoint = Vector3.zero;

        private float zoom = 1.0f;
        private Vector3 eulerRotation = Vector3.zero;
        private Vector3 positionMomentum = Vector3.zero;

        private Vector3 lastMousePos = Vector3.zero;

        private Quaternion targetRotation = Quaternion.identity;
        private bool rotating = false;

        private void OnEnable()
        {
            focalPoint = transform.position;

            lastMousePos = Input.mousePosition;
            eulerRotation = transform.rotation.eulerAngles;
            targetRotation = Quaternion.Euler(eulerRotation);
            zoom = (focalPoint - transform.position).magnitude;

            if (GetComponent<Camera>())
                GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        }

        public void SetPositionRotation(Pose pose)
        {
            SetPositionRotation(pose.position, pose.rotation);
        }

        public void SetPositionRotation(Vector3 position, Quaternion rotation)
        {
            zoom = 0;
            focalPoint = position;
            eulerRotation = rotation.eulerAngles;
            targetRotation = rotation;
        }

        void LateUpdate()
        {
            // Prevent any interaction if we are currently inputting text
            if (EventSystem.current && EventSystem.current.currentSelectedGameObject &&
                EventSystem.current.currentSelectedGameObject.GetComponent<InputField>())
                return;

            Vector3 mouseDelta = Input.mousePosition - lastMousePos;

            // Right click rotation
            if (Input.GetMouseButton(1) && Input.touchCount < 2)
            {
                if (rotating)
                {
                    eulerRotation.y += mouseDelta.x * rotationSpeed;
                    eulerRotation.x -= mouseDelta.y * rotationSpeed;
                    eulerRotation.x = Mathf.Clamp(eulerRotation.x, yRotationLimits.x, yRotationLimits.y);

                    targetRotation = Quaternion.Euler(eulerRotation);
                }
                rotating = true;
            }
            else
            {
                rotating = false;
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.unscaledDeltaTime * rotationBlendSpeed);

            // Scroll wheel zoom
            if (Input.mouseScrollDelta.magnitude > 0)
            {
                zoom -= Input.mouseScrollDelta.y;
                zoom = Mathf.Clamp(zoom, zoomLimits.x, zoomLimits.y);
            }

            float speed = moveSpeed;

            // Hold Shift for speed boost
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                speed *= 4.0f;

            Vector3 cameraMovement = Vector3.zero;

            // Translation via WASD
            if (Input.GetKey(KeyCode.W))
                cameraMovement += transform.forward * speed;
            if (Input.GetKey(KeyCode.S))
                cameraMovement -= transform.forward * speed;
            if (Input.GetKey(KeyCode.D))
                cameraMovement += transform.right * speed;
            if (Input.GetKey(KeyCode.A))
                cameraMovement -= transform.right * speed;

            positionMomentum = Vector3.Lerp(positionMomentum, cameraMovement, Time.unscaledDeltaTime * momentum);

            focalPoint += positionMomentum * Time.unscaledDeltaTime;
            transform.position = focalPoint - (transform.rotation * Vector3.forward * zoom);

            lastMousePos = Input.mousePosition;
        }
    }
}
