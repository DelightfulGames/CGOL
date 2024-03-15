using Unity.Entities;

namespace DG.CGOL
{
    public struct NodeSpawnerProperties : IComponentData
    {
        public int gridSize;
        public Entity nodePrefab;
    }
}