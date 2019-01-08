using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.SceneManagement;

public sealed class Bootstrap
{
    public static EntityArchetype ASphere;
    public static EntityArchetype AMasterData;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager>();

        ASphere = entityManager.CreateArchetype(typeof(Position), typeof(CPrimitive));
        AMasterData = entityManager.CreateArchetype(typeof(CMasterData));
    }

    public static void NewGame()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager>();

        Entity masterData = entityManager.CreateEntity(AMasterData);

        Entity sphere = entityManager.CreateEntity(ASphere);
        entityManager.SetComponentData(sphere, new Position { Value = new float3(100, 100, 0) });
        entityManager.SetComponentData(sphere, new CPrimitive { Radius = 50.0f });

        Debug.Log("NewGame");
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitializeAfterSceneLoad()
    {
    }
}
