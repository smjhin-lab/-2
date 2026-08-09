using System.Collections.Generic;
using UnityEngine;
using IndoorSim.Beacons; // ★ 비콘 네임스페이스 추가

namespace IndoorSim.Gen
{
    /// <summary>
    /// 가상 환승역 "한빛역" 생성기
    /// </summary>
    public static class StationGenerator
    {
        // ★ 백화점 비콘과 구분하기 위한 카운터 (BCN-S001 형식 사용)
        static int stationBeaconCounter = 1;

        public static void Generate(Transform root)
        {
            stationBeaconCounter = 1; // 재생성 시 카운터 초기화
            var st = ProcBuilder.Group(root, "Station_한빛역").transform;

            // ---- 재질 캐시 시스템 적용 ----
            var mFloor = ProcBuilder.LoadMat("M_StationFloor");
            var mWall = ProcBuilder.LoadMat("M_StationWall");
            var mCeil = ProcBuilder.LoadMat("M_StationCeil");
            var mConc = ProcBuilder.LoadMat("M_RealConcrete");
            var mStair = ProcBuilder.LoadMat("M_StairStone");
            var mDark = ProcBuilder.LoadMat("M_DarkMetal");
            var mYellow = ProcBuilder.LoadMat("M_TactilePaving");
            var mLine3 = ProcBuilder.LoadMat("M_Line3Metal");
            var mLine9 = ProcBuilder.LoadMat("M_Line9Metal");
            var glass = ProcBuilder.Glass();

            var b1Holes = new List<Rect>();
            var paHoles = new List<Rect>();
            var pbCeilHoles = new List<Rect>();

            var circ = ProcBuilder.Group(st, "수직동선").transform;

            b1Holes.Add(ProcBuilder.Stairs(circ, "계단_B1-3호선_2", new Vector3(-40f, Lv.PlatA, -3f), Vector3.right, 6f, 3f, mStair));
            b1Holes.Add(TransitKit.Escalator(circ, "에스컬레이터_상행_2", new Vector3(-40f, Lv.PlatA, 1.0f), Vector3.right, 6f, true));
            b1Holes.Add(TransitKit.Escalator(circ, "에스컬레이터_하행_2", new Vector3(-40f, Lv.PlatA, 2.8f), Vector3.right, 6f, false));

            var trEsc = TransitKit.Escalator(circ, "환승에스컬레이터_상행", new Vector3(-19f, Lv.PlatB, 1.0f), Vector3.left, 6f, true);
            var trStair = ProcBuilder.Stairs(circ, "환승계단_3-9호선", new Vector3(-19f, Lv.PlatB, 2.8f), Vector3.left, 6f, 1.8f, mStair);

            /* ★ 수직 동선(계단/환승) 병목 지점 비콘 배치
            AutoPlaceBeacon("환승계단_시작점_B2", new Vector3(-19f, Lv.PlatA + 2.5f, 2.8f), -2); // B2층
            AutoPlaceBeacon("환승계단_종료점_B3", new Vector3(-19f, Lv.PlatB + 2.5f, 2.8f), -3); // B3층*/

            var fenceGlass = ProcBuilder.Glass();
            var fenceFrame = ProcBuilder.LoadMat("M_BrushedSteel", false) ?? mDark;

            TransitKit.CreateGlassFence(circ, "환승_외곽_왼쪽가장자리", new Vector3(-28.5f, Lv.PlatA, 0.1f), 9.5f, true, fenceGlass, fenceFrame);
            TransitKit.CreateGlassFence(circ, "환승_외곽_오른쪽가장자리", new Vector3(-28.5f, Lv.PlatA, 3.7f), 9.5f, true, fenceGlass, fenceFrame);
            TransitKit.CreateGlassFence(circ, "환승_외곽_후면벽", new Vector3(-19.0f, Lv.PlatA, 0.1f), 3.6f, false, fenceGlass, fenceFrame);

            var transSideWall = mConc ?? mWall;
            var transStairBox = ProcBuilder.Group(circ, "환승구역_계단측면_마감판").transform;

            float transMidX = -25.0f;
            float transLengthX = 12.0f;
            float transMidY = (Lv.PlatB + Lv.PlatA) / 2f;
            float transHeightY = Lv.PlatA - Lv.PlatB;

            ProcBuilder.Box(transStairBox, "환승측면판_우측", new Vector3(transMidX, transMidY, 3.8f), new Vector3(transLengthX, transHeightY, 0.1f), transSideWall);
            ProcBuilder.Box(transStairBox, "환승측면판_좌측", new Vector3(transMidX, transMidY, 0.4f), new Vector3(transLengthX, transHeightY, 0.1f), transSideWall);

            float b1Level = Lv.PlatA + 6f;

            TransitKit.CreateGlassFence(circ, "개찰구_계단_좌측", new Vector3(-40.5f, b1Level, -4.6f), 9.5f, true, fenceGlass, fenceFrame);
            TransitKit.CreateGlassFence(circ, "개찰구_계단_우측", new Vector3(-40.5f, b1Level, -1.4f), 9.5f, true, fenceGlass, fenceFrame);
            TransitKit.CreateGlassFence(circ, "개찰구_계단_후면", new Vector3(-40.5f, b1Level, -4.6f), 3.2f, false, fenceGlass, fenceFrame);

            TransitKit.CreateGlassFence(circ, "개찰구_에스컬1_좌측", new Vector3(-40.5f, b1Level, 0.3f), 9.5f, true, fenceGlass, fenceFrame);
            TransitKit.CreateGlassFence(circ, "개찰구_에스컬1_우측", new Vector3(-40.5f, b1Level, 1.5f), 9.5f, true, fenceGlass, fenceFrame);
            TransitKit.CreateGlassFence(circ, "개찰구_에스컬1_후면", new Vector3(-40.5f, b1Level, 0.3f), 1.2f, false, fenceGlass, fenceFrame);

            TransitKit.CreateGlassFence(circ, "개찰구_에스컬2_좌측", new Vector3(-40.5f, b1Level, 2.1f), 9.5f, true, fenceGlass, fenceFrame);
            TransitKit.CreateGlassFence(circ, "개찰구_에스컬2_우측", new Vector3(-40.5f, b1Level, 3.3f), 9.5f, true, fenceGlass, fenceFrame);
            TransitKit.CreateGlassFence(circ, "개찰구_에스컬2_후면", new Vector3(-40.5f, b1Level, 2.1f), 1.2f, false, fenceGlass, fenceFrame);

            var mSideWall = mConc ?? mWall;
            var stairBox = ProcBuilder.Group(circ, "개찰구_계단측면_마감판").transform;

            float midX = -35.75f;
            float lengthX = 9.5f;
            float midY = (Lv.PlatA + Lv.B1) / 2f;
            float heightY = Lv.B1 - Lv.PlatA;

            ProcBuilder.Box(stairBox, "측면판_좌측", new Vector3(midX, midY, -4.6f), new Vector3(lengthX, heightY, 0.1f), mSideWall);

            float frontX = -40.0f;
            float frontMidY = Lv.B1 - 1.0f;
            float frontHeightY = 2.5f;
            float frontMidZ = -0.65f;
            float frontWidthZ = 10.0f;

            ProcBuilder.Box(stairBox, "단차마감판_X40", new Vector3(frontX, frontMidY, frontMidZ), new Vector3(0.1f, frontHeightY, frontWidthZ), mSideWall);

            var m9FloorWall = mConc ?? mWall;

            float fillX = -19.0f;
            float fillMidY = Lv.PlatA - 1f;
            float fillHeightY = 1.5f;
            float fillMidZ = 0.0f;
            float fillWidthZ = 9.2f;

            ProcBuilder.Box(circ, "9호선상부_허공마감판_수정", new Vector3(fillX, fillMidY, fillMidZ), new Vector3(0.1f, fillHeightY, fillWidthZ), m9FloorWall);

            paHoles.Add(trStair); paHoles.Add(trEsc);
            pbCeilHoles.Add(trStair); pbCeilHoles.Add(trEsc);

            var elvRect = TransitKit.ElevatorShaft(circ, "엘리베이터_역사", new Vector2(-14f, -2.5f), new[] { Lv.PlatB, Lv.PlatA, Lv.B1 }, Vector3.forward);
            b1Holes.Add(elvRect); paHoles.Add(elvRect); pbCeilHoles.Add(elvRect);

            /* ★ 엘리베이터 앞 대기홀 비콘 자동 배치 (층별)
            AutoPlaceBeacon("엘리베이터홀_B1", new Vector3(-14f, Lv.B1 + 2.5f, -6f), -1);
            AutoPlaceBeacon("엘리베이터홀_B2", new Vector3(-14f, Lv.PlatA + 2.5f, -6f), -2);
            AutoPlaceBeacon("엘리베이터홀_B3", new Vector3(-14f, Lv.PlatB + 2.5f, -6f), -3);*/

            // =========================================================
            // B1 대합실 (층 인덱스: -1)
            var b1 = ProcBuilder.Group(st, "B1_대합실").transform;
            Rect concourse = TransitKit.RectXZ(-40f, 40f, -20f, 20f);
            ProcBuilder.SlabWithHoles(b1, "바닥", Lv.B1, concourse, b1Holes, mFloor);
            ProcBuilder.SlabWithHoles(b1, "천장", Lv.B1 + Lv.FloorH + Lv.Thick, concourse, null, mCeil);

            float[] exitNorthX = { 0f, 12f, 24f, 36f };
            float[] exitSouthX = { 4f, 18f, 32f };
            float[] mallCorrZ = { -12f, 0f, 12f };
            ProcBuilder.WallX(b1, "북벽", 20f, -40f, 40f, Lv.B1, Lv.FloorH, mWall, 0.3f, (-2.2f, 2.2f), (9.8f, 14.2f), (21.8f, 26.2f), (33.8f, 38.2f));
            ProcBuilder.WallX(b1, "남벽", -20f, -40f, 40f, Lv.B1, Lv.FloorH, mWall, 0.3f, (1.8f, 6.2f), (15.8f, 20.2f), (29.8f, 34.2f));
            ProcBuilder.WallZ(b1, "동벽", 40f, -20f, 20f, Lv.B1, Lv.FloorH, mWall, 0.3f, (-14.5f, -9.5f), (-2.5f, 2.5f), (9.5f, 14.5f));
            ProcBuilder.WallZ(b1, "서벽", -40f, -20f, 20f, Lv.B1, Lv.FloorH, mWall);

            TransitKit.FareGates(b1, "개찰구", -5f, -8f, 8f, Lv.B1, 0f);
            ProcBuilder.WallZ(b1, "펜스남", -5f, -20f, -8f, Lv.B1, 1.2f, glass, 0.1f);
            ProcBuilder.WallZ(b1, "펜스북", -5f, 8f, 20f, Lv.B1, 1.2f, glass, 0.1f);

            /* ★ B1 대합실 주요 구역 비콘 배치
            AutoPlaceBeacon("대합실_중앙_B1", new Vector3(15f, Lv.B1 + 2.5f, 0f), -1);
            AutoPlaceBeacon("개찰구_앞_B1", new Vector3(-2f, Lv.B1 + 2.5f, 0f), -1);
            AutoPlaceBeacon("개찰구_뒤_B1", new Vector3(-12f, Lv.B1 + 2.5f, 0f), -1);*/

            for (float x = -30f; x <= 30f; x += 15f)
                foreach (float z in new[] { -10f, 10f })
                    ProcBuilder.Cyl(b1, "기둥", new Vector3(x, Lv.B1, z), 0.35f, Lv.FloorH, mConc);
            for (float x = -30f; x <= 30f; x += 15f)
                for (float z = -10f; z <= 10f; z += 10f)
                    ProcBuilder.Lamp(b1, new Vector3(x, Lv.B1 + 3.5f, z));
            ProcBuilder.KLabel(b1, "한빛역", new Vector3(0f, Lv.B1 + 3.4f, 0f), 0.8f, Color.white, 0f, "station_hanbit", "station");
            ProcBuilder.KLabel(b1, "3호선 · 9호선 환승역", new Vector3(0f, Lv.B1 + 2.7f, 0f), 0.35f, new Color(0.94f, 0.49f, 0.11f));

            var amen = ProcBuilder.Group(b1, "편의시설").transform;
            foreach (float x in new[] { 8f, 10.5f })
                ProcBuilder.Box(amen, "충전기", new Vector3(x, Lv.B1 + 0.8f, 18.8f), new Vector3(0.9f, 1.6f, 0.6f), mDark);
            ProcBuilder.KLabel(amen, "교통카드 충전기", new Vector3(9.2f, Lv.B1 + 2.1f, 18.8f), 0.25f, Color.white, 180f, "ticket_machine", "facility");
            ProcBuilder.Box(amen, "안내센터", new Vector3(2f, Lv.B1 + 1.25f, 12f), new Vector3(3f, 2.5f, 2f), mWall);
            ProcBuilder.KLabel(amen, "고객안내센터", new Vector3(2f, Lv.B1 + 2.9f, 12f), 0.3f, Color.white, 0f, "info_center", "facility");

            for (int i = 0; i < exitNorthX.Length; i++)
                ProcBuilder.KLabel(b1, (i + 1) + "번 출구 ↑", new Vector3(exitNorthX[i], Lv.B1 + 2.6f, 18.5f), 0.3f, new Color(1f, 0.85f, 0.2f), 0f);
            for (int i = 0; i < exitSouthX.Length; i++)
                ProcBuilder.KLabel(b1, (i + 5) + "번 출구 ↓", new Vector3(exitSouthX[i], Lv.B1 + 2.6f, -18.5f), 0.3f, new Color(1f, 0.85f, 0.2f), 180f);
            ProcBuilder.KLabel(b1, "한빛백화점 →", new Vector3(38f, Lv.B1 + 2.6f, 0f), 0.35f, new Color(0.5f, 1f, 0.6f), 90f);
            ProcBuilder.KLabel(b1, "승강장 (3·9호선) ←", new Vector3(-7f, Lv.B1 + 2.6f, 0f), 0.32f, Color.white, -90f);

            var exits = ProcBuilder.Group(st, "지상출구").transform;

            for (int i = 0; i < exitNorthX.Length; i++)
            {
                TransitKit.GroundExit(exits, i + 1, exitNorthX[i], 20f, +1f);
                Vector3 coverPos = new Vector3(exitNorthX[i], -0.75f, 25.5f);
                ProcBuilder.Box(exits, (i + 1) + "번출구_마감벽", coverPos, new Vector3(4.4f, 2.0f, 0.5f), mWall);
                //AutoPlaceBeacon($"출구_{i + 1}번_B1", new Vector3(exitNorthX[i], Lv.B1 + 2.5f, 16f), -1); // ★ 출구 앞 비콘
            }

            for (int i = 0; i < exitSouthX.Length; i++)
            {
                TransitKit.GroundExit(exits, i + 5, exitSouthX[i], -20f, -1f);
                Vector3 coverPos = new Vector3(exitSouthX[i], -0.75f, -25.5f);
                ProcBuilder.Box(exits, (i + 5) + "번출구_마감벽", coverPos, new Vector3(4.4f, 2.0f, 0.5f), mWall);
                //AutoPlaceBeacon($"출구_{i + 5}번_B1", new Vector3(exitSouthX[i], Lv.B1 + 2.5f, -16f), -1); // ★ 출구 앞 비콘
            }

            var corr = ProcBuilder.Group(st, "백화점연결통로").transform;
            for (int i = 0; i < mallCorrZ.Length; i++)
            {
                float cz = mallCorrZ[i];
                int no = 8 + i;
                var g = ProcBuilder.Group(corr, "통로_" + no + "번출구").transform;
                ProcBuilder.Slab(g, "바닥", Lv.B1, TransitKit.RectXZ(40f, 60f, cz - 2.5f, cz + 2.5f), mFloor);
                ProcBuilder.Slab(g, "천장", Lv.B1 + Lv.FloorH + Lv.Thick, TransitKit.RectXZ(40f, 60f, cz - 2.8f, cz + 2.8f), mCeil);
                ProcBuilder.WallX(g, "벽L", cz - 2.65f, 40f, 60f, Lv.B1, Lv.FloorH, mWall);
                ProcBuilder.WallX(g, "벽R", cz + 2.65f, 40f, 60f, Lv.B1, Lv.FloorH, mWall);
                ProcBuilder.Lamp(g, new Vector3(45f, Lv.B1 + 3.4f, cz), 10f, 1.8f);
                ProcBuilder.Lamp(g, new Vector3(55f, Lv.B1 + 3.4f, cz), 10f, 1.8f);
                ProcBuilder.KLabel(g, no + "번 출구 · 한빛백화점 방면", new Vector3(50f, Lv.B1 + 2.7f, cz), 0.3f, new Color(0.5f, 1f, 0.6f), 90f, "exit_" + no, "exit");
            }

            // =========================================================
            // B2 3호선 승강장 (층 인덱스: -2)
            var pa = ProcBuilder.Group(st, "B2_3호선승강장").transform;
            ProcBuilder.SlabWithHoles(pa, "바닥", Lv.PlatA, TransitKit.RectXZ(-70f, 50f, -5f, 5f), paHoles, mFloor);
            ProcBuilder.Slab(pa, "천장서", Lv.PlatA + Lv.FloorH + Lv.Thick, TransitKit.RectXZ(-70f, -40f, -9.5f, 9.5f), mCeil);
            ProcBuilder.Slab(pa, "천장동", Lv.PlatA + Lv.FloorH + Lv.Thick, TransitKit.RectXZ(40f, 50f, -9.5f, 9.5f), mCeil);

            TransitKit.TrackX(pa, "선로_상행", 7f, -100f, 80f, -13.4f);
            TransitKit.TrackX(pa, "선로_하행", -7f, -100f, 80f, -13.4f);
            TransitKit.ScreenDoorX(pa, "스크린도어_북", 5f, -70f, 50f, Lv.PlatA, mLine3);
            TransitKit.ScreenDoorX(pa, "스크린도어_남", -5f, -70f, 50f, Lv.PlatA, mLine3);
            foreach (float z in new[] { -4.2f, 4.2f })
                ProcBuilder.Box(pa, "안전선", new Vector3(-10f, Lv.PlatA + 0.02f, z), new Vector3(120f, 0.02f, 0.4f), mYellow, false);

            foreach (float z in new[] { -9.5f, 9.5f })
                ProcBuilder.WallX(pa, "궤도벽", z, -70f, 50f, -13.4f, 7.1f, mConc);
            BuildTunnelX(pa, "터널서", -100f, -70f, mDark, mConc);
            BuildTunnelX(pa, "터널동", 50f, 80f, mDark, mConc);

            /* ★ B2 3호선 승강장 플랫폼 분산 비콘 배치
            for (float bx = -60f; bx <= 40f; bx += 20f)
            {
                AutoPlaceBeacon($"3호선_승강장_{bx}_B2", new Vector3(bx, Lv.PlatA + 2.5f, 0f), -2);
            }*/

            for (float x = -66f; x <= 46f; x += 12f)
                ProcBuilder.Cyl(pa, "기둥", new Vector3(x, Lv.PlatA, 0f), 0.35f, (x >= -40f && x <= 40f) ? 5.7f : 4f, mConc);
            for (float x = -60f; x <= 45f; x += 15f)
                ProcBuilder.Lamp(pa, new Vector3(x, Lv.PlatA + 3.4f, 0f));

            ProcBuilder.KLabel(pa, "3호선 승강장 · 중앙공원 방면", new Vector3(-10f, Lv.PlatA + 3.2f, 3.5f), 0.4f, new Color(0.94f, 0.49f, 0.11f), 0f, "platform_line3_up", "platform");
            ProcBuilder.KLabel(pa, "3호선 승강장 · 남산호수 방면", new Vector3(-10f, Lv.PlatA + 3.2f, -3.5f), 0.4f, new Color(0.94f, 0.49f, 0.11f), 180f, "platform_line3_down", "platform");
            ProcBuilder.KLabel(pa, "환승 → 9호선 · 나가는 곳 ↑", new Vector3(-12f, Lv.PlatA + 2.4f, 0f), 0.3f, new Color(0.74f, 0.69f, 0.57f), 90f);
            foreach (float x in new[] { -50f, 30f })
                ProcBuilder.Box(pa, "벤치", new Vector3(x, Lv.PlatA + 0.25f, 0f), new Vector3(3f, 0.5f, 0.6f), mDark);

            var train = ProcBuilder.Group(pa, "열차_3호선").transform;
            var mTrain = ProcBuilder.Mat("train", new Color(0.85f, 0.86f, 0.88f), 0.4f, 0.6f);
            var mTrainWin = ProcBuilder.Mat("trainWin", new Color(0.1f, 0.12f, 0.15f), 0.2f, 0.8f);
            for (int car = 0; car < 3; car++)
            {
                float cx = -30f + car * 20.5f;
                ProcBuilder.Box(train, "차량" + car, new Vector3(cx, -11.5f, 7f), new Vector3(19.5f, 3.4f, 3.2f), mTrain);
                ProcBuilder.Box(train, "창문" + car, new Vector3(cx, -11.0f, 7f), new Vector3(19.6f, 1.0f, 3.25f), mTrainWin, false);
                ProcBuilder.Box(train, "라인" + car, new Vector3(cx, -12.6f, 7f), new Vector3(19.6f, 0.3f, 3.25f), mLine3, false);
            }
            var shuttle = train.gameObject.AddComponent<TrainShuttle>();
            shuttle.axis = Vector3.right; shuttle.fromOffset = -65f; shuttle.toOffset = 65f; shuttle.stopOffset = 0f;

            // =========================================================
            // B3 9호선 승강장 (층 인덱스: -3)
            var pb = ProcBuilder.Group(st, "B3_9호선승강장").transform;

            ProcBuilder.Slab(pb, "바닥", Lv.PlatB, TransitKit.RectXZ(-32.5f, -2.5f, -60f, 60f), mFloor);
            ProcBuilder.SlabWithHoles(pb, "천장", Lv.PlatB + Lv.FloorH + Lv.Thick, TransitKit.RectXZ(-36.5f, 1.5f, -60f, 60f), pbCeilHoles, mCeil);

            TransitKit.TrackZ(pb, "선로_동", -0.5f, -90f, 90f, -19.4f);
            TransitKit.TrackZ(pb, "선로_서", -34.5f, -90f, 90f, -19.4f);
            TransitKit.ScreenDoorZ(pb, "스크린도어_동", -2.5f, -60f, 60f, Lv.PlatB, mLine9);
            TransitKit.ScreenDoorZ(pb, "스크린도어_서", -32.5f, -60f, 60f, Lv.PlatB, mLine9);

            foreach (float x in new[] { -31.7f, -3.3f })
                ProcBuilder.Box(pb, "안전선", new Vector3(x, Lv.PlatB + 0.02f, 0f), new Vector3(0.4f, 0.02f, 120f), mYellow, false);

            foreach (float x in new[] { -36.5f, 1.5f })
                ProcBuilder.WallZ(pb, "궤도벽", x, -60f, 60f, -19.4f, 5.7f, mConc);

            BuildTunnelZ(pb, "터널남", -90f, -60f, mDark, mConc);
            BuildTunnelZ(pb, "터널북", 60f, 90f, mDark, mConc);

            /* ★ B3 9호선 승강장 플랫폼 분산 비콘 배치
            for (float bz = -50f; bz <= 50f; bz += 20f)
            {
                AutoPlaceBeacon($"9호선_승강장_{bz}_B3", new Vector3(-17.5f, Lv.PlatB + 2.5f, bz), -3);
            }*/

            for (float z = -56f; z <= 56f; z += 12f)
                ProcBuilder.Cyl(pb, "기둥", new Vector3(-17.5f, Lv.PlatB, z), 0.35f, Lv.FloorH, mConc);
            for (float z = -50f; z <= 55f; z += 15f)
                ProcBuilder.Lamp(pb, new Vector3(-17.5f, Lv.PlatB + 3.4f, z));

            ProcBuilder.KLabel(pb, "9호선 승강장 · 은하대교 방면", new Vector3(-14f, Lv.PlatB + 3.2f, 20f), 0.4f, new Color(0.74f, 0.69f, 0.57f), 90f, "platform_line9_up", "platform");
            ProcBuilder.KLabel(pb, "9호선 승강장 · 별빛터미널 방면", new Vector3(-21f, Lv.PlatB + 3.2f, -20f), 0.4f, new Color(0.74f, 0.69f, 0.57f), -90f, "platform_line9_down", "platform");
            ProcBuilder.KLabel(pb, "환승 → 3호선 · 나가는 곳 ↑", new Vector3(-17.5f, Lv.PlatB + 2.4f, 8f), 0.3f, new Color(0.94f, 0.49f, 0.11f), 0f);
            foreach (float z in new[] { -40f, 40f })
                ProcBuilder.Box(pb, "벤치", new Vector3(-17.5f, Lv.PlatB + 0.25f, z), new Vector3(0.6f, 0.5f, 3f), mDark);

            // 🚀 배칭 최적화 호출
            ProcBuilder.OptimizeBatching(st.gameObject);
        }

