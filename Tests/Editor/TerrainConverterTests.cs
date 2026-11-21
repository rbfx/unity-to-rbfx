using System.Linq;
using NUnit.Framework;
using UnityToRebelFork.Editor.Terrains;

namespace UnityToRebelFork
{
    /// <summary>Plain C# (no Unity types): which layers of a terrain the blend material takes and how the weights picture holds them.</summary>
    [TestFixture]
    public class TerrainConverterTests
    {
        private const int Alpha = 4; // alphamap samples across

        /// <summary>A terrain with the given layer for each of the 4 x 4 alphamap pixels (all the weight on that layer), row by row from the south.</summary>
        private static TerrainSourceData Terrain(int layers, params int[] layerOfPixel)
        {
            const int n = 5;
            var source = new TerrainSourceData
            {
                Resolution = n,
                Heights = new float[n * n],
                SizeX = 100,
                SizeY = 20,
                SizeZ = 100,
                AlphamapResolution = Alpha,
                LayerCount = layers,
                Alphamaps = new float[Alpha * Alpha * layers],
                TileSizes = Enumerable.Repeat(10f, layers * 2).ToArray(),
            };
            for (int p = 0; p < Alpha * Alpha; p++)
                source.Alphamaps[p * layers + layerOfPixel[p]] = 1f;
            return source;
        }

        // 16 pixels: layer 3 paints 8, layer 4 paints 5, layer 0 paints 2, layer 1 paints 1, layer 2 none.
        private static readonly int[] FiveLayers = { 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 0, 0, 1 };

        /// <summary>The weights picture's RGB of the alphamap pixel (z from the south, x), whose first row is the north edge.</summary>
        private static byte[] Pixel(TerrainResult result, int z, int x)
        {
            int o = ((Alpha - 1 - z) * Alpha + x) * 3;
            return new[] { result.WeightsRgb[o], result.WeightsRgb[o + 1], result.WeightsRgb[o + 2] };
        }

        [Test]
        public void TheThreeLayersThatPaintMostOfTheTerrainAreExportedMostUsedFirst()
        {
            var result = TerrainConverter.Convert(Terrain(5, FiveLayers));

            Assert.AreEqual(3, result.UsedLayers);
            CollectionAssert.AreEqual(new[] { 3, 4, 0 }, result.LayerOrder);
            Assert.AreEqual(0.5f, result.LayerCoverage[3], 1e-5f);
            Assert.AreEqual(5f / 16f, result.LayerCoverage[4], 1e-5f);
            Assert.AreEqual(0f, result.LayerCoverage[2], 1e-5f);
        }

        [Test]
        public void TheWeightsPictureHoldsTheChosenLayersAsRedGreenAndBlue()
        {
            var result = TerrainConverter.Convert(Terrain(5, FiveLayers));

            CollectionAssert.AreEqual(new byte[] { 255, 0, 0 }, Pixel(result, 0, 0)); // pixel 0: layer 3, the first slot
            CollectionAssert.AreEqual(new byte[] { 0, 255, 0 }, Pixel(result, 2, 0)); // pixel 8: layer 4, the second
            CollectionAssert.AreEqual(new byte[] { 0, 0, 255 }, Pixel(result, 3, 1)); // pixel 13: layer 0, the third
        }

        [Test]
        public void APixelPaintedOnlyWithADroppedLayerShowsTheFirstChosenLayer()
        {
            var result = TerrainConverter.Convert(Terrain(5, FiveLayers));

            CollectionAssert.AreEqual(new byte[] { 255, 0, 0 }, Pixel(result, 3, 3)); // pixel 15: layer 1, not exported
        }

        [Test]
        public void ATerrainWithThreeLayersOrFewerKeepsThemInTheirOwnOrder()
        {
            var two = new int[16];
            for (int p = 8; p < 16; p++) two[p] = 1;
            var result = TerrainConverter.Convert(Terrain(2, two));

            CollectionAssert.AreEqual(new[] { 0, 1 }, result.LayerOrder);
            CollectionAssert.AreEqual(new byte[] { 255, 0, 0 }, Pixel(result, 0, 0));
            CollectionAssert.AreEqual(new byte[] { 0, 255, 0 }, Pixel(result, 3, 3));
            Assert.IsFalse(result.Warnings.Any(_ => _.Contains("layers")));

            var three = Terrain(3, new[] { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 1, 1 });
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, TerrainConverter.Convert(three).LayerOrder);
        }

        [Test]
        public void TheWarningNamesTheExportedLayersAndWhatTheyPaint()
        {
            var result = TerrainConverter.Convert(Terrain(5, FiveLayers));

            var warning = result.Warnings.Single(_ => _.Contains("5 layers"));
            StringAssert.Contains("layers 3, 4, 0", warning);
            StringAssert.Contains("93.8 %", warning); // 15 of 16 pixels
        }

        [Test]
        public void TheDetailTilingFollowsTheFirstChosenLayerAndOnlyChosenLayersMustAgree()
        {
            var source = Terrain(5, FiveLayers);
            source.TileSizes[3 * 2] = source.TileSizes[3 * 2 + 1] = 20f; // layer 3, the first slot
            source.TileSizes[2 * 2] = source.TileSizes[2 * 2 + 1] = 5f;  // layer 2, not exported
            source.TileSizes[4 * 2] = source.TileSizes[4 * 2 + 1] = 20f;
            source.TileSizes[0] = source.TileSizes[1] = 20f;

            var result = TerrainConverter.Convert(source);

            Assert.AreEqual(5f, result.DetailTilingX, 1e-4f); // 100 / 20
            Assert.IsFalse(result.Warnings.Any(_ => _.Contains("different tile sizes")));

            source.TileSizes[0] = source.TileSizes[1] = 40f; // layer 0 is exported
            Assert.IsTrue(TerrainConverter.Convert(source).Warnings.Any(_ => _.Contains("different tile sizes")));
        }

        [Test]
        public void TiesKeepTheLowerLayerFirst()
        {
            // Four layers painting four pixels each: the first three.
            var layerOfPixel = Enumerable.Range(0, 16).Select(p => p / 4).ToArray();
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, TerrainConverter.ChooseLayers(Terrain(4, layerOfPixel), out _));
        }
    }
}
