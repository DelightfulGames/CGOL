using Unity.Collections;
using Unity.Entities;

namespace DG.CGOL
{
    public enum NodeStatus : int
    {
        Dead = 0,
        Born = 1,
        Living = 2,
        DyingUnderPopulation = 3,
        DyingOverPopulation = 4
    }

    public struct NodeStatusProperties : IComponentData
    {
        public NativeArray<NodeStatus> nodeStatusAuthority;
        public NativeArray<NodeStatus> nodeStatusBuffer;
        public int nodesAlive;
        public int generations;
        public NativeArray<int> lastGenerationAlive;
        public bool homeostasisAchieved;
    }
}