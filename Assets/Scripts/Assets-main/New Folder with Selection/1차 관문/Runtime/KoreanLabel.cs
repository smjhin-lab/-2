using UnityEngine;

namespace IndoorSim
{
    /// <summary>
    /// 한글 3D 라벨. 내장 폰트(Liberation Sans)에는 한글 글리프가 없으므로
    /// OS 폰트(맑은 고딕 등)를 동적 로드해서 TextMesh에 적용한다.
    /// 씬 재로드 후에도 OnEnable에서 폰트를 다시 연결하므로 안전.
    /// </summary>
    [ExecuteAlways]
    public class KoreanLabel : MonoBehaviour
    {
        [TextArea] public string text = "라벨";
        public float size = 0.45f;       // 글자 높이(미터 근사)
        public Color color = Color.white;

        static Font _font;
        public static Font KFont
        {
            get { if (_font == null) _font = LoadFont(); return _font; }
        }

        static Font LoadFont()
        {
            string[] preferred = { "Malgun Gothic", "맑은 고딕", "Apple SD Gothic Neo", "Noto Sans CJK KR", "Noto Sans KR", "NanumGothic", "Gulim" };
            try
            {
                var installed = Font.GetOSInstalledFontNames();
                foreach (var want in preferred)
                    foreach (var have in installed)
                        if (string.Equals(have, want, System.StringComparison.OrdinalIgnoreCase))
                            return Font.CreateDynamicFontFromOSFont(have, 32);
                // 이름이 정확히 일치하지 않으면 Gothic 계열 아무거나
                foreach (var have in installed)
                    if (have.Contains("Gothic") || have.Contains("고딕"))
                        return Font.CreateDynamicFontFromOSFont(have, 32);
            }
            catch { }
            try { return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch { }
            try { return Resources.GetBuiltinResource<Font>("Arial.ttf"); }
            catch { return null; }
        }

        void OnEnable() { Rebuild(); }

        public void Rebuild()
        {
            var tm = GetComponent<TextMesh>();
            if (tm == null) tm = gameObject.AddComponent<TextMesh>();
            tm.text = text;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.fontSize = 64;
            tm.characterSize = size * 10f / 64f;
            tm.color = color;
            var f = KFont;
            if (f != null)
            {
                tm.font = f;
                var mr = GetComponent<MeshRenderer>();
                if (mr != null) mr.sharedMaterial = f.material;
            }
        }

#if UNITY_EDITOR
        void OnValidate() { if (GetComponent<TextMesh>() != null) Rebuild(); }
#endif
    }
}