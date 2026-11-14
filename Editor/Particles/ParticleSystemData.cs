#nullable disable
using System.Collections.Generic;

namespace UnityToRebelFork.Editor.Particles
{
    // A plain copy of the settings of a Unity ParticleSystem that the converter uses. It has no reference to UnityEngine, so the converter can be run
    // and tested outside of Unity; ParticleSystemReader (which needs Unity) fills it in. Angles are in degrees, colors are linear RGBA as Unity stores them.

    public enum CurveMode
    {
        Constant,
        Curve,
        TwoCurves,
        TwoConstants,
    }

    /// <summary>One key of an animation curve: Unity's Hermite key with its slopes.</summary>
    public struct KeyData
    {
        public float Time;
        public float Value;
        public float InTangent;
        public float OutTangent;

        public KeyData(float time, float value, float inTangent = 0f, float outTangent = 0f)
        {
            Time = time;
            Value = value;
            InTangent = inTangent;
            OutTangent = outTangent;
        }
    }

    /// <summary>ParticleSystem.MinMaxCurve.</summary>
    public sealed class CurveData
    {
        public CurveMode Mode;
        public float Constant;
        public float ConstantMin;
        public float ConstantMax;

        /// <summary>The curve (of <see cref="CurveMode.Curve"/>) or the upper curve (of <see cref="CurveMode.TwoCurves"/>); values are before the multiplier.</summary>
        public KeyData[] Curve;

        public KeyData[] CurveMin;
        public float Multiplier = 1f;

        public static CurveData Of(float constant) => new CurveData { Mode = CurveMode.Constant, Constant = constant };

        public static CurveData Between(float min, float max) => new CurveData { Mode = CurveMode.TwoConstants, ConstantMin = min, ConstantMax = max };

        public bool IsZero => Mode == CurveMode.Constant && Constant == 0f;
    }

    public struct ColorKeyData
    {
        public float Time;
        public float R, G, B;
    }

    public struct AlphaKeyData
    {
        public float Time;
        public float A;
    }

    /// <summary>UnityEngine.Gradient.</summary>
    public sealed class GradientData
    {
        public ColorKeyData[] Colors = new ColorKeyData[0];
        public AlphaKeyData[] Alphas = new AlphaKeyData[0];

        /// <summary>Fixed mode: the value of a key holds until the next one.</summary>
        public bool Fixed;
    }

    public enum GradientMode
    {
        Color,
        Gradient,
        TwoColors,
        TwoGradients,
        RandomColor,
    }

    /// <summary>ParticleSystem.MinMaxGradient.</summary>
    public sealed class ColorData
    {
        public GradientMode Mode;
        public float[] Color = { 1, 1, 1, 1 };
        public float[] ColorMin = { 1, 1, 1, 1 };
        public float[] ColorMax = { 1, 1, 1, 1 };
        public GradientData Gradient;
        public GradientData GradientMin;
    }

    public enum SimulationSpace
    {
        Local,
        World,
        Custom,
    }

    public enum ShapeType
    {
        None,
        Sphere,
        Hemisphere,
        Cone,
        Box,
        Circle,
        Edge,
        Rectangle,
        Donut,
        Mesh,
        Other,
    }

    public enum ShapeEmit
    {
        /// <summary>From the volume (or, for a cone, from the base disc and along the length).</summary>
        Volume,
        Base,
        Surface,
        Edge,
    }

    public enum RenderMode
    {
        Billboard,
        Stretch,
        HorizontalBillboard,
        VerticalBillboard,
        Mesh,
        None,
    }

    /// <summary>ParticleSystemSubEmitterType.</summary>
    public enum SubEmitterType
    {
        Birth,
        Collision,
        Death,
        Trigger,
        Manual,
    }

    /// <summary>ParticleSystemCollisionType.</summary>
    public enum CollisionType
    {
        Planes,
        World,
    }

    /// <summary>A sub emitter of a system: another system that spawns particles at the events of the particles of this one.</summary>
    public sealed class SubEmitterData
    {
        public const int InheritColor = 1, InheritSize = 2, InheritRotation = 4, InheritLifetime = 8, InheritDuration = 16;

