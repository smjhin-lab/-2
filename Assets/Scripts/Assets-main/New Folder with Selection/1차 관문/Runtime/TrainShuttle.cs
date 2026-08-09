using UnityEngine;

namespace IndoorSim
{
    /// <summary>열차 왕복 이동 (시연용). 승강장 중앙에 정차했다가 출발한다.</summary>
    public class TrainShuttle : MonoBehaviour
    {
        public Vector3 axis = Vector3.right; // 이동 축
        public float fromOffset = -60f;      // 시작 오프셋 (초기 위치 기준)
        public float toOffset = 60f;         // 끝 오프셋
        public float stopOffset = 0f;        // 정차 지점
        public float speed = 12f;
        public float dwellSeconds = 8f;

        Vector3 origin;
        float pos;
        int dirSign = 1;
        float dwell;
        bool stoppedHere;

        void Start()
        {
            origin = transform.position;
            pos = fromOffset;
        }

        void Update()
        {
            if (dwell > 0f) { dwell -= Time.deltaTime; return; }

            float prev = pos;
            pos += speed * dirSign * Time.deltaTime;

            // 정차 지점 통과 시 정차
            if (!stoppedHere && ((prev - stopOffset) * (pos - stopOffset) <= 0f))
            {
                pos = stopOffset;
                dwell = dwellSeconds;
                stoppedHere = true;
            }
            if (pos >= toOffset) { pos = toOffset; dirSign = -1; stoppedHere = false; dwell = 2f; }
            if (pos <= fromOffset) { pos = fromOffset; dirSign = 1; stoppedHere = false; dwell = 2f; }

            transform.position = origin + axis.normalized * pos;
        }
    }
}