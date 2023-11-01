namespace UnityToRebelFork.Editor
{
    public struct Sample<T>
    {
        public Sample(float t, T v)
        {
            Time = t; Value = v;
        }
        public float Time;
        public T Value;

        public override string ToString()
        {
            return $"{Time}: {Value}";
        }
    }
}