using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

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

    [BurstCompile]
    [UpdateAfter(typeof(NodeSpawnSystem))]
    public partial struct NodeUpdateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NodeProperties>();
            state.RequireForUpdate<NodeStatusProperties>();
        }

        // This was split out into the NodeGenerationSystem, but has been collapsed here.
        // We're doing all the calculations in one system, thus one pass of the cells bitmap
        // In theory, it should be faster than 2 systems: O(2n) instead of O(n)

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (nodeSpawnerProperties, nodeStatusProperties, nodeSpawnerEntity) in
                SystemAPI.Query<NodeSpawnerProperties, RefRW<NodeStatusProperties>>()
                .WithNone<NodeSpawnerSetTag>()
                .WithEntityAccess())
            {
                var rowDistance = nodeSpawnerProperties.gridSize;
                var size = nodeStatusProperties.ValueRO.nodeStatusAuthority.Length;
                nodeStatusProperties.ValueRW.nodesAlive = 0;

                foreach (var (nodeProperties, nodeEntity) in
                    SystemAPI.Query<NodeProperties>()
                    .WithEntityAccess())
                {
                    var index = nodeProperties.index;
                    var status = nodeStatusProperties.ValueRO.nodeStatusAuthority[index];

                    if (status == NodeStatus.Dead)
                    {
                        ecb.AddComponent(nodeEntity, new DisableRendering());
                    }
                    else
                    {
                        ecb.RemoveComponent<DisableRendering>(nodeEntity);
                        nodeStatusProperties.ValueRW.nodesAlive++;
                    }

                    //Calculate next generation if necessary
                    if (state.EntityManager.HasComponent<NodeGenerationPlayTag>(nodeSpawnerEntity) ||
                        state.EntityManager.HasComponent<NodeGenerationStepTag>(nodeSpawnerEntity))
                    {
                        var neighborsAlive = 0;

                        var tempIndex = GetNodeIndex(index, NeighborPosition.TopLeft, rowDistance, size);
                        if (nodeStatusProperties.ValueRO.nodeStatusAuthority[tempIndex] != NodeStatus.Dead)
                            neighborsAlive++;
                        tempIndex = GetNodeIndex(index, NeighborPosition.Top, rowDistance, size);
                        if (nodeStatusProperties.ValueRO.nodeStatusAuthority[tempIndex] != NodeStatus.Dead)
                            neighborsAlive++;
                        tempIndex = GetNodeIndex(index, NeighborPosition.TopRight, rowDistance, size);
                        if (nodeStatusProperties.ValueRO.nodeStatusAuthority[tempIndex] != NodeStatus.Dead)
                            neighborsAlive++;
                        tempIndex = GetNodeIndex(index, NeighborPosition.Left, rowDistance, size);
                        if (nodeStatusProperties.ValueRO.nodeStatusAuthority[tempIndex] != NodeStatus.Dead)
                            neighborsAlive++;
                        tempIndex = GetNodeIndex(index, NeighborPosition.Right, rowDistance, size);
                        if (nodeStatusProperties.ValueRO.nodeStatusAuthority[tempIndex] != NodeStatus.Dead)
                            neighborsAlive++;
                        tempIndex = GetNodeIndex(index, NeighborPosition.BottomLeft, rowDistance, size);
                        if (nodeStatusProperties.ValueRO.nodeStatusAuthority[tempIndex] != NodeStatus.Dead)
                            neighborsAlive++;
                        tempIndex = GetNodeIndex(index, NeighborPosition.Bottom, rowDistance, size);
                        if (nodeStatusProperties.ValueRO.nodeStatusAuthority[tempIndex] != NodeStatus.Dead)
                            neighborsAlive++;
                        tempIndex = GetNodeIndex(index, NeighborPosition.BottomRight, rowDistance, size);
                        if (nodeStatusProperties.ValueRO.nodeStatusAuthority[tempIndex] != NodeStatus.Dead)
                            neighborsAlive++;

                        //Default state of a cell
                        nodeStatusProperties.ValueRW.nodeStatusBuffer[index] = NodeStatus.Dead;

                        if (nodeStatusProperties.ValueRO.nodeStatusAuthority[index] == NodeStatus.Dead)
                            if (neighborsAlive == 3)
                                nodeStatusProperties.ValueRW.nodeStatusBuffer[index] = NodeStatus.Living;

                        if (nodeStatusProperties.ValueRO.nodeStatusAuthority[index] != NodeStatus.Dead)
                        {
                            if (neighborsAlive < 2)
                                nodeStatusProperties.ValueRW.nodeStatusBuffer[index] = NodeStatus.Dead;
                            if (neighborsAlive >= 2 && neighborsAlive < 4)
                                nodeStatusProperties.ValueRW.nodeStatusBuffer[index] = NodeStatus.Living;
                            if (neighborsAlive >= 4)
                                nodeStatusProperties.ValueRW.nodeStatusBuffer[index] = NodeStatus.Dead;
                        }
                    }
                }

                if (state.EntityManager.HasComponent<NodeGenerationPlayTag>(nodeSpawnerEntity) ||
                    state.EntityManager.HasComponent<NodeGenerationStepTag>(nodeSpawnerEntity))
                {
                    //Swap Arrays here
                    var tempArray = nodeStatusProperties.ValueRO.nodeStatusAuthority;
                    nodeStatusProperties.ValueRW.nodeStatusAuthority =
                        nodeStatusProperties.ValueRO.nodeStatusBuffer;
                    nodeStatusProperties.ValueRW.nodeStatusBuffer = tempArray;

                    if (nodeStatusProperties.ValueRO.lastGenerationAlive ==
                        nodeStatusProperties.ValueRO.nodesAlive)
                    {
                        nodeStatusProperties.ValueRW.homeostatisCheck++;
                        if (nodeStatusProperties.ValueRO.homeostatisCheck > 7)
                            nodeStatusProperties.ValueRW.homeostasisAchieved = true;
                    }
                    else
                    {
                        nodeStatusProperties.ValueRW.homeostatisCheck = 0;
                        nodeStatusProperties.ValueRW.lastGenerationAlive =
                            nodeStatusProperties.ValueRO.nodesAlive;
                        nodeStatusProperties.ValueRW.generations++;
                    }

                    if (state.EntityManager.HasComponent<NodeGenerationStepTag>(nodeSpawnerEntity))
                        ecb.RemoveComponent<NodeGenerationStepTag>(nodeSpawnerEntity);
                }
            }
        }

        public void OnDestroy(ref SystemState state)
        {
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
}