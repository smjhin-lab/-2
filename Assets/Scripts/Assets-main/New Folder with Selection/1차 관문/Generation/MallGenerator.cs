using System.Collections.Generic;
using UnityEngine;
//using IndoorSim.Beacons; // ★ 비콘 네임스페이스 추가

namespace IndoorSim.Gen
{
    /// <summary>
    /// 가상 백화점 "한빛백화점" 생성기 (x 60..120, z -30..30).
    /// </summary>
    public static class MallGenerator
    {
        static readonly string[] ShopsB1 = { "그린마트", "하늘베이커리", "분식천국", "한빛반찬가게", "커피빈스", "온달과일", "우가정육점", "바다수산", "푸드코트", "델리코너", "꽃집 피오레", "약국 온누리" };
        static readonly string[] Shops1F = { "루미에르 화장품", "오로라 향수", "가온 주얼리", "메종 핸드백", "스텔라 슈즈", "안경나라", "온타임 시계", "실크스카프", "선글라스랩", "에떼르 명품관", "벨르 모자", "라벤더 란제리" };
        static readonly string[] Shops2F = { "소반 한식당", "만리 중식당", "스시로엔", "알로 파스타", "카페 루나", "달빛서점", "모리문구", "키즈랜드", "볼트가전", "홈앤리빙", "무드 헤어살롱", "봄 네일아트" };

        // ★ 비콘 식별 번호 (BCN-001 부터 순차 부여)
        static int beaconCounter = 1;