        public ParticleSystemData Child;
        public SubEmitterType Type;

        /// <summary>ParticleSystemSubEmitterProperties: what the particles of the child take from the parent particle.</summary>
        public int Inherit;

        public float Probability = 1f;

        public bool IsSupported => Type == SubEmitterType.Birth || Type == SubEmitterType.Death || Type == SubEmitterType.Collision;
    }

    public sealed class BurstData
    {
        public float Time;
        public CurveData Count = CurveData.Of(30);
        public int Cycles = 1;
        public float Interval = 0.01f;
        public float Probability = 1f;
    }

    public sealed class ParticleSystemData
    {
        public string Name = "ParticleSystem";

        // Main module.
        public float Duration = 5f;
        public bool Looping = true;
        public bool Prewarm;
        public CurveData StartDelay = CurveData.Of(0f);
        public CurveData StartLifetime = CurveData.Of(5f);
        public CurveData StartSpeed = CurveData.Of(5f);
        public bool StartSize3D;
        public CurveData StartSizeX = CurveData.Of(1f);
        public CurveData StartSizeY = CurveData.Of(1f);
        public CurveData StartSizeZ = CurveData.Of(1f);
        public bool StartRotation3D;
        public CurveData StartRotationX = CurveData.Of(0f);
        public CurveData StartRotationY = CurveData.Of(0f);
        public CurveData StartRotationZ = CurveData.Of(0f);
        public float FlipRotation;
        public ColorData StartColor = new ColorData { Mode = GradientMode.Color };
        public CurveData GravityModifier = CurveData.Of(0f);
        public float GravityY = -9.81f;
        public SimulationSpace Space = SimulationSpace.Local;
        public float SimulationSpeed = 1f;
        public int MaxParticles = 1000;
        public bool PlayOnAwake = true;

        // Emission module.
        public bool EmissionEnabled = true;
        public CurveData RateOverTime = CurveData.Of(10f);
        public float RateOverDistance;
        public List<BurstData> Bursts = new List<BurstData>();

        /// <summary>The systems that spawn particles at the events of this system's particles; they become layers of the same effect, after this one.</summary>
        public List<SubEmitterData> SubEmitters = new List<SubEmitterData>();

        // Shape module.
        public ShapeType Shape = ShapeType.Cone;
        public ShapeEmit ShapeEmitFrom = ShapeEmit.Volume;
        public float ShapeAngle = 25f;
        public float ShapeRadius = 1f;
        public float ShapeRadiusThickness = 1f;
        public float ShapeArc = 360f;
        public float ShapeLength = 5f;
        public float[] ShapeBox = { 1, 1, 1 };
        public float[] ShapePosition = { 0, 0, 0 };
        public float[] ShapeRotation = { 0, 0, 0, 1 }; // x, y, z, w
        public float[] ShapeScale = { 1, 1, 1 };
        public float RandomizeDirection;
        public float SphericalizeDirection;
        public float RandomizePosition;

        // Over lifetime modules.
        public bool VelocityEnabled;
        public CurveData VelocityX = CurveData.Of(0f), VelocityY = CurveData.Of(0f), VelocityZ = CurveData.Of(0f);
        public SimulationSpace VelocitySpace = SimulationSpace.Local;
        public CurveData VelocityOrbitalX = CurveData.Of(0f), VelocityOrbitalY = CurveData.Of(0f), VelocityOrbitalZ = CurveData.Of(0f); // radians a second
        public CurveData VelocityRadial = CurveData.Of(0f); // units a second, away from the centre
        public CurveData VelocityOffsetX = CurveData.Of(0f), VelocityOffsetY = CurveData.Of(0f), VelocityOffsetZ = CurveData.Of(0f); // the centre of the orbit

        public bool VelocityHasOrbital => !VelocityOrbitalX.IsZero || !VelocityOrbitalY.IsZero || !VelocityOrbitalZ.IsZero;

        public bool VelocityHasRadial => !VelocityRadial.IsZero;

