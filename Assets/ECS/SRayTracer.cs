using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class SRayTracer : ComponentSystem
{
#pragma warning disable 649
    public struct Data
    {
        public readonly int Length;
        public ComponentDataArray<CPrimitive> Primitives;
    }
    [Inject] private Data m_Data;

    public struct MasterData
    {
        public readonly int Length;
        public ComponentDataArray<CMasterData> Data;
    }
    [Inject] private MasterData m_MasterData;
#pragma warning restore 649

    private _ClassicObject _obj;

    protected override void OnUpdate()
    {
        if(m_MasterData.Length > 0)
        {
            CMasterData data = m_MasterData.Data[0];
            data.TotalRadius = 0;
            for (int i = 0; i < m_Data.Length; ++i)
            {
                data.TotalRadius += m_Data.Primitives[i].Radius;
            }
            if(_obj == null)
            {
                _obj = GameObject.Find("Main Camera").GetComponent<_ClassicObject>();
            }
            if(_obj != null)
            {
                _obj.TotalRadiusDebug = data.TotalRadius;
            }
        }
    }
}
