using UnityEngine;
using UnityEngine.AI;
using IndoorSim;
using System.Linq;
using System.Collections.Generic;
using IndoorSim.PlayerCtrl;
using Unity.AI.Navigation;

[RequireComponent(typeof(LineRenderer))]
public class NavigationGuide : MonoBehaviour
{
    public Transform player;
    public float lineWidth = 0.2f;
    public Color lineColor = new Color(0.2f, 0.8f, 1f, 0.7f);
    public Color elevatorLineColor = new Color(0.4f, 1f, 0.4f, 0.7f);

    private LineRenderer lineRenderer;
    private NavMeshPath path;
    private Transform currentDestination;

    private bool showMenu = false;
    public bool useElevatorMode = false;

    private Vector2 scrollPosition;
    private GUIStyle titleStyle;
    private GUIStyle btnStyle;
    private List<PlaceLabel> cachedPlaces = new List<PlaceLabel>();

    private float pathCalcTimer = 0f;
    private bool isArrived = false;
    private float arrivalTimer = 0f;

    // 🚀 안내선 독립 시스템을 위한 경로 저장 리스트
    private List<Vector3> savedPathPoints = new List<Vector3>();

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = Canvas.GetDefaultCanvasMaterial();
        lineRenderer.positionCount = 0;

        path = new NavMeshPath();
        if (player == null) player = this.transform;

