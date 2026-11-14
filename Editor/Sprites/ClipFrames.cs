using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityToRebelFork.Editor
{
    /// <summary>A key of a sprite animation: from <see cref="Time"/> on the clip shows the sprite called <see cref="Sprite"/>.</summary>
    public struct ClipKey
    {
        public float Time;
        public string Sprite;
        public ClipKey(float time, string sprite) { Time = time; Sprite = sprite; }
    }

    /// <summary>An animation event of a clip.</summary>
    public struct ClipEvent
    {
        public float Time;
        public string Name;
        public ClipEvent(float time, string name) { Time = time; Name = name; }
    }

    /// <summary>A clip as the Toe engine plays it: sprite names at a fixed rate, looping or not, with events at frame numbers.</summary>
    public sealed class FrameList
    {
        public string Name;
        public List<string> Frames = new List<string>();
        public float Fps;
        public bool Loop;
        public List<KeyValuePair<int, string>> Events = new List<KeyValuePair<int, string>>();

        /// <summary>What was not exact (uneven key times that had to be rounded to the finest step).</summary>
        public List<string> Warnings = new List<string>();
    }

    /// <summary>
    /// Turns the keys of a Unity sprite animation into a Toe flipbook. Unity keys when a sprite starts; Toe has a rate and a list. Every key lasts until the next one, the
    /// last until the end of the clip; a key at the very end (the closing key that makes a loop wrap smoothly) lasts no time and is dropped. If the keys are an even
    /// number of steps apart the rate is one over the step and each sprite is one frame; otherwise a sprite is repeated for the steps it lasts.
    /// </summary>
    public static class ClipFrames
    {
        /// <summary>Keys closer than this to the next one (seconds) last no time.</summary>
        public const float MinKeyDuration = 1e-3f;

        public static FrameList Build(string name, IEnumerable<ClipKey> keys, float length, bool loop, IEnumerable<ClipEvent> events = null, int maxFrames = 512)
        {
            var sorted = keys.OrderBy(k => k.Time).ToList();
            var durations = new List<float>();
            var kept = new List<ClipKey>();
            for (int i = 0; i < sorted.Count; i++)
            {
                float end = i + 1 < sorted.Count ? sorted[i + 1].Time : Math.Max(length, sorted[i].Time);
                float duration = end - sorted[i].Time;
                if (duration < MinKeyDuration) continue;
                kept.Add(sorted[i]);
                durations.Add(duration);
            }
            if (kept.Count == 0) return null;

            // A key at the very end of the clip lasts no time. In a clip that loops, or that returns to the sprite it started with, it only closes the clip and is dropped. In a
            // clip that does not loop and ends on a sprite of its own (the last pose of a death animation) it is that pose: it is kept, for one step.
            ClipKey? finalPose = null;
            var lastKey = sorted[sorted.Count - 1];
            if (!loop && Math.Max(length, lastKey.Time) - lastKey.Time < MinKeyDuration
                && lastKey.Sprite != sorted[0].Sprite && lastKey.Sprite != kept[kept.Count - 1].Sprite)
                finalPose = lastKey;

            var result = new FrameList { Name = name, Loop = loop };
            // Each key is at least one frame, so more keys than frames can never fit however coarse the step: only the first ones are kept.
            if (kept.Count > maxFrames)
            {
                result.Warnings.Add("the clip has " + kept.Count + " keys; only the first " + maxFrames + " are exported");
                kept = kept.GetRange(0, maxFrames);
                durations = durations.GetRange(0, maxFrames);
            }
            float step = durations.Min();
            if (finalPose.HasValue && kept.Count < maxFrames)
            {
                kept.Add(finalPose.Value);
                durations.Add(step);
            }
            var counts = new List<int>();
            bool exact = true;
            for (int i = 0; i < durations.Count; i++)
            {
                int n = Math.Max(1, (int)Math.Round(durations[i] / step));
                if (Math.Abs(durations[i] - n * step) > 0.05f * step) exact = false;
                counts.Add(n);
            }
            if (!exact)
                result.Warnings.Add("the keys are not an even number of steps apart; the times are rounded to steps of " + Math.Round(step, 4) + " s");

            // Never more than maxFrames: a clip with a very fine step makes a coarser flipbook.
            while (counts.Sum() > maxFrames)
            {
                step *= 2f;
                for (int i = 0; i < counts.Count; i++) counts[i] = Math.Max(1, (int)Math.Round(durations[i] / step));
                result.Warnings.Add("more than " + maxFrames + " frames; the step was doubled to " + Math.Round(step, 4) + " s");
            }

            result.Fps = (float)Math.Round(1.0 / step, 3);
            for (int i = 0; i < kept.Count; i++)
            {
                for (int n = 0; n < counts[i]; n++) result.Frames.Add(kept[i].Sprite);
            }
            if (events != null)
            {
                foreach (var e in events.OrderBy(e => e.Time))
                {
                    int frame = (int)Math.Floor(e.Time / step + 1e-4);
                    frame = Math.Min(Math.Max(frame, 0), result.Frames.Count - 1);
                    result.Events.Add(new KeyValuePair<int, string>(frame, e.Name));
                }
            }
            return result;
        }
    }

    /// <summary>Writes the animation set file of the Toe engine (<c>SpriteAnimationSet</c>).</summary>
    public static class SpriteAnimationJson
    {
        /// <summary>
        /// <code>{ "sheet": "Wolf.json",
        ///   "clips": { "Idle": { "frames": ["Idle_0", "Idle_1"], "fps": 4, "loop": true, "events": [{ "frame": 1, "name": "hit" }] } },
        ///   "states": { "idle": "Idle" } }</code>
        /// </summary>
        public static string Build(string sheetPath, IReadOnlyList<FrameList> clips, IReadOnlyList<KeyValuePair<string, string>> states)
        {
            var json = new StringBuilder();
            json.Append("{ \"sheet\": ").Append(SpriteSheetJson.Quote(sheetPath)).AppendLine(",");
            json.AppendLine("  \"clips\": {");
            for (int i = 0; i < clips.Count; i++)
            {
                var c = clips[i];
                json.Append("    ").Append(SpriteSheetJson.Quote(c.Name)).Append(": { \"frames\": [")
                    .Append(string.Join(", ", c.Frames.Select(SpriteSheetJson.Quote))).Append("], \"fps\": ")
                    .Append(SpriteSheetJson.Number(c.Fps)).Append(", \"loop\": ").Append(c.Loop ? "true" : "false");
                if (c.Events.Count > 0)
                {
                    json.Append(", \"events\": [").Append(string.Join(", ", c.Events.Select(e =>
                        "{ \"frame\": " + e.Key.ToString(System.Globalization.CultureInfo.InvariantCulture) + ", \"name\": " + SpriteSheetJson.Quote(e.Value) + " }"))).Append("]");
                }
                json.Append(" }").AppendLine(i + 1 < clips.Count ? "," : string.Empty);
            }
            json.AppendLine("  },");
            json.AppendLine("  \"states\": {");
            for (int i = 0; i < states.Count; i++)
            {
                json.Append("    ").Append(SpriteSheetJson.Quote(states[i].Key)).Append(": ").Append(SpriteSheetJson.Quote(states[i].Value))
                    .AppendLine(i + 1 < states.Count ? "," : string.Empty);
            }
            json.AppendLine("  }");
            json.AppendLine("}");
            return json.ToString();
        }
    }
}
