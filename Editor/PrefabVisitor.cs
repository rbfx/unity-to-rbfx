using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

namespace UnityToRebelFork.Editor
{
    public class PrefabVisitor
    {
        [Inject]
        private readonly ExportOrchestrator _orchestrator;

        [Inject]
        private readonly PrefabExporter prefabExporter;

        [Inject]
        private readonly MeshExporter meshExporter;

        [Inject]
        private readonly MaterialExporter materialExporter;

        [Inject]
        private readonly TextureExporter textureExporter;

        /// <summary>
        /// Unique Id counter.
        /// </summary>
        private int uniqueIdCounter = 0;

        private static HashSet<string> prefabPropertiesToIgnore = new HashSet<string>()
        {
            "m_Name", "m_RootOrder", "m_LocalPosition.x", "m_LocalPosition.y", "m_LocalPosition.z", "m_LocalRotation.w",
            "m_LocalRotation.x", "m_LocalRotation.y", "m_LocalRotation.z", "m_LocalEulerAnglesHint.x",
            "m_LocalEulerAnglesHint.y", "m_LocalEulerAnglesHint.z",
            "m_LocalScale.x","m_LocalScale.y","m_LocalScale.z", "TAAQuality", "antialiasing", "m_StaticEditorFlags", "m_RenderingLayerMask"
        };

        private HashSet<UnityEngine.Component> skipList = new HashSet<UnityEngine.Component>();

        public PrefabVisitor()
        {
        }

        public Scene Visit(UnityEngine.SceneManagement.Scene unityScene)
        {
            uniqueIdCounter = 0;

            var scene = Helpers.CreateDefaultScene(unityScene.name);
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


            if (!isRoot && PrefabUtility.IsAnyPrefabInstanceRoot(gameObject))
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
                                    Prefab = new ResourceRef(nameof(PrefabResource), _orchestrator.ScheduleExport(originalSource))
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

            for (int childIndex = 0; childIndex < gameObject.transform.childCount; ++childIndex)
            {
                node.Children.Add(Visit(gameObject.transform.GetChild(childIndex).gameObject));
            }

            return node;
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
            else if (component is UnityEngine.Collider collider)
            {
                VisitCollisionShape(node, collider);
            }
            else if (component is UnityEngine.Joint joint)
            {
                VisitJoint(node, joint);
            }
        }

        private void VisitJoint(Node node, Joint joint)
        {

        }

        private void VisitSkinnedMeshRenderer(Node node, UnityEngine.SkinnedMeshRenderer skinnedMeshRenderer)
        {
            var component = new AnimatedModel();
            component.Model = ResourceRef.Create<Model>(_orchestrator.ScheduleExport(skinnedMeshRenderer.sharedMesh));
            component.Material = ResourceRefList.Create<Material>(skinnedMeshRenderer.sharedMaterials.Select(_ => _orchestrator.ScheduleExport(_)));
            component.CastShadows = skinnedMeshRenderer.shadowCastingMode != ShadowCastingMode.Off;
            node.Components.Add(component);
        }

        private void VisitMeshRenderer(Node node, UnityEngine.MeshRenderer meshRenderer)
        {
            var component = new StaticModel();
            var meshFilter = meshRenderer.gameObject.GetComponent<MeshFilter>();
            component.Model = ResourceRef.Create<Model>(_orchestrator.ScheduleExport(meshFilter?.sharedMesh));
            component.Material = ResourceRefList.Create<Material>(meshRenderer.sharedMaterials.Select(_=>_orchestrator.ScheduleExport(_)));
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
                component.LightShapeTexture = ResourceRef.Create<Texture2D>(_orchestrator.ScheduleExport(light.cookie));
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
            var meshRef = new MeshReference(lodGroup, _orchestrator);
            component.Model = ResourceRef.Create<Model>(_orchestrator.ScheduleExport(meshRef));
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


        private void VisitCollisionShape(Node node, Collider collider)
        {
            CollisionShape component = null;
            if (collider is BoxCollider boxCollider)
            {
                component = VisitBoxCollider(boxCollider);
            }
            else if (collider is TerrainCollider terrainCollider)
            {
                component = VisitTerrainCollider(terrainCollider);
            }
            else if (collider is SphereCollider sphereCollider)
            {
                component = VisitSphereCollider(sphereCollider);
            }
            else if (collider is CapsuleCollider capsuleCollider)
            {
                component = VisitCapsuleCollider(capsuleCollider);
            }
            else if (collider is MeshCollider meshCollider)
            {
                component = VisitMeshCollider(meshCollider);
            }

            if (component != null)
            {
                component.CollisionMargin = collider.contactOffset;
                node.Components.Add(component);
            }
        }

        private CollisionShape VisitMeshCollider(MeshCollider meshCollider)
        {
            var component = new CollisionShape();
            component.ShapeType = ShapeType.TriangleMesh;
            component.Model = ResourceRef.Create<Model>(_orchestrator.ScheduleExport(meshCollider.sharedMesh));
            return component;
        }

        private CollisionShape VisitCapsuleCollider(CapsuleCollider capsuleCollider)
        {
            var component = new CollisionShape();
            if (capsuleCollider.name == "Cylinder")
                component.ShapeType = ShapeType.Cylinder;
            else
                component.ShapeType = ShapeType.Capsule;
            var d = capsuleCollider.radius * 2.0f;
            component.Size = new Vector3(d, capsuleCollider.height, d);
            component.OffsetPosition = capsuleCollider.center;
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

        private CollisionShape VisitSphereCollider(SphereCollider sphereCollider)
        {
            var component = new CollisionShape();
            component.ShapeType = ShapeType.Sphere;
            component.Size = new Vector3(sphereCollider.radius, sphereCollider.radius, sphereCollider.radius);
            component.OffsetPosition = sphereCollider.center;
            return component;
        }

        private CollisionShape VisitTerrainCollider(TerrainCollider terrainCollider)
        {
            var component = new CollisionShape();
            component.ShapeType = ShapeType.Terrain;
            return component;
        }

        private CollisionShape VisitBoxCollider(BoxCollider boxCollider)
        {
            var component = new CollisionShape();
            component.ShapeType = ShapeType.Box;
            component.Size = boxCollider.size;
            component.OffsetPosition = boxCollider.center;
            return component;
        }
    }
}