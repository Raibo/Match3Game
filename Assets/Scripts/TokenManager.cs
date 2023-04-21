using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Hudossay.Match3.Assets.Scripts
{
    public class TokenManager : MonoBehaviour
    {
        public TokenDefinition TokenDefinition;
        public RectTransform RectTransform;
        public Task DestinationReach => _taskSource?.Task ?? Task.CompletedTask;

        [SerializeField] private Image _image;

        private Vector2 _destination;
        private bool _isMoving;
        private ObjectCounter _movingObjectsCounter;
        private TaskCompletionSource<bool> _taskSource;
        private bool _isMovingDiagonally;

        private const float ProximityThreshold = 15f;
        private const float Speed = 700f;
        private const float DiagonalSpeedFactor = 1.42f;


        private void FixedUpdate() =>
            MoveTowardsDestination();


        public void Init(TokenDefinition newDefinition, ObjectCounter movingObjectsCounter)
        {
            _image.sprite = newDefinition.Sprite;
            TokenDefinition = newDefinition;
            _movingObjectsCounter = movingObjectsCounter;
        }


        public void SetTravelDestination(Vector2 newDestination, bool isDiagonal)
        {
            _destination = newDestination;
            _isMovingDiagonally = isDiagonal;

            if (!_isMoving)
                _movingObjectsCounter.IncreaseCount();

            _isMoving = true;

            if (_taskSource is null || _taskSource.Task.IsCompleted)
                _taskSource = new();
        }


        private void MoveTowardsDestination()
        {
            if (!_isMoving)
                return;

            var speed = _isMovingDiagonally ? Speed * DiagonalSpeedFactor : Speed;
            var threshold = _isMovingDiagonally ? ProximityThreshold * DiagonalSpeedFactor : ProximityThreshold;

            var toNextDestinationVector = _destination - RectTransform.anchoredPosition;

            if (toNextDestinationVector.sqrMagnitude <= threshold * threshold)
            {
                RectTransform.anchoredPosition = _destination;
                _isMoving = false;
                _movingObjectsCounter.DecreaseCount();
                _taskSource.SetResult(true);

                return;
            }

            var movingVector = toNextDestinationVector.normalized * speed * Time.deltaTime;

            if (movingVector.sqrMagnitude > toNextDestinationVector.sqrMagnitude)
                movingVector = toNextDestinationVector;

            RectTransform.anchoredPosition += movingVector;
        }


        private void Reset()
        {
            _image = GetComponent<Image>();
            RectTransform = GetComponent<RectTransform>();
        }
    }
}
