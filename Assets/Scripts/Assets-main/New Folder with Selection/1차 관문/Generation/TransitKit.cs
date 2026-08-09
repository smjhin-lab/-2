using System.Collections.Generic;
using UnityEngine;

namespace IndoorSim.Gen
{
    /// <summary>역/건물 제반시설 키트: 에스컬레이터, 엘리베이터, 개찰구, 스크린도어, 선로, 출구 등</summary>
    public static class TransitKit
    {
        public static Rect Escalator(Transform parent, string name, Vector3 bottom, Vector3 dir, float rise, bool goesUp, float width = 1.2f)
        {
            var grp = ProcBuilder.Group(parent, name);
            float run = rise * 1.5f;
            Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;
            Vector3 top = bottom + dir * run + Vector3.up * rise;

            // 캐시 매니저 적용
            var mBelt = ProcBuilder.LoadMat("M_EscalatorBelt");
            var mSteel = ProcBuilder.LoadMat("M_BrushedSteel");
            var mYellow = ProcBuilder.LoadMat("M_YellowPlastic");
            var mHandrail = ProcBuilder.LoadMat("M_HandrailRubber");
            var mBlackPanel = ProcBuilder.LoadMat("M_HandrailRubber");

            float extend = 0.8f;

            ProcBuilder.Box(grp.transform, "plateIn", bottom - dir * (extend * 0.5f) + Vector3.up * 0.02f,
                ProcBuilder.Abs(dir) * extend + ProcBuilder.Abs(side) * width, mSteel);
            ProcBuilder.Box(grp.transform, "plateOut", top + dir * (extend * 0.5f) + Vector3.up * 0.02f,
                ProcBuilder.Abs(dir) * extend + ProcBuilder.Abs(side) * width, mSteel);

            ProcBuilder.Box(grp.transform, "combIn", bottom - dir * 0.05f + Vector3.up * 0.025f,
                ProcBuilder.Abs(dir) * 0.1f + ProcBuilder.Abs(side) * (width - 0.2f), mYellow, false);
            ProcBuilder.Box(grp.transform, "combOut", top + dir * 0.05f + Vector3.up * 0.025f,
                ProcBuilder.Abs(dir) * 0.1f + ProcBuilder.Abs(side) * (width - 0.2f), mYellow, false);

            ProcBuilder.Ramp(grp.transform, "belt", bottom, dir, run, rise, width - 0.2f, 0.2f, mBelt, true, true);

            foreach (float s in new[] { -1f, 1f })
            {
                Vector3 off = side * (s * (width * 0.5f - 0.05f));

                ProcBuilder.Ramp(grp.transform, "side_bot", bottom + off + Vector3.up * 0.525f - dir * extend, dir, extend + 0.5f, 0f, 0.12f, 0.95f, mBlackPanel, true, false);
                ProcBuilder.Ramp(grp.transform, "handrail_bot", bottom + off + Vector3.up * 1.0f - dir * extend, dir, extend, 0f, 0.12f, 0.08f, mHandrail, true, false);

                ProcBuilder.Ramp(grp.transform, "side_mid", bottom + off + Vector3.up * 0.6f, dir, run, rise, 0.14f, 1.5f, mBlackPanel, true, false);
                ProcBuilder.Ramp(grp.transform, "handrail_mid", bottom + off + Vector3.up * 1.0f, dir, run, rise, 0.12f, 0.08f, mHandrail, true, false);

                ProcBuilder.Ramp(grp.transform, "side_top", top + off + Vector3.up * 0.525f - dir * 0.5f, dir, extend + 0.5f, 0f, 0.12f, 0.95f, mBlackPanel, true, false);
                ProcBuilder.Ramp(grp.transform, "handrail_top", top + off + Vector3.up * 1.0f, dir, extend, 0f, 0.12f, 0.08f, mHandrail, true, false);

                CreateHandrailEnd(grp.transform, bottom + off + Vector3.up * 0.525f - dir * extend, dir, mHandrail, mBlackPanel);
                CreateHandrailEnd(grp.transform, top + off + Vector3.up * 0.525f + dir * extend, dir, mHandrail, mBlackPanel);
            }

            Vector3 v = dir * run + Vector3.up * rise;
            var trig = new GameObject("beltTrigger");
            trig.transform.SetParent(grp.transform, false);
            trig.transform.localRotation = Quaternion.LookRotation(v.normalized, Vector3.up);
            trig.transform.localPosition = bottom + v * 0.5f + Vector3.up * 0.9f;
            var bc = trig.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = Vector3.one;
            trig.transform.localScale = new Vector3(width, 2.0f, v.magnitude);

            var belt = trig.AddComponent<EscalatorBelt>();
            belt.moveDirection = (goesUp ? v : -v).normalized;
            belt.speed = 0.75f;

            ProcBuilder.KLabel(grp.transform, goesUp ? "▲" : "▼", bottom + dir * (run * 0.5f) + Vector3.up * (rise * 0.5f + 2.2f), 0.5f, new Color(1f, 0.85f, 0.2f));

            return ProcBuilder.FootRect(bottom, dir, run, width + 0.3f);
        }

