using UnityEngine;

public class BeaconNode : MonoBehaviour
{
    [Header("비콘 정보 설정")]
    public string beaconName = "BCN-001";
    public float txPower = -59f;
    public float pathLossN = 2.6f;

    [Header("실시간 수신 신호 (테스트용)")]
    // 시뮬레이션 중에 인스펙터 창에서 이 값을 조절해서 위치가 변하는지 테스트할 수 있습니다.
    [Range(-100f, -40f)]
    public float currentRssi = -100f;
}