using System.Collections.Generic;
using UnityEngine;

namespace Hudossay.Match3.Assets.Scripts
{
    public class ObjectPool : MonoBehaviour
    {
        public GameObject Prefab;
        public int MinimumCapacity;

        private Stack<GameObject> _elements;
        [SerializeField] private Transform _transform;


        private void Awake()
        {
            _elements = new Stack<GameObject>(MinimumCapacity);

            for (int i = 0; i < MinimumCapacity; i++)
                _elements.Push(CreateNewElement());
        }


        private GameObject CreateNewElement()
        {
            var newObj = Instantiate(Prefab, _transform);
            newObj.GetComponent<Poolable>().Init(this);
            newObj.SetActive(false);
            return newObj;
        }


        public GameObject Rent()
        {
            if (_elements.Count <= 0)
                _elements.Push(CreateNewElement());

            return _elements.Pop();
        }


        public void Return(GameObject element)
        {
            element.SetActive(false);
            _elements.Push(element);
        }


        private void Reset() =>
            _transform = transform;
    }
}
