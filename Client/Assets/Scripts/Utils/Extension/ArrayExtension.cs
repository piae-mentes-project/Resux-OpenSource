namespace Resux.Extension
{
    public static class ArrayExtension
    {
        public static T GetCircle<T>(this T[] array, int index)
        {
            var offset = index % array.Length;
            return array[offset >= 0 ? offset : offset + array.Length];
        }
    }
}