        public static void Generate(Transform root)
        {
            beaconCounter = 1; // 생성 시 카운터 초기화
            var mall = ProcBuilder.Group(root, "Mall_한빛백화점").transform;

            // 캐시 매니저 적용
            var mFloor = ProcBuilder.LoadMat("M_StationFloor");
            var mWall = ProcBuilder.LoadMat("M_StationWall");
            var mCeil = ProcBuilder.LoadMat("M_StationCeil");
            var mAccent = ProcBuilder.LoadMat("M_MallAccent");
            var mSign = ProcBuilder.LoadMat("M_ShopSign");
            var mStair = ProcBuilder.LoadMat("M_StairStone");
            var mDark = ProcBuilder.LoadMat("M_DarkMetal");
            var glass = ProcBuilder.Glass();

            Rect footprint = TransitKit.RectXZ(60f, 120f, -30f, 30f);
            Rect atrium = TransitKit.RectXZ(82f, 98f, -3.6f, 3.6f);

            var holes1F = new List<Rect> { atrium };
            var holes2F = new List<Rect> { atrium };

            // ---- 수직 동선 ----
            var circ = ProcBuilder.Group(mall, "수직동선").transform;

            holes1F.Add(TransitKit.Escalator(circ, "에스컬레이터_B1-1F_상행", new Vector3(89f, Lv.B1, -4.8f), Vector3.right, 6f, true));
            holes1F.Add(TransitKit.Escalator(circ, "에스컬레이터_B1-1F_하행", new Vector3(89f, Lv.B1, -6.4f), Vector3.right, 6f, false));
            holes2F.Add(TransitKit.Escalator(circ, "에스컬레이터_1F-2F_상행", new Vector3(91f, 0f, 4.8f), Vector3.left, 6f, true));
            holes2F.Add(TransitKit.Escalator(circ, "에스컬레이터_1F-2F_하행", new Vector3(91f, 0f, 6.4f), Vector3.left, 6f, false));

            // ★ 에스컬레이터 주변 주요 동선 비콘 자동 배치 (층별)
            /*AutoPlaceBeacon("에스컬레이터_B1_홀", new Vector3(85f, Lv.B1 + 2.5f, -5.5f), Lv.B1);
            AutoPlaceBeacon("에스컬레이터_1F_홀", new Vector3(95f, 0f + 2.5f, -5.5f), 0f);
            AutoPlaceBeacon("에스컬레이터_2F_홀", new Vector3(85f, 6f + 2.5f, 5.5f), 6f);*/

            var fenceFrame = mDark;

            TransitKit.CreateGlassFence(circ, "백화점_에스컬1F_좌측펜스", new Vector3(89f, 0f, -3.8f), 9.5f, true, glass, fenceFrame);
            TransitKit.CreateGlassFence(circ, "백화점_에스컬1F_우측펜스", new Vector3(89f, 0f, -7.4f), 9.5f, true, glass, fenceFrame);
            TransitKit.CreateGlassFence(circ, "백화점_에스컬1F_후면펜스", new Vector3(89f, 0f, -7.4f), 3.6f, false, glass, fenceFrame);

            TransitKit.CreateGlassFence(circ, "백화점_에스컬2F_좌측펜스", new Vector3(81.5f, 6f, 7.4f), 9.5f, true, glass, fenceFrame);
            TransitKit.CreateGlassFence(circ, "백화점_에스컬2F_우측펜스", new Vector3(81.5f, 6f, 3.8f), 9.5f, true, glass, fenceFrame);
            TransitKit.CreateGlassFence(circ, "백화점_에스컬2F_후면펜스", new Vector3(91f, 6f, 3.8f), 3.6f, false, glass, fenceFrame);

            TransitKit.CreateGlassFence(circ, "비상계단1F_후면펜스", new Vector3(112.7f, 0f, 19f), 2.6f, true, glass, fenceFrame);
            TransitKit.CreateGlassFence(circ, "비상계단1F_좌측펜스", new Vector3(112.7f, 0f, 19f), 8.8f, false, glass, fenceFrame);
            TransitKit.CreateGlassFence(circ, "비상계단1F_우측펜스", new Vector3(115.3f, 0f, 19f), 8.8f, false, glass, fenceFrame);

            TransitKit.CreateGlassFence(circ, "비상계단2F_후면펜스", new Vector3(115.3f, 6f, 28f), 2.6f, true, glass, fenceFrame);
            TransitKit.CreateGlassFence(circ, "비상계단2F_좌측펜스", new Vector3(115.3f, 6f, 28f), -8.8f, false, glass, fenceFrame);
            TransitKit.CreateGlassFence(circ, "비상계단2F_우측펜스", new Vector3(117.9f, 6f, 28f), -8.8f, false, glass, fenceFrame);

            // 캐시 매니저 적용 (대체 재질 폴백 포함)
            var mCeilCover = ProcBuilder.LoadMat("M_Concrete", false) ?? mWall;
            float ceilHeight = -0.4f;
            Vector3 coverPos = new Vector3(91f, ceilHeight, 6.6f);
            ProcBuilder.Box(circ, "쇼핑몰_에스컬_천장가림막", coverPos, new Vector3(5.0f, 0.5f, 6.0f), mCeilCover);

            var elvRect = TransitKit.ElevatorShaft(circ, "엘리베이터_백화점", new Vector2(108f, 0f), new[] { Lv.B1, 0f, 6f }, Vector3.left);
            holes1F.Add(elvRect); holes2F.Add(elvRect);

            holes1F.Add(ProcBuilder.Stairs(circ, "비상계단_B1-1F", new Vector3(114f, Lv.B1, 19f), Vector3.forward, 6f, 2.4f, mStair));
            holes2F.Add(ProcBuilder.Stairs(circ, "비상계단_1F-2F", new Vector3(116.6f, 0f, 28f), Vector3.back, 6f, 2.4f, mStair));
            ProcBuilder.KLabel(circ, "비상계단", new Vector3(114f, Lv.B1 + 2.5f, 17f), 0.3f, new Color(0.5f, 1f, 0.6f), 180f, "mall_stairs", "facility");

            /* ★ 엘리베이터 앞 대기홀 비콘 자동 배치 (층별)
            AutoPlaceBeacon("엘리베이터홀_B1", new Vector3(104f, Lv.B1 + 2.5f, 0f), Lv.B1);
            AutoPlaceBeacon("엘리베이터홀_1F", new Vector3(104f, 0f + 2.5f, 0f), 0f);
            AutoPlaceBeacon("엘리베이터홀_2F", new Vector3(104f, 6f + 2.5f, 0f), 6f);*/

            // ---- 슬래브 ----
            var sl = ProcBuilder.Group(mall, "슬래브").transform;
            ProcBuilder.Slab(sl, "B1바닥", Lv.B1, footprint, mFloor);
            ProcBuilder.SlabWithHoles(sl, "1F바닥", 0f, footprint, holes1F, mFloor);
            ProcBuilder.SlabWithHoles(sl, "2F바닥", 6f, footprint, holes2F, mFloor);
            ProcBuilder.SlabWithHoles(sl, "지붕", 10.3f, footprint, new[] { atrium }, mWall);
            ProcBuilder.Slab(sl, "천창유리", 10.28f, TransitKit.RectXZ(81.5f, 98.5f, -4.1f, 4.1f), glass, 0.15f);

            // ---- 아트리움 난간 ----
            var rail = ProcBuilder.Group(mall, "아트리움난간").transform;
            foreach (float fy in new[] { 0f, 6f })
            {
                ProcBuilder.WallX(rail, "난간N", atrium.yMax + 0.1f, atrium.xMin - 0.2f, atrium.xMax + 0.2f, fy, 1.1f, glass, 0.08f);
                ProcBuilder.WallX(rail, "난간S", atrium.yMin - 0.1f, atrium.xMin - 0.2f, atrium.xMax + 0.2f, fy, 1.1f, glass, 0.08f);
                ProcBuilder.WallZ(rail, "난간W", atrium.xMin - 0.1f, atrium.yMin, atrium.yMax, fy, 1.1f, glass, 0.08f);
                ProcBuilder.WallZ(rail, "난간E", atrium.xMax + 0.1f, atrium.yMin, atrium.yMax, fy, 1.1f, glass, 0.08f);
            }
            ProcBuilder.WallZ(rail, "난간_에스컬B1뒤", 88.3f, -7.6f, -3.6f, 0f, 1.1f, glass, 0.08f);
            ProcBuilder.WallZ(rail, "난간_에스컬1F뒤", 91.7f, 3.6f, 7.6f, 6f, 1.1f, glass, 0.08f);

            // ---- 외벽 ----
            var walls = ProcBuilder.Group(mall, "외벽").transform;
            ProcBuilder.WallZ(walls, "B1_서벽", 60f, -30f, 30f, Lv.B1, 6f, mWall, 0.35f, (-14.5f, -9.5f), (-2.5f, 2.5f), (9.5f, 14.5f));
            ProcBuilder.WallZ(walls, "B1_동벽", 120f, -30f, 30f, Lv.B1, 6f, mWall, 0.35f);
            ProcBuilder.WallX(walls, "B1_북벽", 30f, 60f, 120f, Lv.B1, 6f, mWall, 0.35f);
            ProcBuilder.WallX(walls, "B1_남벽", -30f, 60f, 120f, Lv.B1, 6f, mWall, 0.35f);
            ProcBuilder.WallZ(walls, "1F_서벽", 60f, -30f, 30f, 0f, 6f, mWall, 0.35f, (-3f, 3f));
            ProcBuilder.WallZ(walls, "1F_동벽", 120f, -30f, 30f, 0f, 6f, mWall, 0.35f);
            ProcBuilder.WallX(walls, "1F_북벽", 30f, 60f, 120f, 0f, 6f, mWall, 0.35f);
            ProcBuilder.WallX(walls, "1F_남벽", -30f, 60f, 120f, 0f, 6f, mWall, 0.35f, (86f, 92f));
            ProcBuilder.WallZ(walls, "2F_서벽", 60f, -30f, 30f, 6f, 4.3f, mWall, 0.35f);
            ProcBuilder.WallZ(walls, "2F_동벽", 120f, -30f, 30f, 6f, 4.3f, mWall, 0.35f);
            ProcBuilder.WallX(walls, "2F_북벽", 30f, 60f, 120f, 6f, 4.3f, mWall, 0.35f);
            ProcBuilder.WallX(walls, "2F_남벽", -30f, 60f, 120f, 6f, 4.3f, mWall, 0.35f);

            ProcBuilder.Slab(walls, "정문캐노피", 4.6f, TransitKit.RectXZ(56.5f, 60.5f, -4.5f, 4.5f), mAccent, 0.25f);
            ProcBuilder.KLabel(walls, "한빛백화점", new Vector3(59.3f, 8f, 0f), 1.1f, new Color(1f, 0.9f, 0.7f), 90f, "mall_hanbit", "mall");
            ProcBuilder.KLabel(walls, "정문", new Vector3(59.3f, 3.6f, 0f), 0.4f, Color.white, 90f, "mall_main_entrance", "entrance");
            ProcBuilder.KLabel(walls, "남문", new Vector3(89f, 3.6f, -29.3f), 0.4f, Color.white, 180f, "mall_south_entrance", "entrance");

            // ---- 층별 인테리어 ----
            BuildFloor(mall, "B1_식품관", Lv.B1, 5.7f, ShopsB1, "b1", mSign, mAccent, mCeil);
            BuildFloor(mall, "1F_패션뷰티", 0f, 5.7f, Shops1F, "1f", mSign, mAccent, mCeil);
            BuildFloor(mall, "2F_식당가리빙", 6f, 3.7f, Shops2F, "2f", mSign, mAccent, mCeil);

            ProcBuilder.KLabel(mall, "B1 식품관 · 1F 패션/뷰티 · 2F 식당가/리빙", new Vector3(78f, Lv.B1 + 2.8f, 0f), 0.3f, Color.white, 90f, "mall_directory_b1", "facility");
            ProcBuilder.KLabel(mall, "B1 식품관 · 1F 패션/뷰티 · 2F 식당가/리빙", new Vector3(78f, 2.8f, 0f), 0.3f, Color.white, 90f, "mall_directory_1f", "facility");
            ProcBuilder.KLabel(mall, "B1 식품관 · 1F 패션/뷰티 · 2F 식당가/리빙", new Vector3(78f, 8.5f, 0f), 0.3f, Color.white, 90f, "mall_directory_2f", "facility");

            ProcBuilder.GroundHoles.Add(TransitKit.RectXZ(59.8f, 120.2f, -30.2f, 30.2f));

            // 🚀 배칭 최적화 호출
            ProcBuilder.OptimizeBatching(mall.gameObject);
        }