        public static void CreateGlassFence(Transform parent, string name, Vector3 startPos, float length, bool isXAxis, Material glass, Material frameMat)
        {
            var grp = ProcBuilder.Group(parent, name).transform;
            float fenceH = 1.2f;
            float thickness = 0.04f;

            Vector3 coverSize = isXAxis ? new Vector3(Mathf.Abs(length) + 0.1f, 0.04f, 0.15f) : new Vector3(0.15f, 0.04f, Mathf.Abs(length) + 0.1f);
            Vector3 coverPos = startPos + (isXAxis ? Vector3.right * length * 0.5f : Vector3.forward * length * 0.5f) + Vector3.down * 0.02f;
            ProcBuilder.Box(grp, "gap_cover", coverPos, coverSize, frameMat);

            Vector3 glassCenter = startPos + Vector3.up * (fenceH * 0.5f);
            Vector3 glassSize;
            if (isXAxis)
            {
                glassCenter.x += length * 0.5f;
                glassSize = new Vector3(Mathf.Abs(length), fenceH, thickness);
            }
            else
            {
                glassCenter.z += length * 0.5f;
                glassSize = new Vector3(thickness, fenceH, Mathf.Abs(length));
            }
            ProcBuilder.Box(grp, "glass_panel", glassCenter, glassSize, glass);

            Vector3 frameSize = isXAxis ? new Vector3(Mathf.Abs(length), 0.06f, 0.06f) : new Vector3(0.06f, 0.06f, Mathf.Abs(length));
            ProcBuilder.Box(grp, "frame_bottom", startPos + Vector3.up * 0.03f + (isXAxis ? Vector3.right * length * 0.5f : Vector3.forward * length * 0.5f), frameSize, frameMat);
            ProcBuilder.Box(grp, "frame_top", startPos + Vector3.up * (fenceH - 0.03f) + (isXAxis ? Vector3.right * length * 0.5f : Vector3.forward * length * 0.5f), frameSize, frameMat);

            float step = 2.0f;
            float absLen = Mathf.Abs(length);
            int postCount = Mathf.Max(2, Mathf.CeilToInt(absLen / step) + 1);

            for (int i = 0; i < postCount; i++)
            {
                float t = (postCount > 1) ? (float)i / (postCount - 1) : 0f;
                float currDist = t * length;

                Vector3 postPos = startPos + Vector3.up * (fenceH * 0.5f);
                if (isXAxis) postPos.x += currDist;
                else postPos.z += currDist;

                ProcBuilder.Box(grp, $"post_{i}", postPos, new Vector3(0.05f, fenceH, 0.05f), frameMat);
            }
        }

