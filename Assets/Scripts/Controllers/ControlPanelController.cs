using DG.CGOL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public Texture2D surprise1;
    public Texture2D surprise2;
    public Texture2D surprise3;
    public Texture2D surprise4;

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

        ResetNodes();

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
                lastGenerationAlive = new NativeArray<int>(10000, Allocator.Persistent, NativeArrayOptions.ClearMemory),
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

    public void Surprise1Clicked()
    {
        ResetNodes();
        var pixels = surprise1.GetPixels();
        GenerateSurprise(pixels, surprise1.width);
    }

    private void GenerateSurprise(Color[] pixels, int gridSize)
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
            var nodeSpawnerProperties = entityManager.GetComponentData<NodeSpawnerProperties>(entity);

            nodeSpawnerProperties.gridSize = gridSize;
            nodeSpawnerProperties.nodeType = nodeType;
            entityManager.SetComponentData(entity, nodeSpawnerProperties);

            var nodeStatuses = new NativeArray<NodeStatus>(gridSize * gridSize, Allocator.Persistent);

            // Apparently Unity loads textures left to right, BOTTOM TO TOP (like Direct3D)
            // instead of Top to bottom like the documentation says -_-
            for (int row = 0; row < gridSize; ++row)
                Array.Reverse(pixels, row * gridSize, gridSize);

            var pixelData = pixels.Select(c =>
            {
                if (c == Color.black)
                    return NodeStatus.Dead;
                else
                    return NodeStatus.Living;
            }).ToArray();

            for (int i = 0; i < nodeStatuses.Length; i++)
            {
                nodeStatuses[i] = pixelData[i];
            }

            var nodeStatusProperties = new NodeStatusProperties()
            {
                nodeStatusAuthority = nodeStatuses,
                nodeStatusBuffer = new NativeArray<NodeStatus>(nodeStatuses.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory),
                homeostasisAchieved = false,
                generations = 0,
                lastGenerationAlive = new NativeArray<int>(10000, Allocator.Persistent, NativeArrayOptions.ClearMemory),
                nodesAlive = 0
            };

            if (entityManager.HasComponent<NodeStatusProperties>(entity))
                entityManager.SetComponentData(entity, nodeStatusProperties);
            else
                entityManager.AddComponentData(entity, nodeStatusProperties);
            entityManager.AddComponentData(entity, new NodeSpawnerSetTag());

            if (entityManager.HasComponent<NodeGenerationPlayTag>(entity))
                entityManager.RemoveComponent<NodeGenerationPlayTag>(entity);
        }
    }

    public void Surprise2Clicked()
    {
        ResetNodes();
        var pixels = surprise2.GetPixels();
        GenerateSurprise(pixels, surprise2.width);
    }

    public void Surprise3Clicked()
    {
        ResetNodes();
        var pixels = surprise3.GetPixels();
        GenerateSurprise(pixels, surprise3.width);
    }

    public void Surprise4Clicked()
    {
        ResetNodes();
        var pixels = surprise4.GetPixels();
        GenerateSurprise(pixels, surprise4.width);
    }

    private void ResetNodes()
    {
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