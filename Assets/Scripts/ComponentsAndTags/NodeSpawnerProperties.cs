using Unity.Entities;

namespace DG.CGOL
{
    public struct NodeSpawnerProperties : IComponentData
    {
        public int gridSize;
        public int nodeType;
    }

    public struct NodeSpawnerPrefabs : IBufferElementData
    {
        public Entity nodePrefab;
    }
}