        public static Rect ElevatorShaft(Transform parent, string name, Vector2 center, float[] floorYs, Vector3 front, float size = 2.4f)
        {
            var grp = ProcBuilder.Group(parent, name);
            var glass = ProcBuilder.Glass();
            // 캐시 매니저 적용
            var mSteel = ProcBuilder.LoadMat("M_DarkMetal");
            float yLow = floorYs[0], yHigh = floorYs[floorYs.Length - 1];
            float h = (yHigh - yLow) + 3.2f;
            float half = size * 0.5f;
            Vector3 side = Vector3.Cross(Vector3.up, front).normalized;
            Vector3 c3 = new Vector3(center.x, 0f, center.y);

            ProcBuilder.Box(grp.transform, "wallBack", c3 - front * half + Vector3.up * (yLow + h * 0.5f),
                ProcBuilder.Abs(side) * size + ProcBuilder.Abs(front) * 0.12f + Vector3.up * h, glass);
            foreach (float s in new[] { -1f, 1f })
                ProcBuilder.Box(grp.transform, "wallSide", c3 + side * (s * half) + Vector3.up * (yLow + h * 0.5f),
                    ProcBuilder.Abs(front) * size + ProcBuilder.Abs(side) * 0.12f + Vector3.up * h, glass);

            var cab = ProcBuilder.Group(grp.transform, "Cab");
            float inner = size - 0.35f;
            ProcBuilder.Box(cab.transform, "cabFloor", Vector3.up * 0.08f,
                ProcBuilder.Abs(side) * inner + ProcBuilder.Abs(front) * inner + Vector3.up * 0.16f, mSteel);
            ProcBuilder.Box(cab.transform, "cabBack", -front * (inner * 0.5f) + Vector3.up * 0.7f,
                ProcBuilder.Abs(side) * inner + ProcBuilder.Abs(front) * 0.08f + Vector3.up * 1.1f, mSteel);
            foreach (float s in new[] { -1f, 1f })
                ProcBuilder.Box(cab.transform, "cabSide", side * (s * inner * 0.5f) + Vector3.up * 0.7f,
                    ProcBuilder.Abs(front) * inner + ProcBuilder.Abs(side) * 0.08f + Vector3.up * 1.1f, mSteel);

            cab.transform.localPosition = c3 + Vector3.up * floorYs[0];
            var ec = cab.AddComponent<ElevatorCab>();
            ec.floorYs = floorYs;
            ec.cabSize = inner;

            var trig = new GameObject("cabTrigger");
            trig.transform.SetParent(cab.transform, false);
            trig.transform.localPosition = Vector3.up * 1.1f;
            var bc = trig.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(inner, 2.2f, inner);
            trig.AddComponent<ElevatorCabTrigger>();

            foreach (float fy in floorYs)
                ProcBuilder.KLabel(grp.transform, "엘리베이터", c3 + front * (half + 0.6f) + Vector3.up * (fy + 2.3f), 0.3f, new Color(0.4f, 0.9f, 1f));

            return new Rect(center.x - half - 0.15f, center.y - half - 0.15f, size + 0.3f, size + 0.3f);
        }

        public static void FareGates(Transform parent, string name, float x, float zFrom, float zTo, float floorY, float wideGateZ)
        {
            var grp = ProcBuilder.Group(parent, name);
            // 캐시 매니저 적용
            var mBody = ProcBuilder.LoadMat("M_GateBody");
            var mPad = ProcBuilder.LoadMat("M_YellowPlastic");

            for (float z = zFrom; z <= zTo + 0.01f; z += 0.9f)
            {
                if (Mathf.Abs(z - wideGateZ) < 0.9f) continue;
                ProcBuilder.Box(grp.transform, "pillar", new Vector3(x, floorY + 0.55f, z), new Vector3(0.9f, 1.1f, 0.22f), mBody);
                ProcBuilder.Box(grp.transform, "pad", new Vector3(x, floorY + 1.12f, z), new Vector3(0.85f, 0.05f, 0.2f), mPad, false);
            }
            ProcBuilder.KLabel(grp.transform, "개찰구", new Vector3(x, floorY + 2.6f, (zFrom + zTo) * 0.5f), 0.5f, new Color(1f, 0.85f, 0.2f), 90f, "gate_main", "gate");
            ProcBuilder.KLabel(grp.transform, "교통약자 게이트", new Vector3(x, floorY + 1.8f, wideGateZ), 0.28f, new Color(0.4f, 1f, 0.6f), 90f, "gate_wide", "gate");
        }

        public static void ScreenDoorX(Transform parent, string name, float z, float xFrom, float xTo, float floorY, Material lineColor)
        {
            var grp = ProcBuilder.Group(parent, name);
            var glass = ProcBuilder.Glass();
            var mSteel = ProcBuilder.LoadMat("M_DarkMetal");
            float len = xTo - xFrom;
            ProcBuilder.Box(grp.transform, "glassWall", new Vector3((xFrom + xTo) * 0.5f, floorY + 1.4f, z), new Vector3(len, 2.8f, 0.1f), glass);
            for (float x = xFrom; x <= xTo + 0.01f; x += 6f)
                ProcBuilder.Box(grp.transform, "post", new Vector3(x, floorY + 1.4f, z), new Vector3(0.15f, 2.8f, 0.16f), mSteel);
            ProcBuilder.Box(grp.transform, "header", new Vector3((xFrom + xTo) * 0.5f, floorY + 3.0f, z), new Vector3(len, 0.4f, 0.25f), mSteel);
            ProcBuilder.Box(grp.transform, "lineStripe", new Vector3((xFrom + xTo) * 0.5f, floorY + 3.0f, z + 0.14f), new Vector3(len, 0.15f, 0.03f), lineColor, false);
        }

