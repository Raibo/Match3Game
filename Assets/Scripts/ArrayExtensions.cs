namespace Hudossay.Match3.Assets.Scripts
{
    public static class ArrayExtensions
    {
        public static bool TryGetValue<T>(this T[,] array, int x, int y, out T value)
        {
            if (x < 0 || y < 0 || x > array.GetLength(0) - 1 || y > array.GetLength(1) - 1)
            {
                value = default;
                return false;
            }

            value = array[x, y];
            return true;
        }
    }
}
