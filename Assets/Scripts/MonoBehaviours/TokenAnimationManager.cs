using Hudossay.AttributeEvents.Assets.Runtime;
using Hudossay.AttributeEvents.Assets.Runtime.Attributes;
using Hudossay.Match3.Assets.Scripts.EventLabelEnums;
using Hudossay.Match3.Assets.Scripts.ScriptableObjects;
using UnityEngine;

namespace Hudossay.Match3.Assets.Scripts.MonoBehaviours
{
    [RequireComponent(typeof(EventLinker))]
    public class TokenAnimationManager : MonoBehaviour
    {
        public Animator Animator;

        private const string ResetAnimationParameterName = "Reset";
        private const string DeathAnimationParameterName = "Death";


        [ResponseLocal(TokenEventKind.Initialized)]
        public void SetNewAnimatorController(TokenDefinition newTokenDefiniton) =>
            Animator.runtimeAnimatorController = newTokenDefiniton.AnimatorController;


        [ResponseLocal(TokenEventKind.DeathStarted)]
        public void PlayDeathAnimation() =>
            Animator.SetTrigger(DeathAnimationParameterName);


        [ResponseLocal(TokenEventKind.DeathFinished)]
        public void ResetAnimation() =>
            Animator.SetTrigger(ResetAnimationParameterName);


        private void Reset() =>
            Animator = GetComponent<Animator>();
    }
}
