using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;


public struct RayTracingParallelJob : IJobParallelFor, System.IDisposable
{
    [ReadOnly]
    public NativeArray<RayTracingSphere> SceneSpheres;
    [ReadOnly]
    public NativeArray<long> RandomSeeds;

    //public RayTracingScene Scene;

    public int Width, Height;
    public float StartX, StartY, StepX, StepY;
    public Vector3 Origin;
    public Matrix4x4 CameraLocal2World;
    public bool GammarCorrection;
    public int ScatterRayCount;
    public int AntiAliasRayCount;

    [WriteOnly]
    public NativeArray<Color32> OutputColors;
    [WriteOnly]
    public NativeArray<int> MaxDepth;
    public bool depthOverflow;

    public void Execute(int i)
    {
        long seed = RandomSeeds[i];
        int maxDepth = 0;
        OutputColors[i] = CalcPixelColor(ref seed, ref maxDepth, i % Width, i / Width, null);
        MaxDepth[i] = maxDepth;
    }

    public Color CalcPixelColor(ref long seed, ref int maxDepth, int x, int y, PixelDebugData debug)
    {
        if(debug != null)
        {
            debug.Points.Add(Origin);
        }

        Color accumulateColor = Color.black;
        int c = debug == null ? AntiAliasRayCount : 1;
        for (int i = 0; i < c; ++i)
        {
            var offsetx = (debug == null ? SimpleRandom.RandNorm(ref seed) : 0.5f) * StepX;
            var offsety = (debug == null ? SimpleRandom.RandNorm(ref seed) : 0.5f) * StepY;
            var dir = new Vector4(StartX + x * StepX + offsetx, StartY + y * StepY + offsety, 1, 0);
            var dir2 = CameraLocal2World * dir;
            Ray ray = new Ray(Origin, dir2.normalized);
            accumulateColor += TraceScene(ref seed, ref maxDepth, ray, 0, debug);
        }
        Color col = accumulateColor / c;
        if (GammarCorrection)
        {
            col.r = Mathf.Sqrt(col.r);
            col.g = Mathf.Sqrt(col.g);
            col.b = Mathf.Sqrt(col.b);
        }
        col.a = 1;
        return col;
    }

    private static Color Color0 = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    private static Color Color1 = new Color(0.5f, 0.7f, 1.0f, 1.0f);

    private Color TraceScene(ref long seed, ref int maxDepth, Ray ray, int depth, PixelDebugData debug)
    {
        HitRecord hit = RaycastScene(ray);
        //HitRecord hit = Scene.RayCast(ray);
        if (debug != null)
        {
            if (hit.t > 0)
            {
                debug.Points.Add(hit.Point);
                debug.DebugText += ("Hit " + hit.Point.ToString() + " " + hit.Normal.ToString() + "\n");
            }
            else
            {
                debug.Points.Add(ray.origin + ray.direction * 100);
                debug.DebugText += "Miss\n";
            }
        }
        Color col = new Color(0,0,0,0);
        if (hit.t > 0 && depth < 20)
        {
            maxDepth = depth > maxDepth ? depth : maxDepth;
            //if (depth > 18)
            //{
            //    depthOverflow = true;
            //}
            var N = hit.Normal;
            var C = (debug == null && depth < 3) ? ScatterRayCount : 1;
            int c = 0;
            for (int i = 0; i < C; ++i)
            {
                Color atten;
                Ray rayNext;
                if (hit.Material.Scatter(debug, ref seed, ray, ref hit, out atten, out rayNext))
                {
                    col += atten * TraceScene(ref seed, ref maxDepth, rayNext, depth + 1, debug);
                    c += 1;
                }
            }
            if (c == 0)
            {
                col = Color.Lerp(Color0, Color1, 0.5f + ray.direction.y);
            }
            else
            {
                col = col / c;
            }
        }
        else
        {
            col = Color.Lerp(Color0, Color1, 0.5f + ray.direction.y);
        }
        if (debug != null)
        {
            debug.DebugText += (" Color:" + col.ToString() + "\n");
        }
        return col;
    }

    private HitRecord RaycastScene(Ray ray)
    {
        HitRecord closest = new HitRecord { t = 1000000 };
        foreach (var sphere in SceneSpheres)
        {
            var hit = sphere.Raycast(ray);
            if (hit.t > 0.0001f && hit.t < closest.t)
            {
                closest = hit;
            } 
        }
        if (closest.t > 999999)
        {
            closest.t = -1;
        }
        return closest;
    }

    public void Dispose()
    {
        SceneSpheres.Dispose();
        RandomSeeds.Dispose();
        OutputColors.Dispose();
        MaxDepth.Dispose();
    }
}

public struct SimpleRandom
{
    public const long MaxValue = 0xFFFFFF;

    public static long RandInt(ref long seed)
    {
        seed = seed * 214013L + 2531011L;
        return ((seed >> 16) & MaxValue);
    }

    public static float RandNorm(ref long seed)
    {
        return (float)(RandInt(ref seed)) / (float)(MaxValue);
    }

    public static Vector3 RandomInsideUnitSphere(ref long seed)
    {
        while(true)
        {
            Vector3 r;
            r.x = RandNorm(ref seed) * 2 - 1;
            r.y = RandNorm(ref seed) * 2 - 1;
            r.z = RandNorm(ref seed) * 2 - 1;
            if(r.sqrMagnitude <= 1)
            {
                return r;
            }
        }
    }
}
