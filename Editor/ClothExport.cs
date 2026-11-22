using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    /// <summary>
    /// The <c>cloth</c> section of the character manifest (<c>Characters/&lt;name&gt;.json</c>, Toe target) from the Magica Cloth 2 components on the character's prefab. Nothing here
    /// refers to Magica Cloth 2 at compile time: the components are found by the name of their type (<c>MagicaCloth2.MagicaCloth</c>, and the colliders by a base type named
    /// <c>ColliderComponent</c>) and their data is read by reflection, so the package builds and works without it, and a version of it that renamed a member gives a warning and a default
    /// instead of a failed export. What is written is what the Toe engine's Toe.Cloth reads (<c>ClothConfigResource</c>): the authoring data (the root bones, the connection mode,
    /// the parameters with their curves sampled at 16 points along the depth, the colliders), not the proxy mesh, which the engine builds from the bones itself.
    /// </summary>
    public static class ClothExport
    {
        /// <summary>The full name of the type of the cloth component (the tests point it at stand-ins: Unity cannot tell two scripts of the same name apart).</summary>
        public static string ClothTypeName { get; set; } = "MagicaCloth2.MagicaCloth";
        private const string ColliderBaseName = "ColliderComponent";
        private const int CurveSamples = 16;
        private const BindingFlags Any = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>The cloths and colliders below <paramref name="root"/> (the character) as the <c>cloth</c> section; null when there are none. <paramref name="warn"/> is told what is wrong.</summary>
        public static JObject Collect(Transform root, Action<string> warn)
        {
            var components = root.GetComponentsInChildren<UnityEngine.Component>(true).Where(c => c != null).ToList();
            var clothComponents = components.Where(c => c.GetType().FullName == ClothTypeName && IsEnabled(c)).ToList();
            var colliderComponents = components.Where(c => IsCollider(c.GetType()) && IsEnabled(c)).ToList();
            if (clothComponents.Count == 0 && colliderComponents.Count == 0) return null;

            var colliderNames = new Dictionary<UnityEngine.Component, string>();
            var colliders = new JArray();
            foreach (var collider in colliderComponents)
            {
                var json = ColliderJson(root, collider, colliderNames.Values, warn, out string name);
                if (json == null) continue;
                colliderNames[collider] = name;
                colliders.Add(json);
            }

            var cloths = new JArray();
            foreach (var cloth in clothComponents)
            {
                var json = ClothJson(root, cloth, colliderNames, warn);
                if (json != null) cloths.Add(json);
            }
            if (cloths.Count == 0 && colliders.Count == 0) return null;
            return new JObject { ["colliders"] = colliders, ["cloths"] = cloths };
        }

        /// <summary>The manifest text of the section, indented for the manifest file.</summary>
        public static string ToJson(JObject section, string indent) => section.ToString(Newtonsoft.Json.Formatting.Indented).Replace("\n", "\n" + indent);

        private static bool IsEnabled(UnityEngine.Component component) => !(component is Behaviour behaviour) || behaviour.enabled;

        private static bool IsCollider(Type type)
        {
            for (var t = type; t != null; t = t.BaseType)
                if (t.Name == ColliderBaseName) return true;
            return false;
        }

        // ---- A cloth ----------------------------------------------------------------------------------------------

        private static JObject ClothJson(Transform root, UnityEngine.Component cloth, Dictionary<UnityEngine.Component, string> colliderNames, Action<string> warn)
        {
            var data = Get(cloth, "SerializeData") ?? Get(cloth, "serializeData");
            if (data == null)
            {
                warn($"The cloth '{cloth.name}' is left out: its SerializeData could not be read (a version of Magica Cloth 2 this exporter does not know).");
                return null;
            }

            string type = ClothTypeOf(Int(data, "clothType", 1));
            if (type == "MeshCloth")
            {
                warn($"The cloth '{cloth.name}' is a Mesh Cloth, which Toe.Cloth does not support yet: it is left out. Only Bone Cloth is exported.");
                return null;
            }

            var roots = Transforms(Get(data, "rootBones")).ToList();
            if (roots.Count == 0)
            {
                warn($"The cloth '{cloth.name}' has no root bones and is left out.");
                return null;
            }
            foreach (var bone in roots)
                if (!bone.IsChildOf(root)) warn($"The root bone '{bone.name}' of the cloth '{cloth.name}' is not below the character '{root.name}'.");

            var json = new JObject
            {
                ["name"] = cloth.name,
                ["type"] = type,
                ["roots"] = new JArray(roots.Select(Name)),
                ["connection"] = ConnectionOf(Int(data, "connectionMode", 0)),
                ["rotationalInterpolation"] = Float(data, "rotationalInterpolation", 0.5f),
                ["rootRotation"] = Float(data, "rootRotation", 0.5f),
                ["animationPoseRatio"] = Float(data, "animationPoseRatio", 0f),
                ["blendWeight"] = Float(data, "blendWeight", 1f),
                ["gravity"] = Float(data, "gravity", 5f),
                ["gravityDirection"] = Vector(data, "gravityDirection", new Vector3(0f, -1f, 0f)),
                ["gravityFalloff"] = Float(data, "gravityFalloff", 0f),
                ["stabilizationTime"] = Float(data, "stablizationTimeAfterReset", 0.1f),
                ["damping"] = Curve(Get(data, "damping"), 0.05f),
                ["radius"] = Curve(Get(data, "radius"), 0.02f),
                ["tetherCompression"] = Float(Get(data, "tetherConstraint"), "distanceCompression", 0.4f),
                ["distanceStiffness"] = Curve(Get(Get(data, "distanceConstraint"), "stiffness"), 1f),
                ["bendingStiffness"] = Float(Get(data, "triangleBendingConstraint"), "stiffness", 1f),
            };

            var inertia = Get(data, "inertiaConstraint");
            json["inertia"] = new JObject
            {
                ["worldInertia"] = Float(inertia, "worldInertia", Float(inertia, "movementInertia", 1f)),
                ["movementSpeedLimit"] = Limit(Get(inertia, "movementSpeedLimit"), true, 5f),
                ["rotationSpeedLimit"] = Limit(Get(inertia, "rotationSpeedLimit"), true, 720f),
                ["localInertia"] = Float(inertia, "localInertia", 1f),
                ["depthInertia"] = Float(inertia, "depthInertia", 0f),
                ["centrifugalAcceleration"] = Float(inertia, "centrifualAcceleration", Float(inertia, "centrifugalAcceleration", 0f)),
                ["particleSpeedLimit"] = Limit(Get(inertia, "particleSpeedLimit"), true, 4f),
                ["teleportMode"] = TeleportOf(Int(inertia, "teleportMode", 0)),
                ["teleportDistance"] = Float(inertia, "teleportDistance", 0.5f),
                ["teleportRotation"] = Float(inertia, "teleportRotation", 90f),
            };

            var restoration = Get(data, "angleRestorationConstraint");
            json["angleRestoration"] = new JObject
            {
                ["use"] = Bool(restoration, "useAngleRestoration", true),
                ["stiffness"] = Curve(Get(restoration, "stiffness"), 0.2f),
                ["velocityAttenuation"] = Float(restoration, "velocityAttenuation", 0.8f),
                ["gravityFalloff"] = Float(restoration, "gravityFalloff", 0f),
            };
            var limit = Get(data, "angleLimitConstraint");
            json["angleLimit"] = new JObject
            {
                ["use"] = Bool(limit, "useAngleLimit", false),
                ["limitAngle"] = Curve(Get(limit, "limitAngle"), 60f),
                ["stiffness"] = Float(limit, "stiffness", 1f),
            };
            var motion = Get(data, "motionConstraint");
            json["motion"] = new JObject
            {
                ["useMaxDistance"] = Bool(motion, "useMaxDistance", false),
                ["maxDistance"] = Curve(Get(motion, "maxDistance"), 0.3f),
                ["useBackstop"] = Bool(motion, "useBackstop", false),
                ["backstopRadius"] = Float(motion, "backstopRadius", 10f),
                ["backstopDistance"] = Curve(Get(motion, "backstopDistance"), 0f),
                ["stiffness"] = Float(motion, "stiffness", 1f),
            };

            var collision = Get(data, "colliderCollisionConstraint");
            var listed = new JArray();
            foreach (var collider in Components(Get(collision, "colliderList")))
            {
                if (colliderNames.TryGetValue(collider, out string colliderName)) listed.Add(colliderName);
                else warn($"The cloth '{cloth.name}' lists the collider '{collider.name}', which is not exported (it is disabled or not below '{root.name}').");
            }
            json["collision"] = new JObject { ["mode"] = CollisionModeOf(Int(collision, "mode", 1)), ["friction"] = Float(collision, "friction", 0.05f), ["colliders"] = listed };

            AddPaint(root, cloth, data, roots, json, warn);
            return json;
        }

        private static string ClothTypeOf(int value) => value == 0 ? "MeshCloth" : value == 10 ? "BoneSpring" : "BoneCloth";

        private static string ConnectionOf(int value) => value == 1 ? "AutomaticMesh" : value == 2 ? "SequentialLoopMesh" : value == 3 ? "SequentialNonLoopMesh" : "Line";

        private static string TeleportOf(int value) => value == 1 ? "Reset" : value == 2 ? "Keep" : "None";

        private static string CollisionModeOf(int value) => value == 0 ? "None" : value == 2 ? "Edge" : "Point";

        /// <summary>
        /// Bones that were painted in Magica Cloth's editor: its selection data holds a point and an attribute (1 fixed, 2 move, neither: invalid) for each bone, in the space of the cloth
        /// object. They are matched to the bones by the nearest place; nothing is written when the selection was not edited by hand (the roots are fixed and the rest moves anyway).
        /// </summary>
        private static void AddPaint(Transform root, UnityEngine.Component cloth, object data, List<Transform> roots, JObject json, Action<string> warn)
        {
            var selection = Get(Get(cloth, "serializeData2"), "selectionData");
            if (selection == null || !Bool(selection, "userEdit", false)) return;
            if (!(Get(selection, "positions") is IList positions) || !(Get(selection, "attributes") is IList attributes) || positions.Count != attributes.Count || positions.Count == 0) return;

            var bones = new List<Transform>();
            foreach (var bone in roots) bones.AddRange(bone.GetComponentsInChildren<Transform>(true));
            var fixedBones = new JArray();
            var invalidBones = new JArray();
            for (int i = 0; i < positions.Count; i++)
            {
                var local = ReadVector(positions[i]);
                var world = cloth.transform.TransformPoint(local);
                var nearest = bones.OrderBy(b => (b.position - world).sqrMagnitude).FirstOrDefault();
                if (nearest == null || (nearest.position - world).magnitude > 0.01f) continue;
                int attribute = Convert.ToInt32(Get(attributes[i], "Value") ?? 0);
                if ((attribute & 0x01) != 0 && !roots.Contains(nearest)) fixedBones.Add(Name(nearest));
                else if ((attribute & 0x03) == 0) invalidBones.Add(Name(nearest));
            }
            if (fixedBones.Count > 0) json["fixed"] = fixedBones;
            if (invalidBones.Count > 0) json["invalid"] = invalidBones;
        }

        // ---- A collider -------------------------------------------------------------------------------------------

        private static JObject ColliderJson(Transform root, UnityEngine.Component collider, IEnumerable<string> used, Action<string> warn, out string name)
        {
            name = null;
            string typeName = collider.GetType().Name;
            string shape = typeName.Contains("Sphere") ? "Sphere" : typeName.Contains("Capsule") ? "Capsule" : typeName.Contains("Plane") ? "Plane" : null;
            if (shape == null)
            {
                warn($"The collider '{collider.name}' ({typeName}) is of a kind Toe.Cloth does not know and is left out.");
                return null;
            }
            if (Int(collider, "symmetryMode", 0) != 0)
                warn($"The collider '{collider.name}' has a symmetry mode, which is not exported: add a collider on the other side by hand.");

            var scale = Divide(collider.transform.lossyScale, root.lossyScale);
            var size = ReadVector(Get(collider, "size"), new Vector3(0.05f, 0.05f, 0.3f));
            var center = Vector3.Scale(ReadVector(Get(collider, "center")), new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            float maxScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            string direction = "X";
            bool reverse = false, aligned = true;

            if (shape == "Capsule")
            {
                int d = Int(collider, "direction", 0);
                direction = d == 1 ? "Y" : d == 2 ? "Z" : "X";
                reverse = Bool(collider, "reverseDirection", false);
                aligned = Bool(collider, "alignedOnCenter", true);
                bool separated = Bool(collider, "radiusSeparation", false);
                float along = Mathf.Abs(direction == "X" ? scale.x : direction == "Y" ? scale.y : scale.z);
                float across = Mathf.Max(direction == "X" ? 0f : Mathf.Abs(scale.x), Mathf.Max(direction == "Y" ? 0f : Mathf.Abs(scale.y), direction == "Z" ? 0f : Mathf.Abs(scale.z)));
                size = new Vector3(size.x * across, (separated ? size.y : size.x) * across, size.z * along);
            }
            else
            {
                size = new Vector3(size.x * maxScale, 0f, 0f);
            }

            string baseName = collider.name + "_" + shape.ToLowerInvariant();
            name = baseName;
            for (int n = 2; used.Contains(name); n++) name = baseName + n;
            return new JObject
            {
                ["name"] = name,
                ["bone"] = Name(collider.transform),
                ["shape"] = shape,
                ["center"] = new JArray(center.x, center.y, center.z),
                ["size"] = new JArray(size.x, size.y, size.z),
                ["direction"] = direction,
                ["reverse"] = reverse,
                ["alignedOnCenter"] = aligned,
            };
        }

        // ---- Reading what Magica Cloth keeps ------------------------------------------------------------------------

        /// <summary>The names the model files give the bones (see <c>ExporterBase.DecorateName</c>).</summary>
        private static string Name(Transform transform) => Uri.EscapeDataString(transform.name);

        /// <summary>A field or a property (public or not) of an object, or null when it has none of that name.</summary>
        private static object Get(object target, string name)
        {
            if (target == null) return null;
            var type = target.GetType();
            for (var t = type; t != null; t = t.BaseType)
            {
                var field = t.GetField(name, Any | BindingFlags.DeclaredOnly);
                if (field != null) return field.GetValue(target);
                var property = t.GetProperty(name, Any | BindingFlags.DeclaredOnly);
                if (property != null && property.GetIndexParameters().Length == 0 && property.CanRead) return property.GetValue(target);
            }
            return null;
        }

        private static float Float(object target, string name, float fallback)
        {
            var value = Get(target, name);
            return value == null ? fallback : Convert.ToSingle(value);
        }

        private static int Int(object target, string name, int fallback)
        {
            var value = Get(target, name);
            return value == null ? fallback : Convert.ToInt32(value);
        }

        private static bool Bool(object target, string name, bool fallback)
        {
            var value = Get(target, name);
            return value == null ? fallback : Convert.ToBoolean(value);
        }

        private static JArray Vector(object target, string name, Vector3 fallback)
        {
            var v = ReadVector(Get(target, name), fallback);
            return new JArray(v.x, v.y, v.z);
        }

        /// <summary>A vector from a Unity Vector3 or from Unity.Mathematics' float3 (both have x, y and z fields).</summary>
        private static Vector3 ReadVector(object value, Vector3 fallback = default)
        {
            if (value == null) return fallback;
            if (value is Vector3 vector) return vector;
            var x = Get(value, "x");
            var y = Get(value, "y");
            var z = Get(value, "z");
            return x == null || y == null || z == null ? fallback : new Vector3(Convert.ToSingle(x), Convert.ToSingle(y), Convert.ToSingle(z));
        }

        private static IEnumerable<Transform> Transforms(object list) => Components(list).Select(c => c.transform).Distinct();

        private static IEnumerable<UnityEngine.Component> Components(object list)
        {
            if (!(list is IEnumerable items) || list is string) yield break;
            foreach (var item in items)
                if (item is UnityEngine.Component component && component != null) yield return component;
        }

        /// <summary>A Magica Cloth curve (<c>CurveSerializeData</c>: value, useCurve, curve): a number when it is constant, else 16 samples along the depth, already multiplied by the value.</summary>
        private static JToken Curve(object curve, float fallback)
        {
            if (curve == null) return fallback;
            float value = Float(curve, "value", fallback);
            if (!Bool(curve, "useCurve", false) || !(Get(curve, "curve") is AnimationCurve animation) || animation.length == 0) return value;
            var samples = new JArray();
            for (int i = 0; i < CurveSamples; i++) samples.Add(animation.Evaluate(i / (float)(CurveSamples - 1)) * value);
            return new JObject { ["value"] = value, ["samples"] = samples };
        }

        /// <summary>A limit that can be switched off: <c>CheckSliderSerializeData</c> (value, use) or <c>MinMax</c>-like data with the same two names.</summary>
        private static JObject Limit(object limit, bool defaultUse, float defaultValue)
            => new JObject { ["use"] = Bool(limit, "use", defaultUse), ["value"] = Float(limit, "value", defaultValue) };

        private static Vector3 Divide(Vector3 a, Vector3 b)
            => new Vector3(Mathf.Approximately(b.x, 0f) ? a.x : a.x / b.x, Mathf.Approximately(b.y, 0f) ? a.y : a.y / b.y, Mathf.Approximately(b.z, 0f) ? a.z : a.z / b.z);
    }
}
