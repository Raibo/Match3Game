using Hudossay.AttributeEvents.Assets.Runtime;
using Hudossay.AttributeEvents.Assets.Runtime.Attributes;
using Hudossay.Match3.Assets.Scripts.EventLabelEnums;
using UnityEngine;

namespace Hudossay.Match3.Assets.Scripts.MonoBehaviours
{
    [RequireComponent(typeof(Token))]
    [RequireComponent(typeof(EventLinker))]
    public class Poolable : MonoBehaviour
    {
        public TokenPool Pool;
        private Token _tokenManager;


        public void Init(TokenPool pool, Token tokenManager)
        {
            _tokenManager = tokenManager;
            Pool = pool;
        }


        [ResponseLocal(TokenEventKind.DisposeRequested)]
        public void Return() =>
            Pool.Return(_tokenManager);
    }
}
