using DG.CGOL;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class HiScoreController : MonoBehaviour
{
    public TextMeshProUGUI aliveValue;
    public TextMeshProUGUI densityValue;
    public TextMeshProUGUI generationsValue;

    private EntityManager entityManager;

    private void Awake()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    private void Start()
    {
        aliveValue.text = PlayerPrefs.GetInt("Alive", 0).ToString();
        densityValue.text = PlayerPrefs.GetFloat("Density", 0.00f).ToString();
        generationsValue.text = PlayerPrefs.GetInt("Generations", 0).ToString();
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
            var nodeSpawnerProperties = entityManager.GetComponentData<NodeSpawnerProperties>(entity);

            if (nodeStatusProperties.homeostasisAchieved)
            {
                var density = ((float)nodeStatusProperties.nodesAlive / (nodeSpawnerProperties.gridSize * nodeSpawnerProperties.gridSize));
                var alive = nodeStatusProperties.nodesAlive;
                var generations = nodeStatusProperties.generations;

                if (alive > PlayerPrefs.GetInt("Alive", 0))
                {
                    PlayerPrefs.SetInt("Alive", alive);
                    aliveValue.text = alive.ToString();
                }
                if (density > PlayerPrefs.GetFloat("Density", 0.00f))
                {
                    PlayerPrefs.SetFloat("Density", density);
                    densityValue.text = density.ToString();
                }
                if (generations > PlayerPrefs.GetInt("Generations", 0))
                {
                    PlayerPrefs.SetInt("Generations", generations);
                    generationsValue.text = generations.ToString();
                }
            }
        }
    }
}