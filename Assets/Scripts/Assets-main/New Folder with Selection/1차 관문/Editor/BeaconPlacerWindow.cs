using System.Linq;
using UnityEngine;
using UnityEditor;
using IndoorSim.Beacons;

namespace IndoorSim.EditorTools
{
    /// <summary>
    /// 비콘 배치 툴 (명세 4.3): 씬 뷰 클릭으로 비콘 설치, 목록 관리, 배치안 프리셋 저장/불러오기.
    /// 사용법: "클릭 배치 모드" 켜기 -> 씬 뷰에서 벽/천장/바닥 클릭 -> 표면에서 오프셋만큼 띄워 설치.
    /// </summary>
    public class BeaconPlacerWindow : EditorWindow
    {
        bool placing;
        float surfaceOffset = 0.05f;   // 클릭 표면에서 법선 방향 오프셋
        float txPowerDbm = 0f;
        float measuredPowerAt1m = -59f;
        int advertisingIntervalMs = 100;
        Vector2 scroll;

        public static void Open()
        {
            var w = GetWindow<BeaconPlacerWindow>("비콘 배치 툴");
            w.minSize = new Vector2(320f, 420f);
        }

        void OnEnable() { SceneView.duringSceneGui += OnSceneGUI; }
        void OnDisable() { SceneView.duringSceneGui -= OnSceneGUI; }

        static BleBeacon[] SceneBeacons() =>
        Object.FindObjectsByType<BleBeacon>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).OrderBy(b => b.beaconId).ToArray();

        static Transform BeaconGroup()
        {
            var g = GameObject.Find("Beacons");
            if (g == null)
            {
                g = new GameObject("Beacons");
                if (g.GetComponent<BeaconManager>() == null) g.AddComponent<BeaconManager>();
                var root = GameObject.Find("SimWorld");
                if (root != null) g.transform.SetParent(root.transform, false);
            }
            return g.transform;
        }

        void OnSceneGUI(SceneView sv)
        {
            if (!placing) return;
            var e = Event.current;

            // 클릭 배치 모드 중에는 씬 선택 클릭을 가로챔
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out var hit, 500f))
                {
                    PlaceBeacon(hit.point + hit.normal * surfaceOffset);
                    e.Use();
                }
            }
            sv.Repaint();
        }

        void PlaceBeacon(Vector3 pos)
        {
            var existing = SceneBeacons();
            int no = 1;
            while (existing.Any(b => b.beaconId == $"BCN-{no:000}")) no++;

            var go = new GameObject($"BCN-{no:000}");
            go.transform.SetParent(BeaconGroup(), false);
            go.transform.position = pos;
            var b = go.AddComponent<BleBeacon>();
            b.beaconId = go.name;
            b.minor = no;
            b.txPowerDbm = txPowerDbm;
            b.measuredPowerAt1m = measuredPowerAt1m;
            b.advertisingIntervalMs = advertisingIntervalMs;

            Undo.RegisterCreatedObjectUndo(go, "비콘 설치");
            Selection.activeGameObject = go;
            Repaint();
        }

        void OnGUI()
        {
            EditorGUILayout.Space(6);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                GUILayout.Label("설치 설정", EditorStyles.boldLabel);
                placing = GUILayout.Toggle(placing, placing ? "🔵 클릭 배치 모드 ON (씬 뷰 클릭 = 설치)" : "클릭 배치 모드 OFF", "Button");
                surfaceOffset = EditorGUILayout.FloatField("표면 오프셋 (m)", surfaceOffset);
                txPowerDbm = EditorGUILayout.FloatField("Tx Power (dBm)", txPowerDbm);
                measuredPowerAt1m = EditorGUILayout.FloatField("1m 기준 RSSI (dBm)", measuredPowerAt1m);
                advertisingIntervalMs = EditorGUILayout.IntField("광고 주기 (ms)", advertisingIntervalMs);
            }

            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("배치안 저장 (JSON)")) SavePreset();
                if (GUILayout.Button("배치안 불러오기")) LoadPreset();
            }

            EditorGUILayout.Space(6);
            var beacons = SceneBeacons();
            GUILayout.Label($"씬 비콘 목록 ({beacons.Length}개)", EditorStyles.boldLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var b in beacons)
            {
                using (new EditorGUILayout.HorizontalScope("box"))
                {
                    var p = b.transform.position;
                    GUILayout.Label($"{b.beaconId}  ({p.x:F1}, {p.y:F1}, {p.z:F1})");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("선택", GUILayout.Width(44)))
                    {
                        Selection.activeGameObject = b.gameObject;
                        SceneView.lastActiveSceneView?.FrameSelected();
                    }
                    if (GUILayout.Button("삭제", GUILayout.Width(44)))
                        Undo.DestroyObjectImmediate(b.gameObject);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        void SavePreset()
        {
            string path = EditorUtility.SaveFilePanel("비콘 배치안 저장", Application.dataPath, "beacon_preset", "json");
            if (string.IsNullOrEmpty(path)) return;
            System.IO.File.WriteAllText(path, BeaconManager.ToJson(SceneBeacons(), System.IO.Path.GetFileNameWithoutExtension(path)));
            Debug.Log("[비콘 배치 툴] 저장: " + path);
        }

        void LoadPreset()
        {
            string path = EditorUtility.OpenFilePanel("비콘 배치안 불러오기", Application.dataPath, "json");
            if (string.IsNullOrEmpty(path)) return;
            if (SceneBeacons().Length > 0 &&
                EditorUtility.DisplayDialog("비콘 배치 툴", "기존 비콘을 모두 지우고 불러올까요?", "지우고 불러오기", "유지하고 추가"))
                foreach (var b in SceneBeacons()) Undo.DestroyObjectImmediate(b.gameObject);
            BeaconManager.LoadFromFile(path, BeaconGroup());
        }
    }
}