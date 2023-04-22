using System.Collections.Generic;
using UnityEngine;

namespace Hudossay.Match3.Assets.Scripts
{
    public class TokenPool : MonoBehaviour
    {
        public GameObject Prefab;
        public int MinimumCapacity;

        private Stack<Token> _elements;
        [SerializeField] private Transform _transform;


        private void Awake()
        {
            _elements = new Stack<Token>(MinimumCapacity);

            for (int i = 0; i < MinimumCapacity; i++)
                _elements.Push(CreateNewElement());
        }


        private Token CreateNewElement()
        {
            var newObj = Instantiate(Prefab, _transform);
            var tokenManager = newObj.GetComponent<Token>();

            newObj.GetComponent<Poolable>().Init(this, tokenManager);
            newObj.SetActive(false);

            return tokenManager;
        }


        public Token Rent()
        {
            if (_elements.Count <= 0)
                _elements.Push(CreateNewElement());

            return _elements.Pop();
        }


        public void Return(Token element)
        {
            element.gameObject.SetActive(false);
            _elements.Push(element);
        }


        private void Reset() =>
            _transform = transform;
    }
}