        public bool ForceEnabled;
        public CurveData ForceX = CurveData.Of(0f), ForceY = CurveData.Of(0f), ForceZ = CurveData.Of(0f);
        public SimulationSpace ForceSpace = SimulationSpace.Local;

        public bool LimitEnabled;
        public CurveData LimitSpeed = CurveData.Of(1f);
        public float LimitDampen = 1f;

        public bool ColorEnabled;
        public ColorData ColorOverLifetime = new ColorData { Mode = GradientMode.Color };

        public bool SizeEnabled;
        public bool SizeSeparateAxes;
        public CurveData SizeCurveX = CurveData.Of(1f), SizeCurveY = CurveData.Of(1f), SizeCurveZ = CurveData.Of(1f);

        public bool SizeBySpeedEnabled;
        public bool SizeBySpeedSeparateAxes;
        public CurveData SizeBySpeedX = CurveData.Of(1f), SizeBySpeedY = CurveData.Of(1f), SizeBySpeedZ = CurveData.Of(1f);
        public float SizeBySpeedMin, SizeBySpeedMax = 1f;

        public bool ColorBySpeedEnabled;
        public ColorData ColorBySpeed = new ColorData { Mode = GradientMode.Color };
        public float ColorBySpeedMin, ColorBySpeedMax = 1f;

        public bool LifetimeByEmitterSpeedEnabled;
        public CurveData LifetimeByEmitterSpeed = CurveData.Of(1f);
        public float LifetimeByEmitterSpeedMin, LifetimeByEmitterSpeedMax = 1f;

        public bool CollisionEnabled;
        public CollisionType CollisionType;
        public List<float[]> CollisionPlanes = new List<float[]>(); // normal x, y, z and the distance, in the space of the system
        public CurveData CollisionDampen = CurveData.Of(0f), CollisionBounce = CurveData.Of(0f), CollisionLifetimeLoss = CurveData.Of(0f);
        public float CollisionMinKillSpeed, CollisionMaxKillSpeed = 10000f, CollisionRadiusScale = 1f;

        public bool InheritEnabled;
        public bool InheritCurrent; // false: the emitter's velocity at the spawn is added to the particle's; true: the particles move with the emitter's current velocity too
        public CurveData InheritMultiplier = CurveData.Of(1f);

        public bool RotationBySpeedEnabled;
        public CurveData RotationBySpeedX = CurveData.Of(0f); // degrees a second
        public CurveData RotationBySpeedY = CurveData.Of(0f);
        public CurveData RotationBySpeedZ = CurveData.Of(0f);
        public float RotationBySpeedMin, RotationBySpeedMax = 1f;

        public bool RotationEnabled;
        public CurveData RotationOverLifetimeX = CurveData.Of(0f); // degrees a second
        public CurveData RotationOverLifetimeY = CurveData.Of(0f);
        public CurveData RotationOverLifetimeZ = CurveData.Of(0f);

        public bool NoiseEnabled;
        public CurveData NoiseStrengthX = CurveData.Of(1f), NoiseStrengthY = CurveData.Of(1f), NoiseStrengthZ = CurveData.Of(1f);
        public float NoiseFrequency = 0.5f;
        public CurveData NoiseScrollSpeed = CurveData.Of(0f);
        public float NoiseDamping = 1f;

        // Texture sheet animation.
        public bool SheetEnabled;
        public int SheetTilesX = 1;
        public int SheetTilesY = 1;
        public bool SheetSingleRow;
        public int SheetRow;
        public bool SheetRandomRow;
        public CurveData SheetFrameOverTime = CurveData.Of(0f);
        public CurveData SheetStartFrame = CurveData.Of(0f);
        public float SheetCycles = 1f;

        // Renderer.
        public RenderMode Render = RenderMode.Billboard;
        public string MaterialPath = "";
        public string MeshPath = "";
        public bool SortByDistance;
        public float StretchSpeedScale;
        public float StretchLengthScale = 2f;
        public float StretchCameraScale;
        public float MinParticleSize;
        public float MaxParticleSize = 0.5f;

        /// <summary>Modules that are on and that the converter has no node for: shown in the report.</summary>
        public List<string> Unsupported = new List<string>();
    }
}
