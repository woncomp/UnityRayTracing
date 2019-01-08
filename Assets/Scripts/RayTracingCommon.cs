using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HitRecord
{
    public float t;
    public Vector3 Point;
    public Vector3 Normal;
    public RayTracingMaterial Material;
}

public class PixelDebugData
{
    public string DebugText = "";
    public List<Vector3> Points = new List<Vector3>();
}