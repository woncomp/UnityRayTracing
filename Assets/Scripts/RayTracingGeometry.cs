using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RayTracingSphere
{
    public Vector3 Center;
    public float Radius;
    public float Scale;
    public RayTracingMaterial Material;

    public HitRecord Raycast(Ray ray)
    {
        var oc = ray.origin - Center;
        float a = Vector3.Dot(ray.direction, ray.direction);
        float b = 2.0f * Vector3.Dot(oc, ray.direction);
        float r = Radius * Scale;
        float c = Vector3.Dot(oc, oc) - r * r;
        float discriminant = b * b - 4 * a * c;
        HitRecord result = new HitRecord();
        if (discriminant < 0)
        {
            result.t = -1;
        }
        else
        {
            result.t = (-b - Mathf.Sqrt(discriminant)) / (2.0f * a);
            if (result.t < 0.0001f)
            {
                result.t = (-b + Mathf.Sqrt(discriminant)) / (2.0f * a);
            }
            result.Point = ray.GetPoint(result.t);
            result.Normal = Vector3.Normalize(result.Point - Center) * Mathf.Sign(Scale);
        }
        result.Material = Material;
        return result;
    }
}