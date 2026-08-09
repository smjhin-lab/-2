using UnityEngine;
using UnityEditor;
using Unity.AI.Navigation;
using IndoorSim.Gen;
using IndoorSim.Beacons;
using IndoorSim.PlayerCtrl;

namespace IndoorSim.EditorTools
{
    public static class SimulatorMenu
    {
        const string RootName = "SimWorld";

        [MenuItem("Simulator/전체 월드 생성 (역+백화점+플레이어+샘플비콘)", false, 0)]
        public static void GenerateAll()
        {
            DeleteWorld();
            ProcBuilder.ResetCaches();
            var root = new GameObject(RootName).transform;

            StationGenerator.Generate(root);
            MallGenerator.Generate(root);
            // [기존 코드]
            // [기존 코드]
            EnvironmentBuilder.Generate(root);
            CreateSampleBeacons(root);

            // 👇 [이 부분을 통째로 교체합니다!] 👇
            int safeLayer = LayerMask.NameToLayer("SafePath");

            // 1. 에스컬레이터 자동 페인트칠
            EscalatorBelt[] escalators = UnityEngine.Object.FindObjectsByType<EscalatorBelt>(FindObjectsInactive.Exclude);
            foreach (var esc in escalators)
            {
                Transform[] parts = esc.transform.parent.GetComponentsInChildren<Transform>();
                foreach (var part in parts) part.gameObject.layer = safeLayer;
            }

            // 2. 계단 자동 페인트칠 ("stair"라는 이름이 들어간 모든 바닥)
            MeshRenderer[] allMeshes = UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude);
            foreach (var mesh in allMeshes)
            {
                if (mesh.gameObject.name.ToLower().Contains("step"))
                {
                    mesh.gameObject.layer = safeLayer;
                }
            }

            // 3. 엘리베이터 자동 페인트칠
            ElevatorCab[] elevators = UnityEngine.Object.FindObjectsByType<ElevatorCab>(FindObjectsInactive.Exclude);
            foreach (var cab in elevators)
            {
                Transform[] parts = cab.transform.parent.GetComponentsInChildren<Transform>();
                foreach (var part in parts) part.gameObject.layer = safeLayer;
            }
            // 👆 ------------------------------------------ 👆

            // [기존 코드]
            var navSurface = root.gameObject.AddComponent<NavMeshSurface>();
            navSurface.collectObjects = Unity.AI.Navigation.CollectObjects.All;
            navSurface.layerMask = LayerMask.GetMask("SafePath");
            navSurface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
            navSurface.BuildNavMesh();

            CreatePlayerInternal(root, new Vector3(0f, 0.7f, 42f), 180f);

            Selection.activeGameObject = root.gameObject;
            Debug.Log("[Simulator] 월드 생성 및 NavMesh 굽기 성공!");
        }

        [MenuItem("Simulator/월드 삭제", false, 1)]
        public static void DeleteWorld()
        {
            var old = GameObject.Find(RootName);
            if (old != null) UnityEngine.Object.DestroyImmediate(old);
        }

        [MenuItem("Simulator/플레이어만 다시 생성", false, 20)]
        public static void CreatePlayerOnly()
        {
            var root = GameObject.Find(RootName);
            CreatePlayerInternal(root != null ? root.transform : null, new Vector3(0f, 0.7f, 42f), 180f);
        }

        [MenuItem("Simulator/비콘 배치 툴 열기", false, 40)]
        public static void OpenBeaconTool() => BeaconPlacerWindow.Open();

        static void CreatePlayerInternal(Transform root, Vector3 pos, float yRot)
        {
            var old = GameObject.Find("Player");
            if (old != null) UnityEngine.Object.DestroyImmediate(old);

            var player = new GameObject("Player") { tag = "Player" };
            if (root != null) player.transform.SetParent(root, false);
            player.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yRot, 0f));

            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.stepOffset = 0.4f;
            cc.slopeLimit = 50f;

            // 🚀 핵심 추가: 엘리베이터 트리거를 100% 인식하도록 물리 강체(Rigidbody) 추가!
            // isKinematic을 true로 설정하면 무게나 중력의 영향은 안 받으면서 센서만 칼같이 작동하게 해줍니다.
            var rb = player.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            var camGo = new GameObject("Camera") { tag = "MainCamera" };
            camGo.transform.SetParent(player.transform, false);
            camGo.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 65f;
            cam.nearClipPlane = 0.05f;
            camGo.AddComponent<AudioListener>();

            var fpc = player.AddComponent<FirstPersonController>();
            fpc.cameraPivot = camGo.transform;
            player.AddComponent<SmartphoneReceiver>();
            player.AddComponent<NavigationGuide>();

            Camera[] activeCameras = Camera.allCameras;
            for (int i = activeCameras.Length - 1; i >= 0; i--)
            {
                Camera c = activeCameras[i];
                if (c != null && !c.transform.IsChildOf(player.transform))
                {
                    c.gameObject.SetActive(false);
                }
            }
        }

        static void CreateSampleBeacons(Transform root)
        {
            var grp = new GameObject("Beacons").transform;
            grp.SetParent(root, false);
            grp.gameObject.AddComponent<BeaconManager>();

            Vector3[] positions =
            {
                new Vector3(-20f, -3.5f, 10f), new Vector3(0f, -3.5f, -10f), new Vector3(20f, -3.5f, 10f), new Vector3(-10f, -3.5f, 0f),
                new Vector3(-50f, -9.5f, 0f), new Vector3(-20f, -9.5f, 0f), new Vector3(10f, -9.5f, 0f), new Vector3(40f, -9.5f, 0f),
                new Vector3(-17.5f, -15.5f, -40f), new Vector3(-17.5f, -15.5f, 0f), new Vector3(-17.5f, -15.5f, 40f),
                new Vector3(50f, -3.5f, -12f), new Vector3(50f, -3.5f, 0f), new Vector3(50f, -3.5f, 12f),
                new Vector3(70f, -3.5f, -15f), new Vector3(70f, -3.5f, 15f), new Vector3(105f, -3.5f, 0f),
                new Vector3(70f, 2.5f, 0f), new Vector3(105f, 2.5f, 15f), new Vector3(90f, 2.5f, -15f),
                new Vector3(90f, 8.5f, 0f), new Vector3(70f, 8.5f, -15f),
            };

            for (int i = 0; i < positions.Length; i++)
            {
                var go = new GameObject($"BCN-{i + 1:000}");
                go.transform.SetParent(grp, false);
                go.transform.position = positions[i];
                var b = go.AddComponent<BleBeacon>();
                b.beaconId = go.name;
                b.major = 1;
                b.minor = i + 1;
            }
        }
    }
}
