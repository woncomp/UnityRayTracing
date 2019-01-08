using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RayTracingMaterialType
{
    Lambert,
    Metal,
    Dielectric,
}

public struct RayTracingMaterial
{
    public RayTracingMaterialType MatType;
    public Color Albedo;
    public float RefractionIndex;

    public bool Scatter(PixelDebugData debug, ref long seed, Ray rayIn, ref HitRecord hit, out Color atten, out Ray scattered)
    {
        switch (MatType)
        {
            case RayTracingMaterialType.Lambert:
                {
                    Vector3 dir = hit.Normal * 1.0001f;
                    if (debug == null)
                    {
                        dir += SimpleRandom.RandomInsideUnitSphere(ref seed);
                    }
                    Ray rayOut = new Ray();
                    rayOut.origin = hit.Point;
                    rayOut.direction = Vector3.Normalize(dir);
                    scattered = rayOut;
                    atten = Albedo;
                    return true;
                }
            case RayTracingMaterialType.Metal:
                {
                    Vector3 dir = Reflect(rayIn.direction, hit.Normal);
                    Ray rayOut = new Ray();
                    rayOut.origin = hit.Point;
                    rayOut.direction = Vector3.Normalize(dir);
                    scattered = rayOut;
                    atten = Albedo;
                    return Vector3.Dot(rayOut.direction, hit.Normal) > 0;
                }
            case RayTracingMaterialType.Dielectric:
                {
                    Vector3 N = hit.Normal;
                    Vector3 R = Reflect(rayIn.direction, N);
                    atten = Color.white;
                    float ni_over_nt = Mathf.Max(0.1f, RefractionIndex);
                    if(Vector3.Dot(rayIn.direction, N) > 0)
                    {
                        // Ray travel through inside
                        N = -N;
                    }
                    else
                    {
                        // Ray comming from outside
                        ni_over_nt = 1.0f / ni_over_nt;
                    }
                    Vector3 refracted;
                    float reflect_prob = 1.0f;
                    if(Refract(rayIn.direction, N, ni_over_nt, out refracted))
                    {
                        reflect_prob = Schlick(Vector3.Dot(N, -rayIn.direction), RefractionIndex);
                    }
                    if(SimpleRandom.RandNorm(ref seed) < reflect_prob)
                    {
                        scattered = new Ray(hit.Point, R);
                    }
                    else
                    {
                        scattered = new Ray(hit.Point, refracted);
                    }
                    return true;
                }
        }
        atten = Color.black;
        scattered = new Ray();
        return false;
    }

    public static Vector3 Reflect(Vector3 a, Vector3 n)
    {
        return a - 2 * Vector3.Dot(a, n) * n;
    }

    public static bool Refract(Vector3 v, Vector3 n, float ni_over_nt, out Vector3 refracted)
    {
        //Vector3 v0 = v.normalized;
        //float dt = Vector3.Dot(v0, n);
        //float discriminant = 1.0f - ni_over_nt * ni_over_nt * (1 - dt * dt);
        //if (discriminant > 0)
        //{
        //    refracted = ni_over_nt * (v0 - n * dt) - n * Mathf.Sqrt(discriminant);
        //    return true;
        //}
        //else
        //{
        //    refracted = Vector3.zero;
        //    return false;
        //}

        Vector3 v0 = v.normalized;
        float dt = Vector3.Dot(v0, n);
        float discriminant = 1 - ni_over_nt * ni_over_nt * (1 - dt * dt);
        if (discriminant > 0)
        {
            refracted = (n * dt + v) * ni_over_nt - Mathf.Sqrt(discriminant) * n;
            return true;
        }
        else
        {
            refracted = Vector3.zero;
            return false;
        }
    }

    public static float Schlick(float cosine, float ref_idx)
    {
        float r0 = (1 - ref_idx) / (1 + ref_idx);
        r0 = r0 * r0;
        return r0 + (1 - r0) * Mathf.Pow(1 - cosine, 5);
    }
}

