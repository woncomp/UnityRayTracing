using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SdSphere : MonoBehaviour
{

    public RayTracingMaterialType MaterialType;
    public Color Albedo;
    public float RefractionIndex = 1.3f;
    public float Scale = 1.0f;

    public RayTracingSphere CreateRTGeometry()
    {
        RayTracingSphere geo;
        geo.Center = transform.position;
        geo.Radius = transform.lossyScale.x * 0.5f;
        geo.Scale = Scale;
        geo.Material = new RayTracingMaterial();
        geo.Material.MatType = MaterialType;
        geo.Material.Albedo = Albedo;
        geo.Material.RefractionIndex = 1.3f;
        return geo;
    }
}
