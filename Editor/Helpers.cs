using UnityEditor;

namespace UnityToRebelFork.Editor
{
    public class Helpers
    {
        /// <summary>
        /// Create default scene. This is used to populate scene or prefab scene node.
        /// It creates necessary components: Octree, PhysicsWorld.
        /// Also it adds few nice-to-have components: Skybox, RenderPipeline.
        /// </summary>
        /// <param name="name">Desired name of the scene.</param>
        /// <returns>Populated scene.</returns>
        public static Scene CreateDefaultScene(string name)
        {
            var scene = new Scene();
            scene.Name = name;

            scene.Components.Add(new RenderPipeline());
            var octree = new Octree();
            scene.Components.Add(octree);
            //scene.Components.Add(new ReflectionProbeManager());
            scene.Components.Add(new PhysicsWorld());
            scene.Components.Add(new Zone() { ZoneTexture = ResourceRef.Create<TextureCube>("Textures/DefaultSkybox.xml"), BoundingBoxMin = octree.BoundingBoxMin, BoundingBoxMax = octree.BoundingBoxMax });
            scene.Components.Add(new Skybox()
            {
                Model = new ResourceRef("Model", "Models/Box.mdl"),
                Material = new ResourceRefList("Material", "Materials/DefaultSkybox.xml"),
            });
            return scene;
        }

        /// <summary>
        /// Get stable hash code.
        /// </summary>
        /// <typeparam name="T1">Object type.</typeparam>
        /// <param name="t1">Object.</param>
        /// <returns>Stable hash code.</returns>
        public static int GetHashCode<T1>(T1 t1)
        {
            if (t1 == null) return 0;
            if (t1 is UnityEngine.Object obj)
            {
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var guid, out long localId))
                {
                    return CombineHashCodes(guid, localId);
                }
            }
            return t1.GetHashCode();
        }

        /// <summary>
        /// Combine hash codes with stable algorithm.
        /// </summary>
        /// <typeparam name="T1">First object type.</typeparam>
        /// <typeparam name="T2">Second object type.</typeparam>
        /// <param name="t1">First object.</param>
        /// <param name="t2">Second object.</param>
        /// <returns>Combined stable hash code.</returns>
        public static int CombineHashCodes<T1, T2>(T1 t1, T2 t2)
        {
            var code = GetHashCode(t1);
            code = code * 31 + GetHashCode(t2);
            return code;
        }

        /// <summary>
        /// Combine hash codes with stable algorithm.
        /// </summary>
        /// <typeparam name="T2">Second object type.</typeparam>
        /// <param name="code">Existing hashcode.</param>
        /// <param name="t2">Second object.</param>
        /// <returns>Combined stable hash code.</returns>
        public static int CombineHashCodes<T2>(int code, T2 t2)
        {
            code = code * 31 + GetHashCode(t2);
            return code;
        }

        /// <summary>
        /// Combine hash codes with stable algorithm.
        /// </summary>
        /// <returns>Combined stable hash code.</returns>
        public static int CombineHashCodes(params object[] objects)
        {
            int code = 0;
            foreach (var o in objects)
            {
                code = CombineHashCodes(code, o);
            }
            return code;
        }
    }
}