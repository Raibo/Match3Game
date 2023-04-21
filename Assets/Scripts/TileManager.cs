using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Hudossay.Match3.Assets.Scripts
{
    public class TileManager : MonoBehaviour
    {
        public RectTransform RectTransform;
        public TokenManager Token;
        public Vector2Int Position;

        public Task TokenAvailableStreight => _streightTokenAvailabilitySource.Task;
        public Task TokenAvailableDiagonal => _diagonalTokenAvailabilitySource.Task;

        public bool IsGenerator;
        public bool IsBlocked;
        public bool CanAcceptDiagonal;

        public bool HasToken => Token != null;

        [Space(15)]
        public Sprite BackgroundEven;
        public Sprite BackgroundOdd;

        [SerializeField] private Image _image;

        private TaskCompletionSource<bool> _streightTokenAvailabilitySource;
        private TaskCompletionSource<bool> _diagonalTokenAvailabilitySource;

        private TileManager _upperLeftTile;
        private TileManager _upperMiddleTile;
        private TileManager _upperRightTile;

        private TokenPool _tokenPool;
        private ObjectCounter _movingObjectsCounter;
        private List<TileManager> _diagonalWaitingList;
        private GameConfig _gameConfig;


        public void Init(Vector2Int position, TileManager upperLeftTile, TileManager upperMiddleTile, TileManager upperRightTile,
            TokenPool tokenPool, ObjectCounter movingObjectsCounter, GameConfig gameConfig)
        {
            Position = position;
            _upperMiddleTile = upperMiddleTile;
            _upperLeftTile = upperLeftTile;
            _upperRightTile = upperRightTile;
            _tokenPool = tokenPool;
            _movingObjectsCounter = movingObjectsCounter;
            _diagonalWaitingList = new(2);
            _gameConfig = gameConfig;

            _image.sprite = (Position.x + Position.y) % 2 == 0
                ? BackgroundEven
                : BackgroundOdd;

            CanAcceptDiagonal = CheckShouldAcceptDiagonal();

            if (CanAcceptDiagonal)
                _image.color = Color.red;

            _streightTokenAvailabilitySource = new TaskCompletionSource<bool>();
            _diagonalTokenAvailabilitySource = new TaskCompletionSource<bool>();


            bool CheckShouldAcceptDiagonal()
            {
                if (IsBlocked || IsGenerator)
                    return false;

                var upperMiddleTileBlocked = _upperMiddleTile?.IsBlocked ?? false;

                var upperLeftFreeSpace = !_upperLeftTile?.IsBlocked ?? false;
                var upperRightFreeSpace = !_upperRightTile?.IsBlocked ?? false;

                var upperLeftTileDiagonal = _upperLeftTile?.CanAcceptDiagonal ?? false;
                var upperMiddleTileDiagonal = _upperMiddleTile?.CanAcceptDiagonal ?? false;
                var upperRightTileDiagonal = _upperRightTile?.CanAcceptDiagonal ?? false;

                var underBlockWithSpace = upperMiddleTileBlocked && (upperLeftFreeSpace || upperRightFreeSpace);
                var underBothDiagonal = upperLeftTileDiagonal && upperMiddleTileDiagonal && upperRightTileDiagonal;

                return underBlockWithSpace || underBothDiagonal;
            }
        }


        public void PullTokenFromAbove()
        {
            if (IsBlocked || HasToken)
                return;

            switch (IsGenerator, CanAcceptDiagonal)
            {
                case (true, _):
                    GenerateToken();
                    break;
                case (false, true):
                    PullFromDiagonal();
                    break;
                case (false, false):
                    PullFromMiddle();
                    break;
            }


            void GenerateToken()
            {
                var newToken = _tokenPool.Rent();
                var tokenDefinition = _gameConfig.TokenDefinitionOptions.PickRandom(o => o.ProbabilityWeight).TokenDefinition;

                newToken.RectTransform.anchoredPosition = RectTransform.anchoredPosition + _gameConfig.GeneratedTokensDisplacement;

                newToken.Init(tokenDefinition, _movingObjectsCounter, _gameConfig);
                newToken.SetTravelDestination(RectTransform.anchoredPosition, false);
                newToken.gameObject.SetActive(true);

                Token = newToken;
                ScheduleTokenAvailability();
            }


            async void PullFromMiddle()
            {
                if (_upperMiddleTile.IsBlocked)
                    return;

                await _upperMiddleTile.TokenAvailableStreight;
                TakeTokenFrom(_upperMiddleTile, isDiagonal: false);
            }


            async void PullFromDiagonal()
            {
                _upperLeftTile?.EnterDiagonalWaitingList(this);
                _upperRightTile?.EnterDiagonalWaitingList(this);

                TileManager tileToPull = null;

                while (true)
                {
                    await WhenAnyUpperTileHasToken();

                    var leftTokenAvailable = _upperLeftTile && _upperLeftTile.TokenAvailableDiagonal.IsCompleted &&
                        _upperLeftTile.IsFirstInDiagonalWaitingList(this);

                    var rightTokenAvailable = _upperRightTile && _upperRightTile.TokenAvailableDiagonal.IsCompleted &&
                        _upperRightTile.IsFirstInDiagonalWaitingList(this);

                    tileToPull = (leftTokenAvailable, rightTokenAvailable) switch
                    {
                        (true, true) => UnityEngine.Random.Range(0, 2) > 0 ? _upperLeftTile : _upperRightTile,
                        (true, false) => _upperLeftTile,
                        (false, true) => _upperRightTile,
                        _ => null,
                    };

                    if (tileToPull != null)
                        break;

                    await Task.Yield();
                }

                _upperLeftTile?.LeaveDiagonalWaitingList(this);
                _upperRightTile?.LeaveDiagonalWaitingList(this);

                TakeTokenFrom(tileToPull, isDiagonal: true);
            }


            Task WhenAnyUpperTileHasToken() =>
                (_upperLeftTile, _upperRightTile) switch
                {
                    ({ IsBlocked: false }, { IsBlocked: false }) => Task.WhenAny(_upperLeftTile.TokenAvailableDiagonal, _upperRightTile.TokenAvailableDiagonal),
                    ({ IsBlocked: false }, null or { IsBlocked: true }) => _upperLeftTile.TokenAvailableDiagonal,
                    (null or { IsBlocked: true }, { IsBlocked: false }) => _upperRightTile.TokenAvailableDiagonal,
                    _ => throw new Exception($"No upper diagonal tiles in tile {Position}"),
                };


            void TakeTokenFrom(TileManager tileToPullFrom, bool isDiagonal)
            {
                Token = tileToPullFrom.Token;

                tileToPullFrom.Token = null;
                tileToPullFrom.ResetTokenAvailability();
                tileToPullFrom.PullTokenFromAbove();

                Token.SetTravelDestination(RectTransform.anchoredPosition, isDiagonal);
                ScheduleTokenAvailability();
            }


            async void ScheduleTokenAvailability()
            {
                await Token.DestinationReach;

                _streightTokenAvailabilitySource.SetResult(true);

                if (HasToken && TokenAvailableStreight.IsCompleted)
                    _diagonalTokenAvailabilitySource.SetResult(true);
            }
        }


        public void ResetTokenAvailability()
        {
            if (_streightTokenAvailabilitySource.Task.IsCompleted)
                _streightTokenAvailabilitySource = new();

            if (_diagonalTokenAvailabilitySource.Task.IsCompleted)
                _diagonalTokenAvailabilitySource = new();
        }


        public void EnterDiagonalWaitingList(TileManager waiter)
        {
            _diagonalWaitingList.Add(waiter);

            if (_diagonalWaitingList.Count == 2 && UnityEngine.Random.Range(0, 2) > 0)
                (_diagonalWaitingList[0], _diagonalWaitingList[1]) = (_diagonalWaitingList[1], _diagonalWaitingList[0]);
        }


        public void LeaveDiagonalWaitingList(TileManager waiter) =>
            _diagonalWaitingList.Remove(waiter);

        public bool IsFirstInDiagonalWaitingList(TileManager waiter) =>
            _diagonalWaitingList.Count > 0 && _diagonalWaitingList[0] == waiter;


        private void Reset()
        {
            _image = GetComponent<Image>();
            RectTransform = GetComponent<RectTransform>();
        }
    }
}
