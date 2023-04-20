using UnityEngine;

namespace Hudossay.Match3.Assets.Scripts
{
    public class Poolable : MonoBehaviour
    {
        public ObjectPool Pool;
        private GameObject _gameObject;


        public void Init(ObjectPool pool)
        {
            _gameObject = gameObject;
            Pool = pool;
        }


        public void Return() =>
            Pool.Return(_gameObject);
    }
}
