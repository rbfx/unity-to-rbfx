#nullable disable
using System;

namespace UnityToRebelFork.Editor.Particles
{
    /// <summary>
    /// How Unity's soft particle settings of a material become Toe's: a <c>SOFTPARTICLES</c> define and the <c>FadeOffsetScale</c> parameter (the distance from the surface behind where
    /// the fade starts, and the length of the fade, both in world units; Toe's shaders fade the alpha by <c>saturate((gap - offset) / length)</c>). Unity-free so that it can be tested.
    /// </summary>
    public static class SoftParticleRules
    {
        /// <summary>Shortest fade length: a fade of no length is a hard cut, which is what the particle already is without the fade.</summary>
        public const float MinLength = 0.01f;

        /// <summary>
        /// URP's particle shaders: <c>_SoftParticlesEnabled</c> switches the fade on, and it runs from the near distance (alpha 0) to the far distance (alpha 1) from the surface behind.
        /// </summary>
        public static bool TryFromUrp(bool enabled, float nearFade, float farFade, out float offset, out float length)
        {
            offset = 0f;
            length = 0f;
            if (!enabled) return false;
            offset = Math.Max(nearFade, 0f);
            length = Math.Max(farFade - offset, MinLength);
            return true;
        }

        /// <summary>
        /// The built-in particle shaders: <c>_InvFade</c> ("Soft Particles Factor") scales the gap, <c>saturate(_InvFade * gap)</c>, so the fade is 1 / _InvFade long and starts at the
        /// surface. A factor that is zero or less has no fade.
        /// </summary>
        public static bool TryFromInvFade(float invFade, out float offset, out float length)
        {
            offset = 0f;
            length = 0f;
            if (!(invFade > 0f)) return false;
            length = Math.Max(1f / invFade, MinLength);
            return true;
        }
    }
}
