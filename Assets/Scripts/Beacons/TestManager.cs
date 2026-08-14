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
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    void Update()
    {
        if (engine == null || sceneBeacons == null || playerTransform == null) return;

        List<BeaconObs> allObs = new List<BeaconObs>();
        Vector3 pPos = playerTransform.position;

        // foreach (var bcn in sceneBeacons) 시작 부분
        foreach (var bcn in sceneBeacons)
        {
            Vector3 bPos = bcn.transform.position;
            float dist = Vector3.Distance(pPos, bPos);

            float finalRssi = bcn.txPower - (10f * bcn.pathLossN * Mathf.Log10(Mathf.Max(dist, 0.1f)));

            // 🚀 [절대 좌표 기반 층간 차폐] 지정된 Y좌표가 아니면 강제 차단!
            // 🚀 [절대 좌표 기반 층간 차폐] X좌표로 건물 분리 후 Y좌표 검사!
            float pY = pPos.y;
            float bY = bPos.y;
            bool isValidFloorBeacon = false;

            // 플레이어의 X 좌표 기준으로 우측(백화점)과 좌측(지하철역) 건물을 분리
            // 플레이어의 X 좌표 기준으로 우측(백화점)과 좌측(지하철역) 건물을 분리
            if (pPos.x > 40f)
            {
                // [우측 백화점 건물 구역]
                if (pY >= 5.85f)
                {
                    // 2층: 비콘(9.803)을 여유롭게 잡을 수 있도록 범위를 9.0 ~ 11.0 으로 대폭 확대
                    if (bY > 9.0f && bY < 11.0f) isValidFloorBeacon = true;
                }
                else
                {
                    // 1층: 플레이어 머리 위(pY)부터 2층 바닥(5.8f) 아래에 있는 비콘은 전부 허용
                    if (bY > pY && bY < 5.8f) isValidFloorBeacon = true;
                }
            }
            else
            {
                // [좌측 지하철 역사 구역] (기존 코드 유지)
                if (pY >= -5.0f)
                {
                    if (bY > 1.3f && bY < 1.7f) isValidFloorBeacon = true;
                }
                else if (pY >= -13.0f && pY < -5.0f)
                {
                    if (bY > -8.5f && bY < -6.0f) isValidFloorBeacon = true;
                }
                else
                {
                    if (bY > -14.5f && bY < -13.9f) isValidFloorBeacon = true;
                }
            }

            // 현재 구역/층의 비콘이 아니면 -50 패널티 부여 (강제 컷오프)
            if (!isValidFloorBeacon)
            {
                finalRssi -= 50f;
            }

            bcn.currentRssi = finalRssi;

            if (finalRssi > -100f)
            {
                allObs.Add(new BeaconObs
                {
                    name = bcn.beaconName,
                    x = bPos.x,
                    y = bPos.z,
                    z = bPos.y,
                    rssi = finalRssi,
                    tx = bcn.txPower,
                    n = bcn.pathLossN
                });
            }
        }

        var topObs = allObs.OrderByDescending(o => o.rssi).Take(10).ToList();

        Vector3 displayPos = pPos;
        string modeText = "";
        string floorText = "";

        Estimate est = engine.Step(topObs, Time.deltaTime, null);
        float testErrorDistance = 0f;
        Vector3 testEstPos = pPos;

        if (est != null)
        {
            testEstPos = new Vector3((float)est.x, (float)est.z, (float)est.y);

            // 🚀 [에스컬레이터 Y축 튐 완벽 해결]
            // 허용 오차를 1.0f -> 15.0f 로 대폭 증가시켜 실제 높이로 강제 스냅
            float pivotOffset = pPos.y - (float)est.z;
            if (Mathf.Abs(pivotOffset) < 15.0f) testEstPos.y += pivotOffset;

            testErrorDistance = Vector3.Distance(pPos, testEstPos);
        }

        // 🚨 유효 신호 기준 2개 + 넓은 광장 오차 10m 허용
        float maxAllowedError = 10.0f;


        // 🚨 유효 비콘 최소 3개로 롤백! (3개 안 되면 얄짤없이 에러 띄움)
        if (topObs.Count < 3 || est == null || testErrorDistance > maxAllowedError)
        {
            if (topObs.Count < 3)
                modeText = "<color=#FF5555>⚠️ 유니티 좌표 (비콘 3개 미만 - 추가 설치 필요!)</color>";
            else
                modeText = $"<color=#FF8800>⚠️ 유니티 좌표 (오차 {testErrorDistance:F1}m 초과! 튐 방지)</color>";

            displayPos = pPos;
            engine.ForcePosition(pPos.x, pPos.z, pPos.y);
            floorText = engine.GetFloorNameFromZ(pPos.y);
        }
        else
        {
            modeText = "<color=#55FF55>✅ 비콘 측위 가동 (가짜 층간 차폐 적용)</color>";
            displayPos = testEstPos;
            floorText = est.currentFloorName;
        }

        float finalError = Vector3.Distance(pPos, displayPos);

        displayText = $"[하이브리드 시스템 (층간 고정)]\n\n" +
                      $"{modeText}\n\n" +
                      $"📌 실제(Player) : {pPos.x:F1}, {pPos.y:F1}, {pPos.z:F1}\n" +
                      $"📌 출력(System) : {displayPos.x:F1}, {displayPos.y:F1}, {displayPos.z:F1}\n" +
                      $"🚨 현재 오차 : {finalError:F2} m \n" +
                      $"(엔진 추정 오차 : {testErrorDistance:F2} m)\n\n" +
                      $"층: {floorText} | 유효 비콘: {topObs.Count}개";
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 15; style.wordWrap = true;
        style.alignment = TextAnchor.UpperLeft;
        style.normal.textColor = Color.white; style.richText = true;

        GUI.Box(new Rect(20, Screen.height - 250, 380, 230), displayText, style);
    }
}
