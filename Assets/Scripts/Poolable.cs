using UnityEngine;

namespace Hudossay.Match3.Assets.Scripts
{
    public class Poolable : MonoBehaviour
    {
        public TokenPool Pool;
        private Token _tokenManager;


        public void Init(TokenPool pool, Token tokenManager)
        {
            _tokenManager = tokenManager;
            Pool = pool;
        }


        public void Return() =>
            Pool.Return(_tokenManager);
    }
}