        // ==============================================================================
        // ★ 비콘 자동 매핑 유틸리티 (명세 준수, 명시적 층 인덱스 사용)
        // ==============================================================================
        /*static void AutoPlaceBeacon(string nameId, Vector3 pos, int floorIdx)
        {
            var beaconsRoot = GameObject.Find("Beacons");
            if (beaconsRoot == null)
            {
                beaconsRoot = new GameObject("Beacons");
                if (beaconsRoot.GetComponent<BeaconManager>() == null)
                    beaconsRoot.AddComponent<BeaconManager>();
            }

            string groupName = BeaconManager.FloorGroupName(floorIdx);
            var floorGroup = beaconsRoot.transform.Find(groupName);
            if (floorGroup == null)
            {
                floorGroup = new GameObject(groupName).transform;
                floorGroup.SetParent(beaconsRoot.transform, false);
            }

            // 역사는 BCN-S 프리픽스를 사용하여 백화점(BCN-)과 충돌 방지
            string bcnId = $"BCN-S{stationBeaconCounter:000}";
            var go = new GameObject($"{bcnId}_{nameId}");
            go.transform.SetParent(floorGroup, false);
            go.transform.position = pos;

            var beacon = go.AddComponent<BleBeacon>();
            beacon.beaconId = bcnId;
            beacon.minor = stationBeaconCounter;
            beacon.floorIndex = floorIdx;
            beacon.isInElevator = false;
            beacon.txPowerDbm = 0f;
            beacon.measuredPowerAt1m = -59f;
            beacon.advertisingIntervalMs = 100;

            stationBeaconCounter++;
        }*/

