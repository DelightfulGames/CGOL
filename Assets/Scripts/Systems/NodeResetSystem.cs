using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace DG.CGOL
{
    [BurstCompile]
    public partial struct NodeResetSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NodeDeletedTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (nodeDeletedTag, nodeEntity) in SystemAPI.Query<NodeDeletedTag>()
                .WithEntityAccess())
            {
                ecb.DestroyEntity(nodeEntity);
            }

            //Populate shared? datacomponent/entity with new randomized bitmaps
        }

        public void OnDestroy(ref SystemState state)
        {
        }
    }
}