using Unity.Collections;
using Unity.Entities;

namespace Unity.Animation
{
    [GenerateTestsForBurstCompatibility]
    public struct WeightEntry
    {
        public int Index;
        public float Weight;
    }

    [GenerateTestsForBurstCompatibility]
    public struct ChannelWeightTable
    {
        public BlobArray<WeightEntry> Weights;
    }
}
