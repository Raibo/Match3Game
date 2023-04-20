using System;
using System.Buffers;
using System.Collections.Generic;

namespace Hudossay.Match3.Assets.Scripts
{
    public static class CollectionsExtensions
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


        public static T PickRandom<T>(this List<T> collection)
        {
            if (collection is null || collection.Count == 0)
                throw new ArgumentException("Collection is null or empty");

            var index = UnityEngine.Random.Range(0, collection.Count);
            return collection[index];
        }


        public static T PickRandom<T>(this List<T> collection, Func<T, float> weightFunc)
        {
            if (collection is null || collection.Count == 0)
                throw new ArgumentException("Collection is null or empty");

            var weights = ArrayPool<float>.Shared.Rent(collection.Count);
            var lastWeight = 0f;

            try
            {
                for (int i = 0; i < collection.Count; i++)
                {
                    var weight = Math.Max(0f, weightFunc(collection[i])) + lastWeight;
                    lastWeight = weight;
                    weights[i] = weight;
                }

                var maxWeight = weights[collection.Count - 1];

                if (maxWeight <= 0)
                    return collection.PickRandom();

                var randomNumber = UnityEngine.Random.Range(0f, maxWeight);

                for (int i = 0; i < collection.Count; i++)
                    if (weights[i] >= randomNumber)
                        return collection[i];

                throw new Exception("Error during picking a weighted random element");
            }
            finally
            {
                ArrayPool<float>.Shared.Return(weights);
            }
        }
    }
}
