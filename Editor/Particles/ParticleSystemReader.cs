using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityToRebelFork.Editor.Particles
{
    /// <summary>Copies the settings of a Unity <see cref="ParticleSystem"/> into <see cref="ParticleSystemData"/> for the converter.</summary>
    public static class ParticleSystemReader
    {
        public static ParticleSystemData Read(ParticleSystem system, string materialPath, string meshPath)
        {
            var data = new ParticleSystemData { Name = system.name, MaterialPath = materialPath ?? "", MeshPath = meshPath ?? "" };
            var main = system.main;
            data.Duration = main.duration;
            data.Looping = main.loop;
            data.Prewarm = main.prewarm;
            data.StartDelay = Convert(main.startDelay);
            data.StartLifetime = Convert(main.startLifetime);
            data.StartSpeed = Convert(main.startSpeed);
            data.StartSize3D = main.startSize3D;
            data.StartSizeX = Convert(main.startSizeX);
            data.StartSizeY = main.startSize3D ? Convert(main.startSizeY) : data.StartSizeX;
            data.StartSizeZ = main.startSize3D ? Convert(main.startSizeZ) : data.StartSizeX;
            data.StartRotation3D = main.startRotation3D;
            data.StartRotationX = Degrees(Convert(main.startRotationX));
            data.StartRotationY = Degrees(Convert(main.startRotationY));
            data.StartRotationZ = Degrees(Convert(main.startRotationZ));
            data.FlipRotation = main.flipRotation;
            data.StartColor = Convert(main.startColor);
            data.GravityModifier = Convert(main.gravityModifier);
            data.GravityY = Physics.gravity.y;
            data.Space = Convert(main.simulationSpace);
            data.SimulationSpeed = main.simulationSpeed;
            data.MaxParticles = main.maxParticles;
            data.PlayOnAwake = main.playOnAwake;

            var emission = system.emission;
            data.EmissionEnabled = emission.enabled;
            data.RateOverTime = Convert(emission.rateOverTime);
            data.RateOverDistance = emission.rateOverDistance.constantMax;
            var bursts = new ParticleSystem.Burst[emission.burstCount];
            emission.GetBursts(bursts);
            foreach (var burst in bursts)
            {
                data.Bursts.Add(new BurstData
                {
                    Time = burst.time,
                    Count = Convert(burst.count),
                    Cycles = burst.cycleCount,
                    Interval = burst.repeatInterval,
                    Probability = burst.probability,
                });
            }

            ReadShape(system, data);
            ReadOverLifetime(system, data);
            ReadRenderer(system, data);
            ReportUnsupported(system, data);
            return data;
        }

        // ---- Curves and gradients ----

        public static CurveData Convert(ParticleSystem.MinMaxCurve curve)
        {
            var data = new CurveData { Multiplier = curve.curveMultiplier };
            switch (curve.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    data.Mode = CurveMode.Constant;
                    data.Constant = curve.constant;
                    break;
                case ParticleSystemCurveMode.TwoConstants:
                    data.Mode = CurveMode.TwoConstants;
                    data.ConstantMin = curve.constantMin;
                    data.ConstantMax = curve.constantMax;
                    break;
                case ParticleSystemCurveMode.Curve:
                    data.Mode = CurveMode.Curve;
                    data.Curve = Keys(curve.curve);
                    break;
                default:
                    data.Mode = CurveMode.TwoCurves;
                    data.Curve = Keys(curve.curveMax);
                    data.CurveMin = Keys(curve.curveMin);
                    break;
            }
            return data;
        }

        private static KeyData[] Keys(AnimationCurve curve)
            => curve == null ? new KeyData[0] : curve.keys.Select(k => new KeyData(k.time, k.value, k.inTangent, k.outTangent)).ToArray();

        private static CurveData Degrees(CurveData curve)
        {
            const float toDegrees = 57.29578f;
            curve.Constant *= toDegrees;
            curve.ConstantMin *= toDegrees;
            curve.ConstantMax *= toDegrees;
            curve.Multiplier *= toDegrees;
            return curve;
        }

        private static GradientData Convert(Gradient gradient)
        {
            if (gradient == null) return new GradientData();
            return new GradientData
            {
                Colors = gradient.colorKeys.Select(k => new ColorKeyData { Time = k.time, R = k.color.r, G = k.color.g, B = k.color.b }).ToArray(),
                Alphas = gradient.alphaKeys.Select(k => new AlphaKeyData { Time = k.time, A = k.alpha }).ToArray(),
                Fixed = gradient.mode == UnityEngine.GradientMode.Fixed,
            };
        }

        private static float[] Rgba(Color c) => new[] { c.r, c.g, c.b, c.a };

        public static ColorData Convert(ParticleSystem.MinMaxGradient gradient)
        {
            var data = new ColorData();
            switch (gradient.mode)
            {
                case ParticleSystemGradientMode.Color:
                    data.Mode = Particles.GradientMode.Color;
                    data.Color = Rgba(gradient.color);
                    break;
                case ParticleSystemGradientMode.TwoColors:
                    data.Mode = Particles.GradientMode.TwoColors;
                    data.ColorMin = Rgba(gradient.colorMin);
                    data.ColorMax = Rgba(gradient.colorMax);
                    break;
                case ParticleSystemGradientMode.Gradient:
                    data.Mode = Particles.GradientMode.Gradient;
                    data.Gradient = Convert(gradient.gradient);
                    break;
                case ParticleSystemGradientMode.TwoGradients:
                    data.Mode = Particles.GradientMode.TwoGradients;
                    data.Gradient = Convert(gradient.gradientMax);
                    data.GradientMin = Convert(gradient.gradientMin);
                    break;
                default:
                    data.Mode = Particles.GradientMode.RandomColor;
                    data.Gradient = Convert(gradient.gradient);
                    break;
            }
            return data;
        }

        private static SimulationSpace Convert(ParticleSystemSimulationSpace space)
            => space == ParticleSystemSimulationSpace.World ? SimulationSpace.World
                : space == ParticleSystemSimulationSpace.Custom ? SimulationSpace.Custom : SimulationSpace.Local;

        // ---- Modules ----

        private static void ReadShape(ParticleSystem system, ParticleSystemData data)
        {
            var shape = system.shape;
            if (!shape.enabled)
            {
                data.Shape = ShapeType.None;
                return;
            }
            data.ShapeAngle = shape.angle;
            data.ShapeRadius = shape.radius;
            data.ShapeRadiusThickness = shape.radiusThickness;
            data.ShapeArc = shape.arc;
            data.ShapeLength = shape.length;
            data.ShapePosition = new[] { shape.position.x, shape.position.y, shape.position.z };
            var q = Quaternion.Euler(shape.rotation);
            data.ShapeRotation = new[] { q.x, q.y, q.z, q.w };
            data.ShapeScale = new[] { shape.scale.x, shape.scale.y, shape.scale.z };
            data.RandomizeDirection = shape.randomDirectionAmount;
            data.SphericalizeDirection = shape.sphericalDirectionAmount;
            data.RandomizePosition = shape.randomPositionAmount;

            switch (shape.shapeType)
            {
                case ParticleSystemShapeType.Sphere:
                    data.Shape = ShapeType.Sphere;
                    break;
                case ParticleSystemShapeType.SphereShell:
                    data.Shape = ShapeType.Sphere;
                    data.ShapeEmitFrom = ShapeEmit.Surface;
                    break;
                case ParticleSystemShapeType.Hemisphere:
                    data.Shape = ShapeType.Hemisphere;
                    break;
                case ParticleSystemShapeType.HemisphereShell:
                    data.Shape = ShapeType.Hemisphere;
                    data.ShapeEmitFrom = ShapeEmit.Surface;
                    break;
                case ParticleSystemShapeType.Cone:
                    data.Shape = ShapeType.Cone;
                    data.ShapeEmitFrom = ShapeEmit.Base;
                    break;
                case ParticleSystemShapeType.ConeVolume:
                    data.Shape = ShapeType.Cone;
                    break;
                case ParticleSystemShapeType.ConeShell:
                case ParticleSystemShapeType.ConeVolumeShell:
                    data.Shape = ShapeType.Cone;
                    data.ShapeEmitFrom = ShapeEmit.Surface;
                    break;
                case ParticleSystemShapeType.Box:
                    data.Shape = ShapeType.Box;
                    break;
                case ParticleSystemShapeType.BoxShell:
                    data.Shape = ShapeType.Box;
                    data.ShapeEmitFrom = ShapeEmit.Surface;
                    break;
                case ParticleSystemShapeType.BoxEdge:
                    data.Shape = ShapeType.Box;
                    data.ShapeEmitFrom = ShapeEmit.Edge;
                    break;
                case ParticleSystemShapeType.Circle:
                    data.Shape = ShapeType.Circle;
                    break;
                case ParticleSystemShapeType.CircleEdge:
                    data.Shape = ShapeType.Circle;
                    data.ShapeRadiusThickness = 0f;
                    break;
                case ParticleSystemShapeType.SingleSidedEdge:
                    data.Shape = ShapeType.Edge;
                    break;
                case ParticleSystemShapeType.Rectangle:
                    data.Shape = ShapeType.Rectangle;
                    break;
                default:
                    data.Shape = ShapeType.Other;
                    data.Unsupported.Add("shape " + shape.shapeType + " is not supported");
                    break;
            }
        }

        private static void ReadOverLifetime(ParticleSystem system, ParticleSystemData data)
        {
            var velocity = system.velocityOverLifetime;
            if (velocity.enabled)
            {
                data.VelocityEnabled = true;
                data.VelocityX = Convert(velocity.x);
                data.VelocityY = Convert(velocity.y);
                data.VelocityZ = Convert(velocity.z);
                data.VelocitySpace = Convert(velocity.space);
                data.VelocityOrbitalX = Convert(velocity.orbitalX);
                data.VelocityOrbitalY = Convert(velocity.orbitalY);
                data.VelocityOrbitalZ = Convert(velocity.orbitalZ);
                data.VelocityRadial = Convert(velocity.radial);
                data.VelocityOffsetX = Convert(velocity.orbitalOffsetX);
                data.VelocityOffsetY = Convert(velocity.orbitalOffsetY);
                data.VelocityOffsetZ = Convert(velocity.orbitalOffsetZ);
            }

            var force = system.forceOverLifetime;
            if (force.enabled)
            {
                data.ForceEnabled = true;
                data.ForceX = Convert(force.x);
                data.ForceY = Convert(force.y);
                data.ForceZ = Convert(force.z);
                data.ForceSpace = Convert(force.space);
            }

            var limit = system.limitVelocityOverLifetime;
            if (limit.enabled)
            {
                data.LimitEnabled = true;
                data.LimitSpeed = Convert(limit.limit);
                data.LimitDampen = limit.dampen;
                if (limit.separateAxes) data.Unsupported.Add("limit velocity over lifetime per axis is taken from the overall limit");
            }

            var color = system.colorOverLifetime;
            if (color.enabled)
            {
                data.ColorEnabled = true;
                data.ColorOverLifetime = Convert(color.color);
            }

            var size = system.sizeOverLifetime;
            if (size.enabled)
            {
                data.SizeEnabled = true;
                data.SizeSeparateAxes = size.separateAxes;
                data.SizeCurveX = Convert(size.separateAxes ? size.x : size.size);
                data.SizeCurveY = size.separateAxes ? Convert(size.y) : data.SizeCurveX;
                data.SizeCurveZ = size.separateAxes ? Convert(size.z) : data.SizeCurveX;
            }

            var rotation = system.rotationOverLifetime;
            if (rotation.enabled)
            {
                data.RotationEnabled = true;
                data.RotationOverLifetimeZ = Degrees(Convert(rotation.z));
                if (rotation.separateAxes)
                {
                    data.RotationOverLifetimeX = Degrees(Convert(rotation.x));
                    data.RotationOverLifetimeY = Degrees(Convert(rotation.y));
                }
            }

            var sizeBySpeed = system.sizeBySpeed;
            if (sizeBySpeed.enabled)
            {
                data.SizeBySpeedEnabled = true;
                data.SizeBySpeedMin = sizeBySpeed.range.x;
                data.SizeBySpeedMax = sizeBySpeed.range.y;
                data.SizeBySpeedSeparateAxes = sizeBySpeed.separateAxes;
                data.SizeBySpeedX = Convert(sizeBySpeed.separateAxes ? sizeBySpeed.x : sizeBySpeed.size);
                data.SizeBySpeedY = sizeBySpeed.separateAxes ? Convert(sizeBySpeed.y) : data.SizeBySpeedX;
                data.SizeBySpeedZ = sizeBySpeed.separateAxes ? Convert(sizeBySpeed.z) : data.SizeBySpeedX;
            }

            var colorBySpeed = system.colorBySpeed;
            if (colorBySpeed.enabled)
            {
                data.ColorBySpeedEnabled = true;
                data.ColorBySpeedMin = colorBySpeed.range.x;
                data.ColorBySpeedMax = colorBySpeed.range.y;
                data.ColorBySpeed = Convert(colorBySpeed.color);
            }

            var lifetimeByEmitterSpeed = system.lifetimeByEmitterSpeed;
            if (lifetimeByEmitterSpeed.enabled)
            {
                data.LifetimeByEmitterSpeedEnabled = true;
                data.LifetimeByEmitterSpeedMin = lifetimeByEmitterSpeed.range.x;
                data.LifetimeByEmitterSpeedMax = lifetimeByEmitterSpeed.range.y;
                data.LifetimeByEmitterSpeed = Convert(lifetimeByEmitterSpeed.curve);
            }

            var collision = system.collision;
            if (collision.enabled)
            {
                data.CollisionEnabled = true;
                data.CollisionType = collision.type == ParticleSystemCollisionType.World ? CollisionType.World : CollisionType.Planes;
                for (int i = 0; i < collision.planeCount; i++)
                {
                    var plane = collision.GetPlane(i);
                    if (plane == null) continue;
                    // The plane of a transform (its up is the normal) in the space of the system.
                    var point = system.transform.InverseTransformPoint(plane.position);
                    var normal = system.transform.InverseTransformDirection(plane.up).normalized;
                    data.CollisionPlanes.Add(new[] { normal.x, normal.y, normal.z, Vector3.Dot(normal, point) });
                }
                data.CollisionDampen = Convert(collision.dampen);
                data.CollisionBounce = Convert(collision.bounce);
                data.CollisionLifetimeLoss = Convert(collision.lifetimeLoss);
                data.CollisionMinKillSpeed = collision.minKillSpeed;
                data.CollisionMaxKillSpeed = collision.maxKillSpeed;
                data.CollisionRadiusScale = collision.radiusScale;
                if (collision.mode == ParticleSystemCollisionMode.Collision2D) data.Unsupported.Add("2D collision is not supported");
                if (collision.type == ParticleSystemCollisionType.World && collision.collidesWith.value != -1)
                    data.Unsupported.Add("the layers the particles collide with are not taken into account");
            }

            var inherit = system.inheritVelocity;
            if (inherit.enabled)
            {
                data.InheritEnabled = true;
                data.InheritCurrent = inherit.mode == ParticleSystemInheritVelocityMode.Current;
                data.InheritMultiplier = Convert(inherit.curve);
            }

            var bySpeed = system.rotationBySpeed;
            if (bySpeed.enabled)
            {
                data.RotationBySpeedEnabled = true;
                data.RotationBySpeedMin = bySpeed.range.x;
                data.RotationBySpeedMax = bySpeed.range.y;
                data.RotationBySpeedZ = Degrees(Convert(bySpeed.z));
                if (bySpeed.separateAxes)
                {
                    data.RotationBySpeedX = Degrees(Convert(bySpeed.x));
                    data.RotationBySpeedY = Degrees(Convert(bySpeed.y));
                }
            }

            var noise = system.noise;
            if (noise.enabled)
            {
                data.NoiseEnabled = true;
                data.NoiseStrengthX = Convert(noise.separateAxes ? noise.strengthX : noise.strength);
                data.NoiseStrengthY = noise.separateAxes ? Convert(noise.strengthY) : data.NoiseStrengthX;
                data.NoiseStrengthZ = noise.separateAxes ? Convert(noise.strengthZ) : data.NoiseStrengthX;
                data.NoiseFrequency = noise.frequency;
                data.NoiseScrollSpeed = Convert(noise.scrollSpeed);
                data.NoiseDamping = noise.damping ? 1f : 0f;
            }

            var sheet = system.textureSheetAnimation;
            if (sheet.enabled)
            {
                data.SheetEnabled = true;
                data.SheetTilesX = sheet.numTilesX;
                data.SheetTilesY = sheet.numTilesY;
                data.SheetSingleRow = sheet.animation == ParticleSystemAnimationType.SingleRow;
                data.SheetRow = sheet.rowIndex;
                data.SheetRandomRow = sheet.rowMode == ParticleSystemAnimationRowMode.Random;
                data.SheetFrameOverTime = Convert(sheet.frameOverTime);
                data.SheetStartFrame = Convert(sheet.startFrame);
                data.SheetCycles = sheet.cycleCount;
            }
        }

        private static void ReadRenderer(ParticleSystem system, ParticleSystemData data)
        {
            var renderer = system.GetComponent<ParticleSystemRenderer>();
            if (renderer == null || !renderer.enabled)
            {
                data.Render = RenderMode.None;
                return;
            }
            switch (renderer.renderMode)
            {
                case ParticleSystemRenderMode.Stretch: data.Render = RenderMode.Stretch; break;
                case ParticleSystemRenderMode.HorizontalBillboard: data.Render = RenderMode.HorizontalBillboard; break;
                case ParticleSystemRenderMode.VerticalBillboard: data.Render = RenderMode.VerticalBillboard; break;
                case ParticleSystemRenderMode.Mesh: data.Render = RenderMode.Mesh; break;
                case ParticleSystemRenderMode.None: data.Render = RenderMode.None; break;
                default: data.Render = RenderMode.Billboard; break;
            }
            data.SortByDistance = renderer.sortMode == ParticleSystemSortMode.Distance;
            data.StretchSpeedScale = renderer.velocityScale;
            data.StretchLengthScale = renderer.lengthScale;
            data.StretchCameraScale = renderer.cameraVelocityScale;
            data.MinParticleSize = renderer.minParticleSize;
            data.MaxParticleSize = renderer.maxParticleSize;
            if (renderer.sortMode == ParticleSystemSortMode.OldestInFront || renderer.sortMode == ParticleSystemSortMode.YoungestInFront)
                data.Unsupported.Add("sorting by age is not supported (" + renderer.sortMode + ")");
        }

        private static void ReportUnsupported(ParticleSystem system, ParticleSystemData data)
        {
            var unsupported = new List<string>();
            if (system.trails.enabled) unsupported.Add("trails are not supported");
            if (system.trigger.enabled) unsupported.Add("trigger is not supported");
            if (system.lights.enabled) unsupported.Add("lights are not supported");
            if (system.externalForces.enabled) unsupported.Add("external forces are not supported");
            if (system.customData.enabled) unsupported.Add("custom data is not supported");
            if (system.useAutoRandomSeed == false) unsupported.Add("a fixed random seed is not reproduced");
            data.Unsupported.AddRange(unsupported);
        }
    }
}