        NavMesh.SetAreaCost(3, 500.0f);
        PatchAllElevators();
    }

    private void PatchAllElevators()
    {
        NavMeshLink[] oldLinks = Object.FindObjectsByType<NavMeshLink>(FindObjectsInactive.Exclude);
        foreach (var link in oldLinks)
        {
            if (link.area == 3) Destroy(link.gameObject);
        }

        ElevatorCab[] allElevators = Object.FindObjectsByType<ElevatorCab>(FindObjectsInactive.Exclude);
        GameObject globalLinks = new GameObject("Global_Elevator_Links");
        globalLinks.transform.position = Vector3.zero;

        foreach (var ec in allElevators)
        {
            if (ec.GetComponentInChildren<ElevatorCabTrigger>() == null)
            {
                var trig = new GameObject("cabTrigger_Patched");
                trig.transform.SetParent(ec.transform, false);
                trig.transform.localPosition = Vector3.up * 1.1f;
                var bc = trig.AddComponent<BoxCollider>();
                bc.isTrigger = true;
                float size = ec.cabSize > 0 ? ec.cabSize : 2.0f;
                bc.size = new Vector3(size, 2.2f, size);
                trig.AddComponent<ElevatorCabTrigger>();
            }

            for (int i = 0; i < ec.floorYs.Length - 1; i++)
            {
                var linkObj = new GameObject($"Link_{ec.gameObject.name}_{i}");
                linkObj.transform.SetParent(globalLinks.transform);
                linkObj.transform.position = Vector3.zero;

                var link = linkObj.AddComponent<NavMeshLink>();
                Vector3 c3 = new Vector3(ec.transform.position.x, 0f, ec.transform.position.z);

                Vector3 rawPos1 = c3 + Vector3.up * ec.floorYs[i];
                Vector3 rawPos2 = c3 + Vector3.up * ec.floorYs[i + 1];

                Vector3 exactPos1 = rawPos1;
                Vector3 exactPos2 = rawPos2;

                if (NavMesh.SamplePosition(rawPos1, out NavMeshHit hit1, 5.0f, NavMesh.AllAreas))
                    exactPos1 = hit1.position;

                if (NavMesh.SamplePosition(rawPos2, out NavMeshHit hit2, 5.0f, NavMesh.AllAreas))
                    exactPos2 = hit2.position;

                link.startPoint = exactPos1;
                link.endPoint = exactPos2;
                link.width = 4.0f;
                link.bidirectional = true;
                link.area = 3;
            }
        }
    }

    public void SetElevatorMode()
    {
        useElevatorMode = true;
        NavMesh.SetAreaCost(3, 0.1f);
        if (currentDestination != null) CalculateAndSavePath(); // 모드 변경 시에만 전체 경로 새로 그리기
    }

    public void SetNormalMode()
    {
        useElevatorMode = false;
        NavMesh.SetAreaCost(3, 500.0f);
        if (currentDestination != null) CalculateAndSavePath(); // 모드 변경 시에만 전체 경로 새로 그리기
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N)) ToggleMenu();

        if (!showMenu && currentDestination != null)
        {
            Vector3 pPos = player.position;
            Vector3 dFloorPos = currentDestination.position - Vector3.up * 3.1f;

            float horizontalDist = Vector2.Distance(new Vector2(pPos.x, pPos.z), new Vector2(dFloorPos.x, dFloorPos.z));
            float verticalDist = Mathf.Abs(pPos.y - dFloorPos.y);

            if (horizontalDist <= 1.5f && verticalDist <= 2.0f)
            {
                StopNavigation();
                isArrived = true;
                arrivalTimer = 3.0f;
                return;
            }

            pathCalcTimer += Time.deltaTime;
            if (pathCalcTimer >= 0.1f) // 0.1초마다 지나간 길 지우기 체크
            {
                pathCalcTimer = 0f;
                UpdatePathProgress(); // 🚀 매번 새로 굽지 않고 남은 길만 업데이트
            }
        }

        if (isArrived)
        {
            arrivalTimer -= Time.deltaTime;
            if (arrivalTimer <= 0f) isArrived = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha8)) SetNormalMode();
        if (Input.GetKeyDown(KeyCode.Alpha9)) SetElevatorMode();
    }

    private void ToggleMenu()
    {
        showMenu = !showMenu;
        var fpc = GetComponent<FirstPersonController>();

        if (showMenu)
        {
            if (fpc != null) fpc.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            cachedPlaces = PlaceLabel.All
                .Where(p => p != null && !string.IsNullOrEmpty(p.displayName))
                .OrderBy(p => p.displayName)
                .ToList();
        }
        else
        {
            if (fpc != null) fpc.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void OnGUI()
    {
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
            btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 15 };
            if (IndoorSim.KoreanLabel.KFont != null)
            {
                titleStyle.font = IndoorSim.KoreanLabel.KFont;
                btnStyle.font = IndoorSim.KoreanLabel.KFont;
            }
        }

        if (GUI.Button(new Rect(20, 20, 200, 45), "내비게이션 목적지 검색 (N)", btnStyle)) ToggleMenu();

        if (currentDestination != null)
        {
            if (GUI.Button(new Rect(230, 20, 120, 45), "안내 종료", btnStyle)) StopNavigation();

            string destName = currentDestination.GetComponent<PlaceLabel>()?.displayName ?? "목적지";
            string modeText = useElevatorMode ? "<color=#55FF55>[장애인: 엘리베이터 우선]</color>" :
                                                "<color=#FFD700>[일반: 계단/에스컬레이터]</color>";

            GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height - 130, 600, 100),
                $"<color=#00E5FF>▶ 목적지: {destName} ◀</color>\n{modeText}\n<size=18>단축키 - 8: 일반 모드 / 9: 장애인 모드</size>",
                new GUIStyle(titleStyle) { alignment = TextAnchor.MiddleCenter, richText = true, fontSize = 24 });
        }

        if (isArrived)
        {
            GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height / 2 - 50, 600, 100),
                "<color=#55FF55>🎉 목적지에 도착하였습니다! 🎉</color>",
                new GUIStyle(titleStyle) { alignment = TextAnchor.MiddleCenter, richText = true, fontSize = 36 });
        }

        if (showMenu)
        {
            GUI.Box(new Rect(20, 75, 300, Screen.height - 120), "");
            GUI.Label(new Rect(30, 85, 280, 30), "어디로 갈까요?", titleStyle);

            scrollPosition = GUI.BeginScrollView(
                new Rect(20, 125, 300, Screen.height - 180), scrollPosition, new Rect(0, 0, 280, cachedPlaces.Count * 40));

            int yPos = 0;
            foreach (var place in cachedPlaces)
            {
                if (GUI.Button(new Rect(10, yPos, 260, 35), place.displayName, btnStyle))
                {
                    StartNavigation(place.transform);
                    ToggleMenu();
                }
                yPos += 40;
            }
            GUI.EndScrollView();
        }
    }

    public void StartNavigation(Transform destination)
    {
        isArrived = false;
        currentDestination = destination;
        CalculateAndSavePath(); // 목적지를 지정할 때 전체 경로를 1번만 계산해서 저장
    }

    public void StopNavigation()
    {
        currentDestination = null;
        savedPathPoints.Clear();
        lineRenderer.positionCount = 0;
    }

    // 🚀 1. 최초 목적지 지정 시, 목적지까지의 궤도를 1번만 계산하여 리스트에 통째로 저장합니다.
    private void CalculateAndSavePath()
    {
        if (currentDestination == null) return;

        Vector3 startPos = player.position;
        Vector3 targetPos = currentDestination.position;

        // 플레이어가 'SafePath'에서 벗어나 용암에 있더라도 가장 가까운 안전 지대(10m 이내)를 찾아 연결
        if (NavMesh.SamplePosition(player.position, out NavMeshHit startHit, 10.0f, NavMesh.AllAreas))
            startPos = startHit.position;

        Vector3 logicalFloorPos = currentDestination.position - Vector3.up * 3.1f;
        if (NavMesh.SamplePosition(logicalFloorPos, out NavMeshHit targetHit, 10.0f, NavMesh.AllAreas))
            targetPos = targetHit.position;

        if (NavMesh.CalculatePath(startPos, targetPos, NavMesh.AllAreas, path))
        {
            savedPathPoints = GetSmoothPathOnGround(path.corners);
            UpdateLineRenderer();
        }
        else
        {
            savedPathPoints.Clear();
            lineRenderer.positionCount = 0;
        }
    }

    // 🚀 2. 플레이어가 움직일 때마다 지나간 궤도를 지우고 선을 업데이트합니다.
    private void UpdatePathProgress()
    {
        if (savedPathPoints == null || savedPathPoints.Count == 0) return;

        int closestIndex = 0;
        float minDistance = float.MaxValue;
        Vector3 pPos = player.position;

        // 남은 선의 포인트들 중 플레이어와 가장 가까운 점을 찾음
        for (int i = 0; i < savedPathPoints.Count; i++)
        {
            float dist = Vector3.Distance(pPos, savedPathPoints[i]);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestIndex = i;
            }
        }

        // 플레이어가 해당 점 반경 3.0m 이내로 접근했다면, 지나왔다고 판단하고 리스트에서 지워버림
        if (minDistance < 3.0f && closestIndex > 0)
        {
            savedPathPoints.RemoveRange(0, closestIndex);
        }

        UpdateLineRenderer(); // 삭제하고 남은 부분만 선으로 그림
    }

    private void UpdateLineRenderer()
    {
        lineRenderer.positionCount = savedPathPoints.Count;
        for (int i = 0; i < savedPathPoints.Count; i++)
        {
            lineRenderer.SetPosition(i, savedPathPoints[i]);
        }
        lineRenderer.startColor = useElevatorMode ? elevatorLineColor : lineColor;
        lineRenderer.endColor = useElevatorMode ? elevatorLineColor : lineColor;
    }

    private List<Vector3> GetSmoothPathOnGround(Vector3[] corners)
    {
        List<Vector3> smoothPoints = new List<Vector3>();
        if (corners.Length == 0) return smoothPoints;

        float sampleInterval = 0.5f;
        float lineOffset = 0.3f;

        for (int i = 0; i < corners.Length - 1; i++)
        {
            Vector3 start = corners[i];
            Vector3 end = corners[i + 1];

            float distance = Vector3.Distance(start, end);
            int steps = Mathf.Max(1, Mathf.CeilToInt(distance / sampleInterval));

            for (int s = 0; s < steps; s++)
            {
                float t = (float)s / steps;
                Vector3 point = Vector3.Lerp(start, end, t);

                Vector3 rayOrigin = point + Vector3.up * 1.5f;

                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 4.0f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                {
                    smoothPoints.Add(hit.point + Vector3.up * lineOffset);
                }
                else
                {
                    smoothPoints.Add(point + Vector3.up * lineOffset);
                }
            }
        }

        Vector3 lastPoint = corners[corners.Length - 1];
        if (Physics.Raycast(lastPoint + Vector3.up * 1.5f, Vector3.down, out RaycastHit lastHit, 4.0f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            smoothPoints.Add(lastHit.point + Vector3.up * lineOffset);
        }
        else
        {
            smoothPoints.Add(lastPoint + Vector3.up * lineOffset);
        }

        return smoothPoints;
    }
}