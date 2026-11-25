using System;
using System.Collections.Generic;
using UnityEngine;

// Stand-ins for the data classes of Magica Cloth 2, with the names of its members (in a namespace of their own: Unity cannot tell two scripts of the same full name apart): the cloth exporter reads those by name (reflection), so it can be tested without the asset.
namespace MagicaFakes
{
    [Serializable]
    public class CurveSerializeData
    {
        public float value = 1f;
        public bool useCurve;
        public AnimationCurve curve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    }

    [Serializable]
    public class CheckSliderSerializeData
    {
        public float value;
        public bool use;
    }

    [Serializable]
    public class InertiaData
    {
        public float worldInertia = 1f;
        public CheckSliderSerializeData movementSpeedLimit = new CheckSliderSerializeData { value = 5f, use = true };
        public CheckSliderSerializeData rotationSpeedLimit = new CheckSliderSerializeData { value = 720f, use = true };
        public float localInertia = 1f;
        public float depthInertia;
        public float centrifualAcceleration;
        public CheckSliderSerializeData particleSpeedLimit = new CheckSliderSerializeData { value = 4f, use = true };
        public int teleportMode;
        public float teleportDistance = 0.5f;
        public float teleportRotation = 90f;
    }

    [Serializable] public class TetherData { public float distanceCompression = 0.4f; }
    [Serializable] public class DistanceData { public CurveSerializeData stiffness = new CurveSerializeData(); }
    [Serializable] public class BendingData { public float stiffness = 1f; }

    [Serializable]
    public class AngleRestorationData
    {
        public bool useAngleRestoration = true;
        public CurveSerializeData stiffness = new CurveSerializeData { value = 0.2f };
        public float velocityAttenuation = 0.8f;
        public float gravityFalloff;
    }

    [Serializable]
    public class AngleLimitData
    {
        public bool useAngleLimit;
        public CurveSerializeData limitAngle = new CurveSerializeData { value = 60f };
        public float stiffness = 1f;
    }

    [Serializable]
    public class MotionData
    {
        public bool useMaxDistance;
        public CurveSerializeData maxDistance = new CurveSerializeData { value = 0.3f };
        public bool useBackstop;
        public float backstopRadius = 10f;
        public CurveSerializeData backstopDistance = new CurveSerializeData { value = 0f };
        public float stiffness = 1f;
    }

    [Serializable]
    public class SpringData
    {
        public bool useSpring = true;
        public float springPower = 0.04f;
        public float limitDistance = 0.1f;
        public float normalLimitRatio = 1f;
        public float springNoise;
    }

    [Serializable]
    public class ColliderCollisionData
    {
        public int mode = 1;
        public float friction = 0.05f;
        public CurveSerializeData limitDistance = new CurveSerializeData { value = 0.05f };
        public List<ColliderComponent> colliderList = new List<ColliderComponent>();
    }

    [Serializable]
    public class ClothSerializeData
    {
        public int clothType = 1;
        public List<Renderer> sourceRenderers = new List<Renderer>();
        public List<Transform> rootBones = new List<Transform>();
        public int connectionMode;
        public float rotationalInterpolation = 0.5f;
        public float rootRotation = 0.5f;
        public float animationPoseRatio;
        public float gravity = 5f;
        public Vector3 gravityDirection = new Vector3(0f, -1f, 0f);
        public float gravityFalloff;
        public float stablizationTimeAfterReset = 0.1f;
        public float blendWeight = 1f;
        public CurveSerializeData damping = new CurveSerializeData { value = 0.05f };
        public CurveSerializeData radius = new CurveSerializeData { value = 0.02f };
        public InertiaData inertiaConstraint = new InertiaData();
        public TetherData tetherConstraint = new TetherData();
        public DistanceData distanceConstraint = new DistanceData();
        public BendingData triangleBendingConstraint = new BendingData();
        public AngleRestorationData angleRestorationConstraint = new AngleRestorationData();
        public AngleLimitData angleLimitConstraint = new AngleLimitData();
        public MotionData motionConstraint = new MotionData();
        public ColliderCollisionData colliderCollisionConstraint = new ColliderCollisionData();
        public SpringData springConstraint = new SpringData();
        public int normalAxis;
    }

    [Serializable] public struct VertexAttribute { public byte Value; }

    [Serializable]
    public class SelectionData
    {
        public Vector3[] positions = new Vector3[0];
        public VertexAttribute[] attributes = new VertexAttribute[0];
        public bool userEdit;
    }

    [Serializable]
    public class ClothSerializeData2
    {
        public SelectionData selectionData = new SelectionData();
    }
}