        public static void ScreenDoorZ(Transform parent, string name, float x, float zFrom, float zTo, float floorY, Material lineColor)
        {
            var grp = ProcBuilder.Group(parent, name);
            var glass = ProcBuilder.Glass();
            var mSteel = ProcBuilder.LoadMat("M_DarkMetal");
            float len = zTo - zFrom;
            ProcBuilder.Box(grp.transform, "glassWall", new Vector3(x, floorY + 1.4f, (zFrom + zTo) * 0.5f), new Vector3(0.1f, 2.8f, len), glass);
            for (float z = zFrom; z <= zTo + 0.01f; z += 6f)
                ProcBuilder.Box(grp.transform, "post", new Vector3(x, floorY + 1.4f, z), new Vector3(0.16f, 2.8f, 0.15f), mSteel);
            ProcBuilder.Box(grp.transform, "header", new Vector3(x, floorY + 3.0f, (zFrom + zTo) * 0.5f), new Vector3(0.25f, 0.4f, len), mSteel);
            ProcBuilder.Box(grp.transform, "lineStripe", new Vector3(x + 0.14f, floorY + 3.0f, (zFrom + zTo) * 0.5f), new Vector3(0.03f, 0.15f, len), lineColor, false);
        }

        public static void TrackX(Transform parent, string name, float zCenter, float xFrom, float xTo, float trenchTop)
        {
            var grp = ProcBuilder.Group(parent, name);
            // 캐시 매니저 적용
            var mBed = ProcBuilder.LoadMat("M_RealConcrete");
            var mRail = ProcBuilder.LoadMat("M_SteelRail");
            float len = xTo - xFrom, cx = (xFrom + xTo) * 0.5f;
            ProcBuilder.Box(grp.transform, "bed", new Vector3(cx, trenchTop - 0.15f, zCenter), new Vector3(len, 0.3f, 4f), mBed);
            foreach (float s in new[] { -0.72f, 0.72f })
                ProcBuilder.Box(grp.transform, "rail", new Vector3(cx, trenchTop + 0.08f, zCenter + s), new Vector3(len, 0.16f, 0.12f), mRail);
        }

        public static void TrackZ(Transform parent, string name, float xCenter, float zFrom, float zTo, float trenchTop)
        {
            var grp = ProcBuilder.Group(parent, name);
            // 캐시 매니저 적용
            var mBed = ProcBuilder.LoadMat("M_RealConcrete");
            var mRail = ProcBuilder.LoadMat("M_SteelRail");
            float len = zTo - zFrom, cz = (zFrom + zTo) * 0.5f;
            ProcBuilder.Box(grp.transform, "bed", new Vector3(xCenter, trenchTop - 0.15f, cz), new Vector3(4f, 0.3f, len), mBed);
            foreach (float s in new[] { -0.72f, 0.72f })
                ProcBuilder.Box(grp.transform, "rail", new Vector3(xCenter + s, trenchTop + 0.08f, cz), new Vector3(0.12f, 0.16f, len), mRail);
        }

