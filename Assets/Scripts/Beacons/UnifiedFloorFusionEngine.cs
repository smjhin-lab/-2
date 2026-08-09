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
    public string Name => "Clean Hybrid Particle Filter";

    private const int N = 500; 
    private const double GAMMA = 3.5;
    private const double VEL_ACC = 0.5;
    private const double VMAX = 1.5;
    private const double POS_DIFF = 0.1;

    private readonly double[] FLOOR_LEVELS = { 6.03, 0.0, -6.15, -12.15, -18.0 };
    private readonly string[] FLOOR_NAMES = { "지상 2층", "지상 1층", "지하 1층", "지하 2층", "지하 3층" };

    private System.Random rng = new System.Random(2024);
    private bool initialized = false;
    private int currentFloorIdx = 1; 

    private double[] x = new double[N], y = new double[N], z = new double[N];
    private double[] vx = new double[N], vy = new double[N], vz = new double[N];
    private double[] logw = new double[N];

    public void Reset() { initialized = false; }

    private int GetFloorIndex(double zPos)
    {
        if (zPos >= 3.0) return 0;         
        if (zPos >= -3.0) return 1;       
        if (zPos >= -9.0) return 2;       
        if (zPos >= -15.0) return 3;      
        return 4;                          
    }

    public string GetFloorNameFromZ(double zPos) {
        return FLOOR_NAMES[GetFloorIndex(zPos)];
    }

    // 🚀 유니티 좌표로 파티클들을 강제로 묶어두는 함수 (튐 방지용)
    public void ForcePosition(double px, double py, double pz)
    {
        currentFloorIdx = GetFloorIndex(pz);
        double floorHeight = FLOOR_LEVELS[currentFloorIdx];
        
        for (int i = 0; i < N; i++)
        {
            x[i] = px + NextGaussian(rng) * 0.1;
            y[i] = py + NextGaussian(rng) * 0.1;
            z[i] = floorHeight; 
            vx[i] = 0; vy[i] = 0;
            logw[i] = -Math.Log(N);
        }
        initialized = true;
    }

    public Estimate Step(List<BeaconObs> obs, double dt, ImuSample imu)
    {
        if (!initialized) { return null; }
        dt = Math.Max(0.01, Math.Min(dt, 0.5));
        
        if (obs != null && obs.Count > 0)
        {
            currentFloorIdx = GetFloorIndex(obs[0].z);
            double currentFloorHeight = FLOOR_LEVELS[currentFloorIdx]; 
            for (int i = 0; i < N; i++) z[i] = currentFloorHeight;
        }

        for (int i = 0; i < N; i++)
        {
            vx[i] += NextGaussian(rng) * VEL_ACC * dt;
            vy[i] += NextGaussian(rng) * VEL_ACC * dt;
            double sp = Math.Sqrt(vx[i] * vx[i] + vy[i] * vy[i]);
            if (sp > VMAX) { vx[i] *= VMAX / sp; vy[i] *= VMAX / sp; }
            x[i] += vx[i] * dt + NextGaussian(rng) * POS_DIFF;
            y[i] += vy[i] * dt + NextGaussian(rng) * POS_DIFF;
            logw[i] = 0.0;
        }

        foreach (var o in obs)
        {
            double d = Math.Pow(10.0, (o.tx - o.rssi) / (10.0 * o.n)); 
            double obsGamma = GAMMA * (1.0 + 0.1 * d);
            for (int i = 0; i < N; i++)
            {
                double dx = x[i] - o.x; double dy = y[i] - o.y;
                double exp = Math.Sqrt(dx * dx + dy * dy); 
                double r = (d - exp) / obsGamma;
                logw[i] += -Math.Log(1.0 + r * r); 
            }
        }

        double m = double.NegativeInfinity;
        for (int i = 0; i < N; i++) if (logw[i] > m) m = logw[i];
        if (!double.IsInfinity(m) && !double.IsNaN(m))
        {
            double sum = 0.0;
            for (int i = 0; i < N; i++) { logw[i] = Math.Exp(logw[i] - m); sum += logw[i]; }
            if (sum > 0)
            {
                for (int i = 0; i < N; i++) logw[i] = Math.Log(logw[i] / sum + 1e-300);
                
                double[] nx = new double[N], ny = new double[N];
                double[] resampleCum = new double[N];
                double acc = 0.0;
                for (int i = 0; i < N; i++) { acc += Math.Exp(logw[i]); resampleCum[i] = acc; }
                resampleCum[N - 1] = 1.0;
                double start = rng.NextDouble() / N;
                int j = 0;
                for (int i = 0; i < N; i++)
                {
                    double pos = start + (double)i / N;
                    while (j < N - 1 && resampleCum[j] < pos) j++;
                    nx[i] = x[j] + NextGaussian(rng) * 0.1;
                    ny[i] = y[j] + NextGaussian(rng) * 0.1;
                }
                for (int i = 0; i < N; i++) { x[i] = nx[i]; y[i] = ny[i]; logw[i] = -Math.Log(N); }
            }
        }

        double sw = 0.0, mx = 0.0, my = 0.0, mz = 0.0;
        for (int i = 0; i < N; i++) { double w = Math.Exp(logw[i]); sw += w; mx += w * x[i]; my += w * y[i]; mz += w * z[i]; }
        if (sw <= 0) sw = 1.0;
        mx /= sw; my /= sw; mz /= sw;

        return new Estimate(mx, mz, 0.0, $"PF", null, null)
        {
            y = my, z = mz, currentFloorName = FLOOR_NAMES[currentFloorIdx]
        };
    }

    private double NextGaussian(System.Random rng) { 
        return Math.Sqrt(-2.0 * Math.Log(1.0 - rng.NextDouble())) * Math.Sin(2.0 * Math.PI * (1.0 - rng.NextDouble())); 
    }
}
