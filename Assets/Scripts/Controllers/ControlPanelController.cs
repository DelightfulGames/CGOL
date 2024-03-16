using DG.CGOL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class ControlPanelController : MonoBehaviour
{
    public TextMeshProUGUI inputField;
    public GameObject randomizer;
    public TextMeshProUGUI nodesAlive;
    public TextMeshProUGUI density;
    public TextMeshProUGUI generations;

    private float initialEntropy;
    private int nodeType;

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
            var nodeSpawnerProperties = entityManager.GetComponentData<NodeSpawnerProperties>(entity);
            density.text = ((float)nodeStatusProperties.nodesAlive / (nodeSpawnerProperties.gridSize * nodeSpawnerProperties.gridSize)).ToString();
            nodesAlive.text = nodeStatusProperties.nodesAlive.ToString();
            generations.text = nodeStatusProperties.generations.ToString();
        }
    }

    public void ResetButtonClicked()
    {
        initialEntropy = randomizer.GetComponent<Slider>().value;

        var queryDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] {
                typeof(NodeProperties)
            }
        };

        var query = entityManager.CreateEntityQuery(queryDesc);
        var entities = query.ToEntityArray(Allocator.Temp);

        foreach (var entity in entities)
        {
            entityManager.AddComponentData(entity, new NodeDeletedTag());
        }

        queryDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] {
                typeof(NodeSpawnerProperties)
            }
        };

        query = entityManager.CreateEntityQuery(queryDesc);
        entities = query.ToEntityArray(Allocator.Temp);

        foreach (var entity in entities)
        {
            var nodeSpawnerProperties = entityManager.GetComponentData<NodeSpawnerProperties>(entity);
            //Thank you stack overflow! https://stackoverflow.com/questions/58734779/uint-parse-on-a-valid-string-throws-system-formatexception-input-string-was-no
            var gridSize = Convert.ToInt32(Regex.Replace(inputField.text, @"\p{C}+", string.Empty));
            nodeSpawnerProperties.gridSize = gridSize;
            nodeSpawnerProperties.nodeType = nodeType;
            entityManager.SetComponentData(entity, nodeSpawnerProperties);

            var nodeStatuses = GetNewNodeStatuses(gridSize, initialEntropy);
            var nodeStatusProperties = new NodeStatusProperties()
            {
                nodeStatusAuthority = nodeStatuses,
                nodeStatusBuffer = new NativeArray<NodeStatus>(nodeStatuses.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory),
                homeostasisAchieved = false,
                generations = 0,
                lastGenerationAlive = 0,
                nodesAlive = 0
            };

            if (entityManager.HasComponent<NodeStatusProperties>(entity))
                entityManager.SetComponentData(entity, nodeStatusProperties);
            else
                entityManager.AddComponentData(entity, nodeStatusProperties);
            entityManager.AddComponentData(entity, new NodeSpawnerSetTag());
        }
    }

    public void PlayButtonClicked()
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
            entityManager.AddComponentData(entity, new NodeGenerationPlayTag());
            if (entityManager.HasComponent<NodeGenerationStepTag>(entity))
                entityManager.RemoveComponent<NodeGenerationStepTag>(entity);
        }
    }

    public void StepPauseButtonClicked()
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
            entityManager.AddComponentData(entity, new NodeGenerationStepTag());
            if (entityManager.HasComponent<NodeGenerationPlayTag>(entity))
                entityManager.RemoveComponent<NodeGenerationPlayTag>(entity);
        }
    }

    public void StandardClicked()
    {
        nodeType = 0;
    }

    public void CubeClicked()
    {
        nodeType = 1;
    }

    public void SphereClicked()
    {
        nodeType = 2;
    }

    public void SmudgeClicked()
    {
        nodeType = 3;
    }

    private NativeArray<NodeStatus> GetNewNodeStatuses(int gridSize, float entropy)
    {
        //cap entropy at 10% & 95%
        entropy = Math.Clamp(entropy, 0.1f, 0.95f);

        var nodeStatuses = new NodeStatus[gridSize * gridSize];
        for (int i = 0; i < nodeStatuses.Length; i++)
        {
            var entropySeed = UnityEngine.Random.Range(0.1f, 0.95f);
            if (entropySeed < entropy)
                nodeStatuses[i] = NodeStatus.Dead;
            else
            {
                var randomNumber = UnityEngine.Random.Range(0, Enum.GetValues(typeof(NodeStatus)).Length);
                nodeStatuses[i] = (NodeStatus)randomNumber;
            }
        }

        return new NativeArray<NodeStatus>(nodeStatuses, Allocator.Persistent);
    }
}