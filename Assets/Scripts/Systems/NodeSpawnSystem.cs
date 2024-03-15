using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;

namespace DG.CGOL
{
    [BurstCompile]
    public partial struct NodeSpawnSystem : ISystem
    {
        private const float closest = 5.0f;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NodeSpawnerProperties>();
            state.RequireForUpdate<NodeSpawnerSetTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (nodeSpawnerProperties, nodeSpawnerEntity) in
                SystemAPI.Query<NodeSpawnerProperties>()
                .WithAll<NodeSpawnerSetTag>()
                .WithEntityAccess())
            {
                ecb.RemoveComponent<NodeSpawnerSetTag>(nodeSpawnerEntity);

                var gridSize = nodeSpawnerProperties.gridSize;
                var a = gridSize / 2;
                var distance = 1 / (MathF.Tan(0.523599f) / a);  //hardcoded to half the angle of the camera fov, but it's fine for demonstration purposes
                distance = MathF.Max(distance, closest) + 10;   //padding for strange even number gridSize bug

                var origin = -(gridSize / 2);

                for (int col = 0; col < gridSize; col++)
                {
                    for (int row = 0; row < gridSize; row++)
                    {
                        var nodeEntity = ecb.Instantiate(nodeSpawnerProperties.nodePrefab);
                        ecb.AddComponent(nodeEntity, LocalTransform.FromPosition(origin + col, origin + row, distance));
                        ecb.AddComponent(nodeEntity, new NodeProperties()
                        {
                            index = (col * gridSize) + row
                        });
                    }
                }
            }
        }

        public void OnDestroy(ref SystemState state)
        {
        }
    }
}