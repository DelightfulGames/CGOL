using UnityEngine;
using Unity.Entities;

namespace DG.CGOL
{
    public class NodeSpawnerMonoBaker : Baker<NodeSpawnerMono>
    {
        public override void Bake(NodeSpawnerMono authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new NodeSpawnerProperties
            {
                gridSize = authoring.gridSize,
                nodePrefab = GetEntity(authoring.nodePrefab, TransformUsageFlags.Dynamic)
            });
            AddComponent(entity, new NodeSpawnerSetTag());
        }
    }
}