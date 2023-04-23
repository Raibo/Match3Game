using System.Collections.Generic;
using UnityEngine;

namespace Hudossay.Match3.Assets.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = nameof(TokenDefinition), menuName = "Scriptable Objects/" + nameof(TokenDefinition))]
    public class TokenDefinition : ScriptableObject
    {
        public Sprite Sprite;
        public ExplosionPattern ExplosionPattern;
        public string DeathAnimationTrigger;
        public string ResetAnimationTrigger;
        public float DeathDelaySeconds;
        public List<MatchingGroup> MatchingGroups;
    }
}
