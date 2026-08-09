using System.Collections.Generic;
using UnityEngine;

using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TestManager : MonoBehaviour
{
    private UnifiedFloorFusionEngine engine;
    private BeaconNode[] sceneBeacons;
    private string displayText = "측위 대기중...";
    private Transform playerTransform;

    void Start()
    {
        engine = new UnifiedFloorFusionEngine();
        sceneBeacons = FindObjectsByType<BeaconNode>(FindObjectsSortMode.None);

        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        if (engine == null || sceneBeacons == null) return;

        List<BeaconObs> allObs = new List<BeaconObs>();

        foreach (var bcn in sceneBeacons)
        {
            if (playerTransform != null)
            {
                float dist = Vector3.Distance(playerTransform.position, bcn.transform.position);
                dist = Mathf.Max(dist, 0.1f);
                bcn.currentRssi = bcn.txPower - (10f * bcn.pathLossN * Mathf.Log10(dist));
            }

            if (bcn.currentRssi > -110f)
            {
                allObs.Add(new BeaconObs
                {
                    name = bcn.beaconName,
                    x = bcn.transform.position.x,
                    y = bcn.transform.position.z,
                    z = bcn.transform.position.y,
                    rssi = bcn.currentRssi,
                    tx = bcn.txPower,
                    n = bcn.pathLossN
                });
            }
        }

        var topObs = allObs.OrderByDescending(o => o.rssi).Take(10).ToList();

        // 🔥 [해결 포인트] 플레이어의 실제 X, Z(엔진의 Y), Y(높이=엔진의 Z) 좌표를 엔진에 누락 없이 정확히 전달!
        double pX = playerTransform != null ? playerTransform.position.x : 0;
        double pY = playerTransform != null ? playerTransform.position.z : 0;
        double pZ = playerTransform != null ? playerTransform.position.y : 0;

        Estimate est = engine.Step(topObs, Time.deltaTime, null, pX, pY, pZ);

        if (est != null)
        {
            displayText = $"[통합 측위 엔진 결과 (완벽 동기화)]\n\n" +
                          $"추정 X 좌표 : {est.x:F2}\n" +
                          $"추정 Y 좌표 (높이) : {est.z:F2}\n" +
                          $"추정 Z 좌표 : {est.y:F2}\n\n" +
                          $"📌 판정된 층 : {est.currentFloorName}\n" +
                          $"📡 사용된 핵심 비콘 : {topObs.Count}개";
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 13;
        style.wordWrap = true;
        style.alignment = TextAnchor.UpperLeft;
        style.normal.textColor = Color.green;

        // 1. 박스 크기 설정
        int boxWidth = 200;
        int boxHeight = 140;
        int margin = 20; // 화면 끝에서 띄울 간격

        // 2. 🚀 핵심: Y 좌표를 (전체 높이 - 박스 높이 - 마진)으로 계산해서 아래로 보냄
        float xPos = margin;
        float yPos = Screen.height - boxHeight - margin;

        GUI.Box(new Rect(xPos, yPos, boxWidth, boxHeight), displayText, style);
    }
}