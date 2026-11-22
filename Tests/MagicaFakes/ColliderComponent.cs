using UnityEngine;

namespace MagicaFakes
{
    /// <summary>A stand-in for the base of the colliders of Magica Cloth 2.</summary>
    public abstract class ColliderComponent : MonoBehaviour
    {
        public Vector3 center;
        public int symmetryMode;
        [SerializeField] protected Vector3 size = new Vector3(0.1f, 0.1f, 0.1f);

        public void SetSize(Vector3 value) => size = value;
    }
}
