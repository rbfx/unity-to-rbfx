#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace UnityToRebelFork.Editor.Particles
{
    /// <summary>The effect file made from a particle system, and what could not be converted.</summary>
    public sealed class ConvertedEffect
    {
        public string Xml = "";
        public List<string> Warnings = new List<string>();
        public List<string> NodesUsed = new List<string>();
    }

    /// <summary>
    /// Turns the settings of a Unity ParticleSystem (<see cref="ParticleSystemData"/>) into one layer of an rbfx/Toe particle graph effect:
    /// the emit graph makes particles (rate over time, bursts), the init graph gives them their start values (lifetime, shape, speed, size, rotation,
    /// color) and the update graph moves and draws them (forces, over-lifetime modules, texture sheet, renderer). Every Unity value that varies (a
    /// MinMaxCurve or MinMaxGradient) becomes nodes: constants, random numbers, curves sampled by the age of the particle, and lerps between two of them
    /// by a random number the particle keeps.
    /// </summary>
    public sealed class ParticleSystemConverter
    {
        private readonly ParticleSystemData _d;
        private readonly GraphBuilder _emit = new GraphBuilder();
        private readonly GraphBuilder _init = new GraphBuilder();
        private readonly GraphBuilder _update = new GraphBuilder();
        private readonly ConvertedEffect _result;
        private readonly HashSet<string> _randoms = new HashSet<string>();

        // A sub emitter is a layer that is driven by the particles of the layer of its parent system.
        private readonly SubEmitterData _link;
        private readonly ParticleSystemData _parent;
        private readonly int _sourceIndex;
        private readonly string _prefix;
        private readonly float _inheritedPrewarm;
        private readonly bool _needsColorNow, _needsSizeNow;

        private ParticleSystemConverter(ParticleSystemData data, ConvertedEffect result, SubEmitterData link, ParticleSystemData parent, int sourceIndex, string prefix, float inheritedPrewarm)
        {
            _d = data;
            _result = result;
            _link = link;
            _parent = parent;
            _sourceIndex = sourceIndex;
            _prefix = prefix;
            _inheritedPrewarm = inheritedPrewarm;
            // The children that inherit the colour or size of a particle take the values it has when the event happens, which only the update graph knows.
            foreach (var sub in data.SubEmitters)
            {
                if (!sub.IsSupported) continue;
                _needsColorNow |= (sub.Inherit & SubEmitterData.InheritColor) != 0;
                _needsSizeNow |= (sub.Inherit & SubEmitterData.InheritSize) != 0;
            }
        }

        private bool Driven => _link != null;

        /// <summary>The system collides with something the runtime can do: planes it has, or the world.</summary>
        private bool CollisionActive => CollisionOf(_d);

        private static bool CollisionOf(ParticleSystemData d)
            => d.CollisionEnabled && (d.CollisionType == CollisionType.World || d.CollisionPlanes.Count > 0);

        /// <summary>Part of the lifetime lost at a collision, below 1 (1 or more removes the particle: the node does it).</summary>
        private bool PartialLifetimeLoss => CollisionActive && ConstantOf(_d.CollisionLifetimeLoss) > 1e-6f && ConstantOf(_d.CollisionLifetimeLoss) < 1f;

        /// <summary>The value of a curve that is used as a number: the constant, the middle of two constants, the mean of a curve.</summary>
        private float ConstantOf(CurveData c)
        {
            switch (c.Mode)
            {
                case CurveMode.Constant: return c.Constant;
                case CurveMode.TwoConstants: return (c.ConstantMin + c.ConstantMax) * 0.5f;
                default: return MeanOf(c);
            }
        }

        /// <summary>
        /// The effect of a system: its layer, then (depth first) the layers of its sub emitters, which are driven by the particles of the layer
        /// before them. Only birth and death sub emitters are converted.
        /// </summary>
        public static ConvertedEffect Convert(ParticleSystemData data)
        {
            var result = new ConvertedEffect();
            var layers = new List<XElement>();
            AddLayers(result, layers, data, null, null, -1, "", float.NaN);
            var document = new XDocument(new XElement("particleGraphEffect", new XElement("layers", layers)));
            result.Xml = document.Declaration + document.ToString();
            result.NodesUsed = layers.SelectMany(l => l.Descendants("node")).Select(n => (string)n.Attribute("name")).Distinct().OrderBy(n => n, StringComparer.Ordinal).ToList();
            return result;
        }

        private static void AddLayers(ConvertedEffect result, List<XElement> layers, ParticleSystemData data, SubEmitterData link, ParticleSystemData parent,
            int sourceIndex, string prefix, float prewarm)
        {
            int index = layers.Count;
            var converter = new ParticleSystemConverter(data, result, link, parent, sourceIndex, prefix, prewarm);
            layers.Add(converter.Run());
            foreach (var sub in data.SubEmitters)
            {
                if (sub.IsSupported && sub.Type == SubEmitterType.Collision && !converter.CollisionActive)
                {
                    result.Warnings.Add(prefix + "the sub emitter '" + sub.Child.Name + "' is not exported: its Collision event needs a collision module that is on, with planes");
                    continue;
                }
                if (!sub.IsSupported)
                {
                    result.Warnings.Add(prefix + "the sub emitter '" + sub.Child.Name + "' is not exported: its " + sub.Type + " event needs "
                                        + (sub.Type == SubEmitterType.Collision ? "collision, which is not supported" : "a script to trigger it"));
                    continue;
                }
                AddLayers(result, layers, sub.Child, sub, data, index, prefix + "sub emitter '" + sub.Child.Name + "': ", converter._prewarmSeconds);
            }
        }

        private float _prewarmSeconds;

        private void Warn(string message) => _result.Warnings.Add(_prefix + message);

        // ---- Values that vary ----

        /// <summary>A value of the curve at a point of its range, as a number the nodes can use. <paramref name="t"/> is made when needed.</summary>
        private Pin Evaluate(GraphBuilder g, CurveData c, Func<Pin> t, Func<Pin> random)
        {
            switch (c.Mode)
            {
                case CurveMode.Constant:
                    return g.Const(c.Constant);
                case CurveMode.TwoConstants:
                    return random == null ? g.Random(c.ConstantMin, c.ConstantMax) : g.Lerp(g.Const(c.ConstantMin), g.Const(c.ConstantMax), random());
                case CurveMode.Curve:
                    return g.Curve(t(), "Float", FloatKeys(c.Curve, c.Multiplier));
                default:
                {
                    var upper = g.Curve(t(), "Float", FloatKeys(c.Curve, c.Multiplier));
                    var lower = g.Curve(t(), "Float", FloatKeys(c.CurveMin ?? c.Curve, c.Multiplier));
                    return g.Lerp(lower, upper, random != null ? random() : g.Random(0f, 1f));
                }
            }
        }

        /// <summary>A start value: a curve is read along the duration of the system.</summary>
        private Pin StartValue(CurveData c)
        {
            Pin time = default;
            return Evaluate(_init, c, () => time.IsValid ? time : time = _init.NormalizedEffectTime(_d.Duration, _d.Looping), null);
        }

        /// <summary>A value over the lifetime of a particle: a curve is read at its age, and two of them are blended by the random number of the particle.</summary>
        private Pin OverLife(CurveData c, Pin age, string randomName)
            => Evaluate(_update, c, () => age, () => RandomAttribute(randomName));

        /// <summary>The random number (0 to 1) a particle keeps under a name.</summary>
        private Pin RandomAttribute(string name)
        {
            if (_randoms.Add(name)) _init.Set(name, "Float", _init.Random(0f, 1f));
            return _update.Get(name, "Float");
        }

        // ---- Curves and gradients as keys ----

        private static List<float[]> FloatKeys(KeyData[] keys, float multiplier)
        {
            var result = new List<float[]>();
            if (keys == null || keys.Length == 0)
            {
                result.Add(new[] { 0f, 0f });
                return result;
            }
            if (keys.Length == 1)
            {
                result.Add(new[] { keys[0].Time, keys[0].Value * multiplier });
                return result;
            }
            for (int i = 0; i < keys.Length - 1; i++)
            {
                var a = keys[i];
                var b = keys[i + 1];
                float dt = b.Time - a.Time;
                bool step = float.IsInfinity(a.OutTangent) || float.IsInfinity(b.InTangent);
                const int segments = 6;
                if (i == 0) result.Add(new[] { a.Time, a.Value * multiplier });
                if (step)
                {
                    if (dt > 1e-5f) result.Add(new[] { b.Time - 1e-4f, a.Value * multiplier });
                }
                else
                {
                    for (int s = 1; s < segments; s++)
                    {
                        float t = (float)s / segments;
                        float t2 = t * t, t3 = t2 * t;
                        float value = (2 * t3 - 3 * t2 + 1) * a.Value + (t3 - 2 * t2 + t) * dt * a.OutTangent
                                      + (-2 * t3 + 3 * t2) * b.Value + (t3 - t2) * dt * b.InTangent;
                        result.Add(new[] { a.Time + dt * t, value * multiplier });
                    }
                }
                result.Add(new[] { b.Time, b.Value * multiplier });
            }
            return result;
        }

        private static float[] GradientAt(GradientData g, float time)
        {
            float[] rgb = { 1f, 1f, 1f };
            float alpha = 1f;
            if (g.Colors.Length > 0)
            {
                var colors = g.Colors;
                int i = 0;
                while (i < colors.Length - 1 && colors[i + 1].Time <= time) i++;
                var a = colors[i];
                var b = colors[Math.Min(i + 1, colors.Length - 1)];
                float t = g.Fixed || b.Time <= a.Time || time <= a.Time ? 0f : Math.Min(1f, (time - a.Time) / (b.Time - a.Time));
                if (time < colors[0].Time) { a = colors[0]; b = a; t = 0f; }
                rgb = new[] { a.R + (b.R - a.R) * t, a.G + (b.G - a.G) * t, a.B + (b.B - a.B) * t };
            }
            if (g.Alphas.Length > 0)
            {
                var alphas = g.Alphas;
                int i = 0;
                while (i < alphas.Length - 1 && alphas[i + 1].Time <= time) i++;
                var a = alphas[i];
                var b = alphas[Math.Min(i + 1, alphas.Length - 1)];
                float t = g.Fixed || b.Time <= a.Time || time <= a.Time ? 0f : Math.Min(1f, (time - a.Time) / (b.Time - a.Time));
                if (time < alphas[0].Time) { a = alphas[0]; b = a; t = 0f; }
                alpha = a.A + (b.A - a.A) * t;
            }
            return new[] { time, rgb[0], rgb[1], rgb[2], alpha };
        }

        private static List<float[]> ColorKeys(GradientData g)
        {
            var times = new SortedSet<float> { 0f, 1f };
            foreach (var key in g.Colors) times.Add(Math.Min(1f, Math.Max(0f, key.Time)));
            foreach (var key in g.Alphas) times.Add(Math.Min(1f, Math.Max(0f, key.Time)));
            var result = new List<float[]>();
            float previous = float.NaN;
            foreach (float time in times)
            {
                if (g.Fixed && !float.IsNaN(previous)) result.Add(GradientAt(g, time - 1e-4f).Select((v, i) => i == 0 ? time - 1e-4f : v).ToArray());
                result.Add(GradientAt(g, time));
                previous = time;
            }
            return result;
        }

        private Pin ColorValue(GraphBuilder g, ColorData c, Func<Pin> t, Func<Pin> random)
        {
            Pin Color(float[] v) => g.ConstColor(v[0], v[1], v[2], v[3]);
            switch (c.Mode)
            {
                case GradientMode.Color:
                    return Color(c.Color);
                case GradientMode.TwoColors:
                    return g.Lerp(Color(c.ColorMin), Color(c.ColorMax), random());
                case GradientMode.Gradient:
                    return g.Curve(t(), "Color", ColorKeys(c.Gradient));
                case GradientMode.TwoGradients:
                    return g.Lerp(g.Curve(t(), "Color", ColorKeys(c.GradientMin ?? c.Gradient)), g.Curve(t(), "Color", ColorKeys(c.Gradient)), random());
                default:
                    return g.Curve(random(), "Color", ColorKeys(c.Gradient));
            }
        }

        // ---- The layer ----

        private XElement Run()
        {
            foreach (var unsupported in _d.Unsupported) Warn(unsupported);
            // Prewarm only does something for a looping system: it starts as if one full cycle had run.
            // A sub emitter is prewarmed like its parent (it is driven by it): the same seconds.
            bool prewarm = Driven ? _inheritedPrewarm > 0f : _d.Prewarm && _d.Looping;
            _prewarmSeconds = !prewarm ? 0f : Driven ? _inheritedPrewarm : Math.Max(1e-3f, _d.Duration);
            float startDelay = _d.StartDelay.Mode == CurveMode.TwoConstants ? _d.StartDelay.ConstantMin : _d.StartDelay.Constant;
            if (prewarm && !Driven && startDelay > _d.Duration) Warn("prewarm with a start delay longer than the duration starts empty");
            if (!Driven && _d.RateOverDistance > 0f) Warn("emission over distance is not supported");
            if (_d.CollisionEnabled && !CollisionActive) Warn("collision with planes: no plane is set, nothing to collide with");
            if (_d.Space == SimulationSpace.Custom) Warn("custom simulation space is treated as local");

            XElement subEmission = null;
            if (Driven) subEmission = BuildSubEmission();
            else BuildEmit();
            BuildInit();
            BuildUpdate();

            return new XElement("layer",
                new XAttribute("type", "ParticleGraphLayer"),
                new XAttribute("capacity", Math.Max(1, _d.MaxParticles)),
                new XAttribute("timeScale", GraphBuilder.F(_d.SimulationSpeed)),
                new XAttribute("duration", GraphBuilder.F(Math.Max(1e-3f, _d.Duration))),
                new XAttribute("loop", _d.Looping ? "True" : "False"),
                prewarm ? new XAttribute("prewarm", GraphBuilder.F(_prewarmSeconds)) : null,
                subEmission,
                _emit.ToXml("emit"), _init.ToXml("init"), _update.ToXml("update"));
        }

        // ---- Sub emission ----

        /// <summary>
        /// What drives a sub emitter's layer: its trigger and probability, and what the child's own emission module says when it is driven (a Death child
        /// emits its first burst, a Birth child its rate and bursts on the clock of every parent particle).
        /// </summary>
        private XElement BuildSubEmission()
        {
            bool death = _link.Type != SubEmitterType.Birth; // a Death and a Collision sub emitter emit one burst; Birth: a rate, bursts on the clock of the parent particle
            if ((_link.Inherit & (SubEmitterData.InheritLifetime | SubEmitterData.InheritDuration)) != 0)
                Warn("inheriting the lifetime or the duration of the parent particle is not supported");
            // A Birth sub emitter drops particles as its parent particle moves (a trail); a Death one has a single burst and no distance.
            float rateOverDistance = !death && _d.EmissionEnabled ? _d.RateOverDistance : 0f;
            if (death && _d.RateOverDistance > 0f) Warn("emission over distance of a death sub emitter is not used");

            float probability = _link.Probability;
            float rate = 0f, rateMax = 0f;
            var bursts = new List<XElement>();
            if (_d.EmissionEnabled)
            {
                var r = _d.RateOverTime;
                if (!death && !r.IsZero)
                {
                    switch (r.Mode)
                    {
                        case CurveMode.Constant:
                            rate = rateMax = r.Constant;
                            break;
                        case CurveMode.TwoConstants:
                            rate = r.ConstantMin;
                            rateMax = r.ConstantMax;
                            break;
                        default:
                            // A curve over the clock of the child: its mean.
                            rate = rateMax = MeanOf(r);
                            Warn("a rate over time that is a curve is averaged");
                            break;
                    }
                }

                int taken = 0;
                foreach (var burst in _d.Bursts)
                {
                    if (death && taken > 0) { Warn("only the first burst of a death sub emitter is emitted"); break; }
                    int count, countMax;
                    switch (burst.Count.Mode)
                    {
                        case CurveMode.Constant:
                            count = countMax = (int)Math.Round(burst.Count.Constant);
                            break;
                        case CurveMode.TwoConstants:
                            count = (int)Math.Round(burst.Count.ConstantMin);
                            countMax = (int)Math.Round(burst.Count.ConstantMax);
                            break;
                        default:
                            count = countMax = (int)Math.Round(MeanOf(burst.Count));
                            Warn("a burst count that is a curve is averaged");
                            break;
                    }
                    if (burst.Probability < 1f)
                    {
                        if (death) probability *= burst.Probability;
                        else Warn("the probability of a burst of a sub emitter is not supported");
                    }
                    int cycles = death ? 1 : Math.Min(Math.Max(1, burst.Cycles), 64);
                    for (int c = 0; c < cycles; c++)
                        bursts.Add(new XElement("burst", new XAttribute("time", GraphBuilder.F(burst.Time + c * Math.Max(burst.Interval, 1e-3f))),
                            new XAttribute("count", count), new XAttribute("countMax", countMax)));
                    taken++;
                }
            }
            if (rate <= 0f && rateMax <= 0f && rateOverDistance <= 0f && bursts.Count == 0) Warn("the sub emitter has no emission: it emits nothing");

            float delay = _d.StartDelay.Mode == CurveMode.TwoConstants ? _d.StartDelay.ConstantMin : _d.StartDelay.Constant;
            return new XElement("subEmission",
                new XAttribute("source", _sourceIndex),
                new XAttribute("trigger", _link.Type == SubEmitterType.Death ? "death" : _link.Type == SubEmitterType.Collision ? "collision" : "birth"),
                new XAttribute("probability", GraphBuilder.F(probability)),
                new XAttribute("rate", GraphBuilder.F(rate)),
                new XAttribute("rateMax", GraphBuilder.F(rateMax)),
                new XAttribute("rateOverDistance", GraphBuilder.F(rateOverDistance)),
                new XAttribute("delay", GraphBuilder.F(Math.Max(0f, delay))),
                new XAttribute("duration", GraphBuilder.F(Math.Max(1e-3f, _d.Duration))),
                new XAttribute("loop", _d.Looping ? "true" : "false"),
                bursts);
        }

        /// <summary>The mean of the values of a curve (the average of its keys), times its multiplier.</summary>
        private static float MeanOf(CurveData c)
        {
            if (c.Curve == null || c.Curve.Length == 0) return c.Constant;
            float sum = 0f;
            foreach (var key in c.Curve) sum += key.Value;
            return sum / c.Curve.Length * c.Multiplier;
        }

        // ---- Emit ----

        private void BuildEmit()
        {
            if (!_d.EmissionEnabled) return;
            var g = _emit;
            Pin perFrame = default;
            float delay = _d.StartDelay.Mode == CurveMode.TwoConstants ? _d.StartDelay.ConstantMin : _d.StartDelay.Constant;

            var rate = _d.RateOverTime;
            if (!rate.IsZero)
            {
                Pin value;
                switch (rate.Mode)
                {
                    case CurveMode.Constant:
                        value = g.Const(rate.Constant);
                        break;
                    case CurveMode.TwoConstants:
                        value = g.Const((rate.ConstantMin + rate.ConstantMax) * 0.5f);
                        break;
                    default:
                        // A curve of the rate along the duration (the mean of two curves).
                        value = g.Curve(g.NormalizedEffectTime(_d.Duration, _d.Looping), "Float", FloatKeys(rate.Curve, rate.Multiplier));
                        break;
                }

                if (!_d.Looping || delay > 0f)
                {
                    var time = g.EffectTime();
                    var gate = g.Greater(time, g.Const(delay - 1e-4f));
                    if (!_d.Looping) gate = g.And(gate, g.Less(time, g.Const(delay + _d.Duration)));
                    value = g.Select(gate, value, g.Const(0f));
                }
                perFrame = g.TimeStepScale(value);
            }

            foreach (var burst in _d.Bursts)
            {
                var count = burst.Probability < 1f && burst.Count.Mode == CurveMode.Constant
                    ? g.Const(burst.Count.Constant * burst.Probability)
                    : EvaluateBurstCount(g, burst.Count);
                int cycles = Math.Max(1, burst.Cycles);
                float interval = Math.Max(burst.Interval, 1e-3f);
                if (_d.Looping)
                {
                    // A burst comes again with every loop of the system.
                    if (cycles > 1) Warn("a repeating burst of a looping system comes once per loop");
                    interval = Math.Max(_d.Duration, 1e-3f);
                    cycles = 1000000;
                }
                var timer = g.BurstTimer(count, delay + burst.Time, interval, cycles);
                perFrame = perFrame.IsValid ? g.Add(perFrame, timer) : timer;
            }

            if (perFrame.IsValid) g.Emit(perFrame);
        }

        private Pin EvaluateBurstCount(GraphBuilder g, CurveData c)
        {
            switch (c.Mode)
            {
                case CurveMode.Constant:
                    return g.Const(c.Constant);
                case CurveMode.TwoConstants:
                    return g.Random(c.ConstantMin, c.ConstantMax);
                default:
                    Warn("a burst count that is a curve is taken at its start");
                    return g.Const((c.Curve != null && c.Curve.Length > 0 ? c.Curve[0].Value : 0f) * c.Multiplier);
            }
        }

        // ---- Init ----

        private void BuildInit()
        {
            var g = _init;
            g.Set("time", "Float", g.Const(0f));
            if (PartialLifetimeLoss) g.Set("lost", "Float", g.Const(0f));
            var startLifetime = StartValue(_d.StartLifetime);
            if (_d.LifetimeByEmitterSpeedEnabled)
            {
                // A new particle lives longer or shorter by how fast the emitter moves.
                var fraction = SpeedFraction(g, g.Length(g.EmitterVelocity()), _d.LifetimeByEmitterSpeedMin, _d.LifetimeByEmitterSpeedMax);
                startLifetime = g.Multiply(startLifetime, Evaluate(g, _d.LifetimeByEmitterSpeed, () => fraction, null));
            }
            g.Set("lifetime", "Float", startLifetime);

            // Where particles come from and which way they go.
            Pin position, direction;
            BuildShape(g, out position, out direction);
            var velocity = g.Multiply(direction, StartValue(_d.StartSpeed));
            Pin parentWorldVelocity = default;
            if (Driven)
            {
                // The particles start where the parent particle is (the layer of the parent is in the same emitter, so they share the transform to the world);
                // the position of the shape is an offset from it.
                var toWorld = g.LocalToWorld();
                var parentPosition = g.Get("parent_pos", "Vector3");
                var parentVelocity = g.Get("parent_vel", "Vector3");
                bool parentInWorld = _parent.Space == SimulationSpace.World;
                var worldPosition = parentInWorld ? parentPosition : g.TransformPoint(toWorld, parentPosition);
                parentWorldVelocity = parentInWorld ? parentVelocity : g.TransformDirection(toWorld, parentVelocity);
                if (_d.Space == SimulationSpace.World)
                {
                    position = g.Add(worldPosition, g.TransformDirection(toWorld, position));
                    velocity = g.TransformDirection(toWorld, velocity);
                }
                else
                {
                    position = g.Add(g.TransformPoint(g.WorldToLocal(), worldPosition), position);
                }
            }
            else if (_d.Space == SimulationSpace.World)
            {
                var toWorld = g.LocalToWorld();
                position = g.TransformPoint(toWorld, position);
                velocity = g.TransformDirection(toWorld, velocity);
            }
            // The velocity of the emitter at the spawn is added to the particle's, when the particles do not move with the emitter anyway. For a sub emitter
            // it is the velocity of the parent particle.
            if (_d.InheritEnabled && _d.Space == SimulationSpace.World && (Driven || !_d.InheritCurrent))
                velocity = g.Add(velocity, g.Multiply(Driven ? parentWorldVelocity : g.EmitterVelocity(), StartValue(_d.InheritMultiplier)));
            g.Set("pos", "Vector3", position);
            g.Set("vel", "Vector3", velocity);

            // Size and rotation are Vector3 attributes: x, y of the size are the width and height of a billboard, z is for meshes.
            Pin size;
            if (_d.StartSize3D) size = g.Make(StartValue(_d.StartSizeX), StartValue(_d.StartSizeY), StartValue(_d.StartSizeZ));
            else { var s = StartValue(_d.StartSizeX); size = g.Make(s, s, s); }
            if (Driven && (_link.Inherit & SubEmitterData.InheritSize) != 0) size = g.Multiply(size, g.Get("parent_size_now", "Vector3"));
            g.Set("size", "Vector3", size);

            Pin rotation;
            var zero = g.Const(0f);
            if (_d.StartRotation3D) rotation = g.Make(StartValue(_d.StartRotationX), StartValue(_d.StartRotationY), StartValue(_d.StartRotationZ));
            else rotation = g.Make(zero, zero, StartValue(_d.StartRotationZ));
            if (Driven && (_link.Inherit & SubEmitterData.InheritRotation) != 0) rotation = g.Add(rotation, g.Get("parent_rotation", "Vector3"));
            g.Set("rotation", "Vector3", rotation);

            Pin time = default;
            var startColor = ColorValue(g, _d.StartColor,
                () => time.IsValid ? time : time = g.NormalizedEffectTime(_d.Duration, _d.Looping), () => g.Random(0f, 1f));
            if (Driven && (_link.Inherit & SubEmitterData.InheritColor) != 0) startColor = g.Multiply(startColor, g.Get("parent_color_now", "Color"));
            g.Set("color", "Color", startColor);

            if (_d.SheetEnabled) g.Set("frame0", "Float", StartValue(_d.SheetStartFrame));
        }

        private static IEnumerable<XElement> ShapeProperties(ParticleSystemData d, float radius, float angle, float length, int from, bool hasArc, bool hasFrom = true)
        {
            var q = d.ShapeRotation;
            yield return GraphBuilder.Property("Radius", "Float", GraphBuilder.F(radius));
            yield return GraphBuilder.Property("Radius Thickness", "Float", GraphBuilder.F(d.ShapeRadiusThickness));
            if (angle >= 0f) yield return GraphBuilder.Property("Angle", "Float", GraphBuilder.F(angle));
            if (length >= 0f) yield return GraphBuilder.Property("Length", "Float", GraphBuilder.F(length));
            yield return GraphBuilder.Property("Translation", "Vector3", GraphBuilder.V3(d.ShapePosition[0], d.ShapePosition[1], d.ShapePosition[2]));
            yield return GraphBuilder.Property("Rotation", "Quaternion", GraphBuilder.V4(q[3], q[0], q[1], q[2]));
            yield return GraphBuilder.Property("Scale", "Vector3", GraphBuilder.V3(d.ShapeScale[0], d.ShapeScale[1], d.ShapeScale[2]));
            if (hasFrom) yield return GraphBuilder.Property("From", "Int", from.ToString());
            if (hasArc) yield return GraphBuilder.Property("Arc", "Float", GraphBuilder.F(d.ShapeArc));
        }

        private void BuildShape(GraphBuilder g, out Pin position, out Pin direction)
        {
            // rbfx's EmitFrom: Base 0, Volume 1, Surface 2, Edge 3.
            int from = _d.ShapeEmitFrom == ShapeEmit.Base ? 0 : _d.ShapeEmitFrom == ShapeEmit.Surface ? 2 : _d.ShapeEmitFrom == ShapeEmit.Edge ? 3 : 1;
            Pin[] shape;
            switch (_d.Shape)
            {
                case ShapeType.Cone:
                    shape = g.Shape("Cone", ShapeProperties(_d, _d.ShapeRadius, _d.ShapeAngle, _d.ShapeEmitFrom == ShapeEmit.Base ? 0f : _d.ShapeLength, from, true));
                    break;
                case ShapeType.Sphere:
                    shape = g.Shape("Sphere", ShapeProperties(_d, _d.ShapeRadius, -1f, -1f, from, true));
                    break;
                case ShapeType.Hemisphere:
                    shape = g.Shape("Hemisphere", ShapeProperties(_d, _d.ShapeRadius, -1f, -1f, from, true));
                    break;
                case ShapeType.Circle:
                    shape = g.Shape("Circle", ShapeProperties(_d, _d.ShapeRadius, -1f, -1f, from, true, hasFrom: false));
                    break;
                case ShapeType.Box:
                case ShapeType.Rectangle:
                case ShapeType.Edge:
                    shape = g.Shape("Box", BoxProperties());
                    break;
                case ShapeType.None:
                    position = g.Const(0f, 0f, 0f);
                    direction = g.Const(0f, 0f, 1f);
                    return;
                default:
                    Warn("the shape " + _d.Shape + " is not supported: particles start at the origin");
                    position = g.Const(0f, 0f, 0f);
                    direction = g.Const(0f, 0f, 1f);
                    return;
            }
            position = shape[0];
            direction = shape[1];

            // Unity blends the direction towards a random one, and towards the one pointing away from the centre of the shape.
            if (_d.RandomizeDirection > 0f)
            {
                var random = g.Shape("Sphere", new[]
                {
                    GraphBuilder.Property("Radius", "Float", "1"), GraphBuilder.Property("Radius Thickness", "Float", "0"), GraphBuilder.Property("From", "Int", "2"),
                });
                direction = g.Normalized(g.Lerp(direction, random[1], g.Const(Math.Min(1f, _d.RandomizeDirection))));
            }
            if (_d.SphericalizeDirection > 0f)
                direction = g.Normalized(g.Lerp(direction, g.Normalized(position), g.Const(Math.Min(1f, _d.SphericalizeDirection))));
        }

        private IEnumerable<XElement> BoxProperties()
        {
            var q = _d.ShapeRotation;
            float[] box = _d.ShapeBox;
            // The box node spans -1 to 1; the scale gives its size (half of Unity's box), and a rectangle or an edge is flat or thin.
            float sx = _d.ShapeScale[0] * box[0] * 0.5f;
            float sy = _d.Shape == ShapeType.Edge ? 0f : _d.ShapeScale[1] * box[1] * 0.5f;
            float sz = _d.Shape == ShapeType.Rectangle || _d.Shape == ShapeType.Edge ? 0f : _d.ShapeScale[2] * box[2] * 0.5f;
            if (_d.Shape == ShapeType.Edge) { sx = _d.ShapeRadius * _d.ShapeScale[0]; }
            int from = _d.ShapeEmitFrom == ShapeEmit.Surface ? 2 : _d.ShapeEmitFrom == ShapeEmit.Edge ? 3 : 1;
            yield return GraphBuilder.Property("Translation", "Vector3", GraphBuilder.V3(_d.ShapePosition[0], _d.ShapePosition[1], _d.ShapePosition[2]));
            yield return GraphBuilder.Property("Rotation", "Quaternion", GraphBuilder.V4(q[3], q[0], q[1], q[2]));
            yield return GraphBuilder.Property("Scale", "Vector3", GraphBuilder.V3(sx, sy, sz));
            yield return GraphBuilder.Property("From", "Int", from.ToString());
        }

        // ---- Update ----

        private Pin Vector3Of(Pin x, Pin y, Pin z) => _update.Make(x, y, z);

        /// <summary>The properties of the Collide node: the planes in the space of the system and the response of the module.</summary>
        private IEnumerable<XElement> CollisionProperties()
        {
            // The planes of the module, or the colliders of the scene (a ray along the move of the particle).
            if (_d.CollisionType == CollisionType.World) yield return GraphBuilder.Property("Mode", "Int", "1");
            else yield return GraphBuilder.Property("Planes", "String", string.Join(";", _d.CollisionPlanes.Select(p => string.Join(" ", p.Select(GraphBuilder.F)))));
            yield return GraphBuilder.Property("Is Worldspace", "Bool", _d.Space == SimulationSpace.World ? "true" : "false");
            yield return GraphBuilder.Property("Dampen", "Float", GraphBuilder.F(Math.Max(0f, Math.Min(1f, ConstantOf(_d.CollisionDampen)))));
            yield return GraphBuilder.Property("Bounce", "Float", GraphBuilder.F(Math.Max(0f, ConstantOf(_d.CollisionBounce))));
            yield return GraphBuilder.Property("Lifetime Loss", "Float", GraphBuilder.F(Math.Max(0f, ConstantOf(_d.CollisionLifetimeLoss))));
            yield return GraphBuilder.Property("Min Kill Speed", "Float", GraphBuilder.F(_d.CollisionMinKillSpeed));
            if (_d.CollisionMaxKillSpeed < 9999f) yield return GraphBuilder.Property("Max Kill Speed", "Float", GraphBuilder.F(_d.CollisionMaxKillSpeed));
        }

        /// <summary>The unit of the orbital velocity of Unity: radians a second. The graph's rotations are in degrees.</summary>
        private const float OrbitalUnit = 180f / (float)Math.PI;

        /// <summary>Takes a direction (an axis, a speed) given in the space of the velocity module to the space the particles are simulated in.</summary>
        private Pin ToSimDirection(GraphBuilder g, Pin direction)
        {
            bool simWorld = _d.Space == SimulationSpace.World;
            bool moduleWorld = _d.VelocitySpace == SimulationSpace.World;
            if (simWorld == moduleWorld) return direction;
            return g.TransformDirection(simWorld ? g.LocalToWorld() : g.WorldToLocal(), direction);
        }

        /// <summary>Takes a point given in the space of the velocity module (relative to the system) to the space the particles are simulated in.</summary>
        private Pin ToSimPoint(GraphBuilder g, Pin point)
        {
            bool simWorld = _d.Space == SimulationSpace.World;
            bool moduleWorld = _d.VelocitySpace == SimulationSpace.World;
            if (!simWorld && !moduleWorld) return point;
            if (simWorld && !moduleWorld) return g.TransformPoint(g.LocalToWorld(), point);
            var inWorld = g.Add(g.EmitterPosition(), point); // The axes of the world, the origin at the system.
            return simWorld ? inWorld : g.TransformPoint(g.WorldToLocal(), inWorld);
        }

        /// <summary>Where a speed is within a range, from 0 to 1 (the range is made at least a hair wide).</summary>
        private static Pin SpeedFraction(GraphBuilder g, Pin speed, float min, float max)
        {
            float range = max - min;
            var fraction = g.Divide(g.Subtract(speed, g.Const(min)), g.Const(range > 1e-6f ? range : 1f));
            return g.Max(g.Min(fraction, g.Const(1f)), g.Const(0f));
        }

        private void BuildUpdate()
        {
            var g = _update;
            // The time lost at a collision of the last tick is taken now: the node that finds the collision comes after this one.
            var advance = PartialLifetimeLoss ? g.Add(g.TimeStep(), g.Get("lost", "Float")) : g.TimeStep();
            var time = g.Set("time", "Float", g.Add(g.Get("time", "Float"), advance));
            var lifetime = g.Get("lifetime", "Float");
            g.Expire(time, lifetime);
            var age = g.Divide(time, lifetime);

            var velocity = g.Get("vel", "Vector3");
            var zero = g.Const(0f);

            if (!_d.GravityModifier.IsZero)
            {
                var modifier = OverLife(_d.GravityModifier, age, "rnd_gravity");
                var force = g.Make(zero, g.Multiply(modifier, g.Const(_d.GravityY)), zero);
                velocity = g.ApplyForce(velocity, force);
            }
            if (_d.ForceEnabled)
            {
                if (_d.ForceSpace != _d.Space && _d.Space != SimulationSpace.Custom) Warn("force over lifetime in another space than the simulation is taken in the simulation space");
                var force = Vector3Of(OverLife(_d.ForceX, age, "rnd_forcex"), OverLife(_d.ForceY, age, "rnd_forcey"), OverLife(_d.ForceZ, age, "rnd_forcez"));
                velocity = g.ApplyForce(velocity, force);
            }
            if (_d.LimitEnabled) velocity = g.LimitVelocity(velocity, OverLife(_d.LimitSpeed, age, "rnd_limit"), _d.LimitDampen);
            // (The velocity is stored after the collision, below.)

            // The size of the particle now: its start size and what changes it. The radius of a collision comes from it.
            var size = g.Get("size", "Vector3");
            if (_d.SizeEnabled)
            {
                var factor = _d.SizeSeparateAxes
                    ? Vector3Of(OverLife(_d.SizeCurveX, age, "rnd_sizex"), OverLife(_d.SizeCurveY, age, "rnd_sizey"), OverLife(_d.SizeCurveZ, age, "rnd_sizez"))
                    : OverLife(_d.SizeCurveX, age, "rnd_size");
                size = g.Multiply(size, factor);
            }

            if (_d.SizeBySpeedEnabled)
            {
                var fraction = SpeedFraction(g, g.Length(velocity), _d.SizeBySpeedMin, _d.SizeBySpeedMax);
                var factor = _d.SizeBySpeedSeparateAxes
                    ? Vector3Of(OverLife(_d.SizeBySpeedX, fraction, "rnd_sizespeedx"), OverLife(_d.SizeBySpeedY, fraction, "rnd_sizespeedy"), OverLife(_d.SizeBySpeedZ, fraction, "rnd_sizespeedz"))
                    : OverLife(_d.SizeBySpeedX, fraction, "rnd_sizespeed");
                size = g.Multiply(size, factor);
            }

            // What moves a particle without changing its velocity.
            var move = velocity;
            if (_d.VelocityEnabled)
            {
                var added = Vector3Of(OverLife(_d.VelocityX, age, "rnd_velx"), OverLife(_d.VelocityY, age, "rnd_vely"), OverLife(_d.VelocityZ, age, "rnd_velz"));
                move = g.Add(move, ToSimDirection(g, added));
            }
            if (_d.InheritEnabled && _d.InheritCurrent && _d.Space == SimulationSpace.World && !Driven)
                move = g.Add(move, g.Multiply(g.EmitterVelocity(), OverLife(_d.InheritMultiplier, age, "rnd_inherit")));
            var position = g.Get("pos", "Vector3");

            // Orbital and radial velocity are around a centre: the system (or the offset from it), in the space of the velocity module.
            Pin centre = default;
            if (_d.VelocityEnabled && (_d.VelocityHasOrbital || _d.VelocityHasRadial))
            {
                var offset = Vector3Of(OverLife(_d.VelocityOffsetX, age, "rnd_offx"), OverLife(_d.VelocityOffsetY, age, "rnd_offy"), OverLife(_d.VelocityOffsetZ, age, "rnd_offz"));
                centre = ToSimPoint(g, offset);
                if (_d.VelocityHasRadial)
                    move = g.Add(move, g.Multiply(g.Normalized(g.Subtract(position, centre)), OverLife(_d.VelocityRadial, age, "rnd_radial")));
            }
            if (_d.NoiseEnabled)
            {
                float scroll = _d.NoiseScrollSpeed.Mode == CurveMode.TwoConstants ? _d.NoiseScrollSpeed.ConstantMax : _d.NoiseScrollSpeed.Constant;
                var noise = g.NoiseVector(position, _d.NoiseFrequency, 1f, scroll);
                var strength = Vector3Of(OverLife(_d.NoiseStrengthX, age, "rnd_noisex"), OverLife(_d.NoiseStrengthY, age, "rnd_noisey"), OverLife(_d.NoiseStrengthZ, age, "rnd_noisez"));
                move = g.Add(move, g.Multiply(noise, strength));
            }
            var moved = g.Move(position, move);
            if (centre.IsValid && _d.VelocityHasOrbital)
            {
                // The particle turns around the centre by the angular velocity of this tick (the graph's rotation is in degrees: pitch about x, yaw about y, roll about z).
                var omega = ToSimDirection(g, Vector3Of(OverLife(_d.VelocityOrbitalX, age, "rnd_orbx"), OverLife(_d.VelocityOrbitalY, age, "rnd_orby"), OverLife(_d.VelocityOrbitalZ, age, "rnd_orbz")));
                var degrees = g.Multiply(omega, g.Multiply(g.TimeStep(), g.Const(OrbitalUnit)));
                var angles = g.Break(degrees);
                var turn = g.MakeRotation(angles[0], angles[1], angles[2]);
                moved = g.Add(centre, g.Rotate(turn, g.Subtract(moved, centre)));
            }

            // Collision: the move is tested against the planes; the velocity after it is what the particle keeps.
            Pin velocityAfter = velocity;
            Pin collided = default;
            if (CollisionActive)
            {
                var radius = g.Multiply(g.Break(size)[0], g.Const(0.5f * _d.CollisionRadiusScale));
                var result = g.Collide(CollisionProperties(), position, moved, velocity, radius);
                moved = result[0];
                velocityAfter = result[1];
                collided = result[2];
                if (PartialLifetimeLoss)
                    g.Set("lost", "Float", g.Select(collided, g.Multiply(g.Const(ConstantOf(_d.CollisionLifetimeLoss)), lifetime), zero));
            }
            g.Set("vel", "Vector3", velocityAfter);
            position = g.Set("pos", "Vector3", moved);

            // Rotation by the angular speed over lifetime (degrees a second).
            var rotation = g.Get("rotation", "Vector3");
            if (_d.RotationEnabled || _d.RotationBySpeedEnabled)
            {
                var dt = g.TimeStep();
                Pin turn = default;
                if (_d.RotationEnabled)
                {
                    var omegaZ = g.Multiply(OverLife(_d.RotationOverLifetimeZ, age, "rnd_rotation"), dt);
                    var omegaX = !_d.RotationOverLifetimeX.IsZero ? g.Multiply(OverLife(_d.RotationOverLifetimeX, age, "rnd_rotationx"), dt) : zero;
                    var omegaY = !_d.RotationOverLifetimeY.IsZero ? g.Multiply(OverLife(_d.RotationOverLifetimeY, age, "rnd_rotationy"), dt) : zero;
                    turn = g.Make(omegaX, omegaY, omegaZ);
                }
                if (_d.RotationBySpeedEnabled)
                {
                    // The turn depends on the speed within the range.
                    var fraction = SpeedFraction(g, g.Length(velocity), _d.RotationBySpeedMin, _d.RotationBySpeedMax);
                    var turnZ = g.Multiply(OverLife(_d.RotationBySpeedZ, fraction, "rnd_rotspeed"), dt);
                    var turnX = !_d.RotationBySpeedX.IsZero ? g.Multiply(OverLife(_d.RotationBySpeedX, fraction, "rnd_rotspeedx"), dt) : zero;
                    var turnY = !_d.RotationBySpeedY.IsZero ? g.Multiply(OverLife(_d.RotationBySpeedY, fraction, "rnd_rotspeedy"), dt) : zero;
                    var bySpeed = g.Make(turnX, turnY, turnZ);
                    turn = turn.IsValid ? g.Add(turn, bySpeed) : bySpeed;
                }
                rotation = g.Set("rotation", "Vector3", g.Add(rotation, turn));
            }

            var color = g.Get("color", "Color");
            if (_d.ColorEnabled)
                color = g.Multiply(color, ColorValue(g, _d.ColorOverLifetime, () => age, () => RandomAttribute("rnd_color")));

            if (_d.ColorBySpeedEnabled)
            {
                var fraction = SpeedFraction(g, g.Length(velocity), _d.ColorBySpeedMin, _d.ColorBySpeedMax);
                color = g.Multiply(color, ColorValue(g, _d.ColorBySpeed, () => fraction, () => RandomAttribute("rnd_colorspeed")));
            }

            // What the sub emitters that inherit a colour or a size take from a particle: the values it has now, not its start values.
            if (_needsColorNow) g.Set("color_now", "Color", color);
            if (_needsSizeNow) g.Set("size_now", "Vector3", size);

            Pin frame = zero;
            int rows = 0, columns = 0;
            if (_d.SheetEnabled)
            {
                columns = Math.Max(1, _d.SheetTilesX);
                rows = Math.Max(1, _d.SheetTilesY);
                var progress = OverLife(_d.SheetFrameOverTime, age, "rnd_sheet");
                var start = g.Get("frame0", "Float");
                var cycled = g.Add(start, g.Multiply(progress, g.Const(_d.SheetCycles)));
                if (_d.SheetSingleRow)
                {
                    var rowStart = g.Const((_d.SheetRandomRow ? 0 : Math.Min(_d.SheetRow, rows - 1)) * columns);
                    frame = g.Add(rowStart, g.Repeat(g.Multiply(cycled, g.Const(columns)), g.Const(columns)));
                }
                else
                {
                    var total = g.Const(rows * columns);
                    frame = g.Repeat(g.Multiply(cycled, total), total);
                }
            }

            BuildRender(g, position, velocity, size, rotation, color, frame, rows, columns);
        }

        private void BuildRender(GraphBuilder g, Pin position, Pin velocity, Pin size, Pin rotation, Pin color, Pin frame, int rows, int columns)
        {
            var axes = g.Break(size);
            var angles = g.Break(rotation);
            var half = g.Const(0.5f);
            bool world = _d.Space == SimulationSpace.World;
            string material = _d.MaterialPath;
            switch (_d.Render)
            {
                case RenderMode.None:
                    return;
                case RenderMode.Mesh:
                {
                    if (string.IsNullOrEmpty(_d.MeshPath)) { Warn("the mesh renderer has no mesh: nothing is drawn"); return; }
                    var turn = g.MakeRotation(angles[0], angles[1], angles[2]);
                    g.RenderMesh(_d.MeshPath, material, world, g.MakeTransform(position, turn, size), color);
                    return;
                }
                case RenderMode.Stretch:
                {
                    // The quad is as wide as the particle and as long as it, times the length scale, plus the speed times the speed scale; it points along the velocity.
                    var length = g.Multiply(axes[1], g.Const(_d.StretchLengthScale));
                    if (_d.StretchSpeedScale > 0f) length = g.Add(length, g.Multiply(g.Length(velocity), g.Const(_d.StretchSpeedScale)));
                    var extent = g.Make(g.Multiply(axes[0], half), g.Multiply(length, half));
                    g.RenderBillboard(material, rows, columns, 6, _d.SortByDistance, world, position, extent, frame, color, angles[2], g.Normalized(velocity));
                    return;
                }
                default:
                {
                    var extent = g.Make(g.Multiply(axes[0], half), g.Multiply(axes[1], half));
                    // Billboard: 1 (the camera's rotation); horizontal: 7 (turned by an angle about its own axis, which points up); vertical: 2 (the camera's yaw).
                    int mode = _d.Render == RenderMode.HorizontalBillboard ? 7 : _d.Render == RenderMode.VerticalBillboard ? 2 : 1;
                    g.RenderBillboard(material, rows, columns, mode, _d.SortByDistance, world, position, extent, frame, color, angles[2], g.Const(0f, 1f, 0f));
                    return;
                }
            }
        }
    }
}
