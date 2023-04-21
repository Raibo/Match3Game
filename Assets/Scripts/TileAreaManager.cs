using Hudossay.AttributeEvents.Assets.Runtime;
using Hudossay.AttributeEvents.Assets.Runtime.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hudossay.Match3.Assets.Scripts
{
    [RequireComponent(typeof(TokenPool))]
    [RequireComponent(typeof(EventLinker))]
    public class TileAreaManager : MonoBehaviour
    {
        public GameConfig GameConfig;

        [Space(15)]
        [SerializeField] private Transform _tilesParent;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private RectTransform _parentRectTransform;
        [SerializeField] private TokenPool _tokenPool;
        [SerializeField] private EventLinker _eventLinker;

        private TileManager[,] _tiles;
        private List<TileManager> _generators;
        private List<TileManager> _diagonalTiles;
        private HashSet<TileManager> _matchBuffer;
        private HashSet<TileManager> _ExplodeBuffer;

        private TileManager _selectedTile;


        private void Start()
        {
            RescaleArea();
            Init();
            TriggerTilesPull();
        }


        [ResponseLocal(TileEventKind.Settled)]
        public void HandleSettledTile(TileManager settledTile)
        {
            var matchingCross = new MatchingCross(settledTile.Position, settledTile.Token.TokenDefinition.MatchingGroups, _tiles);
            matchingCross.GetMatchedTiles(_matchBuffer);

            _ExplodeBuffer.Clear();

            foreach (var match in _matchBuffer)
                match.Token.TokenDefinition.ExplosionPattern?.AddTilesToExplode(match.Position, _tiles, _ExplodeBuffer);

            foreach (var match in _matchBuffer)
                _ExplodeBuffer.Add(match);

            foreach (var tileToExplode in _ExplodeBuffer)
                tileToExplode.KillToken();
        }


        [ResponseLocal(TileEventKind.ClickedRight)]
        public void ApplyExplosion(TileManager target)
        {
            _ExplodeBuffer.Clear();
            GameConfig.OnClickExplosion?.AddTilesToExplode(target.Position, _tiles, _ExplodeBuffer);

            foreach (var tileToExplode in _ExplodeBuffer)
                tileToExplode.KillToken();
        }


        [ResponseLocal(TileEventKind.Selected)]
        public void UpdateSelectedTile(TileManager newSelected) =>
            _selectedTile = newSelected;


        [ResponseLocal(TileEventKind.DragEnd)]
        public void OnDragEnd(TileManager draggedTile)
        {
            if (draggedTile != _selectedTile)
                draggedTile.SwapTokensWith(_selectedTile);
        }


        private void Init()
        {
            _tiles = new TileManager[GameConfig.Width, GameConfig.Height];
            _generators = new List<TileManager>(GameConfig.Width);
            _diagonalTiles = new List<TileManager>();
            _matchBuffer = new(7);
            _ExplodeBuffer = new();

            InitializeTiles();


            void InitializeTiles()
            {
                for (int y = GameConfig.Height - 1; y >= 0; y--)
                    for (int x = 0; x < GameConfig.Width; x++)
                    {
                        var position = new Vector2Int(x, y);
                        var prefab = position switch
                        {
                            _ when GameConfig.BlockedTiles.Contains(position) => GameConfig.BlockedTilePrefab,
                            _ when position.y == GameConfig.Height - 1 => GameConfig.GeneratorTilePrefab,
                            _ => GameConfig.RegularTilePrefab,
                        };

                        var tileObject = Instantiate(prefab, _tilesParent);
                        var tileManager = tileObject.GetComponent<TileManager>();
                        _tiles[x, y] = tileManager;

                        var rectTroansform = tileObject.GetComponent<RectTransform>();
                        var rectPosition = new Vector2((x + 0.5f) * GameConfig.TileWidth, (y + 0.5f) * GameConfig.TileHeight);
                        rectTroansform.anchoredPosition = rectPosition;

                        _tiles.TryGetValue(x - 1, y + 1, out var upperLeftTile);
                        _tiles.TryGetValue(x , y + 1, out var upperMiddleTile);
                        _tiles.TryGetValue(x + 1, y + 1, out var upperRightTile);

                        tileManager.Init(position, upperLeftTile, upperMiddleTile, upperRightTile, _tokenPool, GameConfig);

                        if (tileManager.IsGenerator)
                            _generators.Add(tileManager);

                        if (tileManager.CanAcceptDiagonal)
                            _diagonalTiles.Add(tileManager);

                        _eventLinker.StartListeningTo(tileObject);
                        tileObject.name += tileManager.Position;
                    }
            }
        }


        private void TriggerTilesPull()
        {
            for (int x = 0; x < GameConfig.Width; x++)
                for (int y = 0; y < GameConfig.Height; y++)
                    _tiles[x, y].PullTokenFromAbove();
        }


        [ContextMenu("Rescale")]
        private void RescaleArea()
        {
            var parentSize = _parentRectTransform.sizeDelta;
            var newSizeDelta = new Vector2(GameConfig.Width * GameConfig.TileWidth, GameConfig.Height * GameConfig.TileHeight);

            var minimumParentDimension = Math.Min(parentSize.x, parentSize.y);
            var maximumAreaDimansion = Mathf.Max(newSizeDelta.x, newSizeDelta.y);
            var scale = minimumParentDimension / maximumAreaDimansion;
            var newScaleVEctor = new Vector3(scale, scale, scale);

            if (_rectTransform.sizeDelta != newSizeDelta || _rectTransform.localScale != newScaleVEctor)
            {
                _rectTransform.sizeDelta = newSizeDelta;
                _rectTransform.localScale = newScaleVEctor;
            }
        }


        private void OnValidate()
        {
            _rectTransform = GetComponent<RectTransform>();
            _parentRectTransform = transform.parent.GetComponent<RectTransform>();
            _tokenPool = GetComponent<TokenPool>();
            _eventLinker = GetComponent<EventLinker>();
        }
    }
}
