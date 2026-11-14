using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityToRebelFork.Editor;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace UnityToRebelFork
{
    [TestFixture]
    public class SpriteHashTests
    {
        [Test]
        public void TheHashOfALabelIsTheCrc32OfCategoryAndLabelWithTheTopBitsCleared()
        {
            // Read from the Hippo monsters: the animation's float m_SpriteKey 1.3971405e-28 holds the bits of this key, and the Sprite Library lists "Idle" / "0".
            Assert.AreEqual(288431053, SpriteHash.Key("Idle", "0"));
            Assert.AreEqual(288431053, SpriteHash.KeyFromCurveValue(1.3971405e-28f));
        }

#if UNITY_EDITOR
        // Needs Unity's own hash, so it only runs in the editor (the other tests of this file are plain C# and also run under dotnet test).
        [Test]
        public void TheHashAgreesWithUnitysOwn()
        {
            foreach (var text in new[] { "Idle", "Idle_0", "Attack_5", "Death_3", "Fire_2", "", "Ünïcode_1" })
                Assert.AreEqual(Animator.StringToHash(text) & 0x3FFFFFFF, SpriteHash.Hash30(text), text);
        }
#endif

        [Test]
        public void TheFullHashOfANameAloneIsTheSignedCrc32()
        {
            // The m_Hash of a category and of a label in a Sprite Library asset.
            Assert.AreEqual(2081823275, SpriteHash.Crc32("Idle"));
            Assert.AreEqual(-186917087, SpriteHash.Crc32("0"));
        }
    }

    [TestFixture]
    public class SpriteSheetJsonTests
    {
        [Test]
        public void UnityRectanglesAreFlippedToTopLeftAndPivotsAreMadeRelative()
        {
            // Idle_0 of a Hippo monster: the top row of a 192 x 160 sheet is y = 128 in Unity, 0 in Toe.
            var entry = SpriteEntry.FromUnity("Idle_0", 0, 128, 32, 32, 160, 16, 0, 16);

            Assert.AreEqual(0, entry.Y);
            Assert.AreEqual(0, entry.X);
            Assert.AreEqual(0.5f, entry.PivotX, 1e-6f);
            Assert.AreEqual(0f, entry.PivotY, 1e-6f);

            var bottom = SpriteEntry.FromUnity("Death_5", 160, 0, 32, 32, 160, 16, 0, 16);
            Assert.AreEqual(128, bottom.Y);
            Assert.AreEqual(160, bottom.X);
        }

        [Test]
        public void TheSheetListsSpritesWithTheirRectsAndLeavesOutWhatIsTheDefault()
        {
            var json = SpriteSheetJson.Build("Wolf.png", true, new List<SpriteEntry>
            {
                new SpriteEntry { Name = "a", X = 0, Y = 0, Width = 32, Height = 32, PivotX = 0.5f, PivotY = 0f, PixelsPerUnit = 16 },
                new SpriteEntry { Name = "b \"quoted\"", X = 32, Y = 0, Width = 32, Height = 32, PixelsPerUnit = 16 },
                new SpriteEntry { Name = "c", X = 64, Y = 0, Width = 16, Height = 16, PivotX = 0.25f, PivotY = 0.75f, PixelsPerUnit = 32 },
            });

            const string expected =
                "{ \"texture\": \"Wolf.png\", \"pixelsPerUnit\": 16, \"pixelArt\": true,\n" +
                "  \"sprites\": {\n" +
                "    \"a\": { \"rect\": [0, 0, 32, 32], \"pivot\": [0.5, 0] },\n" +
                "    \"b \\\"quoted\\\"\": { \"rect\": [32, 0, 32, 32] },\n" +
                "    \"c\": { \"rect\": [64, 0, 16, 16], \"pivot\": [0.25, 0.75], \"ppu\": 32 }\n" +
                "  }\n" +
                "}\n";
            Assert.AreEqual(expected, json.Replace("\r\n", "\n"));
        }

        [Test]
        public void NumbersDoNotDependOnTheCulture()
        {
            var old = System.Threading.Thread.CurrentThread.CurrentCulture;
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ru-RU");
                Assert.AreEqual("0.5", SpriteSheetJson.Number(0.5f));
                Assert.AreEqual("12", SpriteSheetJson.Number(12f));
                Assert.AreEqual("0.083333", SpriteSheetJson.Number(1f / 12f));
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = old;
            }
        }
    }

    [TestFixture]
    public class ClipFramesTests
    {
        private static IEnumerable<ClipKey> Keys(string prefix, int count, float step, bool closing, float length)
        {
            for (int i = 0; i < count; i++) yield return new ClipKey(i * step, prefix + "_" + i);
            if (closing) yield return new ClipKey(length, prefix + "_0"); // the key that makes a loop wrap, at the very end
        }

        [Test]
        public void ALoopingClipDropsItsClosingKey()
        {
            // Idle of the Hippo monsters: four sprites every 0.25 s and Idle_0 again at 1.0 s, which is the length of the clip.
            var clip = ClipFrames.Build("Idle", Keys("Idle", 4, 0.25f, true, 1.0f), 1.0f, true);

            Assert.AreEqual(new[] { "Idle_0", "Idle_1", "Idle_2", "Idle_3" }, clip.Frames);
            Assert.AreEqual(4f, clip.Fps);
            Assert.IsTrue(clip.Loop);
            Assert.IsEmpty(clip.Warnings);
        }

        [Test]
        public void TheRateIsRoundedSoTwelveFramesASecondStaysTwelve()
        {
            // Run: six sprites 0.0833334 s apart (a 60 Hz clip) and the closing key at 0.5 s.
            var clip = ClipFrames.Build("Run", Keys("Run", 6, 0.083333336f, true, 0.5f), 0.5f, true);

            Assert.AreEqual(6, clip.Frames.Count);
            Assert.AreEqual(12f, clip.Fps);
        }

        [Test]
        public void AClipThatDoesNotLoopKeepsItsFrames()
        {
            var clip = ClipFrames.Build("Attack", Keys("Attack", 5, 0.083333336f, true, 0.41666666f), 0.41666666f, false);

            Assert.AreEqual(5, clip.Frames.Count);
            Assert.AreEqual(12f, clip.Fps);
            Assert.IsFalse(clip.Loop);
        }

        [Test]
        public void ADeathClipKeepsItsFinalPoseAtTheVeryEnd()
        {
            // Die of the Hippo monsters: Death_0..4 every 1/12 s and Death_5 at 0.41666666 s, which is the length of the clip. Not a closing key but the last pose.
            var keys = Keys("Death", 5, 0.083333336f, false, 0f).Concat(new[] { new ClipKey(0.41666666f, "Death_5") });

            var clip = ClipFrames.Build("Die", keys, 0.41666666f, false);

            Assert.AreEqual(new[] { "Death_0", "Death_1", "Death_2", "Death_3", "Death_4", "Death_5" }, clip.Frames);
            Assert.AreEqual(12f, clip.Fps);
            Assert.IsFalse(clip.Loop);
        }

        [Test]
        public void AFinalKeyThatReturnsToTheFirstSpriteOrWrapsALoopIsStillDropped()
        {
            // A clip that does not loop but returns to where it started (Attack), and a loop whose closing key is some other sprite.
            var returning = ClipFrames.Build("Attack", Keys("Attack", 5, 0.1f, true, 0.5f), 0.5f, false);
            Assert.AreEqual(5, returning.Frames.Count);

            var looping = ClipFrames.Build("Spin", new[] { new ClipKey(0f, "a"), new ClipKey(0.1f, "b"), new ClipKey(0.2f, "c") }, 0.2f, true);
            Assert.AreEqual(new[] { "a", "b" }, looping.Frames);
        }

        [Test]
        public void TheLastKeyLastsUntilTheEndOfTheClip()
        {
            // Without a closing key the last sprite is shown to the end of the clip: here as long as the others.
            var clip = ClipFrames.Build("Die", Keys("Death", 6, 0.1f, false, 0.6f), 0.6f, false);

            Assert.AreEqual(6, clip.Frames.Count);
            Assert.AreEqual(10f, clip.Fps);
        }

        [Test]
        public void KeysAWholeNumberOfStepsApartRepeatTheSpriteForTheStepsItLasts()
        {
            var clip = ClipFrames.Build("Blink", new[] { new ClipKey(0f, "open"), new ClipKey(0.1f, "half"), new ClipKey(0.3f, "closed") }, 0.4f, true);

            Assert.AreEqual(new[] { "open", "half", "half", "closed" }, clip.Frames);
            Assert.AreEqual(10f, clip.Fps);
            Assert.IsEmpty(clip.Warnings);
        }

        [Test]
        public void UnevenKeysAreRoundedAndSaySo()
        {
            var clip = ClipFrames.Build("Odd", new[] { new ClipKey(0f, "a"), new ClipKey(0.1f, "b") }, 0.26f, true);

            Assert.AreEqual(10f, clip.Fps);
            Assert.AreEqual(new[] { "a", "b", "b" }, clip.Frames); // 0.16 s is 1.6 steps: rounded to 2
            Assert.AreEqual(1, clip.Warnings.Count);
        }

        [Test]
        public void EventsLandOnTheFrameTheyAreIn()
        {
            var clip = ClipFrames.Build("Attack", Keys("Attack", 5, 0.1f, false, 0.5f), 0.5f, false,
                new[] { new ClipEvent(0.2f, "hit"), new ClipEvent(0.0f, "start"), new ClipEvent(9f, "late") });

            Assert.AreEqual(3, clip.Events.Count);
            Assert.AreEqual(new KeyValuePair<int, string>(0, "start"), clip.Events[0]);
            Assert.AreEqual(new KeyValuePair<int, string>(2, "hit"), clip.Events[1]);
            Assert.AreEqual(new KeyValuePair<int, string>(4, "late"), clip.Events[2]); // clamped to the last frame
        }

        [Test]
        public void NoKeysOrOnlyAnEmptyOneMakeNoClip()
        {
            Assert.IsNull(ClipFrames.Build("none", new ClipKey[0], 1f, true));
            Assert.IsNull(ClipFrames.Build("zero", new[] { new ClipKey(1f, "a") }, 1f, true)); // the one key is at the end: it lasts no time
        }

        [Test]
        public void AVeryFineStepMakesACoarserFlipbookInsteadOfThousandsOfFrames()
        {
            var keys = Enumerable.Range(0, 2000).Select(i => new ClipKey(i * 0.001f, "f" + i)).ToList();

            var clip = ClipFrames.Build("fine", keys, 2f, true, null, 512);

            Assert.LessOrEqual(clip.Frames.Count, 512);
            Assert.IsNotEmpty(clip.Warnings);
        }
    }

    [TestFixture]
    public class SpriteAnimationJsonTests
    {
        [Test]
        public void TheSetListsClipsWithTheirEventsAndTheStateMap()
        {
            var idle = new FrameList { Name = "Idle", Fps = 4, Loop = true, Frames = { "Idle_0", "Idle_1" } };
            var attack = new FrameList
            {
                Name = "Attack", Fps = 12, Loop = false, Frames = { "Attack_0", "Attack_1" },
                Events = { new KeyValuePair<int, string>(1, "hit") },
            };

            var json = SpriteAnimationJson.Build("Wolf.json", new[] { idle, attack },
                new[] { new KeyValuePair<string, string>("idle", "Idle"), new KeyValuePair<string, string>("attack", "Attack") });

            const string expected =
                "{ \"sheet\": \"Wolf.json\",\n" +
                "  \"clips\": {\n" +
                "    \"Idle\": { \"frames\": [\"Idle_0\", \"Idle_1\"], \"fps\": 4, \"loop\": true },\n" +
                "    \"Attack\": { \"frames\": [\"Attack_0\", \"Attack_1\"], \"fps\": 12, \"loop\": false, \"events\": [{ \"frame\": 1, \"name\": \"hit\" }] }\n" +
                "  },\n" +
                "  \"states\": {\n" +
                "    \"idle\": \"Idle\",\n" +
                "    \"attack\": \"Attack\"\n" +
                "  }\n" +
                "}\n";
            Assert.AreEqual(expected, json.Replace("\r\n", "\n"));
        }
    }
}
