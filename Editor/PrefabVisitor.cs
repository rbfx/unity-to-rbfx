using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityToRebelFork.Editor
{
    public class PrefabVisitor
    {
        private readonly Lazy<ExportOrchestrator> _orchestrator;

        private readonly ExportSettings _settings;

        //private readonly PrefabExporter prefabExporter;

        //private readonly MeshExporter meshExporter;

        //private readonly MaterialExporter materialExporter;

        //private readonly TextureExporter textureExporter;

        /// <summary>
        /// Unique Id counter.
        /// </summary>
        private int uniqueIdCounter = 0;

        public PrefabVisitor(Lazy<ExportOrchestrator> _orchestrator, ExportSettings settings) //, PrefabExporter prefabExporter, MeshExporter meshExporter,MaterialExporter materialExporter,TextureExporter textureExporter)
        {
            this._orchestrator = _orchestrator;
            this._settings = settings;
            //this.prefabExporter = prefabExporter;
            //this.meshExporter = meshExporter;
            //this.materialExporter = materialExporter;
            //this.textureExporter = textureExporter;
        }

        private static HashSet<string> prefabPropertiesToIgnore = new HashSet<string>()
        {
            "m_Name", "m_RootOrder", "m_LocalPosition.x", "m_LocalPosition.y", "m_LocalPosition.z", "m_LocalRotation.w",
            "m_LocalRotation.x", "m_LocalRotation.y", "m_LocalRotation.z", "m_LocalEulerAnglesHint.x",
            "m_LocalEulerAnglesHint.y", "m_LocalEulerAnglesHint.z",
            "m_LocalScale.x","m_LocalScale.y","m_LocalScale.z", "TAAQuality", "antialiasing", "m_StaticEditorFlags", "m_RenderingLayerMask"
        };

        private HashSet<UnityEngine.Component> skipList = new HashSet<UnityEngine.Component>();

        public Scene Visit(UnityEngine.SceneManagement.Scene unityScene)
        {
            uniqueIdCounter = 0;

            var scene = Helpers.CreateDefaultScene(unityScene.name);
            if (!(_settings is null) && !_settings.SceneEnvironment)
            {
                for (int i = scene.Components.Count - 1; i >= 0; --i)
                    if (scene.Components[i] is Zone || scene.Components[i] is Skybox) scene.Components.RemoveAt(i);
            }
            scene.Id = ++uniqueIdCounter;

            foreach (var gameObject in unityScene.GetRootGameObjects())
            {
                scene.Children.Add(Visit(gameObject));
            }
            return scene;
        }

        /// <summary>
        /// Translate game object into RebelFork's Node.
        /// </summary>
        /// <param name="gameObject">Game object.</param>
        /// <param name="isRoot">True if the object is prefab's root, false otherwise.</param>
        /// <returns>Node created from the game object.</returns>
        public Node Visit(UnityEngine.GameObject gameObject, bool isRoot = false)
        {
            if (isRoot)
            {
                uniqueIdCounter = 0;
            }
            var node = new Node();
            node.Id = ++uniqueIdCounter;
            node.Position = gameObject.transform.localPosition;
            node.Rotation = gameObject.transform.localRotation;
            node.Scale = gameObject.transform.localScale;
            node.Name = gameObject.name;
            node.IsEnabled = gameObject.activeSelf;


            // (Settings made in code are ScriptableObjects that opening a scene may unload: Unity's == null would say they are gone.)
            if (!isRoot && !(!(_settings is null) && _settings.InlinePrefabs) && PrefabUtility.IsAnyPrefabInstanceRoot(gameObject))
            {
                var type = PrefabUtility.GetPrefabAssetType(gameObject);
                if (type == PrefabAssetType.Regular)
                {
                    var modifications = PrefabUtility.GetPropertyModifications(gameObject);
                    if (modifications != null && modifications.Length > 0)
                    {
                        if (!modifications
                                .Any(_ => _.propertyPath == null ||
                                          !prefabPropertiesToIgnore.Contains(_.propertyPath)))
                        {
                            var originalSource = PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);
                            if (originalSource)
                            {
                                node.Components.Add(new PrefabReference()
                                {
                                    Prefab = new ResourceRef(nameof(PrefabResource), _orchestrator.Value.ScheduleExport(originalSource))
                                });
                                return node;
                            }
                        }
                        else
                        {
                            //var props = modifications.Select(_ => _.propertyPath).Where(_ => _ != null).Distinct().Where(_ => !prefabPropertiesToIgnore.Contains(_)).ToList();
                        }
                    }
                }
            }

            foreach (var component in gameObject.GetComponents<UnityEngine.Component>())
            {
                Visit(node, component);
            }
            VisitBody(node, gameObject);

            for (int childIndex = 0; childIndex < gameObject.transform.childCount; ++childIndex)
            {
                var child = gameObject.transform.GetChild(childIndex);
                if (IsOrphanSkeleton(child)) continue;
                node.Children.Add(Visit(child.gameObject));
            }

            return node;
        }

        private readonly Dictionary<Transform, SkinnedMeshRenderer[]> skinsByRoot = new Dictionary<Transform, SkinnedMeshRenderer[]>();

        /// <summary>
        /// True for the bones (and the armature above them) of a skinned mesh that sit beside its object rather than below it, as in a model
        /// file's prefab: only plain transforms. The exported model carries its skeleton and the model component makes the bone nodes below its
        /// own node; the ones exported here would be a second, motionless set that a search by bone name can find first.
        /// </summary>
        private bool IsOrphanSkeleton(Transform transform)
        {
            if (!transform.GetComponentsInChildren<UnityEngine.Component>(true).All(c => c is Transform)) return false;
            var root = transform.root;
            if (!skinsByRoot.TryGetValue(root, out var skins))
                skinsByRoot[root] = skins = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var skin in skins)
            {
                if (skin == null || skin.sharedMesh == null || transform.IsChildOf(skin.transform)) continue;
                if (skin.rootBone != null && skin.rootBone.IsChildOf(transform)) return true;
                foreach (var bone in skin.bones)
                    if (bone != null && bone.IsChildOf(transform)) return true;
            }
            return false;
        }

        /// <summary>
        /// Translate Unity component into RebelFork's component.
        /// </summary>
        /// <param name="node">RebelFork Node.</param>
        /// <param name="component">Unity component.</param>
        public void Visit(Node node, UnityEngine.Component component)
        {
            if (skipList.Contains(component))
                return;

            if (component is LODGroup lodGroup)
            {
                VisitLODGroup(node, lodGroup);
            }
            else if (component is MeshRenderer meshRenderer)
            {
                VisitMeshRenderer(node, meshRenderer);
            }
            else if (component is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                VisitSkinnedMeshRenderer(node, skinnedMeshRenderer);
            }
            else if (component is UnityEngine.Camera camera)
            {
                VisitCamera(node, camera);
            }
            else if (component is UnityEngine.Light light)
            {
                VisitLight(node, light);
            }
            else if (component is UnityEngine.Joint joint)
            {
                VisitJoint(node, joint);
            }
            else if (component is MonoBehaviour behaviour && GetExportComponent(behaviour) is { } exportComponent)
            {
                VisitExportComponent(node, behaviour, exportComponent);
            }
            // Colliders and rigidbodies: VisitBody, once per game object.
        }

        private static ExportComponentAttribute GetExportComponent(MonoBehaviour behaviour)
            => behaviour == null ? null : (ExportComponentAttribute)Attribute.GetCustomAttribute(behaviour.GetType(), typeof(ExportComponentAttribute));

        /// <summary>A behaviour marked [ExportComponent]: a component of that type with its fields as attributes (disabled behaviours are left out).</summary>
        private void VisitExportComponent(Node node, MonoBehaviour behaviour, ExportComponentAttribute exportComponent)
        {
            if (!behaviour.enabled) return;
            var component = new GenericComponent(exportComponent.TypeName);
            var types = new List<Type>();
            for (var type = behaviour.GetType(); type != null && type != typeof(MonoBehaviour); type = type.BaseType) types.Insert(0, type);
            foreach (var type in types)
            {
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (field.IsNotSerialized || field.IsDefined(typeof(ExportIgnoreAttribute), true)) continue;
                    if (!field.IsPublic && !field.IsDefined(typeof(SerializeField), true)) continue;
                    if (!(ToVariant(field.GetValue(behaviour)) is { } value))
                    {
                        Debug.LogWarning($"[ExportComponent] {behaviour.GetType().Name}.{field.Name}: {field.FieldType.Name} fields are not exported");
                        continue;
                    }
                    var name = (field.GetCustomAttribute(typeof(ExportNameAttribute), true) as ExportNameAttribute)?.Name
                               ?? ObjectNames.NicifyVariableName(field.Name);
                    component.Set(name, value);
                }
            }
            node.Components.Add(component);
            if (exportComponent.StableId && !node.Components.Any(_ => _ is GenericComponent g && g.TypeName == "StableId"))
                node.Components.Add(new GenericComponent("StableId").Set("Id", new Variant(VariantType.VarString, StableIdOf(behaviour.gameObject))));
        }

        /// <summary>A value as an attribute, or null for a type that has none.</summary>
        private Variant? ToVariant(object value)
        {
            switch (value)
            {
                case bool b: return new Variant(b);
                case int i: return new Variant(i);
                case uint u: return new Variant(unchecked((int)u));
                case float f: return new Variant(f);
                case double d: return new Variant((float)d);
                case string s: return new Variant(VariantType.VarString, s);
                case null: return null;
                case Enum e: return new Variant(VariantType.VarString, e.ToString());
                case Vector2 v2: return new Variant(VariantType.VarVector2, v2);
                case Vector3 v3: return new Variant(VariantType.VarVector3, v3);
                case Vector4 v4: return new Variant(VariantType.VarVector4, v4);
                case Quaternion q: return new Variant(VariantType.VarQuaternion, q);
                case UnityEngine.Color c: return new Variant(VariantType.VarColor, c);
                case UnityEngine.Mesh mesh: return new Variant(VariantType.VarResourceRef, ResourceRef.Create<Model>(_orchestrator.Value.ScheduleExport(mesh)));
                case UnityEngine.Material material: return new Variant(VariantType.VarResourceRef, ResourceRef.Create<Material>(_orchestrator.Value.ScheduleExport(material)));
                case UnityEngine.Texture2D texture: return new Variant(VariantType.VarResourceRef, ResourceRef.Create<Texture2D>(_orchestrator.Value.ScheduleExport(texture)));
                default: return null;
            }
        }

        /// <summary>
        /// 32 hex digits from the object's GlobalObjectId: the same on every export of a saved scene, different for every object, prefab
        /// instances included. An object of an unsaved scene falls back to its path in the hierarchy.
        /// </summary>
        public static string StableIdOf(GameObject gameObject)
        {
            var id = GlobalObjectId.GetGlobalObjectIdSlow(gameObject);
            string key = id.targetObjectId != 0 ? id.ToString() : gameObject.scene.path + "/" + HierarchyPath(gameObject.transform);
            using (var md5 = MD5.Create())
                return new Guid(md5.ComputeHash(Encoding.UTF8.GetBytes(key))).ToString("N");
        }

        private static string HierarchyPath(Transform transform)
            => transform.parent == null ? transform.name + "#" + transform.GetSiblingIndex()
                : HierarchyPath(transform.parent) + "/" + transform.name + "#" + transform.GetSiblingIndex();

        /// <summary>
        /// The colliders of a game object and the body they belong to. Shapes do not scale with their node in the engine, so the object's
        /// scale is baked into their sizes. Colliders without a Rigidbody make a static body; colliders under another object's Rigidbody
        /// (compound bodies) are not supported and are left out.
        /// </summary>
        private void VisitBody(Node node, GameObject gameObject)
        {
            var rigidbody = gameObject.GetComponent<Rigidbody>();
            var settings = gameObject.GetComponent<ExportBody>();
            var colliders = new List<Collider>();
            foreach (var collider in gameObject.GetComponents<Collider>())
            {
                if (!collider.enabled) continue;
                if (collider.attachedRigidbody != null && collider.attachedRigidbody != rigidbody)
                {
                    Debug.LogWarning($"[PrefabVisitor] {gameObject.name}: a collider under the Rigidbody of {collider.attachedRigidbody.name} is not exported (no compound bodies)");
                    continue;
                }
                colliders.Add(collider);
            }
            var scale = gameObject.transform.lossyScale;
            var shapes = colliders.Select(_ => VisitCollisionShape(_, scale)).Where(_ => _ != null).ToList();
            if (shapes.Count == 0 && rigidbody == null) return;
            foreach (var shape in shapes) node.Components.Add(shape);

            var type = settings != null ? settings.bodyType : ExportBodyType.Auto;
            if (type == ExportBodyType.Auto)
                type = rigidbody == null ? ExportBodyType.Static : rigidbody.isKinematic ? ExportBodyType.Kinematic : ExportBodyType.Dynamic;
            var body = new GenericComponent("RigidBody");
            body.Set("Body Type", new Variant(VariantType.VarString, type.ToString()));
            if (rigidbody != null)
            {
                if (type == ExportBodyType.Dynamic) body.Set("Mass", new Variant(rigidbody.mass));
                if (!rigidbody.useGravity) body.Set("Use Gravity", new Variant(false));
                if (rigidbody.linearDamping > 0f) body.Set("Linear Damping", new Variant(rigidbody.linearDamping));
                if (rigidbody.angularDamping > 0f) body.Set("Angular Damping", new Variant(rigidbody.angularDamping));
            }
            int triggers = colliders.Count(_ => _.isTrigger);
            if (triggers > 0 && triggers == colliders.Count) body.Set("Is Trigger", new Variant(true));
            else if (triggers > 0) Debug.LogWarning($"[PrefabVisitor] {gameObject.name}: trigger and solid colliders on one object; exported as solid");
            if (settings != null && settings.layer != 0) body.Set("Collision Layer", new Variant(settings.layer));
            if (settings != null && settings.mask != 0) body.Set("Collision Mask", new Variant(settings.mask));
            node.Components.Add(body);
        }

        private void VisitJoint(Node node, Joint joint)
        {

        }

        private void VisitSkinnedMeshRenderer(Node node, UnityEngine.SkinnedMeshRenderer skinnedMeshRenderer)
        {
            // A skinned mesh is placed by its bones, not by the transform of its renderer's object (the bind pose puts the vertices in the
            // bones' space, whatever the object does), but a model's node moves the whole skeleton: leave the node where its parent is.
            node.Position = Vector3.zero;
            node.Rotation = Quaternion.identity;
            node.Scale = Vector3.one;
            var component = new AnimatedModel();
            component.Model = ResourceRef.Create<Model>(_orchestrator.Value.ScheduleExport(skinnedMeshRenderer.sharedMesh));
            component.Material = ResourceRefList.Create<Material>(skinnedMeshRenderer.sharedMaterials.Select(_ => _orchestrator.Value.ScheduleExport(_)));
            component.CastShadows = skinnedMeshRenderer.shadowCastingMode != ShadowCastingMode.Off;
            node.Components.Add(component);
        }

        private void VisitMeshRenderer(Node node, UnityEngine.MeshRenderer meshRenderer)
        {
            var component = new StaticModel();
            var meshFilter = meshRenderer.gameObject.GetComponent<MeshFilter>();
            component.Model = ResourceRef.Create<Model>(_orchestrator.Value.ScheduleExport(meshFilter?.sharedMesh));
            component.Material = ResourceRefList.Create<Material>(meshRenderer.sharedMaterials.Select(_=>_orchestrator.Value.ScheduleExport(_)));
            component.CastShadows = meshRenderer.shadowCastingMode != ShadowCastingMode.Off;
            node.Components.Add(component);
        }

        private void VisitLight(Node node, UnityEngine.Light light)
        {
            switch (light.type)
            {
                case UnityEngine.LightType.Directional:
                    node.Components.Add(VisitDirectionalLight(light));
                    return;
                case UnityEngine.LightType.Spot:
                    node.Components.Add(VisitSpotLight(light));
                    return;
                case UnityEngine.LightType.Point:
                    node.Components.Add(VisitPointLight(light));
                    return;
            }
        }

        private void PopulateDefaultLightProperties(Light component, UnityEngine.Light light)
        {
            component.Color = ToRebelForkColorSpace(light.color);
            component.BrightnessMultiplier = light.intensity;
            component.UsePhysicalValues = false;
            component.DepthConstantBias = 0.0001f;
            component.CastShadows = light.shadows != LightShadows.None;
            component.IsEnabled = light.enabled;

            if (light.cookie != null)
            {
                component.LightShapeTexture = ResourceRef.Create<Texture2D>(_orchestrator.Value.ScheduleExport(light.cookie));
            }
        }

        private Color ToRebelForkColorSpace(Color lightColor)
        {
            return lightColor;
        }
        
        private Component VisitPointLight(UnityEngine.Light light)
        {
            var component = new UnityToRebelFork.Editor.Light();
            component.LightType = LightType.Point;
            component.Range = light.range;

            PopulateDefaultLightProperties(component, light);
            return component;
        }

        private Component VisitSpotLight(UnityEngine.Light light)
        {
            var component = new Light();
            component.LightType = LightType.Spot;
            component.SpotFOV = light.spotAngle;
            component.Range = light.range;

            PopulateDefaultLightProperties(component, light);
            return component;
        }

        private Component VisitDirectionalLight(UnityEngine.Light light)
        {
            var component = new Light();
            component.LightType = LightType.Directional;
            var shadowCascades = QualitySettings.shadowCascades;
            if (shadowCascades > 0)
            {
                if (shadowCascades > 4)
                    shadowCascades = 4;
                var cascadeMask = new[]
                {
                    new Vector4(0, 0, 0, 0),
                    new Vector4(1, 0, 0, 0),
                    new Vector4(1, 1, 0, 0),
                    new Vector4(1, 1, 1, 0)
                };
                var shadowCascade4Split = QualitySettings.shadowCascade4Split * QualitySettings.shadowDistance;
                var csmSplits = Vector4.Scale(
                    new Vector4(shadowCascade4Split.x, shadowCascade4Split.y, shadowCascade4Split.z,
                        QualitySettings.shadowDistance),
                    cascadeMask[shadowCascades - 1]);
                switch (shadowCascades)
                {
                    case 1:
                        csmSplits.x = QualitySettings.shadowDistance;
                        break;
                    case 2:
                        csmSplits.y = QualitySettings.shadowDistance;
                        break;
                    case 3:
                        csmSplits.z = QualitySettings.shadowDistance;
                        break;
                    case 4:
                        csmSplits.w = QualitySettings.shadowDistance;
                        break;
                }

                component.CSMSplits = csmSplits;
            }

            PopulateDefaultLightProperties(component, light);
            return component;
        }

        private void VisitCamera(Node node, UnityEngine.Camera camera)
        {
            var component = new Camera();
            component.IsEnabled = camera.enabled;
            component.NearClip = camera.nearClipPlane;
            component.FarClip = camera.farClipPlane;
            component.FOV = camera.fieldOfView;
            component.AspectRatio = camera.aspect;
            component.Orthographic = camera.orthographic;
            component.OrthographicSize = camera.orthographicSize;
            node.Components.Add(component);
        }

        private void VisitLODGroup(Node node, LODGroup lodGroup)
        {
            var component = new StaticModel();
            var meshRef = new MeshReference(lodGroup, _orchestrator.Value);
            component.Model = ResourceRef.Create<Model>(_orchestrator.Value.ScheduleExport(meshRef));
            component.Material = ResourceRefList.Create<Material>(meshRef.Materials.Select(_=>_.Material));
            foreach (var lod in lodGroup.GetLODs())
            {
                foreach (var lodRenderer in lod.renderers)
                {
                    if (lodRenderer != null)
                        skipList.Add(lodRenderer);
                }
            }
            node.Components.Add(component);
        }


        private CollisionShape VisitCollisionShape(Collider collider, Vector3 scale)
        {
            if (collider is BoxCollider boxCollider)
                return VisitBoxCollider(boxCollider, scale);
            if (collider is TerrainCollider terrainCollider)
                return VisitTerrainCollider(terrainCollider);
            if (collider is SphereCollider sphereCollider)
                return VisitSphereCollider(sphereCollider, scale);
            if (collider is CapsuleCollider capsuleCollider)
                return VisitCapsuleCollider(capsuleCollider, scale);
            if (collider is MeshCollider meshCollider)
                return VisitMeshCollider(meshCollider, scale);
            return null;
        }

        private static Vector3 Abs(Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

        private CollisionShape VisitMeshCollider(MeshCollider meshCollider, Vector3 scale)
        {
            if (meshCollider.sharedMesh == null) return null;
            var component = new CollisionShape();
            component.ShapeType = meshCollider.convex ? ShapeType.ConvexHull : ShapeType.TriangleMesh;
            component.Model = ResourceRef.Create<Model>(_orchestrator.Value.ScheduleExport(meshCollider.sharedMesh));
            // The engine scales a mesh shape's vertices by its Size.
            component.Size = scale;
            return component;
        }

        private CollisionShape VisitCapsuleCollider(CapsuleCollider capsuleCollider, Vector3 scale)
        {
            var component = new CollisionShape();
            if (capsuleCollider.name == "Cylinder")
                component.ShapeType = ShapeType.Cylinder;
            else
                component.ShapeType = ShapeType.Capsule;
            var s = Abs(scale);
            int axis = capsuleCollider.direction;
            float radiusScale = axis == 0 ? Mathf.Max(s.y, s.z) : axis == 1 ? Mathf.Max(s.x, s.z) : Mathf.Max(s.x, s.y);
            var d = capsuleCollider.radius * 2.0f * radiusScale;
            var height = Mathf.Max(d, capsuleCollider.height * s[axis]);
            component.Size = new Vector3(d, height, d);
            component.OffsetPosition = Vector3.Scale(capsuleCollider.center, scale);
            switch (capsuleCollider.direction)
            {
                case 0:
                    component.OffsetRotation = new Quaternion(0, 0, -0.707107f, 0.707107f);
                    break;
                case 2:
                    component.OffsetRotation = new Quaternion(0.707107f, 0, 0, 0.707107f);
                    break;
            }
            return component;
        }

        private CollisionShape VisitSphereCollider(SphereCollider sphereCollider, Vector3 scale)
        {
            var component = new CollisionShape();
            component.ShapeType = ShapeType.Sphere;
            var s = Abs(scale);
            // Size is the diameter.
            var d = sphereCollider.radius * 2.0f * Mathf.Max(s.x, Mathf.Max(s.y, s.z));
            component.Size = new Vector3(d, d, d);
            component.OffsetPosition = Vector3.Scale(sphereCollider.center, scale);
            return component;
        }

        private CollisionShape VisitTerrainCollider(TerrainCollider terrainCollider)
        {
            var component = new CollisionShape();
            component.ShapeType = ShapeType.Terrain;
            return component;
        }

        private CollisionShape VisitBoxCollider(BoxCollider boxCollider, Vector3 scale)
        {
            var component = new CollisionShape();
            component.ShapeType = ShapeType.Box;
            component.Size = Abs(Vector3.Scale(boxCollider.size, scale));
            component.OffsetPosition = Vector3.Scale(boxCollider.center, scale);
            return component;
        }
    }
}