        static void BuildTunnelX(Transform parent, string name, float xFrom, float xTo, Material mDark, Material mConc)
        {
            var g = ProcBuilder.Group(parent, name).transform;
            foreach (float z in new[] { -9.5f, 9.5f })
                ProcBuilder.WallX(g, "벽", z, xFrom, xTo, -13.4f, 5.7f, mConc);
            ProcBuilder.Slab(g, "천장", -7.7f, TransitKit.RectXZ(xFrom, xTo, -9.5f, 9.5f), mConc);
            float capX = Mathf.Abs(xFrom) > Mathf.Abs(xTo) ? xFrom : xTo;
            ProcBuilder.WallZ(g, "막장", capX, -9.5f, 9.5f, -13.4f, 5.7f, mDark);
        }

        static void BuildTunnelZ(Transform parent, string name, float zFrom, float zTo, Material mDark, Material mConc)
        {
            var g = ProcBuilder.Group(parent, name).transform;
            foreach (float x in new[] { -36.5f, 1.5f })
                ProcBuilder.WallZ(g, "벽", x, zFrom, zTo, -19.4f, 5.7f, mConc);

            ProcBuilder.Slab(g, "천장", -13.7f, TransitKit.RectXZ(-36.5f, 1.5f, zFrom, zTo), mConc);

            float capZ = Mathf.Abs(zFrom) > Mathf.Abs(zTo) ? zFrom : zTo;
            ProcBuilder.WallX(g, "막장", capZ, -36.5f, 1.5f, -19.4f, 5.7f, mDark);
        }
    }
}
