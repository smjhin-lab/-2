using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI; // 네비게이션 사용 시 필요

public class TestManager : MonoBehaviour
{
    private UnifiedFloorFusionEngine engine;
    private BeaconNode[] sceneBeacons;
    private string displayText = "측위 대기중..."; 
    private Transform playerTransform;
    
    // 네비게이션 연동 원할 경우 주석 해제하여 사용
    // private NavMeshAgent agent; 

    void Start()
    {
        engine = new UnifiedFloorFusionEngine();
        sceneBeacons = FindObjectsByType<BeaconNode>(FindObjectsSortMode.None);
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null) 
        {
            playerTransform = playerObj.transform;
            // agent = playerObj.GetComponent<NavMeshAgent>();
        }
    }

    void Update()
    {
        if (engine == null || sceneBeacons == null || playerTransform == null) return;

        List<BeaconObs> allObs = new List<BeaconObs>();
        Vector3 pPos = playerTransform.position;

        foreach (var bcn in sceneBeacons)
        {
            Vector3 bPos = bcn.transform.position;
            float dist = Vector3.Distance(pPos, bPos);
            float finalRssi = bcn.txPower - (10f * bcn.pathLossN * Mathf.Log10(Mathf.Max(dist, 0.1f)));

            // 🚀 물리 엔진 기반 전파 감쇄 (레이 트레이싱)
            Vector3 direction = (pPos - bPos).normalized;
            RaycastHit[] hits = Physics.RaycastAll(bPos, direction, dist);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("ConcreteFloor")) finalRssi -= 15f; 
                else if (hit.collider.CompareTag("Wall")) finalRssi -= 5f;  
                else if (hit.collider.CompareTag("Glass")) finalRssi -= 2f;  
            }
            bcn.currentRssi = finalRssi;

            if (finalRssi > -90f) 
            {
                allObs.Add(new BeaconObs { 
                    name = bcn.beaconName, 
                    x = bPos.x, y = bPos.z, z = bPos.y, // 축 매핑
                    rssi = finalRssi, tx = bcn.txPower, n = bcn.pathLossN 
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
            float pivotOffset = pPos.y - (float)est.z; // 피벗 높이 보정
            if (Mathf.Abs(pivotOffset) < 1.0f) testEstPos.y += pivotOffset; 
            testErrorDistance = Vector3.Distance(pPos, testEstPos);
        }

        // 🚀 아웃라이어(튐 방지) 및 하이브리드 로직 
        float maxAllowedError = 4.0f; // 4m 이상 오차 발생 시 쓰레기값으로 간주

        if (topObs.Count < 3 || est == null || testErrorDistance > maxAllowedError)
        {
            if (topObs.Count < 3) 
                modeText = "<color=#FF5555>⚠️ 유니티 좌표 (유효 신호 부족)</color>";
            else 
                modeText = $"<color=#FF8800>⚠️ 유니티 좌표 (오차 {testErrorDistance:F1}m 초과! 튐 방지)</color>";
            
            displayPos = pPos; 
            engine.ForcePosition(pPos.x, pPos.z, pPos.y); // 엔진 목줄 잡고 동기화
            floorText = engine.GetFloorNameFromZ(pPos.y);
        }
        else
        {
            modeText = "<color=#55FF55>✅ 비콘 측위 가동 (하이브리드 퓨전)</color>";
            displayPos = testEstPos;
            floorText = est.currentFloorName;
        }

        float finalError = Vector3.Distance(pPos, displayPos);

        displayText = $"[하이브리드 튐 방지 시스템]\n\n" +
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
