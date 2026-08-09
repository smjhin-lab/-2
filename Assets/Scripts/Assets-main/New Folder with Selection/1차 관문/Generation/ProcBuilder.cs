using System.Collections.Generic;
using UnityEngine;

namespace IndoorSim.Gen
{
    /// <summary>층 기준 높이 상수. 1 unit = 1 m (명세 3장: 1:1 스케일)</summary>
    public static class Lv
    {
        public const float Ground = 0f;    // 지상
        public const float B1 = -6f;       // 대합실 + 백화점 지하 연결층
        public const float PlatA = -12f;   // 3호선 승강장 (B2)
        public const float PlatB = -18f;   // 9호선 승강장 (B3)
        public const float FloorH = 4f;    // 지하 천장고
        public const float Thick = 0.3f;   // 슬래브 두께
    }

    /// <summary>절차적 건축 프리미티브 빌더 (박스/슬래브/벽/계단/라벨/조명)</summary>
    public static class ProcBuilder
    {
        public static readonly List<Rect> GroundHoles = new List<Rect>();
        static readonly Dictionary<string, Material> Mats = new Dictionary<string, Material>();

        public static void ResetCaches() { Mats.Clear(); GroundHoles.Clear(); }

        // ==========================================
        // 🚀 [최적화 핵심] 재질 통합 관리 및 캐싱 시스템
        // ==========================================
        public static Material LoadMat(string name, bool useFallback = true)
        {
            // 1. 이미 캐시된 재질이 있다면 즉시 반환 (I/O 비용 제거)
            if (Mats.TryGetValue(name, out var cached) && cached != null) return cached;

            // 2. 캐시에 없으면 Resources 폴더에서 로드
            var m = Resources.Load<Material>(name);
            if (m != null)
            {
                Mats[name] = m;
                return m;
            }

            // 3. 재질이 없을 때의 처리 (에러 방지용 임시 재질 생성)
            if (useFallback)
            {
                Debug.LogWarning($"[ProcBuilder] '{name}' 재질을 찾을 수 없어 임시 회색 재질로 대체합니다.");
                m = Mat(name + "_fallback", Color.gray);
                Mats[name] = m;
                return m;
            }
            return null; // 가벽 판별 등 존재 여부만 체크할 때 사용
        }

        // ==========================================
        // 🚀 [최적화 핵심] 드로우콜 감소를 위한 정적 배칭(Static Batching)
        // ==========================================
        public static void OptimizeBatching(GameObject root)
        {
            // 수천 개의 큐브와 실린더를 하나의 메쉬 덩어리로 묶어 렌더링 성능을 극대화
            StaticBatchingUtility.Combine(root);
        }

        // ---------- 재질 ----------
        public static Material Mat(string name, Color c, float metallic = 0f, float smooth = 0.35f)
        {
            if (Mats.TryGetValue(name, out var cached) && cached != null) return cached;
            var sh = Shader.Find("Universal Render Pipeline/Lit");
            if (sh == null) sh = Shader.Find("Standard");
            var m = new Material(sh) { name = "M_" + name, color = c };
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
            Mats[name] = m;
            return m;
        }

        public static Material Glass()
        {
            if (Mats.TryGetValue("glass", out var cached) && cached != null) return cached;
            var sh = Shader.Find("Universal Render Pipeline/Lit");
            if (sh == null) sh = Shader.Find("Standard");
            var m = new Material(sh) { name = "M_glass" };
            m.color = new Color(0.65f, 0.82f, 0.9f, 0.35f);
            if (m.HasProperty("_Surface"))
            {
                m.SetFloat("_Surface", 1f); // URP Transparent
                m.SetOverrideTag("RenderType", "Transparent");
                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite", 0);
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.renderQueue = 3000;
            }
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.9f);
            Mats["glass"] = m;
            return m;
        }

        // ---------- 기본 오브젝트 ----------
        public static GameObject Group(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }

        public static GameObject Box(Transform parent, string name, Vector3 center, Vector3 size, Material m, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = m;
            if (!collider) SafeDestroy(go.GetComponent<Collider>());
            return go;
        }

