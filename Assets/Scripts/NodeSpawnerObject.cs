using DG.CGOL;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class NodeSpawnerObject : MonoBehaviour
{
    private EntityManager entityManager;

    private void Awake()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        var queryDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] {
                typeof(NodeSpawnerProperties)
            }
        };

        var query = entityManager.CreateEntityQuery(queryDesc);
        var entities = query.ToEntityArray(Allocator.Temp);

        foreach (var entity in entities)
        {
            var entityTransform = entityManager.GetComponentData<LocalTransform>(entity);
            this.transform.position = entityTransform.Position;
        }
    }
}