namespace Nucleus.Gaming.Coop.BasicTypes
{
    internal class Ref<T>
    {
        public T Value { get; set; }

        public Ref()
        {
        }

        public Ref(T value)
        {
            Value = value;
        }
    }
}
