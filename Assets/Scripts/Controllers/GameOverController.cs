using DG.CGOL;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class GameOverController : MonoBehaviour
{
    public GameObject WinScreen;
    public GameObject LoseScreen;

    private EntityManager entityManager;

    private void Awake()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    private void Update()
    {
        var queryDesc = new EntityQueryDesc()
        {
            All = new ComponentType[]
            {
                typeof(NodeStatusProperties)
            }
        };

        var query = entityManager.CreateEntityQuery(queryDesc);
        var entities = query.ToEntityArray(Allocator.Temp);

        foreach (var entity in entities)
        {
            var nodeStatusProperties = entityManager.GetComponentData<NodeStatusProperties>(entity);
            if (nodeStatusProperties.homeostasisAchieved)
            {
                if (nodeStatusProperties.nodesAlive == 0)
                    LoseScreen.SetActive(true);
                else
                    WinScreen.SetActive(true);
            }
            else
            {
                LoseScreen.SetActive(false);
                WinScreen.SetActive(false);
            }
        }
    }
}