        public static void GroundExit(Transform parent, int exitNo, float cx, float wallZ, float dirSign)
        {
            var grp = ProcBuilder.Group(parent, "출구_" + exitNo);
            // 캐시 매니저 적용
            var mWall = ProcBuilder.LoadMat("M_StationWall");
            var mFloor = ProcBuilder.LoadMat("M_StationFloor");
            var mStair = ProcBuilder.LoadMat("M_StairStone");
            var mSign = ProcBuilder.Mat("exitSign", new Color(1f, 0.8f, 0.05f), 0f, 0.6f);
            Vector3 dir = new Vector3(0, 0, dirSign);
            float corrEnd = wallZ + 6f * dirSign;
            float stairTop = corrEnd + 9f * dirSign;

            ProcBuilder.Slab(grp.transform, "corrFloor", Lv.B1, RectXZ(cx - 2f, cx + 2f, wallZ, corrEnd), mFloor);
            ProcBuilder.Slab(grp.transform, "corrCeil", Lv.B1 + Lv.FloorH + Lv.Thick, RectXZ(cx - 2.3f, cx + 2.3f, wallZ, corrEnd), mWall);
            ProcBuilder.WallZ(grp.transform, "corrWallL", cx - 2.15f, Mathf.Min(wallZ, corrEnd), Mathf.Max(wallZ, corrEnd), Lv.B1, Lv.FloorH, mWall);
            ProcBuilder.WallZ(grp.transform, "corrWallR", cx + 2.15f, Mathf.Min(wallZ, corrEnd), Mathf.Max(wallZ, corrEnd), Lv.B1, Lv.FloorH, mWall);
            ProcBuilder.Lamp(grp.transform, new Vector3(cx, Lv.B1 + 3.4f, (wallZ + corrEnd) * 0.5f), 10f, 1.8f);

            ProcBuilder.Stairs(grp.transform, "stairs", new Vector3(cx, Lv.B1, corrEnd), dir, 6f, 3.6f, mStair);

            float zA = Mathf.Min(corrEnd - 0.2f * dirSign, stairTop + 0.4f * dirSign);
            float zB = Mathf.Max(corrEnd - 0.2f * dirSign, stairTop + 0.4f * dirSign);
            ProcBuilder.WallZ(grp.transform, "shaftL", cx - 2.15f, zA, zB, Lv.B1, 9f, mWall, 0.15f);
            ProcBuilder.WallZ(grp.transform, "shaftR", cx + 2.15f, zA, zB, Lv.B1, 9f, mWall, 0.15f);
            ProcBuilder.WallX(grp.transform, "shaftEnd", stairTop + 0.3f * dirSign, cx - 2.3f, cx + 2.3f, Lv.B1, 6f, mWall, 0.15f);
            ProcBuilder.WallX(grp.transform, "kioskBack", corrEnd, cx - 2.3f, cx + 2.3f, 0f, 3f, mWall, 0.15f);
            ProcBuilder.Slab(grp.transform, "canopy", 3.3f, RectXZ(cx - 2.6f, cx + 2.6f, Mathf.Min(corrEnd, stairTop) - 0.5f, Mathf.Max(corrEnd, stairTop) + 1.2f), mWall);
            ProcBuilder.Box(grp.transform, "signBoard", new Vector3(cx, 2.7f, stairTop + 0.9f * dirSign), new Vector3(3.5f, 0.7f, 0.12f), mSign, false);
            ProcBuilder.KLabel(grp.transform, "한빛역 " + exitNo + "번 출구", new Vector3(cx, 2.7f, stairTop + 1.05f * dirSign), 0.32f, Color.black, dirSign > 0 ? 180f : 0f, "exit_" + exitNo, "exit");

            float hz0 = Mathf.Min(corrEnd, stairTop) - 0.4f * 1f;
            float hz1 = Mathf.Max(corrEnd, stairTop) + 0.4f;
            ProcBuilder.GroundHoles.Add(RectXZ(cx - 2.35f, cx + 2.35f, hz0, hz1));
        }

        public static Rect RectXZ(float x0, float x1, float z0, float z1)
        {
            return Rect.MinMaxRect(Mathf.Min(x0, x1), Mathf.Min(z0, z1), Mathf.Max(x0, x1), Mathf.Max(z0, z1));
        }

        static void CreateHandrailEnd(Transform parent, Vector3 center, Vector3 dir, Material mHandrail, Material mBlackPanel)
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.name = "newel_body_black";
            body.transform.SetParent(parent, false);
            body.transform.localPosition = center;
            body.transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, 0f, 90f);
            body.transform.localScale = new Vector3(0.95f, 0.05f, 0.95f);
            body.GetComponent<Renderer>().sharedMaterial = mBlackPanel;
            ProcBuilder.SafeDestroy(body.GetComponent<Collider>());

            var rubber = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rubber.name = "newel_handrail";
            rubber.transform.SetParent(parent, false);
            rubber.transform.localPosition = center;
            rubber.transform.rotation = body.transform.rotation;
            rubber.transform.localScale = new Vector3(1.01f, 0.06f, 1.01f);
            rubber.GetComponent<Renderer>().sharedMaterial = mHandrail;
            ProcBuilder.SafeDestroy(rubber.GetComponent<Collider>());
        }
    }
}
