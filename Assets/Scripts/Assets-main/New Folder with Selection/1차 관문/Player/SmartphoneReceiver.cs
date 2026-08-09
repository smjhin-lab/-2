using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using IndoorSim.Beacons;

namespace IndoorSim.PlayerCtrl
{
    // =====================================================================
    // 신호 모델 확장 포인트 (명세 4.4)
    // 팀 연구 결과(레이캐스트 매질 감쇠, 인체 차폐, 노이즈 모델 등)가 확정되면
    // ISignalModel 구현체를 새로 만들어 SmartphoneReceiver.SignalModel에 주입하면 된다.
    // 예) receiver.SignalModel = new RaycastAttenuationModel(materialDb, crowdSystem);
    // =====================================================================
    public interface ISignalModel
    {
        /// <summary>수신 RSSI(dBm) 계산. 수신 불가면 float.NegativeInfinity 반환.</summary>
        float GetRssi(BleBeacon beacon, Vector3 receiverPos);
    }

    /// <summary>
    /// [임시] 자유공간 로그-거리 경로손실 모델. RSSI = P1m − 10·n·log10(d) + N(0,σ)
    /// 벽/인체 감쇠 미포함 — 추후 연구 결과로 교체 예정.
    /// </summary>
    public class PlaceholderDistanceModel : ISignalModel
    {
        public float pathLossExponent = 2.0f; // n (실측 캘리브레이션 대상)
        public float noiseSigmaDb = 1.5f;     // 가우시안 노이즈 σ
        public float maxRangeMeters = 35f;

        public float GetRssi(BleBeacon b, Vector3 receiverPos)
        {
            float d = Mathf.Max(0.1f, Vector3.Distance(b.Position, receiverPos));
            if (d > maxRangeMeters) return float.NegativeInfinity;
            float rssi = b.measuredPowerAt1m - 10f * pathLossExponent * Mathf.Log10(d);
            rssi += Gaussian() * noiseSigmaDb;
            return rssi;
        }

        static float Gaussian()
        {
            float u1 = Mathf.Max(1e-6f, UnityEngine.Random.value);
            float u2 = UnityEngine.Random.value;
            return Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * u2);
        }
    }

    [Serializable]
    public struct BeaconReading
    {
        public float timestamp;
        public string beaconId;
        public float rssi;
        public Vector3 groundTruthPos; // 시뮬레이터의 실제 플레이어 위치 (측위 오차 검증용)
    }

    /// <summary>
    /// 플레이어가 든 가상 스마트폰. 주기적으로 비콘을 스캔해 RSSI 목록을 화면에 표시하고,
    /// 옵션으로 시계열 CSV를 기록한다 (명세 4.6 Export 스키마와 호환).
    /// </summary>
    public class SmartphoneReceiver : MonoBehaviour
    {
        [Tooltip("스캔 주기(초)")]
        public float scanInterval = 0.3f;
        [Tooltip("화면에 표시할 상위 비콘 수")]
        public int displayCount = 6;

        [Header("데이터 기록 (명세 4.6)")]
        public bool logToCsv = false;
        public string csvFileName = "rssi_log.csv";

        /// <summary>신호 모델 주입 지점. 기본은 임시 거리 기반 모델.</summary>
        public ISignalModel SignalModel = new PlaceholderDistanceModel();

        /// <summary>외부 시스템(측위 알고리즘 등)이 구독할 수 있는 수신 이벤트.</summary>
        public event Action<BeaconReading> OnAdvertisementReceived;

        readonly List<(BleBeacon beacon, float rssi)> lastScan = new List<(BleBeacon, float)>();
        float nextScan;
        StreamWriter csv;
        GUIStyle style;

        void Start()
        {
            if (logToCsv)
            {
                string path = Path.Combine(Application.persistentDataPath, csvFileName);
                csv = new StreamWriter(path, false, Encoding.UTF8);
                csv.WriteLine("timestamp,beacon_id,rssi,ground_truth_x,ground_truth_y,ground_truth_z");
                Debug.Log("[SmartphoneReceiver] CSV 기록: " + path);
            }
        }

        void OnDestroy() { csv?.Dispose(); }

        void Update()
        {
            if (Time.time < nextScan) return;
            nextScan = Time.time + scanInterval;

            lastScan.Clear();
            Vector3 pos = transform.position;
            foreach (var b in BeaconManager.All)
            {
                if (b == null) continue;
                float rssi = SignalModel != null ? SignalModel.GetRssi(b, pos) : float.NegativeInfinity;
                if (float.IsNegativeInfinity(rssi)) continue;
                lastScan.Add((b, rssi));

                var reading = new BeaconReading { timestamp = Time.time, beaconId = b.beaconId, rssi = rssi, groundTruthPos = pos };
                OnAdvertisementReceived?.Invoke(reading);
                csv?.WriteLine($"{reading.timestamp:F3},{reading.beaconId},{reading.rssi:F1},{pos.x:F3},{pos.y:F3},{pos.z:F3}");
            }
            lastScan.Sort((a, b) => b.rssi.CompareTo(a.rssi));
        }

        void OnGUI()
        {
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label) { fontSize = 14, richText = true };
                if (KoreanLabel.KFont != null) style.font = KoreanLabel.KFont;
            }

            float w = 300f, x = Screen.width - w - 12f, y = 12f;
            GUI.Box(new Rect(x - 8, y - 6, w + 16, 30 + Mathf.Min(displayCount, lastScan.Count) * 22 + 40), "");
            GUI.Label(new Rect(x, y, w, 22), "<b>📱 수신 비콘 (RSSI 임시모델)</b>", style);
            y += 26;
            int n = Mathf.Min(displayCount, lastScan.Count);
            for (int i = 0; i < n; i++)
            {
                var (b, rssi) = lastScan[i];
                float d = Vector3.Distance(b.Position, transform.position);
                GUI.Label(new Rect(x, y, w, 22), $"{b.beaconId}   {rssi:F1} dBm   ({d:F1} m)", style);
                y += 22;
            }
            if (lastScan.Count == 0)
            {
                GUI.Label(new Rect(x, y, w, 22), "수신 중인 비콘 없음", style);
                y += 22;
            }
            var p = transform.position;
            GUI.Label(new Rect(x, y + 4, w, 22), $"위치(GT): {p.x:F1}, {p.y:F1}, {p.z:F1}", style);
        }
    }
}