using UnityEngine;
using Unity.Entities;

namespace DG.CGOL
{
    public class NodeSpawnerMonoBaker : Baker<NodeSpawnerMono>
    {
        public override void Bake(NodeSpawnerMono authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var prefabBuffer = AddBuffer<NodeSpawnerPrefabs>(entity);
            AddComponent(entity, new NodeSpawnerProperties
            {
                gridSize = authoring.gridSize,
            });

            foreach (var prefab in authoring.nodePrefabs)
            {
                prefabBuffer.Add(new NodeSpawnerPrefabs()
                {
                    nodePrefab = GetEntity(prefab, TransformUsageFlags.Dynamic),
                });
            }

            AddComponent(entity, new NodeSpawnerSetTag());
        }
    }
}