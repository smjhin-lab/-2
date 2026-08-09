using UnityEngine;

namespace IndoorSim
{
    /// <summary>에스컬레이터 이동 벨트. 트리거 안의 CharacterController를 경사 방향으로 실어 나른다.</summary>
    public class EscalatorBelt : MonoBehaviour
    {
        public Vector3 moveDirection = Vector3.forward; // 정규화된 이동 방향 (하행이면 반대)
        public float speed = 0.75f;                     // m/s

        void OnTriggerStay(Collider other)
        {
            var cc = other.GetComponent<CharacterController>();
            if (cc != null && cc.enabled)
                cc.Move(moveDirection * speed * Time.deltaTime);
        }
    }
}