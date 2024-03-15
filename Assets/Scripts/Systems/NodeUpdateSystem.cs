using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace DG.CGOL
{
    [BurstCompile]
    [UpdateAfter(typeof(NodeSpawnSystem))]
    public partial struct NodeUpdateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NodeProperties>();
            state.RequireForUpdate<NodeStatusProperties>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var nodeStatusProperties in
                SystemAPI.Query<NodeStatusProperties>()
                .WithNone<NodeSpawnerSetTag>())
            {
                foreach (var (nodeProperties, nodeEntity) in
                    SystemAPI.Query<NodeProperties>()
                    .WithEntityAccess())
                {
                    var status = nodeStatusProperties.nodeStatusAuthority[nodeProperties.index];

                    if (status == NodeStatus.Dead)
                        ecb.AddComponent(nodeEntity, new DisableRendering());
                    else
                        ecb.RemoveComponent<DisableRendering>(nodeEntity);
                }
            }
        }

        public void OnDestroy(ref SystemState state)
        {
        }
    }
}