        static void BuildFloor(Transform mall, string name, float floorY, float wallH, string[] shopNames, string floorId, Material mSign, Material mAccent, Material mCeil)
        {
            var fl = ProcBuilder.Group(mall, name).transform;
            int idx = 0;

            for (int i = 0; i < 4; i++)
            {
                Rect r = TransitKit.RectXZ(62f + i * 11f, 62f + (i + 1) * 11f, 22f, 29.7f);
                ApplyShop(fl, shopNames[idx], "shop_" + floorId + "_" + (idx + 1), floorY, wallH, r, 0, mSign, mAccent);
                idx++;
            }
            for (int i = 0; i < 5; i++)
            {
                Rect r = TransitKit.RectXZ(62f + i * 11f, 62f + (i + 1) * 11f, -29.7f, -22f);
                ApplyShop(fl, shopNames[idx], "shop_" + floorId + "_" + (idx + 1), floorY, wallH, r, 1, mSign, mAccent);
                idx++;
            }
            for (int i = 0; i < 3; i++)
            {
                Rect r = TransitKit.RectXZ(112f, 119.7f, -18f + i * 10f, -8f + i * 10f);
                ApplyShop(fl, shopNames[idx], "shop_" + floorId + "_" + (idx + 1), floorY, wallH, r, 2, mSign, mAccent);
                idx++;
            }

            var mConc = ProcBuilder.LoadMat("M_RealConcrete");
            foreach (float x in new[] { 70f, 104f })
                foreach (float z in new[] { -16f, 16f })
                    ProcBuilder.Cyl(fl, "기둥", new Vector3(x, floorY, z), 0.4f, wallH, mConc);
            foreach (float x in new[] { 66f, 86f, 106f })
                foreach (float z in new[] { -20f, 0f, 20f })
                    ProcBuilder.Lamp(fl, new Vector3(x, floorY + wallH - 0.3f, z), 16f, 2.4f);
        }

