using System.Collections.Generic;
using UnityEngine;

namespace IndoorSim
{
    /// <summary>
    /// 엘리베이터 캡. 탑승 후 E키로 다음 층 이동(순환), 숫자키 1~4로 층 직접 선택.
    /// 밖에서 접근하면 캡이 자동으로 그 층으로 호출된다.
    /// </summary>
    public class ElevatorCab : MonoBehaviour
    {
        public float[] floorYs = { -6f, -12f, -18f };
        public float speed = 2.0f;
        public float cabSize = 2.0f;

        readonly HashSet<CharacterController> riders = new HashSet<CharacterController>();
        float targetY;
        bool moving;
        Vector3 basePos; // XZ 고정

        void Start()
        {
            basePos = transform.position;
            targetY = transform.position.y;
        }

        public void RiderEnter(CharacterController cc) { riders.Add(cc); }
        public void RiderExit(CharacterController cc) { riders.Remove(cc); }

        int NearestFloorIndex(float y)
        {
            int best = 0;
            for (int i = 1; i < floorYs.Length; i++)
                if (Mathf.Abs(floorYs[i] - y) < Mathf.Abs(floorYs[best] - y)) best = i;
            return best;
        }

        void Update()
        {
            riders.RemoveWhere(r => r == null);

            if (riders.Count > 0 && !moving)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    int cur = NearestFloorIndex(transform.position.y);
                    targetY = floorYs[(cur + 1) % floorYs.Length];
                    moving = true;
                }
                for (int i = 0; i < floorYs.Length && i < 4; i++)
                    if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                    {
                        targetY = floorYs[i];
                        moving = true;
                    }
            }

            // 밖의 플레이어가 층에서 대기 중이면 호출 (간이 콜 버튼)
            if (riders.Count == 0 && !moving)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    Vector3 d = player.transform.position - new Vector3(basePos.x, player.transform.position.y, basePos.z);
                    if (d.magnitude < 4f)
                    {
                        int f = NearestFloorIndex(player.transform.position.y - 1f);
                        if (Mathf.Abs(floorYs[f] - transform.position.y) > 0.05f)
                        {
                            targetY = floorYs[f];
                            moving = true;
                        }
                    }
                }
            }

            if (moving)
            {
                float newY = Mathf.MoveTowards(transform.position.y, targetY, speed * Time.deltaTime);
                float dy = newY - transform.position.y;
                transform.position = new Vector3(basePos.x, newY, basePos.z);
                if (dy > 0f)
                    foreach (var r in riders)
                        if (r != null && r.enabled) r.Move(Vector3.up * dy);
                if (Mathf.Approximately(newY, targetY)) moving = false;
            }
        }
    }

    /// <summary>캡 내부 트리거 (탑승자 감지)</summary>
    public class ElevatorCabTrigger : MonoBehaviour
    {
        ElevatorCab cab;
        void Awake() { cab = GetComponentInParent<ElevatorCab>(); }
        void OnTriggerEnter(Collider other)
        {
            var cc = other.GetComponent<CharacterController>();
            if (cc != null && cab != null) cab.RiderEnter(cc);
        }
        void OnTriggerExit(Collider other)
        {
            var cc = other.GetComponent<CharacterController>();
            if (cc != null && cab != null) cab.RiderExit(cc);
        }
    }
}