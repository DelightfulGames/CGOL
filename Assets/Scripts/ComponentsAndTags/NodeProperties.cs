using Unity.Entities;

namespace DG.CGOL
{
    public struct NodeProperties : IComponentData
    {
        public uint rowIndex;
        public uint columnIndex;
        public int index;
    }
}