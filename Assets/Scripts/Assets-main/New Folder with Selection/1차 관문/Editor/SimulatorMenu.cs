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

        public static void GenerateCustomSafePath(Transform root)
        {
            int safeLayer = LayerMask.NameToLayer("SafePath");

            var obj0 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj0.transform.SetParent(root);
            obj0.transform.position = new Vector3(-27.98f, -12.49f, -0.94f);
            obj0.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj0.transform.localScale = new Vector3(85.03561f, 1f, 1f);
            obj0.layer = safeLayer;
            obj0.GetComponent<MeshRenderer>().enabled = false;

            var obj1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj1.transform.SetParent(root);
            obj1.transform.position = new Vector3(-2.82f, -12.49f, -4.21f);
            obj1.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj1.transform.localScale = new Vector3(116.1305f, 1f, 1f);
            obj1.layer = safeLayer;
            obj1.GetComponent<MeshRenderer>().enabled = false;

            var obj2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj2.transform.SetParent(root);
            obj2.transform.position = new Vector3(-9.23f, -12.49f, 0.01f);
            obj2.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj2.transform.localScale = new Vector3(1f, 1f, 10.01434f);
            obj2.layer = safeLayer;
            obj2.GetComponent<MeshRenderer>().enabled = false;

            var obj3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj3.transform.SetParent(root);
            obj3.transform.position = new Vector3(2.37f, -6.49f, 15.93f);
            obj3.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj3.transform.localScale = new Vector3(4.907752f, 1f, 1f);
            obj3.layer = safeLayer;
            obj3.GetComponent<MeshRenderer>().enabled = false;

            var obj4 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj4.transform.SetParent(root);
            obj4.transform.position = new Vector3(16.1f, -0.5f, -39.35f);
            obj4.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj4.transform.localScale = new Vector3(48.14131f, 1f, 1.102626f);
            obj4.layer = safeLayer;
            obj4.GetComponent<MeshRenderer>().enabled = false;

            var obj5 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj5.transform.SetParent(root);
            obj5.transform.position = new Vector3(-40.26f, -12.49f, -2.58f);
            obj5.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj5.transform.localScale = new Vector3(1.030344f, 1f, 4.005781f);
            obj5.layer = safeLayer;
            obj5.GetComponent<MeshRenderer>().enabled = false;

            var obj6 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj6.transform.SetParent(root);
            obj6.transform.position = new Vector3(36f, -0.5f, 38.46f);
            obj6.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj6.transform.localScale = new Vector3(1f, 1f, 7.558958f);
            obj6.layer = safeLayer;
            obj6.GetComponent<MeshRenderer>().enabled = false;

            var obj7 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj7.transform.SetParent(root);
            obj7.transform.position = new Vector3(79.38f, 5.52f, -0.08f);
            obj7.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj7.transform.localScale = new Vector3(1f, 1f, 46.3051f);
            obj7.layer = safeLayer;
            obj7.GetComponent<MeshRenderer>().enabled = false;

            var obj8 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj8.transform.SetParent(root);
            obj8.transform.position = new Vector3(11.1f, -0.5f, -30.65f);
            obj8.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj8.transform.localScale = new Vector3(1f, 1f, 19.54214f);
            obj8.layer = safeLayer;
            obj8.GetComponent<MeshRenderer>().enabled = false;

            var obj9 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj9.transform.SetParent(root);
            obj9.transform.position = new Vector3(-42.08f, -12.49f, 1.94f);
            obj9.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj9.transform.localScale = new Vector3(4.244044f, 1f, 2.997191f);
            obj9.layer = safeLayer;
            obj9.GetComponent<MeshRenderer>().enabled = false;

            var obj10 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj10.transform.SetParent(root);
            obj10.transform.position = new Vector3(106.34f, -0.5f, 7f);
            obj10.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj10.transform.localScale = new Vector3(12.347f, 1f, 1f);
            obj10.layer = safeLayer;
            obj10.GetComponent<MeshRenderer>().enabled = false;

            var obj11 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj11.transform.SetParent(root);
            obj11.transform.position = new Vector3(59.05f, -6.49f, 11.83f);
            obj11.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj11.transform.localScale = new Vector3(-115.6557f, 1f, 1f);
            obj11.layer = safeLayer;
            obj11.GetComponent<MeshRenderer>().enabled = false;

            var obj12 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj12.transform.SetParent(root);
            obj12.transform.position = new Vector3(80.3f, 5.52f, 5.58f);
            obj12.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj12.transform.localScale = new Vector3(2.535127f, 1f, 2.371041f);
            obj12.layer = safeLayer;
            obj12.GetComponent<MeshRenderer>().enabled = false;

            var obj13 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj13.transform.SetParent(root);
            obj13.transform.position = new Vector3(87.68f, -0.5f, 18.28f);
            obj13.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj13.transform.localScale = new Vector3(46.47272f, 1f, 1f);
            obj13.layer = safeLayer;
            obj13.GetComponent<MeshRenderer>().enabled = false;

            var obj14 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj14.transform.SetParent(root);
            obj14.transform.position = new Vector3(88.07f, -6.49f, 5.78f);
            obj14.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj14.transform.localScale = new Vector3(1f, 1f, 12.88665f);
            obj14.layer = safeLayer;
            obj14.GetComponent<MeshRenderer>().enabled = false;

            var obj15 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj15.transform.SetParent(root);
            obj15.transform.position = new Vector3(4.01f, -0.5f, -37.43f);
            obj15.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj15.transform.localScale = new Vector3(2.1435f, 1f, 4.883191f);
            obj15.layer = safeLayer;
            obj15.GetComponent<MeshRenderer>().enabled = false;

            var obj16 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj16.transform.SetParent(root);
            obj16.transform.position = new Vector3(16.1f, -0.5f, -21.27f);
            obj16.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj16.transform.localScale = new Vector3(48.14131f, 1f, 1.102626f);
            obj16.layer = safeLayer;
            obj16.GetComponent<MeshRenderer>().enabled = false;

            var obj17 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj17.transform.SetParent(root);
            obj17.transform.position = new Vector3(17.81f, -0.5f, -37.43f);
            obj17.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj17.transform.localScale = new Vector3(2.1435f, 1f, 4.883191f);
            obj17.layer = safeLayer;
            obj17.GetComponent<MeshRenderer>().enabled = false;

            var obj18 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj18.transform.SetParent(root);
            obj18.transform.position = new Vector3(99.42f, -0.5f, -5.61f);
            obj18.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj18.transform.localScale = new Vector3(1.791341f, 1f, 2.6544f);
            obj18.layer = safeLayer;
            obj18.GetComponent<MeshRenderer>().enabled = false;

            var obj19 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj19.transform.SetParent(root);
            obj19.transform.position = new Vector3(111.44f, 5.52f, -17.45f);
            obj19.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj19.transform.localScale = new Vector3(1f, 1f, 9.410058f);
            obj19.layer = safeLayer;
            obj19.GetComponent<MeshRenderer>().enabled = false;

            var obj20 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj20.transform.SetParent(root);
            obj20.transform.position = new Vector3(106.27f, 5.52f, -2.92f);
            obj20.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj20.transform.localScale = new Vector3(11.80027f, 1f, 1f);
            obj20.layer = safeLayer;
            obj20.GetComponent<MeshRenderer>().enabled = false;

            var obj21 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj21.transform.SetParent(root);
            obj21.transform.position = new Vector3(12.39f, -0.5f, 38.46f);
            obj21.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj21.transform.localScale = new Vector3(1f, 1f, 7.558958f);
            obj21.layer = safeLayer;
            obj21.GetComponent<MeshRenderer>().enabled = false;

            var obj22 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj22.transform.SetParent(root);
            obj22.transform.position = new Vector3(59.3f, -6.49f, -12.82f);
            obj22.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj22.transform.localScale = new Vector3(-127.1967f, 1f, 1f);
            obj22.layer = safeLayer;
            obj22.GetComponent<MeshRenderer>().enabled = false;

            var obj23 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj23.transform.SetParent(root);
            obj23.transform.position = new Vector3(67.51f, -0.5f, 0.22f);
            obj23.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj23.transform.localScale = new Vector3(1f, 1f, 45.0817f);
            obj23.layer = safeLayer;
            obj23.GetComponent<MeshRenderer>().enabled = false;

            var obj24 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj24.transform.SetParent(root);
            obj24.transform.position = new Vector3(-10.8f, -18.48f, 1.15f);
            obj24.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj24.transform.localScale = new Vector3(15.55034f, 1f, 1f);
            obj24.layer = safeLayer;
            obj24.GetComponent<MeshRenderer>().enabled = false;

            var obj25 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj25.transform.SetParent(root);
            obj25.transform.position = new Vector3(4f, -0.5f, 42f);
            obj25.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj25.transform.localScale = new Vector3(115f, 1f, 1.102626f);
            obj25.layer = safeLayer;
            obj25.GetComponent<MeshRenderer>().enabled = false;

            var obj26 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj26.transform.SetParent(root);
            obj26.transform.position = new Vector3(116.558f, 5.52f, 18.581f);
            obj26.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj26.transform.localScale = new Vector3(2.671269f, 1f, 1f);
            obj26.layer = safeLayer;
            obj26.GetComponent<MeshRenderer>().enabled = false;

            var obj27 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj27.transform.SetParent(root);
            obj27.transform.position = new Vector3(89.22f, 5.52f, 17.84f);
            obj27.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj27.transform.localScale = new Vector3(56.35821f, 1f, 1f);
            obj27.layer = safeLayer;
            obj27.GetComponent<MeshRenderer>().enabled = false;

            var obj28 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj28.transform.SetParent(root);
            obj28.transform.position = new Vector3(24.09f, -6.49f, 19.3f);
            obj28.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj28.transform.localScale = new Vector3(1f, 1f, 15.79454f);
            obj28.layer = safeLayer;
            obj28.GetComponent<MeshRenderer>().enabled = false;

            var obj29 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj29.transform.SetParent(root);
            obj29.transform.position = new Vector3(-30.7f, -6.49f, -0.57f);
            obj29.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj29.transform.localScale = new Vector3(1.0727f, 1f, 8.784531f);
            obj29.layer = safeLayer;
            obj29.GetComponent<MeshRenderer>().enabled = false;

            var obj30 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj30.transform.SetParent(root);
            obj30.transform.position = new Vector3(33.47f, -0.46f, -0.14f);
            obj30.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj30.transform.localScale = new Vector3(96.22066f, 0.9596778f, 0.92929f);
            obj30.layer = safeLayer;
            obj30.GetComponent<MeshRenderer>().enabled = false;

            var obj31 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj31.transform.SetParent(root);
            obj31.transform.position = new Vector3(11.92f, -6.49f, 19.3f);
            obj31.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj31.transform.localScale = new Vector3(1f, 1f, 15.79454f);
            obj31.layer = safeLayer;
            obj31.GetComponent<MeshRenderer>().enabled = false;

            var obj32 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj32.transform.SetParent(root);
            obj32.transform.position = new Vector3(23.91f, -0.5f, 38.46f);
            obj32.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj32.transform.localScale = new Vector3(1f, 1f, 7.558958f);
            obj32.layer = safeLayer;
            obj32.GetComponent<MeshRenderer>().enabled = false;

            var obj33 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj33.transform.SetParent(root);
            obj33.transform.position = new Vector3(67.57f, 5.52f, 20.01f);
            obj33.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj33.transform.localScale = new Vector3(1f, 1f, 5.009336f);
            obj33.layer = safeLayer;
            obj33.GetComponent<MeshRenderer>().enabled = false;

            var obj34 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj34.transform.SetParent(root);
            obj34.transform.position = new Vector3(42.94f, -6.49f, -0.27f);
            obj34.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj34.transform.localScale = new Vector3(-131.3007f, 1f, 1f);
            obj34.layer = safeLayer;
            obj34.GetComponent<MeshRenderer>().enabled = false;

            var obj35 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj35.transform.SetParent(root);
            obj35.transform.position = new Vector3(35.64f, -6.49f, 19.3f);
            obj35.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj35.transform.localScale = new Vector3(1f, 1f, 15.79454f);
            obj35.layer = safeLayer;
            obj35.GetComponent<MeshRenderer>().enabled = false;

            var obj36 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj36.transform.SetParent(root);
            obj36.transform.position = new Vector3(100.52f, -6.49f, -17.45f);
            obj36.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj36.transform.localScale = new Vector3(1f, 1f, 9.547979f);
            obj36.layer = safeLayer;
            obj36.GetComponent<MeshRenderer>().enabled = false;

            var obj37 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj37.transform.SetParent(root);
            obj37.transform.position = new Vector3(32.2f, -0.5f, -37.43f);
            obj37.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj37.transform.localScale = new Vector3(2.1435f, 1f, 4.883191f);
            obj37.layer = safeLayer;
            obj37.GetComponent<MeshRenderer>().enabled = false;

            var obj38 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj38.transform.SetParent(root);
            obj38.transform.position = new Vector3(-18.32f, -18.48f, 3.12f);
            obj38.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj38.transform.localScale = new Vector3(1.91638f, 1f, 5.334723f);
            obj38.layer = safeLayer;
            obj38.GetComponent<MeshRenderer>().enabled = false;

            var obj39 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj39.transform.SetParent(root);
            obj39.transform.position = new Vector3(95.75f, -0.5f, 5.37f);
            obj39.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj39.transform.localScale = new Vector3(9.6285f, 1f, 3.2383f);
            obj39.layer = safeLayer;
            obj39.GetComponent<MeshRenderer>().enabled = false;

            var obj40 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj40.transform.SetParent(root);
            obj40.transform.position = new Vector3(100.46f, -0.5f, -0.5699987f);
            obj40.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj40.transform.localScale = new Vector3(1f, 1f, 42.172f);
            obj40.layer = safeLayer;
            obj40.GetComponent<MeshRenderer>().enabled = false;

            var obj41 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj41.transform.SetParent(root);
            obj41.transform.position = new Vector3(78.44f, -6.49f, -17.45f);
            obj41.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj41.transform.localScale = new Vector3(1f, 1f, 9.547979f);
            obj41.layer = safeLayer;
            obj41.GetComponent<MeshRenderer>().enabled = false;

            var obj42 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj42.transform.SetParent(root);
            obj42.transform.position = new Vector3(110.25f, -0.5f, 23.91f);
            obj42.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj42.transform.localScale = new Vector3(1f, 1f, 11.99302f);
            obj42.layer = safeLayer;
            obj42.GetComponent<MeshRenderer>().enabled = false;

            var obj43 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj43.transform.SetParent(root);
            obj43.transform.position = new Vector3(106.27f, 5.52f, 6.91f);
            obj43.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj43.transform.localScale = new Vector3(11.80027f, 1f, 1f);
            obj43.layer = safeLayer;
            obj43.GetComponent<MeshRenderer>().enabled = false;

            var obj44 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj44.transform.SetParent(root);
            obj44.transform.position = new Vector3(-3.35f, -18.48f, 4.2f);
            obj44.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj44.transform.localScale = new Vector3(1f, 1f, 99.82317f);
            obj44.layer = safeLayer;
            obj44.GetComponent<MeshRenderer>().enabled = false;

            var obj45 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj45.transform.SetParent(root);
            obj45.transform.position = new Vector3(88.07f, -6.49f, -6.6f);
            obj45.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj45.transform.localScale = new Vector3(1f, 1f, 12.88665f);
            obj45.layer = safeLayer;
            obj45.GetComponent<MeshRenderer>().enabled = false;

            var obj46 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj46.transform.SetParent(root);
            obj46.transform.position = new Vector3(89.22f, 5.52f, -13.03f);
            obj46.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj46.transform.localScale = new Vector3(56.35821f, 1f, 1f);
            obj46.layer = safeLayer;
            obj46.GetComponent<MeshRenderer>().enabled = false;

            var obj47 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj47.transform.SetParent(root);
            obj47.transform.position = new Vector3(39.7f, -0.5f, -30.65f);
            obj47.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj47.transform.localScale = new Vector3(1f, 1f, 19.54214f);
            obj47.layer = safeLayer;
            obj47.GetComponent<MeshRenderer>().enabled = false;

            var obj48 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj48.transform.SetParent(root);
            obj48.transform.position = new Vector3(81.511f, 5.52f, 5.59f);
            obj48.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj48.transform.localScale = new Vector3(0.8704348f, 1f, 3.0208f);
            obj48.layer = safeLayer;
            obj48.GetComponent<MeshRenderer>().enabled = false;

            var obj49 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj49.transform.SetParent(root);
            obj49.transform.position = new Vector3(-31.16f, -12.49f, 1.433f);
            obj49.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj49.transform.localScale = new Vector3(1.030344f, 1f, 5.384635f);
            obj49.layer = safeLayer;
            obj49.GetComponent<MeshRenderer>().enabled = false;

            var obj50 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj50.transform.SetParent(root);
            obj50.transform.position = new Vector3(111.32f, -6.49f, -17.45f);
            obj50.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj50.transform.localScale = new Vector3(1f, 1f, 9.547979f);
            obj50.layer = safeLayer;
            obj50.GetComponent<MeshRenderer>().enabled = false;

            var obj51 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj51.transform.SetParent(root);
            obj51.transform.position = new Vector3(0.08f, -6.49f, 19.3f);
            obj51.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj51.transform.localScale = new Vector3(1f, 1f, 15.79454f);
            obj51.layer = safeLayer;
            obj51.GetComponent<MeshRenderer>().enabled = false;

            var obj52 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj52.transform.SetParent(root);
            obj52.transform.position = new Vector3(113.78f, -0.5f, 28.23f);
            obj52.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj52.transform.localScale = new Vector3(7.570398f, 1f, 1f);
            obj52.layer = safeLayer;
            obj52.GetComponent<MeshRenderer>().enabled = false;

            var obj53 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj53.transform.SetParent(root);
            obj53.transform.position = new Vector3(89.51f, -6.49f, -17.45f);
            obj53.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj53.transform.localScale = new Vector3(1f, 1f, 9.547979f);
            obj53.layer = safeLayer;
            obj53.GetComponent<MeshRenderer>().enabled = false;

            var obj54 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj54.transform.SetParent(root);
            obj54.transform.position = new Vector3(25.5f, -0.5f, -30.65f);
            obj54.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj54.transform.localScale = new Vector3(1f, 1f, 19.54214f);
            obj54.layer = safeLayer;
            obj54.GetComponent<MeshRenderer>().enabled = false;

            var obj55 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj55.transform.SetParent(root);
            obj55.transform.position = new Vector3(67.44f, 5.52f, -17.45f);
            obj55.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj55.transform.localScale = new Vector3(1f, 1f, 9.410058f);
            obj55.layer = safeLayer;
            obj55.GetComponent<MeshRenderer>().enabled = false;

            var obj56 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj56.transform.SetParent(root);
            obj56.transform.position = new Vector3(89.55f, -0.5f, -17.53f);
            obj56.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj56.transform.localScale = new Vector3(1f, 1f, 9.542688f);
            obj56.layer = safeLayer;
            obj56.GetComponent<MeshRenderer>().enabled = false;

            var obj57 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj57.transform.SetParent(root);
            obj57.transform.position = new Vector3(100.54f, 5.52f, 0.03f);
            obj57.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj57.transform.localScale = new Vector3(1f, 1f, 44.32462f);
            obj57.layer = safeLayer;
            obj57.GetComponent<MeshRenderer>().enabled = false;

            var obj58 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj58.transform.SetParent(root);
            obj58.transform.position = new Vector3(88.18f, -0.5f, -12.82f);
            obj58.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj58.transform.localScale = new Vector3(47.30649f, 1f, 1f);
            obj58.layer = safeLayer;
            obj58.GetComponent<MeshRenderer>().enabled = false;

            var obj59 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj59.transform.SetParent(root);
            obj59.transform.position = new Vector3(89.47f, 5.52f, -17.45f);
            obj59.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj59.transform.localScale = new Vector3(1f, 1f, 9.410058f);
            obj59.layer = safeLayer;
            obj59.GetComponent<MeshRenderer>().enabled = false;

            var obj60 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj60.transform.SetParent(root);
            obj60.transform.position = new Vector3(0f, -0.5f, 41.64f);
            obj60.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj60.transform.localScale = new Vector3(10.38f, 1f, 13.6217f);
            obj60.layer = safeLayer;
            obj60.GetComponent<MeshRenderer>().enabled = false;

            var obj61 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj61.transform.SetParent(root);
            obj61.transform.position = new Vector3(-14.34f, -18.45f, -0.16f);
            obj61.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj61.transform.localScale = new Vector3(1.3323f, 1f, 3.1677f);
            obj61.layer = safeLayer;
            obj61.GetComponent<MeshRenderer>().enabled = false;

            var obj62 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj62.transform.SetParent(root);
            obj62.transform.position = new Vector3(110.95f, -0.5f, -17.53f);
            obj62.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj62.transform.localScale = new Vector3(1f, 1f, 9.542688f);
            obj62.layer = safeLayer;
            obj62.GetComponent<MeshRenderer>().enabled = false;

            var obj63 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj63.transform.SetParent(root);
            obj63.transform.position = new Vector3(-6.56f, -0.5f, -30.65f);
            obj63.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj63.transform.localScale = new Vector3(1f, 1f, 19.54214f);
            obj63.layer = safeLayer;
            obj63.GetComponent<MeshRenderer>().enabled = false;

            var obj64 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj64.transform.SetParent(root);
            obj64.transform.position = new Vector3(17.7f, -0.5f, 15.25f);
            obj64.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj64.transform.localScale = new Vector3(1f, 1f, 75.55269f);
            obj64.layer = safeLayer;
            obj64.GetComponent<MeshRenderer>().enabled = false;

            var obj65 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj65.transform.SetParent(root);
            obj65.transform.position = new Vector3(111.47f, -6.49f, 2.62f);
            obj65.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj65.transform.localScale = new Vector3(1f, 1f, 33.08983f);
            obj65.layer = safeLayer;
            obj65.GetComponent<MeshRenderer>().enabled = false;

            var obj66 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj66.transform.SetParent(root);
            obj66.transform.position = new Vector3(-31.7f, -18.48f, 4.2f);
            obj66.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj66.transform.localScale = new Vector3(1f, 1f, 99.82317f);
            obj66.layer = safeLayer;
            obj66.GetComponent<MeshRenderer>().enabled = false;

            var obj67 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj67.transform.SetParent(root);
            obj67.transform.position = new Vector3(89.39f, -6.49f, 16.9f);
            obj67.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj67.transform.localScale = new Vector3(1f, 1f, 10.3946f);
            obj67.layer = safeLayer;
            obj67.GetComponent<MeshRenderer>().enabled = false;

            var obj68 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj68.transform.SetParent(root);
            obj68.transform.position = new Vector3(67.51f, -6.49f, -0.36f);
            obj68.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj68.transform.localScale = new Vector3(1f, 1f, 45.26593f);
            obj68.layer = safeLayer;
            obj68.GetComponent<MeshRenderer>().enabled = false;

            var obj69 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj69.transform.SetParent(root);
            obj69.transform.position = new Vector3(4.7f, -6.49f, 1.68f);
            obj69.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj69.transform.localScale = new Vector3(1f, 1f, 29.21077f);
            obj69.layer = safeLayer;
            obj69.GetComponent<MeshRenderer>().enabled = false;

            var obj70 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj70.transform.SetParent(root);
            obj70.transform.position = new Vector3(78.44f, -6.49f, 16.9f);
            obj70.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj70.transform.localScale = new Vector3(1f, 1f, 10.3946f);
            obj70.layer = safeLayer;
            obj70.GetComponent<MeshRenderer>().enabled = false;

            var obj71 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj71.transform.SetParent(root);
            obj71.transform.position = new Vector3(76.54f, -0.5f, -0.36f);
            obj71.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj71.transform.localScale = new Vector3(1f, 1f, 42.49714f);
            obj71.layer = safeLayer;
            obj71.GetComponent<MeshRenderer>().enabled = false;

            var obj72 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj72.transform.SetParent(root);
            obj72.transform.position = new Vector3(-26.17f, -6.49f, -0.24f);
            obj72.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj72.transform.localScale = new Vector3(9.725f, 1f, 1f);
            obj72.layer = safeLayer;
            obj72.GetComponent<MeshRenderer>().enabled = false;

            var obj73 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj73.transform.SetParent(root);
            obj73.transform.position = new Vector3(106.34f, -0.5f, -2.9f);
            obj73.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj73.transform.localScale = new Vector3(12.347f, 1f, 1f);
            obj73.layer = safeLayer;
            obj73.GetComponent<MeshRenderer>().enabled = false;

            var obj74 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj74.transform.SetParent(root);
            obj74.transform.position = new Vector3(103.7f, 5.52f, -0.19f);
            obj74.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj74.transform.localScale = new Vector3(6.05336f, 1f, 1f);
            obj74.layer = safeLayer;
            obj74.GetComponent<MeshRenderer>().enabled = false;

            var obj75 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj75.transform.SetParent(root);
            obj75.transform.position = new Vector3(83.1f, 5.52f, -10.64f);
            obj75.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj75.transform.localScale = new Vector3(1f, 1f, 1f);
            obj75.layer = safeLayer;
            obj75.GetComponent<MeshRenderer>().enabled = false;

            var obj76 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj76.transform.SetParent(root);
            obj76.transform.position = new Vector3(-29.46f, -12.49f, 2.11f);
            obj76.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj76.transform.localScale = new Vector3(3.330794f, 1f, 3.2925f);
            obj76.layer = safeLayer;
            obj76.GetComponent<MeshRenderer>().enabled = false;

            var obj77 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj77.transform.SetParent(root);
            obj77.transform.position = new Vector3(-5.51f, -12.49f, 4.2f);
            obj77.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj77.transform.localScale = new Vector3(110.8353f, 1f, 1f);
            obj77.layer = safeLayer;
            obj77.GetComponent<MeshRenderer>().enabled = false;

            var obj78 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj78.transform.SetParent(root);
            obj78.transform.position = new Vector3(89.77f, 5.52f, 20.01f);
            obj78.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj78.transform.localScale = new Vector3(1f, 1f, 5.009336f);
            obj78.layer = safeLayer;
            obj78.GetComponent<MeshRenderer>().enabled = false;

            var obj79 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj79.transform.SetParent(root);
            obj79.transform.position = new Vector3(100.66f, -6.49f, 16.9f);
            obj79.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj79.transform.localScale = new Vector3(1f, 1f, 10.3946f);
            obj79.layer = safeLayer;
            obj79.GetComponent<MeshRenderer>().enabled = false;

            var obj80 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj80.transform.SetParent(root);
            obj80.transform.position = new Vector3(-24.76f, -18.48f, 5.05f);
            obj80.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj80.transform.localScale = new Vector3(14.41104f, 1f, 1f);
            obj80.layer = safeLayer;
            obj80.GetComponent<MeshRenderer>().enabled = false;

            var obj81 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj81.transform.SetParent(root);
            obj81.transform.position = new Vector3(-43.5f, -12.49f, -0.08f);
            obj81.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj81.transform.localScale = new Vector3(1.030344f, 1f, 9.244272f);
            obj81.layer = safeLayer;
            obj81.GetComponent<MeshRenderer>().enabled = false;

            var obj82 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj82.transform.SetParent(root);
            obj82.transform.position = new Vector3(113.24f, -6.49f, 18.7f);
            obj82.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            obj82.transform.localScale = new Vector3(3.955686f, 1f, 1.1637f);
            obj82.layer = safeLayer;
            obj82.GetComponent<MeshRenderer>().enabled = false;

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
