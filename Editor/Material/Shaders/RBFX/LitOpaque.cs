namespace UnityToRebelFork.Editor.Shaders.RBFX
{
    public class LitOpaqueShaderAdapter
    {
        public static readonly string ShaderName = "RBFX/LitOpaque";

        /// <summary>The Universal Render Pipeline version of the shader: the same properties.</summary>
        public static readonly string UrpShaderName = "RBFX/URP/LitOpaque";

        /// <summary>The alpha blended variants (the same properties, no cut-off): glass, blood, flames.</summary>
        public static readonly string TransparentShaderName = "RBFX/LitTransparent";
        public static readonly string UrpTransparentShaderName = "RBFX/URP/LitTransparent";

        public static bool IsLitShader(string name)
        {
            return name == ShaderName || name == UrpShaderName || name == TransparentShaderName || name == UrpTransparentShaderName;
        }

        UnityEngine.Material material;

        public LitOpaqueShaderAdapter(UnityEngine.Material material)
        {
            this.material = material;
        }

        public UnityEngine.Texture _MainTex
        {
            get { return material.GetTexture("_MainTex"); }
        }

        public UnityEngine.Texture _BumpMap
        {
            get { return material.GetTexture("_BumpMap"); }
        }

        public UnityEngine.Texture _PBRMap
        {
            get { return material.GetTexture("_PBRMap"); }
        }

        public UnityEngine.Texture _EmissionMap
        {
            get { return material.GetTexture("_EmissionMap"); }
        }

        public UnityEngine.Color _EmissionColor
        {
            get { return material.GetColor("_EmissionColor"); }
        }

        public UnityEngine.Color _Color
        {
            get { return material.GetColor("_Color"); }
        }

        public float _BumpScale
        {
            get { return material.GetFloat("_BumpScale"); }
        }

        public float _Metallic
        {
            get { return material.GetFloat("_Metallic"); }
        }

        public float _Roughness
        {
            get { return material.GetFloat("_Roughness"); }
        }

        /// <summary>The checkbox of the material: the colour of the vertices is not multiplied into the albedo (IGNOREVERTEXCOLOR in rbfx). Off for a material without the property.</summary>
        public bool _IgnoreVertexColor
        {
            get { return material.HasProperty("_IgnoreVertexColor") && material.GetFloat("_IgnoreVertexColor") > 0.5f; }
        }

        public float _Cutoff
        {
            get { return material.HasProperty("_Cutoff") ? material.GetFloat("_Cutoff") : 0.5f; }
        }
    }
}