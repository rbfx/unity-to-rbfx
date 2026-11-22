using UnityEngine;

namespace MagicaFakes
{
    /// <summary>A stand-in for the component of Magica Cloth 2 (the same type name and member names; see MagicaClothData.cs).</summary>
    public class MagicaCloth : MonoBehaviour
    {
        [SerializeField] private ClothSerializeData serializeData = new ClothSerializeData();
        [SerializeField] public ClothSerializeData2 serializeData2 = new ClothSerializeData2();

        public ClothSerializeData SerializeData => serializeData;
    }
}
