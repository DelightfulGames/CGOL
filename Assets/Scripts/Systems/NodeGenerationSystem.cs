using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DG.CGOL
{
    public enum NeighborPosition
    {
        TopLeft,
        Top,
        TopRight,
        Left,
        Right,
        BottomLeft,
        Bottom,
        BottomRight,
    }

    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct NodeGenerationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NodeStatusProperties>();
            state.RequireAnyForUpdate(state.EntityManager.CreateEntityQuery(new EntityQueryDesc[]
            {
                new EntityQueryDesc() {
                    Any = new ComponentType[]
                    {
                        typeof(NodeGenerationPlayTag),
                        typeof(NodeGenerationStepTag)
                    }
                }
            }));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (nodeSpawnerProperties, nodeStatusProperties, nodeSpawnerEntity) in
                SystemAPI.Query<NodeSpawnerProperties, RefRW<NodeStatusProperties>>()
                .WithEntityAccess())
            {
                var rowDistance = nodeSpawnerProperties.gridSize;
                var size = nodeStatusProperties.ValueRO.nodeStatusAuthority.Length;

                for (int i = 0; i < size; i++)
                {
                    nodeStatusProperties.ValueRW.nodesAlive = 0;
                    //Debug.Log($"Index {i}: {nodeStatusProperties.ValueRO.nodeStatusAuthority[i]}");
                    var neighborsAlive = 0;

                    var tempIndex = GetNodeIndex(i, NeighborPosition.TopLeft, rowDistance, size);
                    //Debug.Log($"TempIndex {tempIndex}: TopLeft");
                    if (nodeStatusProperties.ValueRO.nodeStatusAuthority[tempIndex] != NodeStatus.Dead)
                        neighborsAlive++;
                    tempIndex = GetNodeIndex(i, NeighborPosition.Top, rowDistance, size);
                    //Debug.Log($"TempIndex {tempIndex}: Top");
                    if (nodeStatusProperties.ValueRO.nodeStatusAuthority[tempIndex] != NodeStatus.Dead)
                        neighborsAlive++;
                    tempIndex = GetNodeIndex(i, NeighborPosition.TopRight, rowDistance, size);
                    //Debug.Log($"TempIndex {tempIndex}: TopRight");
                    if (nodeStatusProperties.ValueRO.nodeStatusAuthority[tempIndex] != NodeStatus.Dead)
                        neighborsAlive++;
                    tempIndex = GetNodeIndex(i, NeighborPosition.Left, rowDistance, size);
                    //Debug.Log($"TempIndex {tempIndex}: Left");
                    if (nodeStatusProperties.ValueRO.nodeStatusAuthority[tempIndex] != NodeStatus.Dead)
                        neighborsAlive++;
                    tempIndex = GetNodeIndex(i, NeighborPosition.Right, rowDistance, size);
                    //Debug.Log($"TempIndex {tempIndex}: Right");
                    if (nodeStatusProperties.ValueRO.nodeStatusAuthority[tempIndex] != NodeStatus.Dead)
                        neighborsAlive++;
                    tempIndex = GetNodeIndex(i, NeighborPosition.BottomLeft, rowDistance, size);
                    //Debug.Log($"TempIndex {tempIndex}: BottomLeft");
                    if (nodeStatusProperties.ValueRO.nodeStatusAuthority[tempIndex] != NodeStatus.Dead)
                        neighborsAlive++;
                    tempIndex = GetNodeIndex(i, NeighborPosition.Bottom, rowDistance, size);
                    //Debug.Log($"TempIndex {tempIndex}: Bottom");
                    if (nodeStatusProperties.ValueRO.nodeStatusAuthority[tempIndex] != NodeStatus.Dead)
                        neighborsAlive++;
                    tempIndex = GetNodeIndex(i, NeighborPosition.BottomRight, rowDistance, size);
                    //Debug.Log($"TempIndex {tempIndex}: BottomRight");
                    if (nodeStatusProperties.ValueRO.nodeStatusAuthority[tempIndex] != NodeStatus.Dead)
                        neighborsAlive++;

                    //Default state of a cell
                    nodeStatusProperties.ValueRW.nodeStatusBuffer[i] = NodeStatus.Dead;

                    if (nodeStatusProperties.ValueRO.nodeStatusAuthority[i] == NodeStatus.Dead)
                        if (neighborsAlive == 3)
                            nodeStatusProperties.ValueRW.nodeStatusBuffer[i] = NodeStatus.Living;

                    if (nodeStatusProperties.ValueRO.nodeStatusAuthority[i] != NodeStatus.Dead)
                    {
                        if (neighborsAlive < 2)
                            nodeStatusProperties.ValueRW.nodeStatusBuffer[i] = NodeStatus.Dead;
                        if (neighborsAlive >= 2 && neighborsAlive < 4)
                            nodeStatusProperties.ValueRW.nodeStatusBuffer[i] = NodeStatus.Living;
                        if (neighborsAlive >= 4)
                            nodeStatusProperties.ValueRW.nodeStatusBuffer[i] = NodeStatus.Dead;
                    }

                    if (nodeStatusProperties.ValueRO.nodeStatusBuffer[i] != NodeStatus.Dead)
                        nodeStatusProperties.ValueRW.nodesAlive++;
                }

                //Swap Arrays here
                var tempArray = nodeStatusProperties.ValueRO.nodeStatusAuthority;
                nodeStatusProperties.ValueRW.nodeStatusAuthority =
                    nodeStatusProperties.ValueRO.nodeStatusBuffer;
                nodeStatusProperties.ValueRW.nodeStatusBuffer = tempArray;

                nodeStatusProperties.ValueRW.generations++;

                if (state.EntityManager.HasComponent<NodeGenerationStepTag>(nodeSpawnerEntity))
                    ecb.RemoveComponent<NodeGenerationStepTag>(nodeSpawnerEntity);
            }
        }

        private int GetNodeIndex(int index, NeighborPosition neighborPosition, int rowSize, int size)
        {
            float desiredRow = index / rowSize;
            float desiredColumn = index % rowSize;

            //Debug.Log($"Parameters: {index} | {rowSize} | {size}");
            //Debug.Log($"Vars: {desiredRow} | {desiredColumn}");

            if (neighborPosition == NeighborPosition.TopLeft ||
                neighborPosition == NeighborPosition.Top ||
                neighborPosition == NeighborPosition.TopRight)
                desiredRow--;
            if (neighborPosition == NeighborPosition.BottomLeft ||
                neighborPosition == NeighborPosition.Bottom ||
                neighborPosition == NeighborPosition.BottomRight)
                desiredRow++;

            //wrap around
            if (desiredRow < 0)
                desiredRow = rowSize - 1;
            if (desiredRow >= rowSize)
                desiredRow = 0;

            if (neighborPosition == NeighborPosition.TopLeft ||
                neighborPosition == NeighborPosition.Left ||
                neighborPosition == NeighborPosition.BottomLeft)
                desiredColumn--;
            if (neighborPosition == NeighborPosition.TopRight ||
                neighborPosition == NeighborPosition.Right ||
                neighborPosition == NeighborPosition.BottomRight)
                desiredColumn++;

            //wrap around
            if (desiredColumn < 0)
                desiredColumn = rowSize - 1;
            if (desiredColumn >= rowSize)
                desiredColumn = 0;

            //Debug.Log($"Adjusted Vars: {desiredRow} | {desiredColumn}");

            var desiredIndex = (desiredRow * rowSize) + desiredColumn;

            return (int)desiredIndex;
        }
    }

    //Couldn't get NativeArray working in IJob, complained it was nested, which it clearly isn't
    //public partial struct NodeGenerationJob : IJobEntity
    //{
    //    private void Execute(ref NodeStatusProperties nodeStatusProperties,
    //        in NodeSpawnerProperties nodeSpawnerProperties)
    //    {
    //    }
    //}
}