using UnityEngine;
using Unity.Entities;

namespace DG.CGOL
{
    public class NodeBaker : Baker<NodeMono>
    {
        public override void Bake(NodeMono authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new NodeProperties
            {
                rowIndex = authoring.rowIndex,
                columnIndex = authoring.columnIndex,
            });
        }
    }
}