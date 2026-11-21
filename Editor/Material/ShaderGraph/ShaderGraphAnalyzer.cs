using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace UnityToRebelFork.Editor.ShaderGraphs
{
    /// <summary>A value a master block takes from one channel of one texture, or a constant.</summary>
    public sealed class ScalarSource
    {
        /// <summary>The identity of the texture, or null when the value is the constant.</summary>
        public string Texture;

        /// <summary>The texture channel (0 R, 1 G, 2 B, 3 A).</summary>
        public int Channel;

        /// <summary>The value the block takes when the channel is 0 and when it is 1.</summary>
        public float AtZero, AtOne;

        public float Constant;
    }

    /// <summary>What the graph, fed with a material's values, makes of its textures: which one is the albedo, the normal map, the mask channels, and the constants around them.</summary>
    public sealed class ShaderGraphAnalysis
    {
        /// <summary>The albedo texture (null when the colour is constant) and the colour it is multiplied by (linear, W is the alpha factor).</summary>
        public string Albedo;
        public Val AlbedoTint = new Val(1, 1, 1, 1);

        /// <summary>The graph inverts the albedo (or offsets it): the colour at texel 0 and at texel 1 of each channel (linear) replace the multiplier.</summary>
        public bool AlbedoInverted;
        public Val AlbedoAtZero, AlbedoAtOne;

        /// <summary>Where alpha comes from: a texture channel and what alpha is at its ends, or the constant.</summary>
        public ScalarSource Alpha = new ScalarSource { Constant = 1f };

        /// <summary>The alpha is the albedo's own alpha channel, unchanged.</summary>
        public bool AlphaFromAlbedo => Alpha.Texture != null && Alpha.Texture == Albedo && Alpha.Channel == 3 && !AlbedoInverted
                                        && Math.Abs(Alpha.AtZero) < 0.02f && Math.Abs(Alpha.AtOne - 1f) < 0.02f;

        /// <summary>The coordinate transform of the albedo: u' = UA*u + UB*v + UC, v' = VA*u + VB*v + VC.</summary>
        public bool HasUv;
        public float UA = 1, UB, UC, VA, VB = 1, VC;

        public float AlphaCutoff = 0.5f;

        public string Normal;
        public float NormalScale = 1f;

        /// <summary>Roughness (1 - smoothness), metallic and occlusion.</summary>
        public ScalarSource Roughness = new ScalarSource { Constant = 0.5f };
        public ScalarSource Metallic = new ScalarSource();
        public ScalarSource Occlusion = new ScalarSource { Constant = 1f };

        public string Emission;

        /// <summary>The emission colour (linear): a multiplier of the emission texture, or the constant colour.</summary>
        public Val EmissionTint;

        public readonly List<string> Notes = new List<string>();
        public readonly HashSet<string> Warnings = new HashSet<string>();
    }

    /// <summary>
    /// Finds out what a Shader Graph makes of a material by feeding it: every texture is replaced, channel by channel, by 0 and 1 (and is otherwise its mean colour) and the change
    /// of each master block (BaseColor, NormalTS, Metallic...) says how much that channel matters. A texture whose red, green and blue move the colour output one to one is
    /// the albedo; one that moves the normal's x and y is the normal map; the channel that moves smoothness most is the smoothness mask. A texture the material's values switch off
    /// (a second layer at strength 0, a mask that is not used) has no influence and is left out.
    /// </summary>
    public sealed class ShaderGraphAnalyzer
    {
        private const float MinColorInfluence = 0.02f;
        private const float MinNormalInfluence = 0.05f;
        private const float MinMaskInfluence = 0.05f;

        private static readonly string[] Blocks = { "BaseColor", "NormalTS", "Metallic", "Smoothness", "Occlusion", "Emission", "Alpha", "AlphaClipThreshold" };

        private readonly ShaderGraphDocument doc;
        private readonly IShaderGraphInputs inputs;
        private readonly Func<string, ShaderGraphDocument> subGraphs;
        private readonly HashSet<string> warnings = new HashSet<string>();

        public ShaderGraphAnalyzer(ShaderGraphDocument doc, IShaderGraphInputs inputs, Func<string, ShaderGraphDocument> subGraphs)
        {
            this.doc = doc;
            this.inputs = inputs;
            this.subGraphs = subGraphs;
        }

        private sealed class Perturbed : IShaderGraphInputs
        {
            private readonly IShaderGraphInputs inner;
            private readonly string texture;
            private readonly int channel;
            private readonly float level;

            public Perturbed(IShaderGraphInputs inner, string texture, int channel, float level)
            {
                this.inner = inner;
                this.texture = texture;
                this.channel = channel;
                this.level = level;
            }

            public bool TryGetValue(SgProperty property, out Val value) => inner.TryGetValue(property, out value);
            public string GetTexture(SgProperty property) => inner.GetTexture(property);
            public bool IsKeywordEnabled(string reference) => inner.IsKeywordEnabled(reference);

            public Val Sample(string tex, SgSampleKind kind, Val uv)
            {
                var texel = inner.Sample(tex, kind, uv);
                if (tex == texture) texel[channel] = level;
                return texel;
            }
        }

        private sealed class Response
        {
            public float Lo, Hi;
            public Dictionary<string, Val> AtLo, AtHi;

            /// <summary>The change of a block per unit of the texel channel.</summary>
            public Val Slope(string block) => Val.Map(AtHi[block], AtLo[block], (h, l) => (h - l) / (Hi - Lo));
        }

        private static Val BlockDefault(string name)
        {
            switch (name)
            {
                case "BaseColor": return new Val(0.5f, 0.5f, 0.5f, 0f);
                case "NormalTS": return new Val(0f, 0f, 1f, 0f);
                case "Smoothness": return Val.Splat(0.5f);
                case "Occlusion": return Val.Splat(1f);
                case "Alpha": return Val.Splat(1f);
                case "AlphaClipThreshold": return Val.Splat(0.5f);
                default: return default;
            }
        }

        private Dictionary<string, Val> EvaluateBlocks(IShaderGraphInputs source, Val uv, List<SgSampleRecord> samples, out HashSet<string> connected)
        {
            var evaluator = new ShaderGraphEvaluator(doc, source, subGraphs, warnings) { Uv = uv };
            var result = new Dictionary<string, Val>();
            connected = new HashSet<string>();
            foreach (var block in Blocks)
            {
                if (evaluator.EvaluateBlock(block, out var value))
                    connected.Add(block);
                else if (!doc.Blocks.ContainsKey(block))
                    value = BlockDefault(block);
                result[block] = value;
            }
            if (samples != null) samples.AddRange(evaluator.Samples);
            return result;
        }

        public ShaderGraphAnalysis Analyze()
        {
            var analysis = new ShaderGraphAnalysis();
            var samples = new List<SgSampleRecord>();
            var nominalUv = new Val(0.5f, 0.5f, 0f, 0f);
            var nominal = EvaluateBlocks(inputs, nominalUv, samples, out var connected);

            var textures = new List<string>();
            var normalKind = new HashSet<string>();
            foreach (var sample in samples)
            {
                if (sample.Texture.StartsWith("#none", StringComparison.Ordinal)) continue;
                if (!textures.Contains(sample.Texture)) textures.Add(sample.Texture);
                if (sample.Kind == SgSampleKind.Normal) normalKind.Add(sample.Texture);
            }

            // Every channel of every texture at the two ends of its range.
            var responses = new Dictionary<string, Response[]>();
            foreach (var texture in textures)
            {
                var lo = normalKind.Contains(texture) ? 0.25f : 0f;
                var hi = normalKind.Contains(texture) ? 0.75f : 1f;
                var perChannel = new Response[4];
                for (var channel = 0; channel < 4; channel++)
                {
                    perChannel[channel] = new Response
                    {
                        Lo = lo,
                        Hi = hi,
                        AtLo = EvaluateBlocks(new Perturbed(inputs, texture, channel, lo), nominalUv, null, out _),
                        AtHi = EvaluateBlocks(new Perturbed(inputs, texture, channel, hi), nominalUv, null, out _),
                    };
                }
                responses[texture] = perChannel;
            }

            var report = new StringBuilder();
            foreach (var texture in textures)
            {
                var r = responses[texture];
                report.Append(texture).Append(": albedo ").Append(Format(ColorScore(r, "BaseColor"))).Append(", normal ").Append(Format(NormalScore(r, normalKind.Contains(texture))));
                var best = new[] { "Metallic", "Smoothness", "Occlusion", "Alpha", "Emission" }.Select(b => b + " " + Format(Enumerable.Range(0, 4).Max(c => MaxAbs(r[c].Slope(b))))).ToArray();
                report.Append(", ").Append(string.Join(", ", best)).Append("; ");
            }
            analysis.Notes.Add("influence: " + report);

            PickAlbedo(analysis, nominal, textures, normalKind, responses);
            PickNormal(analysis, connected, textures, normalKind, responses);
            PickScalars(analysis, nominal, connected, textures, normalKind, responses);
            PickEmission(analysis, nominal, textures, normalKind, responses);
            PickAlpha(analysis, nominal, textures, normalKind, responses);
            FindUvTransform(analysis, textures);

            foreach (var warning in warnings) analysis.Warnings.Add(warning);
            return analysis;
        }

        private static string Format(float v) => v.ToString("0.##", CultureInfo.InvariantCulture);

        private static float MaxAbs(Val v) => Math.Max(Math.Max(Math.Abs(v.X), Math.Abs(v.Y)), Math.Max(Math.Abs(v.Z), Math.Abs(v.W)));

        private static float ColorScore(Response[] r, string block) => ColorScore(r, block, out _);

        /// <summary>The score of a texture as a colour source: its red, green and blue move the same channels of the output, all up or all down (a graph that inverts the texture).</summary>
        private static float ColorScore(Response[] r, string block, out bool inverted)
        {
            var dr = r[0].Slope(block).X;
            var dg = r[1].Slope(block).Y;
            var db = r[2].Slope(block).Z;
            inverted = dr < 0f && dg < 0f && db < 0f;
            if (!inverted && !(dr > 0f && dg > 0f && db > 0f)) return 0f;
            dr = Math.Abs(dr);
            dg = Math.Abs(dg);
            db = Math.Abs(db);
            return (dr + dg + db) * (Math.Min(dr, Math.Min(dg, db)) / Math.Max(dr, Math.Max(dg, db)));
        }

        private static float NormalScore(Response[] r, bool normalKind)
        {
            // n.x = 2 * r - 1 for the node's own unpacking: a slope of 2 per channel means a full strength normal map.
            return Math.Abs(r[0].Slope("NormalTS").X) + Math.Abs(r[1].Slope("NormalTS").Y);
        }

        private void PickAlbedo(ShaderGraphAnalysis analysis, Dictionary<string, Val> nominal, List<string> textures, HashSet<string> normalKind, Dictionary<string, Response[]> responses)
        {
            // A texture that is the colour as it is wins over one the graph inverts (a mask that feeds the colour channels one to one looks like that).
            string best = null;
            var bestScore = MinColorInfluence;
            var bestInverted = true;
            foreach (var texture in textures)
            {
                if (normalKind.Contains(texture)) continue;
                var score = ColorScore(responses[texture], "BaseColor", out var textureInverted);
                if (score > bestScore && (bestInverted || !textureInverted) || bestInverted && !textureInverted && score > MinColorInfluence)
                {
                    best = texture;
                    bestScore = score;
                    bestInverted = textureInverted;
                }
            }

            var baseColor = nominal["BaseColor"];
            if (best == null)
            {
                analysis.AlbedoTint = new Val(baseColor.X, baseColor.Y, baseColor.Z, 1f);
                analysis.Notes.Add("albedo: constant colour " + baseColor);
                return;
            }

            analysis.Albedo = best;
            ColorScore(responses[best], "BaseColor", out var inverted);
            if (inverted)
            {
                // The graph turns the texture upside down (1 - colour): it is exported as a remapped texture, channel by channel, and the colour is not multiplied.
                var r = responses[best];
                analysis.AlbedoInverted = true;
                analysis.AlbedoAtZero = new Val(r[0].AtLo["BaseColor"].X, r[1].AtLo["BaseColor"].Y, r[2].AtLo["BaseColor"].Z, 0f);
                analysis.AlbedoAtOne = new Val(r[0].AtHi["BaseColor"].X, r[1].AtHi["BaseColor"].Y, r[2].AtHi["BaseColor"].Z, 1f);
                analysis.AlbedoTint = new Val(1f, 1f, 1f, 1f);
                analysis.Notes.Add("albedo: " + best + " (score " + Format(bestScore) + ") inverted by the graph: " + analysis.AlbedoAtZero + " .. " + analysis.AlbedoAtOne);
                return;
            }

            // The colour multiplier is what is left of the colour output when the texture is taken out: the output at the texture's mean over the mean.
            var mean = inputs.Sample(best, SgSampleKind.Default, new Val(0.5f, 0.5f, 0f, 0f));
            analysis.AlbedoTint = new Val(Ratio(baseColor.X, mean.X), Ratio(baseColor.Y, mean.Y), Ratio(baseColor.Z, mean.Z), 1f);
            analysis.Notes.Add("albedo: " + best + " (score " + Format(bestScore) + ") x " + analysis.AlbedoTint);
        }

        private static float Ratio(float value, float mean) => mean < 1e-3f ? 1f : Math.Min(value / mean, 8f);

        private void PickNormal(ShaderGraphAnalysis analysis, HashSet<string> connected, List<string> textures, HashSet<string> normalKind, Dictionary<string, Response[]> responses)
        {
            if (!connected.Contains("NormalTS")) return;
            string best = null;
            var bestScore = MinNormalInfluence;
            foreach (var texture in textures)
            {
                // The texture's red and green move the normal's x and y (one that is read as colour and used as a normal later on does as well).
                var r = responses[texture];
                if (Math.Abs(r[0].Slope("NormalTS").X) < MinNormalInfluence || Math.Abs(r[1].Slope("NormalTS").Y) < MinNormalInfluence) continue;
                var score = NormalScore(r, normalKind.Contains(texture));
                if (score > bestScore)
                {
                    best = texture;
                    bestScore = score;
                }
            }
            if (best == null) return;
            analysis.Normal = best;

            // A normal sample node unpacks the texel (n = 2 * texel - 1): a slope of 2 per channel is a normal map at full strength. A texture sampled as colour is
            // used as it is, so a slope of 1 is full strength.
            var perUnit = normalKind.Contains(best) ? 2f : 1f;
            analysis.NormalScale = Math.Min(bestScore / 2f / perUnit, 4f);
            analysis.Notes.Add("normal: " + best + " (score " + Format(bestScore) + ", scale " + Format(analysis.NormalScale) + ")");
        }

        private ScalarSource FindScalar(string block, bool invert, Dictionary<string, Val> nominal, List<string> textures, HashSet<string> normalKind, Dictionary<string, Response[]> responses, string note, ShaderGraphAnalysis analysis)
        {
            string bestTexture = null;
            var bestChannel = 0;
            var bestInfluence = MinMaskInfluence;
            foreach (var texture in textures)
            {
                if (normalKind.Contains(texture)) continue;
                var r = responses[texture];
                for (var channel = 0; channel < 4; channel++)
                {
                    var influence = Math.Abs(r[channel].AtHi[block].X - r[channel].AtLo[block].X);
                    if (influence > bestInfluence)
                    {
                        bestInfluence = influence;
                        bestTexture = texture;
                        bestChannel = channel;
                    }
                }
            }

            var value = nominal[block].X;
            if (bestTexture == null)
                return new ScalarSource { Constant = invert ? 1f - value : value };

            var response = responses[bestTexture][bestChannel];
            var atZero = response.AtLo[block].X;
            var atOne = response.AtHi[block].X;
            if (response.Lo != 0f || response.Hi != 1f)
            {
                // Extrapolate the measured slope to the ends of the range.
                var slope = (atOne - atZero) / (response.Hi - response.Lo);
                atZero -= slope * response.Lo;
                atOne = atZero + slope;
            }
            analysis.Notes.Add(note + ": " + bestTexture + "." + "rgba"[bestChannel] + " (" + Format(bestInfluence) + ")");
            return new ScalarSource
            {
                Texture = bestTexture,
                Channel = bestChannel,
                AtZero = invert ? 1f - atZero : atZero,
                AtOne = invert ? 1f - atOne : atOne,
                Constant = invert ? 1f - value : value,
            };
        }

        private void PickScalars(ShaderGraphAnalysis analysis, Dictionary<string, Val> nominal, HashSet<string> connected, List<string> textures, HashSet<string> normalKind, Dictionary<string, Response[]> responses)
        {
            analysis.Roughness = FindScalar("Smoothness", true, nominal, textures, normalKind, responses, "roughness", analysis);
            analysis.Metallic = FindScalar("Metallic", false, nominal, textures, normalKind, responses, "metallic", analysis);
            analysis.Occlusion = FindScalar("Occlusion", false, nominal, textures, normalKind, responses, "occlusion", analysis);
        }

        private void PickEmission(ShaderGraphAnalysis analysis, Dictionary<string, Val> nominal, List<string> textures, HashSet<string> normalKind, Dictionary<string, Response[]> responses)
        {
            var emission = nominal["Emission"];
            string best = null;
            var bestScore = MinColorInfluence;
            foreach (var texture in textures)
            {
                if (normalKind.Contains(texture)) continue;
                var score = ColorScore(responses[texture], "Emission");
                if (score > bestScore)
                {
                    best = texture;
                    bestScore = score;
                }
            }
            if (best == null)
            {
                analysis.EmissionTint = emission;
                if (MaxAbs(new Val(emission.X, emission.Y, emission.Z, 0f)) > 1e-4f)
                    analysis.Notes.Add("emission: constant " + emission);
                return;
            }
            var mean = inputs.Sample(best, SgSampleKind.Default, new Val(0.5f, 0.5f, 0f, 0f));
            analysis.Emission = best;
            analysis.EmissionTint = new Val(Ratio(emission.X, mean.X), Ratio(emission.Y, mean.Y), Ratio(emission.Z, mean.Z), 1f);
            analysis.Notes.Add("emission: " + best + " (score " + Format(bestScore) + ") x " + analysis.EmissionTint);
        }

        private void PickAlpha(ShaderGraphAnalysis analysis, Dictionary<string, Val> nominal, List<string> textures, HashSet<string> normalKind, Dictionary<string, Response[]> responses)
        {
            analysis.AlphaCutoff = nominal["AlphaClipThreshold"].X;

            // The channel that moves alpha most: the albedo's alpha channel, or a mask texture of its own (which the exporter then packs into the albedo's alpha).
            analysis.Alpha = FindScalar("Alpha", false, nominal, textures, normalKind, responses, "alpha", analysis);
            analysis.AlbedoTint.W = analysis.Alpha.Texture == null ? Math.Min(Math.Max(analysis.Alpha.Constant, 0f), 1f) : 1f;
            if (analysis.Alpha.Texture != null)
                analysis.Notes.Add("alpha: cutoff " + Format(analysis.AlphaCutoff));
        }

        private void FindUvTransform(ShaderGraphAnalysis analysis, List<string> textures)
        {
            var texture = analysis.Albedo ?? analysis.Normal;
            if (texture == null) return;

            Val At(float u, float v)
            {
                var samples = new List<SgSampleRecord>();
                EvaluateBlocks(inputs, new Val(u, v, 0f, 0f), samples, out _);
                foreach (var sample in samples)
                    if (sample.Texture == texture) return sample.Uv;
                return new Val(u, v, 0f, 0f);
            }

            var p00 = At(0f, 0f);
            var p10 = At(1f, 0f);
            var p01 = At(0f, 1f);
            analysis.UA = p10.X - p00.X;
            analysis.UB = p01.X - p00.X;
            analysis.UC = p00.X;
            analysis.VA = p10.Y - p00.Y;
            analysis.VB = p01.Y - p00.Y;
            analysis.VC = p00.Y;
            analysis.HasUv = Math.Abs(analysis.UA - 1f) > 1e-4f || Math.Abs(analysis.UB) > 1e-4f || Math.Abs(analysis.UC) > 1e-4f
                             || Math.Abs(analysis.VA) > 1e-4f || Math.Abs(analysis.VB - 1f) > 1e-4f || Math.Abs(analysis.VC) > 1e-4f;
            if (analysis.HasUv)
                analysis.Notes.Add("uv: u' = " + Format(analysis.UA) + "u + " + Format(analysis.UB) + "v + " + Format(analysis.UC) + ", v' = " + Format(analysis.VA) + "u + " + Format(analysis.VB) + "v + " + Format(analysis.VC));
        }
    }
}
