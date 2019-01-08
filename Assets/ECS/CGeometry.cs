using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct CMasterData : IComponentData
{
    public float TotalRadius;
}

public struct CPrimitive : IComponentData
{
    public float Radius;
}
