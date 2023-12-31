// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class RaycastVehicle: UnityToRebelFork.Editor.LogicComponent
    {
        protected static readonly RbfxAttribute<VariantList> WheeldataAttr = new ("Wheel data", VariantType.VarVariantList, new Variant(new VariantList() {  }), _=>((RaycastVehicle)_).Wheeldata, (_,v)=>((RaycastVehicle)_).Wheeldata = v);
        protected static readonly RbfxAttribute<float> MaximumsideslipthresholdAttr = new ("Maximum side slip threshold", VariantType.VarFloat, new Variant(4f), _=>((RaycastVehicle)_).Maximumsideslipthreshold, (_,v)=>((RaycastVehicle)_).Maximumsideslipthreshold = v);
        protected static readonly RbfxAttribute<float> RPMforwheelmotorsinairAttr = new ("RPM for wheel motors in air (0=calculate)", VariantType.VarFloat, new Variant(0f), _=>((RaycastVehicle)_).RPMforwheelmotorsinair, (_,v)=>((RaycastVehicle)_).RPMforwheelmotorsinair = v);
        protected static readonly RbfxAttribute<IntVector3> CoordinatesystemAttr = new ("Coordinate system", VariantType.VarIntVector3, new Variant(new IntVector3(0, 1, 2)), _=>((RaycastVehicle)_).Coordinatesystem, (_,v)=>((RaycastVehicle)_).Coordinatesystem = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return WheeldataAttr;
            yield return MaximumsideslipthresholdAttr;
            yield return RPMforwheelmotorsinairAttr;
            yield return CoordinatesystemAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected VariantList _wheeldata = new VariantList() {  };

        protected float _maximumsideslipthreshold = 4f;

        protected float _rPMforwheelmotorsinair = 0f;

        protected IntVector3 _coordinatesystem = new IntVector3(0, 1, 2);

        public VariantList Wheeldata
        {
            get => _wheeldata;
            set => _wheeldata = value;
        }

        public float Maximumsideslipthreshold
        {
            get => _maximumsideslipthreshold;
            set => _maximumsideslipthreshold = value;
        }

        public float RPMforwheelmotorsinair
        {
            get => _rPMforwheelmotorsinair;
            set => _rPMforwheelmotorsinair = value;
        }

        public IntVector3 Coordinatesystem
        {
            get => _coordinatesystem;
            set => _coordinatesystem = value;
        }
    }
}
