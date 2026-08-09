using UnityEngine;

namespace IndoorSim.PlayerCtrl
{
    /// <summary>
    /// 1인칭 플레이어 (스마트폰을 든 사용자 역할).
    /// WASD 이동 / 마우스 시점 / Shift 달리기 / V 관전(비행) 모드 / Esc 커서 잠금 해제.
    /// 구(舊) Input Manager 사용 — Project Settings > Player > Active Input Handling = "Both" 권장.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        public float walkSpeed = 3.2f;
        public float runSpeed = 6f;
        public float mouseSensitivity = 2.2f;
        public float gravity = -18f;
        public Transform cameraPivot;

        CharacterController cc;
        float pitch;
        float velY;
        bool flyMode;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            if (cameraPivot == null && Camera.main != null && Camera.main.transform.IsChildOf(transform))
                cameraPivot = Camera.main.transform;
            Cursor.lockState = CursorLockMode.Locked;
        }

        void Update()
        {
            // 커서 잠금 토글
            if (Input.GetKeyDown(KeyCode.Escape)) Cursor.lockState = CursorLockMode.None;
            if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
                Cursor.lockState = CursorLockMode.Locked;

            // 시점
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
                float my = Input.GetAxis("Mouse Y") * mouseSensitivity;
                transform.Rotate(0f, mx, 0f);
                pitch = Mathf.Clamp(pitch - my, -85f, 85f);
                if (cameraPivot != null) cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }

            if (Input.GetKeyDown(KeyCode.V)) flyMode = !flyMode;

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            Vector3 move = (transform.right * h + transform.forward * v) * speed;

            if (flyMode)
            {
                // 관전 모드: 중력 무시, Q/E 상하 이동, 속도 3배
                float up = (Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f);
                cc.Move((move * 3f + Vector3.up * up * speed * 3f) * Time.deltaTime);
                velY = 0f;
                return;
            }

            if (cc.isGrounded) velY = -1f;
            else velY += gravity * Time.deltaTime;
            move.y = velY;
            cc.Move(move * Time.deltaTime);
        }
    }
}