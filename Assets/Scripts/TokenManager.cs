using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Hudossay.Match3.Assets.Scripts
{
    public class TokenManager : MonoBehaviour
    {
        public TokenDefinition TokenDefinition;
        public Task TravelTask => _taskSource?.Task ?? Task.CompletedTask;

        [SerializeField] private Image _image;
        [SerializeField] private RectTransform _rectTransform;

        private Queue<Vector2> _destinations;
        private TaskCompletionSource<bool> _taskSource;

        private const float ProximityThreshold = 0.01f;
        private const float Speed = 700f;


        private void Awake()
        {
            _destinations ??= new();
            _destinations.Clear();
        }


        private void FixedUpdate() =>
            MoveTowardsNextDestination();


        public void SetNewTokenDefinition(TokenDefinition newDefinition)
        {
            _image.sprite = newDefinition.Sprite;
            TokenDefinition = newDefinition;
        }


        public void AddTravelDestination(Vector2 newDestination)
        {
            _destinations.Enqueue(newDestination);

            if (_taskSource is null || _taskSource.Task.IsCompleted)
                _taskSource = new();
        }


        private void MoveTowardsNextDestination()
        {
            if (_destinations.Count == 0)
                return;

            var currentDestination = _destinations.Peek();

            var toNextDestinationVector = currentDestination - _rectTransform.anchoredPosition;

            if (toNextDestinationVector.sqrMagnitude <= ProximityThreshold * ProximityThreshold)
            {
                _rectTransform.anchoredPosition = currentDestination;
                _destinations.Dequeue();

                if (_destinations.Count == 0)
                    _taskSource.SetResult(true);

                return;
            }

            var movingVector = toNextDestinationVector.normalized * Speed * Time.deltaTime;
            
            if (movingVector.sqrMagnitude > toNextDestinationVector.sqrMagnitude)
                movingVector = toNextDestinationVector;

            _rectTransform.anchoredPosition += movingVector;
        }


        private void Reset()
        {
            _image = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
        }
    }
}
