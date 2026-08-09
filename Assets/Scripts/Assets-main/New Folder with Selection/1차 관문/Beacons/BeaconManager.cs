using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace IndoorSim.Beacons
{
    /// <summary>
    /// 씬의 모든 비콘 레지스트리 + 배치안 프리셋 저장/불러오기(JSON).
    /// 프리셋 스키마는 명세 4.3 (배치안 데이터 분리) 대응.
    /// </summary>
    public class BeaconManager : MonoBehaviour
    {
        static readonly List<BleBeacon> Actives = new List<BleBeacon>();
        public static IReadOnlyList<BleBeacon> All => Actives;

        public static void Register(BleBeacon b) { if (!Actives.Contains(b)) Actives.Add(b); }
        public static void Unregister(BleBeacon b) { Actives.Remove(b); }

        // ---------- 프리셋 직렬화 ----------
        [System.Serializable]
        public class BeaconRecord
        {
            public string beaconId;
            public string uuid;
            public int major;
            public int minor;
            public float x, y, z;
            public float txPowerDbm;
            public float measuredPowerAt1m;
            public int advertisingIntervalMs;
        }

        [System.Serializable]
        public class BeaconPreset
        {
            public string presetName = "preset";
            public List<BeaconRecord> beacons = new List<BeaconRecord>();
        }

        public static string ToJson(IEnumerable<BleBeacon> beacons, string presetName)
        {
            var p = new BeaconPreset { presetName = presetName };
            foreach (var b in beacons)
            {
                if (b == null) continue;
                p.beacons.Add(new BeaconRecord
                {
                    beaconId = b.beaconId, uuid = b.uuid, major = b.major, minor = b.minor,
                    x = b.transform.position.x, y = b.transform.position.y, z = b.transform.position.z,
                    txPowerDbm = b.txPowerDbm, measuredPowerAt1m = b.measuredPowerAt1m,
                    advertisingIntervalMs = b.advertisingIntervalMs
                });
            }
            return JsonUtility.ToJson(p, true);
        }

        /// <summary>JSON 프리셋을 파싱해 parent 아래에 비콘들을 생성. 기존 비콘 삭제는 호출자 책임.</summary>
        public static List<BleBeacon> SpawnFromJson(string json, Transform parent)
        {
            var created = new List<BleBeacon>();
            var p = JsonUtility.FromJson<BeaconPreset>(json);
            if (p == null || p.beacons == null) return created;
            foreach (var r in p.beacons)
            {
                var go = new GameObject(r.beaconId);
                go.transform.SetParent(parent, false);
                go.transform.position = new Vector3(r.x, r.y, r.z);
                var b = go.AddComponent<BleBeacon>();
                b.beaconId = r.beaconId; b.uuid = r.uuid; b.major = r.major; b.minor = r.minor;
                b.txPowerDbm = r.txPowerDbm; b.measuredPowerAt1m = r.measuredPowerAt1m;
                b.advertisingIntervalMs = r.advertisingIntervalMs;
                created.Add(b);
            }
            return created;
        }

        public static void SaveToFile(string path, string presetName)
        {
            File.WriteAllText(path, ToJson(All, presetName));
            Debug.Log("[BeaconManager] 프리셋 저장: " + path + " (" + Actives.Count + "개)");
        }

        public static List<BleBeacon> LoadFromFile(string path, Transform parent)
        {
            var list = SpawnFromJson(File.ReadAllText(path), parent);
            Debug.Log("[BeaconManager] 프리셋 로드: " + path + " (" + list.Count + "개)");
            return list;
        }
    }
}