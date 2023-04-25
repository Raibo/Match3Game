using Hudossay.AttributeEvents.Assets.Runtime;
using Hudossay.AttributeEvents.Assets.Runtime.Attributes;
using Hudossay.AttributeEvents.Assets.Runtime.GameEvents;
using Hudossay.Match3.Assets.Scripts.EventLabelEnums;
using Hudossay.Match3.Assets.Scripts.ScriptableObjects;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Hudossay.Match3.Assets.Scripts.MonoBehaviours
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(EventLinker))]
    public class Token : MonoBehaviour
    {
        public TokenDefinition TokenDefinition;
        public RectTransform RectTransform;

        public Task DestinationReach => _taskSource?.Task ?? Task.CompletedTask;

        [EventLocal(TokenEventKind.Initialized)] public GameEvent<TokenDefinition> Initialized;
        [EventLocal(TokenEventKind.DeathStarted)] public GameEvent DeathStarted;
        [EventLocal(TokenEventKind.DeathFinished)] public GameEvent DeathFinished;

        [SerializeField] private Image _image;

        private Vector2 _destination;
        private bool _isMoving;
        private TaskCompletionSource<bool> _taskSource;
        private GameConfig _gameConfig;

        private bool _isMovingDiagonally;

        private const float DiagonalSpeedFactor = 1.42f;


        private void FixedUpdate() =>
            MoveTowardsDestination();


        public void Init(TokenDefinition newDefinition, GameConfig gameConfig)
        {
            _image.sprite = newDefinition.Sprite;
            TokenDefinition = newDefinition;
            _gameConfig = gameConfig;
            Initialized.RaiseForced(newDefinition);
        }


        public void SetTravelDestination(Vector2 newDestination, bool isDiagonal)
        {
            _destination = newDestination;
            _isMovingDiagonally = isDiagonal;

            _isMoving = true;

            if (_taskSource is null || _taskSource.Task.IsCompleted)
                _taskSource = new();
        }


        public async Task Kill()
        {
            DeathStarted.Raise();
            await Task.Delay(TimeSpan.FromSeconds(TokenDefinition.DeathDelaySeconds));

            DeathFinished.RaiseForced();
        }


        private void MoveTowardsDestination()
        {
            if (!_isMoving)
                return;

            var speed = _isMovingDiagonally ? _gameConfig.TokenSpeed * DiagonalSpeedFactor : _gameConfig.TokenSpeed;

            var toNextDestinationVector = _destination - RectTransform.anchoredPosition;
            var movingVector = speed * Time.deltaTime * toNextDestinationVector.normalized;

            if (movingVector.sqrMagnitude < toNextDestinationVector.sqrMagnitude)
            {
                RectTransform.anchoredPosition += movingVector;
                return;
            }

            RectTransform.anchoredPosition = _destination;
            _isMoving = false;
            _taskSource.SetResult(true);
        }


        private void Reset()
        {
            _image = GetComponent<Image>();
            RectTransform = GetComponent<RectTransform>();
        }
    }
}
