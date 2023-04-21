using UnityEngine;

namespace Hudossay.Match3.Assets.Scripts
{
    public class ObjectCounter
    {
        public int Count { get; private set; }


        public void IncreaseCount() =>
            Count++;

        public void DecreaseCount() =>
            Count--;
    }
}
