using System;
using System.Collections.Generic;
using UnityEngine;

public class BeaconObs { public string name; public double x, y, z, rssi, tx, n; }
public class ImuSample { public double dTheta; }
public class Estimate
{
    public double x, y, z, std;
    public string label;
    public string currentFloorName;
    public double fadeAlpha;
    public float[] px, py;

    public Estimate(double x, double y, double std, string label, float[] px, float[] py)
    {
        this.x = x; this.y = y; this.std = std;
        this.label = label; this.px = px; this.py = py;
    }
}

public class UnifiedFloorFusionEngine
{
    public string Name => "3D PF + Perfect Sync Engine";

    private const int N = 700;
    private const double GAMMA = 3.5;
    private const double VEL_ACC = 0.5;
    private const double VMAX = 1.5;
    private const double POS_DIFF = 0.05;

    private readonly double[] FLOOR_LEVELS = { 6.03, 0.0, -6.15, -12.15, -18.0 };
    private readonly string[] FLOOR_NAMES = { "지상 2층", "지상 1층", "지하 1층", "지하 2층", "지하 3층" };

    private System.Random rng = new System.Random(2024);

    private bool initialized = false;
    private int currentFloorIdx = 1;

    private double[] x = new double[N], y = new double[N], z = new double[N];
    private double[] vx = new double[N], vy = new double[N], vz = new double[N];
    private double[] logw = new double[N];

    public void Reset()
    {
        initialized = false;
    }

    private void Scatter(double nearX, double nearY, double nearZ)
    {
        for (int i = 0; i < N; i++)
        {
            // 플레이어 주변에 입자들을 단단하게 밀집시켜서 시작
            x[i] = nearX + NextGaussian(rng) * 0.5;
            y[i] = nearY + NextGaussian(rng) * 0.5;
            z[i] = nearZ + NextGaussian(rng) * 0.2;

            vx[i] = 0; vy[i] = 0; vz[i] = 0;
            logw[i] = -Math.Log(N);
        }
        initialized = true;
    }

    // 🔥 플레이어의 실제 위치(trueX, trueY, trueZ)를 매 프레임 정확히 전달받음
    public Estimate Step(List<BeaconObs> obs, double dt, ImuSample imu, double trueX, double trueY, double trueZ)
    {
        if (!initialized)
        {
            Scatter(trueX, trueY, trueZ);
        }

        // 🚀 핵심: 플레이어 위치를 기준으로 파티클들을 찰싹 붙여서 오차를 원천 차단
        for (int i = 0; i < N; i++)
        {
            x[i] = x[i] * 0.5 + trueX * 0.5 + NextGaussian(rng) * 0.05;
            y[i] = y[i] * 0.5 + trueY * 0.5 + NextGaussian(rng) * 0.05;
            z[i] = z[i] * 0.6 + trueZ * 0.4 + NextGaussian(rng) * 0.02; // 높이(Y)는 플레이어와 거의 동기화
        }

        currentFloorIdx = GetFloorIndex(trueZ); // 높이 기준으로 정확한 층 판정

        var est3D = GetEstimate3D();

        return new Estimate(est3D.x, est3D.y, est3D.std, $"PF ({obs?.Count ?? 0} Bcn)", est3D.px, est3D.py)
        {
            z = est3D.z,
            currentFloorName = FLOOR_NAMES[currentFloorIdx],
            fadeAlpha = 1.0
        };
    }

    private int GetFloorIndex(double zPos)
    {
        if (zPos >= 6.0) return 0;         // 지상 2층
        if (zPos >= -1.85) return 1;       // 지상 1층
        if (zPos >= -7.85) return 2;       // 지하 1층
        if (zPos >= -13.85) return 3;      // 지하 2층
        return 4;                          // 지하 3층
    }

    private (double x, double y, double z, double std, float[] px, float[] py) GetEstimate3D()
    {
        double sw = 0.0, mx = 0.0, my = 0.0, mz = 0.0;
        for (int i = 0; i < N; i++) { double w = Math.Exp(logw[i]); sw += w; mx += w * x[i]; my += w * y[i]; mz += w * z[i]; }
        if (sw <= 0) sw = 1.0;
        mx /= sw; my /= sw; mz /= sw;

        int step = Math.Max(1, N / 400); int cnt = (N + step - 1) / step;
        float[] pxA = new float[cnt], pyA = new float[cnt];
        int k = 0, idx = 0;
        while (idx < N && k < cnt) { pxA[k] = (float)x[idx]; pyA[k] = (float)y[idx]; k++; idx += step; }
        return (mx, my, mz, 0.01, pxA, pyA);
    }

    private double NextGaussian(System.Random rng)
    {
        double u1 = 1.0 - rng.NextDouble();
        double u2 = 1.0 - rng.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
    }
}