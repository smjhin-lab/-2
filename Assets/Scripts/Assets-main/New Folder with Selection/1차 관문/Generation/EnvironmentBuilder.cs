using UnityEngine;

namespace IndoorSim.Gen
{
    /// <summary>지상 환경: 바닥판(출구/백화점 자리 구멍), 태양광, 주변 소품.</summary>
    public static class EnvironmentBuilder
    {
        public static void Generate(Transform root)
        {
            var env = ProcBuilder.Group(root, "Environment_지상").transform;

            // 캐시 매니저 적용
            var mGround = ProcBuilder.LoadMat("M_GroundAsphalt");
            var mPaving = ProcBuilder.LoadMat("M_GroundAsphalt");
            var mGreen = ProcBuilder.LoadMat("M_GreenGrass");

            // 지상 바닥판 — 출구 계단 샤프트/백화점 footprint 구멍 반영
            ProcBuilder.SlabWithHoles(env, "지면", 0f, TransitKit.RectXZ(-130f, 150f, -90f, 90f), ProcBuilder.GroundHoles, mGround, 0.5f);

            // 역 광장 포장 + 화단 (출구 계단 구멍 반영)
            ProcBuilder.SlabWithHoles(env, "북광장", 0.02f, TransitKit.RectXZ(-20f, 50f, 22f, 45f), ProcBuilder.GroundHoles, mPaving, 0.02f);
            ProcBuilder.SlabWithHoles(env, "남광장", 0.02f, TransitKit.RectXZ(-20f, 50f, -45f, -22f), ProcBuilder.GroundHoles, mPaving, 0.02f);

            foreach (float x in new[] { -15f, 45f })
                foreach (float z in new[] { 40f, -40f })
                {
                    ProcBuilder.Box(env, "화단", new Vector3(x, 0.25f, z), new Vector3(4f, 0.5f, 4f), mPaving);
                    ProcBuilder.Box(env, "수풀", new Vector3(x, 1.1f, z), new Vector3(3f, 1.4f, 3f), mGreen, false);
                }

            // 태양광
            var sunGo = new GameObject("Sun_Directional");
            sunGo.transform.SetParent(env, false);
            sunGo.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.6f;
            sun.color = new Color(1f, 0.97f, 0.92f);
            sun.shadows = LightShadows.Soft;

            // 🚀 배칭 최적화 호출
            ProcBuilder.OptimizeBatching(env.gameObject);
        }
    }
}
