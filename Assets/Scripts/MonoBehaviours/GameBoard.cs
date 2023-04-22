using Hudossay.AttributeEvents.Assets.Runtime;
using Hudossay.AttributeEvents.Assets.Runtime.Attributes;
using Hudossay.Match3.Assets.Scripts.Extensions;
using Hudossay.Match3.Assets.Scripts.ScriptableObjects;
using Hudossay.Match3.Assets.Scripts.SupportStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hudossay.Match3.Assets.Scripts.MonoBehaviours
{
    [RequireComponent(typeof(TokenPool))]
    [RequireComponent(typeof(EventLinker))]
    public class GameBoard : MonoBehaviour
    {
        public GameConfig GameConfig;

        [Space(15)]
        [SerializeField] private Transform _tilesParent;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private RectTransform _parentRectTransform;
        [SerializeField] private TokenPool _tokenPool;
        [SerializeField] private EventLinker _eventLinker;

        [Space(15)]
        [SerializeField] private GameObject _dragImageObject;
        [SerializeField] private Transform _dragImageTransform;
        [SerializeField] private Image _dragImageImage;

        private Tile[,] _tiles;
        private HashSet<Tile> _matchBuffer;
        private HashSet<Tile> _ExplodeBuffer;

        private Tile _selectedTile;
        private bool _triggeredPull;


        private void Start()
        {
            RescaleArea();
            Init();
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && !_triggeredPull)
            {
                _triggeredPull = true;
                TriggerTilesPull();
            }
        }


        [ResponseLocal(TileEventKind.Settled)]
        public void HandleSettledTile(Tile settledTile)
        {
            var matchingCross = new MatchingCross(settledTile.Position, settledTile.Token.TokenDefinition.MatchingGroups, _tiles);
            matchingCross.GetMatchedTiles(_matchBuffer);

            foreach (var match in _matchBuffer)
                KillTokenInTile(match);
        }


        [ResponseLocal(TileEventKind.ClickedRight)]
        public void ApplyExplosion(Tile target)
        {
            _ExplodeBuffer.Clear();
            GameConfig.OnClickExplosion?.AddTilesToExplode(target.Position, _tiles, _ExplodeBuffer);

            foreach (var tileToExplode in _ExplodeBuffer)
                KillTokenInTile(tileToExplode);
        }


        [ResponseLocal(TileEventKind.Selected)]
        public void UpdateSelectedTile(Tile newSelected) =>
            _selectedTile = newSelected;


        [ResponseLocal(TileEventKind.Unselected)]
        public void UpdateUnselectedTile() =>
            _selectedTile = null;


        [ResponseLocal(TileEventKind.DragBegin)]
        public void OnDragBegin(Tile draggedTile)
        {
            if (!draggedTile.IsSettled)
                return;

            _dragImageImage.sprite = draggedTile.Token.TokenDefinition.Sprite;
            _dragImageObject.SetActive(true);
        }


        [ResponseLocal(TileEventKind.DragFrame)]
        public void OnDragFrame(PointerEventData eventData)
        {
            var position = eventData.pressEventCamera.ScreenToWorldPoint(Input.mousePosition);
            position.z = 0f;
            _dragImageTransform.position = position;
        }


        [ResponseLocal(TileEventKind.DragEnd)]
        public void OnDragEnd(Tile draggedTile)
        {
            _dragImageObject.SetActive(false);

            if (_selectedTile is not null && draggedTile != _selectedTile)
                draggedTile.SwapTokensWith(_selectedTile);
        }


        private async void KillTokenInTile(Tile tileToKill)
        {
            if (!tileToKill.IsSettled)
                return;

            var explosionPattern = tileToKill.Token.TokenDefinition.ExplosionPattern;

            _ = tileToKill.KillToken();
            await Task.Yield();

            _ExplodeBuffer.Clear();
            explosionPattern?.AddTilesToExplode(tileToKill.Position, _tiles, _ExplodeBuffer);

            foreach (var chainExplosionTile in _ExplodeBuffer)
                KillTokenInTile(chainExplosionTile);
        }


        private void Init()
        {
            _tiles = new Tile[GameConfig.Width, GameConfig.Height];
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
                        var tile = tileObject.GetComponent<Tile>();
                        _tiles[x, y] = tile;

                        var rectPosition = new Vector2((x + 0.5f) * GameConfig.TileWidth, (y + 0.5f) * GameConfig.TileHeight);
                        tile.RectTransform.anchoredPosition = rectPosition;

                        _tiles.TryGetValue(x - 1, y + 1, out var upperLeftTile);
                        _tiles.TryGetValue(x, y + 1, out var upperMiddleTile);
                        _tiles.TryGetValue(x + 1, y + 1, out var upperRightTile);

                        tile.Init(position, upperLeftTile, upperMiddleTile, upperRightTile, _tokenPool, GameConfig);

                        _eventLinker.StartListeningTo(tileObject);
                        tileObject.name += tile.Position;
                    }
            }
        }


        private void TriggerTilesPull()
        {
            var randomRange = Enumerable.Range(0, GameConfig.Width)
                .OrderBy(x => UnityEngine.Random.Range(0f, 1f));

            foreach (var x in randomRange)
                for (int y = 0; y < GameConfig.Height; y++)
                    _tiles[x, y].PullTokenFromAbove();
        }


        [ContextMenu("Rescale")]
        private void RescaleArea()
        {
            var parentSize = _parentRectTransform.sizeDelta;
            var newSizeDelta = new Vector2(GameConfig.Width * GameConfig.TileWidth, GameConfig.Height * GameConfig.TileHeight);

            var minimumParentDimension = Math.Min(parentSize.x, parentSize.y);
            var maximumBoardDimansion = Mathf.Max(newSizeDelta.x, newSizeDelta.y);
            var scale = minimumParentDimension / maximumBoardDimansion;
            var newScale = new Vector3(scale, scale, scale);

            if (_rectTransform.sizeDelta == newSizeDelta && _rectTransform.localScale == newScale)
                return;

            _rectTransform.sizeDelta = newSizeDelta;
            _rectTransform.localScale = newScale;
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
