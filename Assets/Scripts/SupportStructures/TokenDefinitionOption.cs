using Hudossay.Match3.Assets.Scripts.ScriptableObjects;
using System;

namespace Hudossay.Match3.Assets.Scripts.SupportStructures
{
    [Serializable]
    public struct TokenDefinitionOption
    {
        public float ProbabilityWeight;
        public TokenDefinition TokenDefinition;
    }
}
