using UnityEngine;

namespace Hudossay.Match3.Assets.Scripts
{
    public class Poolable : MonoBehaviour
    {
        public TokenPool Pool;
        private TokenManager _tokenManager;


        public void Init(TokenPool pool, TokenManager tokenManager)
        {
            _tokenManager = tokenManager;
            Pool = pool;
        }


        public void Return() =>
            Pool.Return(_tokenManager);
    }
}
