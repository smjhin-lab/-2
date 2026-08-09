using UnityEngine;

namespace IndoorSim.Beacons
{
    /// <summary>
    /// BLE 비콘 객체 (iBeacon 규격 필드). 씬 어디에나 배치 가능하며 BeaconManager에 자동 등록된다.
    /// RF 파라미터는 전부 인스펙터/프리셋 데이터로 관리 — 하드코딩 금지 (명세 6장).
    /// </summary>
    public class BleBeacon : MonoBehaviour
    {
        [Header("식별자 (iBeacon)")]
        public string beaconId = "BCN-001";
        public string uuid = "F7826DA6-4FA2-4E98-8024-BC5B71E0893E";
        public int major = 1;
        public int minor = 1;

        [Header("송신 특성")]
        [Tooltip("방사 출력(dBm). 실제 광고 패킷의 Tx Power 필드")]
        public float txPowerDbm = 0f;
        [Tooltip("1m 기준 측정 RSSI(dBm) — 경로손실 모델의 기준값. 실측 캘리브레이션으로 교체")]
        public float measuredPowerAt1m = -59f;
        [Tooltip("광고 주기(ms)")]
        public int advertisingIntervalMs = 100;

        [Header("설치 정보")]
        [Tooltip("설치 높이 메모(m). 위치는 Transform이 진실값")]
        public float installHeightNote = 2.5f;

        public Vector3 Position => transform.position;

        void OnEnable() { BeaconManager.Register(this); EnsureVisual(); }
        void OnDisable() { BeaconManager.Unregister(this); }

        /// <summary>플레이 중에도 보이도록 작은 시각 마커 생성</summary>
        void EnsureVisual()
        {
            if (transform.Find("_visual") != null) return;
            var v = GameObject.CreatePrimitive(PrimitiveType.Cube);
            v.name = "_visual";
            v.transform.SetParent(transform, false);
            v.transform.localScale = Vector3.one * 0.14f;
            var col = v.GetComponent<Collider>();
            if (Application.isPlaying) Object.Destroy(col); else Object.DestroyImmediate(col);
            var sh = Shader.Find("Universal Render Pipeline/Lit");
            if (sh == null) sh = Shader.Find("Standard");
            var m = new Material(sh) { color = new Color(0.2f, 0.55f, 1f) };
            if (m.HasProperty("_EmissionColor"))
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", new Color(0.1f, 0.4f, 1f) * 2f);
            }
            v.GetComponent<Renderer>().sharedMaterial = m;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.55f, 1f, 0.9f);
            Gizmos.DrawSphere(transform.position, 0.15f);
            Gizmos.color = new Color(0.2f, 0.55f, 1f, 0.15f);
            Gizmos.DrawWireSphere(transform.position, 10f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.35f, beaconId);
#endif
        }
    }
}