        public static GameObject Cyl(Transform parent, string name, Vector3 baseCenter, float radius, float height, Material m)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = baseCenter + Vector3.up * (height * 0.5f);
            go.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
            go.GetComponent<Renderer>().sharedMaterial = m;
            return go;
        }

        public static GameObject Slab(Transform parent, string name, float yTop, Rect area, Material m, float thick = Lv.Thick, bool collider = true)
        {
            return Box(parent, name,
                new Vector3(area.center.x, yTop - thick * 0.5f, area.center.y),
                new Vector3(area.width, thick, area.height), m, collider);
        }

        public static void SlabWithHoles(Transform parent, string name, float yTop, Rect area, IEnumerable<Rect> holes, Material m, float thick = Lv.Thick)
        {
            var hs = new List<Rect>();
            if (holes != null)
                foreach (var h in holes)
                {
                    var c = Intersect(h, area);
                    if (c.width > 0.01f && c.height > 0.01f) hs.Add(c);
                }
            if (hs.Count == 0) { Slab(parent, name, yTop, area, m, thick); return; }

            var grp = Group(parent, name);
            var xs = new List<float> { area.xMin, area.xMax };
            foreach (var h in hs) { xs.Add(h.xMin); xs.Add(h.xMax); }
            xs.Sort();

            for (int i = 0; i < xs.Count - 1; i++)
            {
                float x0 = xs[i], x1 = xs[i + 1];
                if (x1 - x0 < 0.01f) continue;

                var blocked = new List<Vector2>();
                foreach (var h in hs)
                    if (h.xMin < x1 - 0.005f && h.xMax > x0 + 0.005f)
                        blocked.Add(new Vector2(h.yMin, h.yMax));
                blocked.Sort((a, b) => a.x.CompareTo(b.x));

                float z = area.yMin;
                foreach (var iv in blocked)
                {
                    if (iv.x > z + 0.01f)
                        Slab(grp.transform, "p", yTop, Rect.MinMaxRect(x0, z, x1, iv.x), m, thick);
                    z = Mathf.Max(z, iv.y);
                }
                if (area.yMax > z + 0.01f)
                    Slab(grp.transform, "p", yTop, Rect.MinMaxRect(x0, z, x1, area.yMax), m, thick);
            }
        }

        public static void WallX(Transform parent, string name, float z, float xFrom, float xTo, float yBottom, float height, Material m, float thick = 0.3f, params (float from, float to)[] gaps)
        {
            var grp = Group(parent, name);
            var gl = new List<(float, float)>(gaps ?? new (float, float)[0]);
            gl.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            float x = xFrom;
            foreach (var g in gl)
            {
                if (g.Item1 > x + 0.01f)
                    Box(grp.transform, "seg", new Vector3((x + g.Item1) * 0.5f, yBottom + height * 0.5f, z), new Vector3(g.Item1 - x, height, thick), m);
                x = Mathf.Max(x, g.Item2);
            }
            if (xTo > x + 0.01f)
                Box(grp.transform, "seg", new Vector3((x + xTo) * 0.5f, yBottom + height * 0.5f, z), new Vector3(xTo - x, height, thick), m);
        }

        public static void WallZ(Transform parent, string name, float x, float zFrom, float zTo, float yBottom, float height, Material m, float thick = 0.3f, params (float from, float to)[] gaps)
        {
            var grp = Group(parent, name);
            var gl = new List<(float, float)>(gaps ?? new (float, float)[0]);
            gl.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            float z = zFrom;
            foreach (var g in gl)
            {
                if (g.Item1 > z + 0.01f)
                    Box(grp.transform, "seg", new Vector3(x, yBottom + height * 0.5f, (z + g.Item1) * 0.5f), new Vector3(thick, height, g.Item1 - z), m);
                z = Mathf.Max(z, g.Item2);
            }
            if (zTo > z + 0.01f)
                Box(grp.transform, "seg", new Vector3(x, yBottom + height * 0.5f, (z + zTo) * 0.5f), new Vector3(thick, height, zTo - z), m);
        }

        public static GameObject Ramp(Transform parent, string name, Vector3 bottom, Vector3 dir, float run, float rise, float width, float thick, Material m, bool visible, bool collider = true)
        {
            Vector3 v = dir * run + Vector3.up * rise;
            float len = v.magnitude;
            var rot = Quaternion.LookRotation(v.normalized, Vector3.up);
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localRotation = rot;
            go.transform.localPosition = bottom + v * 0.5f - (rot * Vector3.up) * (thick * 0.5f);
            go.transform.localScale = new Vector3(width, thick, len);
            go.GetComponent<Renderer>().sharedMaterial = m;
            if (!visible) SafeDestroy(go.GetComponent<Renderer>());
            if (!collider) SafeDestroy(go.GetComponent<Collider>());
            return go;
        }

        public static Rect Stairs(Transform parent, string name, Vector3 bottom, Vector3 dir, float rise, float width, Material m, bool handrails = true)
        {
            var grp = Group(parent, name);
            int n = Mathf.CeilToInt(rise / 0.2f);
            float riser = rise / n, tread = 0.3f, run = n * tread;
            Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;

            for (int i = 0; i < n; i++)
            {
                float top = riser * (i + 1);
                Vector3 c = bottom + dir * (tread * (i + 0.5f)) + Vector3.up * (top * 0.5f);
                Vector3 sz = Abs(dir) * tread + Abs(side) * width + Vector3.up * top;
                Box(grp.transform, "step" + i, c, sz, m, false);
            }
            Ramp(grp.transform, "walkRamp", bottom, dir, run, rise, width, 0.12f, m, false, true);
            if (handrails)
            {
                var mRail = Mat("handrail", new Color(0.3f, 0.31f, 0.33f), 0.8f, 0.7f);
                foreach (float s in new[] { -1f, 1f })
                    Ramp(grp.transform, "rail", bottom + side * (s * (width * 0.5f + 0.05f)) + Vector3.up * 0.95f, dir, run, rise, 0.06f, 0.06f, mRail, true, false);
            }
            return FootRect(bottom, dir, run, width);
        }

        public static Rect FootRect(Vector3 bottom, Vector3 dir, float run, float width, float backMargin = 0.5f, float sideMargin = 0.35f)
        {
            Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;
            Vector3 a = bottom - dir * backMargin - side * (width * 0.5f + sideMargin);
            Vector3 b = bottom + dir * run + side * (width * 0.5f + sideMargin);
            return Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.z, b.z), Mathf.Max(a.x, b.x), Mathf.Max(a.z, b.z));
        }

        public static GameObject KLabel(Transform parent, string text, Vector3 pos, float size = 0.45f, Color? color = null, float yRot = 0f, string placeId = null, string category = null)
        {
            var go = new GameObject("LBL_" + text);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(0f, yRot, 0f);

            // 기존과 동일한 KLabel 구조 (실제 컴포넌트는 프로젝트에 존재한다고 가정)
            var kl = go.AddComponent<KoreanLabel>();
            kl.text = text;
            kl.size = size;
            kl.color = color ?? Color.white;
            kl.Rebuild();
            if (!string.IsNullOrEmpty(placeId))
            {
                var pl = go.AddComponent<PlaceLabel>();
                pl.placeId = placeId;
                pl.displayName = text;
                pl.category = category ?? "";
            }
            return go;
        }

        public static void Lamp(Transform parent, Vector3 pos, float range = 14f, float intensity = 2.2f)
        {
            var go = new GameObject("Lamp");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = range;
            l.intensity = intensity;
            l.color = new Color(1f, 0.96f, 0.88f);
            l.shadows = LightShadows.None; // 실시간 조명 그림자 오프 (성능 보호)
        }

        public static Vector3 Abs(Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

        public static Rect Intersect(Rect a, Rect b)
        {
            float x0 = Mathf.Max(a.xMin, b.xMin), x1 = Mathf.Min(a.xMax, b.xMax);
            float z0 = Mathf.Max(a.yMin, b.yMin), z1 = Mathf.Min(a.yMax, b.yMax);
            return Rect.MinMaxRect(x0, z0, Mathf.Max(x0, x1), Mathf.Max(z0, z1));
        }

        public static void SafeDestroy(Object o)
        {
            if (o == null) return;
            if (Application.isPlaying) Object.Destroy(o); else Object.DestroyImmediate(o);
        }
    }
}
