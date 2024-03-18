using System.Diagnostics;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace DG.CGOL
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(NodeUpdateSystem))]
    public partial struct HomeoStasisCheckSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireAnyForUpdate(state.EntityManager.CreateEntityQuery(new EntityQueryDesc()
            {
                Any = new ComponentType[] {
                    typeof(NodeGenerationPlayTag),
                    typeof(NodeGenerationStepTag)
                }
            }));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var nodeStatusProperties in
                SystemAPI.Query<RefRW<NodeStatusProperties>>()
                .WithNone<NodeSpawnerSetTag>())
            {
                if (nodeStatusProperties.ValueRW.homeostasisAchieved)
                    return;

                var lastGens = nodeStatusProperties.ValueRO.lastGenerationAlive;
                var generationsIndex = nodeStatusProperties.ValueRO.generations;

                // Check the last additions first
                int countIndex = generationsIndex - 1;

                if (lastGens[(countIndex + lastGens.Length) % lastGens.Length] == 0)
                {
                    nodeStatusProperties.ValueRW.homeostasisAchieved = true;
                    return;
                }

                var sequenceBeginIndex = countIndex;
                //UnityEngine.Debug.Log(lastGens[(countIndex + lastGens.Length) % lastGens.Length]);

                // I apologize for the modular math hell, but that's efficiency for you
                for (int repeatIndex = countIndex - 1; repeatIndex >= countIndex - lastGens.Length; repeatIndex--)
                {
                    if (lastGens[(repeatIndex + lastGens.Length) % lastGens.Length] == 0)
                        return;

                    if (lastGens[(repeatIndex + lastGens.Length) % lastGens.Length] == lastGens[countIndex % lastGens.Length])
                    {
                        var sequenceEndsIndex = repeatIndex;

                        var matchingSequnceFound = 0;
                        for (int matchingIndex = sequenceBeginIndex;
                            matchingIndex >= sequenceBeginIndex - lastGens.Length;
                            matchingIndex -= math.abs(sequenceBeginIndex - sequenceEndsIndex))
                        {
                            if (lastGens[(matchingIndex + lastGens.Length) % lastGens.Length] == 0)
                                return;

                            //UnityEngine.Debug.Log($"Span: {sequenceBeginIndex - sequenceEndsIndex}");
                            var matchingIndexFound = 0;
                            for (int sequenceIndex = sequenceBeginIndex;
                                sequenceIndex > sequenceEndsIndex;
                                sequenceIndex--)
                            {
                                if (lastGens[((matchingIndex - (sequenceBeginIndex - sequenceIndex)) + lastGens.Length) % lastGens.Length] == 0)
                                    break;

                                if (lastGens[((matchingIndex - (sequenceBeginIndex - sequenceIndex)) + lastGens.Length) % lastGens.Length] ==
                                    lastGens[(sequenceIndex + lastGens.Length) % lastGens.Length])
                                {
                                    matchingIndexFound++;
                                }
                            }

                            //UnityEngine.Debug.Log($"MatchingIndex: {matchingIndexFound}");
                            if (matchingIndexFound == math.abs(sequenceBeginIndex - sequenceEndsIndex))
                                matchingSequnceFound++;
                            else
                                matchingSequnceFound = 0;

                            //UnityEngine.Debug.Log($"MatchingNeeds: {math.sqrt(nodeStatusProperties.ValueRO.nodeStatusAuthority.Length)}");
                            if (matchingSequnceFound > math.sqrt(nodeStatusProperties.ValueRO.nodeStatusAuthority.Length))
                            {
                                nodeStatusProperties.ValueRW.homeostasisAchieved = true;
                                return;
                            }

                            //UnityEngine.Debug.Log($"Matching Sequence: {matchingSequnceFound}");
                        }
                    }
                }
            }
        }

        public void OnDestroy(ref SystemState state)
        {
        }
    }
}