        static void ApplyShop(Transform parent, string shopName, string placeId, float floorY, float wallH, Rect r, int facing, Material mSign, Material mAccent)
        {
            string matName = "M_" + shopName.Replace(" ", "");
            var mCustomImg = ProcBuilder.LoadMat(matName, false);

            if (mCustomImg != null)
            {
                MakeImageShop(parent, shopName, floorY, wallH, r, facing, mCustomImg);
            }
            else
            {
                MakeShop(parent, shopName, placeId, floorY, wallH, r, facing, mSign, mAccent);
            }
        }

        static void MakeImageShop(Transform parent, string shopName, float floorY, float wallH, Rect r, int facing, Material mImage)
        {
            var g = ProcBuilder.Group(parent, "상점_" + shopName).transform;
            Vector3 wallPos;
            Vector3 wallSize;

            if (facing == 0) // 북측
            {
                wallPos = new Vector3(r.center.x, floorY + (wallH / 2f), r.yMin);
                wallSize = new Vector3(r.width, wallH, 0.2f);
            }
            else if (facing == 1) // 남측
            {
                wallPos = new Vector3(r.center.x, floorY + (wallH / 2f), r.yMax);
                wallSize = new Vector3(r.width, wallH, 0.2f);
            }
            else // 동측
            {
                wallPos = new Vector3(r.xMin, floorY + (wallH / 2f), r.center.y);
                wallSize = new Vector3(0.2f, wallH, r.height);
            }

            ProcBuilder.Box(g, shopName + "_통외벽", wallPos, wallSize, mImage);

            // ★ 매장 내부 비콘 생성
            //AutoPlaceBeacon("Shop_" + shopName, wallPos + Vector3.up * 2.5f, floorY); 
        }

