using Hudossay.AttributeEvents.Assets.Runtime;
using Hudossay.AttributeEvents.Assets.Runtime.Attributes;
using Hudossay.AttributeEvents.Assets.Runtime.GameEvents;
using Hudossay.Match3.Assets.Scripts.EventLabelEnums;
using UnityEngine;

namespace Hudossay.Match3.Assets.Scripts.MonoBehaviours
{
    [RequireComponent(typeof(EventLinker))]
    public class KeyboardInputManager : MonoBehaviour
    {
        [EventGlobal(InputEventKind.StartGameRequested)] public GameEvent StartGameRequested;


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                StartGameRequested.Raise();
        }
    }
}
