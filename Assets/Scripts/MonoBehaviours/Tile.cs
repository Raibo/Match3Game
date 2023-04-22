using Hudossay.AttributeEvents.Assets.Runtime;
using Hudossay.AttributeEvents.Assets.Runtime.Attributes;
using Hudossay.AttributeEvents.Assets.Runtime.GameEvents;
using Hudossay.Match3.Assets.Scripts.Extensions;
using Hudossay.Match3.Assets.Scripts.ScriptableObjects;
using Hudossay.Match3.Assets.Scripts.SupportStructures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hudossay.Match3.Assets.Scripts.MonoBehaviours
{
    [RequireComponent(typeof(EventLinker))]
    public class Tile : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler,
        IPointerExitHandler
    {
        public RectTransform RectTransform;
        public Token Token;
        public Vector2Int Position;

        public Task TokenAvailableStreight => _streightTokenAvailabilitySource.Task;
        public Task TokenAvailableDiagonal => _diagonalTokenAvailabilitySource.Task;

        [Space(15)]
        public bool IsGenerator;
        public bool IsBlocked;
        public bool CanAcceptDiagonal;

        [Space(15)]
        public bool IsSettled;

        public bool HasToken => Token != null;

        [Space(15)]
        public Sprite BackgroundEven;
        public Sprite BackgroundOdd;

        [EventLocal(TileEventKind.Settled)] public GameEvent<Tile> Settled;
        [EventLocal(TileEventKind.DragBegin)] public GameEvent<Tile> DragBegin;
        [EventLocal(TileEventKind.DragEnd)] public GameEvent<Tile> DragEnd;
        [EventLocal(TileEventKind.Selected)] public GameEvent<Tile> Selected;
        [EventLocal(TileEventKind.Unselected)] public GameEvent Unselected;
        [EventLocal(TileEventKind.ClickedLeft)] public GameEvent<Tile> ClickedLeft;
        [EventLocal(TileEventKind.ClickedRight)] public GameEvent<Tile> ClickedRight;
        [EventLocal(TileEventKind.DragFrame)] public GameEvent<PointerEventData> DragFrame;

        [SerializeField] private Image _image;

        private TaskCompletionSource<bool> _streightTokenAvailabilitySource;
        private TaskCompletionSource<bool> _diagonalTokenAvailabilitySource;

        private Tile _upperLeftTile;
        private Tile _upperMiddleTile;
        private Tile _upperRightTile;

        private TokenPool _tokenPool;
        private List<Tile> _diagonalWaitingList;
        private GameConfig _gameConfig;

        private bool _isCurrentlyPulling;
        private TimeSpan _generationDelay;
        private float _lastTimeGenerated;


        public void Init(Vector2Int position, Tile upperLeftTile, Tile upperMiddleTile, Tile upperRightTile,
            TokenPool tokenPool, GameConfig gameConfig)
        {
            Position = position;
            _upperMiddleTile = upperMiddleTile;
            _upperLeftTile = upperLeftTile;
            _upperRightTile = upperRightTile;
            _tokenPool = tokenPool;
            _diagonalWaitingList = new(2);
            _gameConfig = gameConfig;

            _image.sprite = (Position.x + Position.y) % 2 == 0
                ? BackgroundEven
                : BackgroundOdd;

            CanAcceptDiagonal = CheckShouldAcceptDiagonal();

            _streightTokenAvailabilitySource = new();
            _diagonalTokenAvailabilitySource = new();


            bool CheckShouldAcceptDiagonal()
            {
                if (IsBlocked || IsGenerator)
                    return false;

                var upperMiddleTileBlocked = _upperMiddleTile?.IsBlocked ?? false;

                var upperLeftFreeSpace = !_upperLeftTile?.IsBlocked ?? false;
                var upperRightFreeSpace = !_upperRightTile?.IsBlocked ?? false;

                var underBlockWithSpace = upperMiddleTileBlocked && (upperLeftFreeSpace || upperRightFreeSpace);

                return underBlockWithSpace;
            }
        }


        public void PullTokenFromAbove()
        {
            if (IsBlocked || HasToken || _isCurrentlyPulling)
                return;

            _isCurrentlyPulling = true;

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

            _isCurrentlyPulling = false;


            async void GenerateToken()
            {
                ResetGenerationDelayIfTimeout();
                await Task.Delay(_generationDelay);

                var newToken = _tokenPool.Rent();
                var tokenDefinition = _gameConfig.TokenDefinitionOptions.PickRandom(o => o.ProbabilityWeight).TokenDefinition;

                newToken.RectTransform.anchoredPosition = RectTransform.anchoredPosition + _gameConfig.GeneratedTokensDisplacement;

                newToken.Init(tokenDefinition, _gameConfig);
                newToken.SetTravelDestination(RectTransform.anchoredPosition, false);
                newToken.gameObject.SetActive(true);

                Token = newToken;
                ScheduleTokenAvailability();
                IncreaseGenerationDelay();


                void ResetGenerationDelayIfTimeout()
                {
                    if (Time.time - _lastTimeGenerated > _gameConfig.GenerationDelayTimeout)
                        _generationDelay = TimeSpan.Zero;
                }


                void IncreaseGenerationDelay()
                {
                    _lastTimeGenerated = Time.time;
                    _generationDelay += TimeSpan.FromSeconds(_gameConfig.GenerationDelayDelta);

                    _generationDelay = _generationDelay < TimeSpan.FromSeconds(_gameConfig.GenerationDelayMax)
                        ? _generationDelay
                        : TimeSpan.FromSeconds(_gameConfig.GenerationDelayMax);
                }
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

                Tile tileToPull = null;

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


            void TakeTokenFrom(Tile tileToPullFrom, bool isDiagonal)
            {
                Token = tileToPullFrom.Token;

                tileToPullFrom.Token = null;
                tileToPullFrom.ResetTokenAvailability();
                tileToPullFrom.PullTokenFromAbove();

                Token.SetTravelDestination(RectTransform.anchoredPosition, isDiagonal);
                ScheduleTokenAvailability();
            }
        }


        public void SwapTokensWith(Tile tileToSwap)
        {
            if (!IsSettled || !tileToSwap.IsSettled)
                return;

            (Token, tileToSwap.Token) = (tileToSwap.Token, Token);

            ResetTokenAvailability();
            tileToSwap.ResetTokenAvailability();

            Token.SetTravelDestination(RectTransform.anchoredPosition, isDiagonal: false);
            tileToSwap.Token.SetTravelDestination(tileToSwap.RectTransform.anchoredPosition, isDiagonal: false);

            ScheduleTokenAvailability();
            tileToSwap.ScheduleTokenAvailability();
        }


        public void ResetTokenAvailability()
        {
            if (_streightTokenAvailabilitySource.Task.IsCompleted)
                _streightTokenAvailabilitySource = new();

            if (_diagonalTokenAvailabilitySource.Task.IsCompleted)
                _diagonalTokenAvailabilitySource = new();

            IsSettled = false;
        }


        public async void ScheduleTokenAvailability()
        {
            await Token.DestinationReach;

            _streightTokenAvailabilitySource.SetResult(true);

            if (HasToken && TokenAvailableStreight.IsCompleted)
                _diagonalTokenAvailabilitySource.SetResult(true);

            if (HasToken && TokenAvailableStreight.IsCompleted)
            {
                IsSettled = true;
                Settled.Raise(this);
            }
        }


        public void EnterDiagonalWaitingList(Tile waiter)
        {
            _diagonalWaitingList.Add(waiter);

            if (_diagonalWaitingList.Count == 2 && UnityEngine.Random.Range(0, 2) > 0)
                (_diagonalWaitingList[0], _diagonalWaitingList[1]) = (_diagonalWaitingList[1], _diagonalWaitingList[0]);
        }


        public void LeaveDiagonalWaitingList(Tile waiter) =>
            _diagonalWaitingList.Remove(waiter);


        public bool IsFirstInDiagonalWaitingList(Tile waiter) =>
            _diagonalWaitingList.Count > 0 && _diagonalWaitingList[0] == waiter;


        public async Task KillToken()
        {
            if (!IsSettled)
                return;

            var token = Token;
            Token = null;
            ResetTokenAvailability();

            await token.Kill();
            PullTokenFromAbove();
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                ClickedLeft.Raise(this);

            if (eventData.button == PointerEventData.InputButton.Right)
                ClickedRight.Raise(this);
        }


        public void OnEndDrag(PointerEventData eventData) =>
            DragEnd.Raise(this);


        public void OnBeginDrag(PointerEventData eventData) =>
            DragBegin.Raise(this);


        public void OnDrag(PointerEventData eventData) =>
            DragFrame.Raise(eventData);


        public void OnPointerEnter(PointerEventData eventData) =>
            Selected.Raise(this);

        public void OnPointerExit(PointerEventData eventData) =>
            Unselected.Raise();


        private void Reset()
        {
            _image = GetComponent<Image>();
            RectTransform = GetComponent<RectTransform>();
        }
    }
}
