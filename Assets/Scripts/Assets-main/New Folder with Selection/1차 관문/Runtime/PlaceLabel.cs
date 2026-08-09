using System.Collections.Generic;
using UnityEngine;

namespace IndoorSim
{
    /// <summary>
    /// 내비게이션 POI. 상점/출구/개찰구/승강장 등에 부착되어 레지스트리에 등록된다.
    /// 추후 경로탐색(길안내)의 목적지 목록으로 사용.
    /// </summary>
    public class PlaceLabel : MonoBehaviour
    {
        public string placeId;      // 예: "shop_1f_3", "exit_8", "platform_line3"
        public string displayName;  // 예: "루미에르 화장품"
        public string category;     // 예: "shop", "exit", "gate", "platform"

        static readonly Dictionary<string, PlaceLabel> Registry = new Dictionary<string, PlaceLabel>();

        public static IEnumerable<PlaceLabel> All => Registry.Values;

        public static PlaceLabel Find(string id)
        {
            Registry.TryGetValue(id, out var p);
            return p;
        }

        void OnEnable()
        {
            if (!string.IsNullOrEmpty(placeId)) Registry[placeId] = this;
        }

        void OnDisable()
        {
            if (!string.IsNullOrEmpty(placeId) && Registry.TryGetValue(placeId, out var p) && p == this)
                Registry.Remove(placeId);
        }
    }
}