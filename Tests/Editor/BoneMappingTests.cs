using NUnit.Framework;
using UnityEngine;
using UnityToRebelFork.Editor;

namespace UnityToRebelFork
{
    public class BoneMappingTests
    {
        private static BoneWeight Weight(int bone, int other = 0, float otherWeight = 0f)
            => new BoneWeight { boneIndex0 = bone, weight0 = 1f - otherWeight, boneIndex1 = other, weight1 = otherWeight };

        [Test]
        public void SmallSkeletonKeepsTheIndicesOfTheSkeleton()
        {
            var mapping = ModelBuilder.BoneMapping(new[] { Weight(3), Weight(100) }, 120, out var warning);
            Assert.IsNull(mapping);
            Assert.IsNull(warning);
            Assert.AreEqual(100, ModelBuilder.LocalBoneIndex(mapping, 100));
        }

        [Test]
        public void BonesPastAByteGetLocalIndices()
        {
            // UnityChan: 321 bones, the right breast is bone 258 and the left shin bone 2; a byte index would turn 258 into 2.
            var weights = new[] { Weight(36), Weight(258, 36, 0.3f), Weight(2), Weight(36, 320, 0.5f) };
            var mapping = ModelBuilder.BoneMapping(weights, 321, out var warning);
            Assert.IsNull(warning);
            CollectionAssert.AreEqual(new[] { 2, 36, 258, 320 }, mapping);
            Assert.AreEqual(2, ModelBuilder.LocalBoneIndex(mapping, 258));
            Assert.AreEqual(0, ModelBuilder.LocalBoneIndex(mapping, 2));
            Assert.AreEqual(3, ModelBuilder.LocalBoneIndex(mapping, 320));
            Assert.AreNotEqual(ModelBuilder.LocalBoneIndex(mapping, 258), ModelBuilder.LocalBoneIndex(mapping, 2));
        }

        [Test]
        public void UnweightedSlotsDoNotAddBones()
        {
            // A slot with weight 0 names bone 0, which this mesh does not use: it must not take a place in the mapping.
            var mapping = ModelBuilder.BoneMapping(new[] { Weight(200), Weight(201) }, 300, out _);
            CollectionAssert.AreEqual(new[] { 200, 201 }, mapping);
            Assert.AreEqual(0, ModelBuilder.LocalBoneIndex(mapping, 0));
        }

        [Test]
        public void TooManyUsedBonesWarns()
        {
            var weights = new BoneWeight[300];
            for (int i = 0; i < weights.Length; i++) weights[i] = Weight(i);
            var mapping = ModelBuilder.BoneMapping(weights, 300, out var warning);
            Assert.IsNull(mapping);
            StringAssert.Contains("300 bones", warning);
        }
    }
}