        static void MakeShop(Transform parent, string shopName, string placeId, float floorY, float wallH, Rect r, int facing, Material mSign, Material mAccent)
        {
            var g = ProcBuilder.Group(parent, "상점_" + shopName).transform;
            var mPart = ProcBuilder.LoadMat("M_ShopPartition");
            var mShelf = ProcBuilder.LoadMat("M_ShopShelf");
            var mFloorAcc = ProcBuilder.LoadMat("M_ShopFloorAcc");

            if (facing == 0 || facing == 1)
            {
                ProcBuilder.WallZ(g, "칸막이L", r.xMin, r.yMin, r.yMax, floorY, wallH, mPart, 0.2f);
                ProcBuilder.WallZ(g, "칸막이R", r.xMax, r.yMin, r.yMax, floorY, wallH, mPart, 0.2f);
            }
            else
            {
                ProcBuilder.WallX(g, "칸막이L", r.yMin, r.xMin, r.xMax, floorY, wallH, mPart, 0.2f);
                ProcBuilder.WallX(g, "칸막이R", r.yMax, r.xMin, r.xMax, floorY, wallH, mPart, 0.2f);
            }

            Vector3 frontCenter; float yRot; Vector3 inward;
            if (facing == 0) { frontCenter = new Vector3(r.center.x, 0, r.yMin); yRot = 0f; inward = Vector3.forward; }
            else if (facing == 1) { frontCenter = new Vector3(r.center.x, 0, r.yMax); yRot = 180f; inward = Vector3.back; }
            else { frontCenter = new Vector3(r.xMin, 0, r.center.y); yRot = 90f; inward = Vector3.right; }

            float frontLen = (facing == 2) ? r.height : r.width;

            Vector3 signSize = (facing == 2) ? new Vector3(0.15f, 0.8f, frontLen) : new Vector3(frontLen, 0.8f, 0.15f);
            ProcBuilder.Box(g, "간판", frontCenter + Vector3.up * (floorY + 3.1f), signSize, mSign, false);
            ProcBuilder.KLabel(g, shopName, frontCenter + Vector3.up * (floorY + 3.1f) - inward * 0.15f, 0.34f, new Color(1f, 0.92f, 0.6f), yRot, placeId, "shop");
            ProcBuilder.Box(g, "어닝", frontCenter + Vector3.up * (floorY + 2.65f) - inward * 0.1f,
                (facing == 2) ? new Vector3(0.5f, 0.08f, frontLen) : new Vector3(frontLen, 0.08f, 0.5f), mAccent, false);

            ProcBuilder.Slab(g, "바닥악센트", floorY + 0.02f, Rect.MinMaxRect(r.xMin + 0.3f, r.yMin + 0.3f, r.xMax - 0.3f, r.yMax - 0.3f), mFloorAcc, 0.02f, false);
            ProcBuilder.Box(g, "카운터", frontCenter + inward * 1.5f + Vector3.up * (floorY + 0.55f),
                (facing == 2) ? new Vector3(0.6f, 1.1f, 2f) : new Vector3(2f, 1.1f, 0.6f), mShelf);
            Vector3 backCenter = frontCenter + inward * ((facing == 2 ? r.width : r.height) - 0.8f);
            ProcBuilder.Box(g, "진열대", backCenter + Vector3.up * (floorY + 0.9f),
                (facing == 2) ? new Vector3(0.7f, 1.8f, frontLen * 0.6f) : new Vector3(frontLen * 0.6f, 1.8f, 0.7f), mShelf);

            /* ★ 매장 내부 카운터 위쪽 비콘 생성
            AutoPlaceBeacon("Shop_" + shopName, frontCenter + inward * 1.5f + Vector3.up * (floorY + 2.5f), floorY);
        }

        // ==============================================================================
        // ★ 비콘 자동 매핑 유틸리티 (BeaconManager.cs 명세 완벽 준수)
        // ==============================================================================
        static void AutoPlaceBeacon(string nameId, Vector3 pos, float floorY)
        {
            // 1. Y좌표를 분석해 정확한 층 인덱스(floorIndex) 도출
            int floorIdx = 0;
            if (floorY < -1f) floorIdx = -1;       // 지하 1층
            else if (floorY < 1f) floorIdx = 1;    // 지상 1층
            else floorIdx = 2;                     // 지상 2층

            // 2. Beacons 루트 찾기 및 층 그룹화
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

            // 3. 비콘 생성 및 속성 입력
            string bcnId = $"BCN-{beaconCounter:000}";
            var go = new GameObject($"{bcnId}_{nameId}");
            go.transform.SetParent(floorGroup, false);
            go.transform.position = pos;

            var beacon = go.AddComponent<BleBeacon>();
            beacon.beaconId = bcnId;
            beacon.minor = beaconCounter;
            beacon.floorIndex = floorIdx;
            beacon.isInElevator = false; // 기본 구조물은 엘리베이터 외부이므로 false
            beacon.txPowerDbm = 0f;
            beacon.measuredPowerAt1m = -59f;
            beacon.advertisingIntervalMs = 100;

            beaconCounter++;
        }*/
        }
    }
}
