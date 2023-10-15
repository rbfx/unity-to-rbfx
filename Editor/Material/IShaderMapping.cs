namespace UnityToRebelFork.Editor
{
    public interface IShaderMapping
    {
        int Priority { get; }

        bool CanMap(UnityEngine.Shader shader);

        MaterialModel Map(UnityEngine.Material shader);
    }
}