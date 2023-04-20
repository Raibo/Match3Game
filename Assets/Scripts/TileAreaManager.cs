using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Hudossay.Match3.Assets.Scripts
{
    [RequireComponent(typeof(ObjectPool))]
    public class TileAreaManager : MonoBehaviour
    {
        public GameConfig GameConfig;

        private TileManager[,] _tileManagers;
        private List<TileManager> _generators;
        private List<TileManager> _diagonalTiles;
        private Task[] _waitBuffer;

        private readonly Vector2 _generatedTokenDisplacement = new(0f, 200f);

        [Space(15)]
        [SerializeField] private Transform _tilesParent;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private RectTransform _parentRectTransform;
        [SerializeField] private ObjectPool _pool;

        private const int GenerationDelayMilliseconds = 300;


        private async void Start()
        {
            RescaleArea();
            Init();
            FillWithTokens();
        }

        private void Init()
        {
            _tileManagers = new TileManager[GameConfig.Width, GameConfig.Height];
            _waitBuffer = new Task[GameConfig.Width * GameConfig.Height];
            _generators = new List<TileManager>(GameConfig.Width);
            _diagonalTiles = new List<TileManager>();

            InitializeTiles();


            void InitializeTiles()
            {
                for (int y = GameConfig.Height - 1; y >= 0; y--)
                    for (int x = 0; x < GameConfig.Width; x++)
                    {
                        var position = new Vector2Int(x, y);
                        var prefab = position switch
                        {
                            _ when position.y == GameConfig.Height - 1 => GameConfig.GeneratorTilePrefab,
                            _ when GameConfig.BlockedTiles.Contains(position) => GameConfig.BlockedTilePrefab,
                            _ => GameConfig.RegularTilePrefab,
                        };

                        var tileObject = Instantiate(prefab, _tilesParent);
                        var tileManager = tileObject.GetComponent<TileManager>();
                        _tileManagers[x, y] = tileManager;

                        var rectTroansform = tileObject.GetComponent<RectTransform>();
                        var rectPosition = new Vector2((x + 0.5f) * GameConfig.TileWidth, (y + 0.5f) * GameConfig.TileHeight);
                        rectTroansform.anchoredPosition = rectPosition;
                        tileManager.Init(position, _tileManagers);

                        if (tileManager.IsGenerator)
                            _generators.Add(tileManager);

                        if (tileManager.CanAcceptDiagonal)
                            _diagonalTiles.Add(tileManager);
                    }
            }
        }


        private async void FillWithTokens()
        {
            while (true)
            {
                if (!TryGenerateTokens())
                    break;

                foreach (var diagonalTile in _diagonalTiles)
                    PullTokenDiagonally(diagonalTile);

                await Task.Delay(GenerationDelayMilliseconds);
            }
        }


        private bool TryGenerateTokens()
        {
            var anyTokensGenerated = false;

            foreach (var generator in _generators)
            {
                if (generator.HasToken)
                    continue;

                anyTokensGenerated = true;
                var tokenDefinition = GameConfig.TokenDefinitionOptions.PickRandom(o => o.ProbabilityWeight).TokenDefinition;

                var token = _pool.Rent();
                var tokenManager = token.GetComponent<TokenManager>();
                var tokenRectTransform = token.GetComponent<RectTransform>();

                tokenRectTransform.anchoredPosition = generator.RectTransform.anchoredPosition + _generatedTokenDisplacement;
                tokenManager.SetNewTokenDefinition(tokenDefinition);
                tokenManager.AddTravelDestination(generator.RectTransform.anchoredPosition);

                token.SetActive(true);

                generator.Token = tokenManager;
                PushTokenDown(generator);
            }

            return anyTokensGenerated;
        }


        private void PushTokenDown(TileManager pushFromTile)
        {
            if (!TryGetBottomEmptyTile(pushFromTile, out var bottomTile))
                return;

            pushFromTile.SendTokenTo(bottomTile);


            bool TryGetBottomEmptyTile(TileManager fromTile, out TileManager foundTile)
            {
                var result = false;
                foundTile = null;

                for(int y = fromTile.Position.y - 1; y >= 0; y--)
                {
                    var currentTile = _tileManagers[fromTile.Position.x, y];

                    if (!currentTile || currentTile.IsBlocked || currentTile.HasToken)
                        break;

                    foundTile = currentTile;
                    result = true;
                }

                return result;
            }
        }


        private void PullTokenDiagonally(TileManager pullToTile)
        {
            var tileToPull = GetTileToPullFrom();

            if (tileToPull == null || pullToTile.HasToken)
                return;

            tileToPull.SendTokenTo(pullToTile);

            PushTokenDown(pullToTile);


            TileManager GetTileToPullFrom()
            {
                var hasLeftTile = _tileManagers.TryGetValue(pullToTile.Position.x - 1, pullToTile.Position.y + 1, out var leftTile) && leftTile.HasToken;
                var hasRightTile = _tileManagers.TryGetValue(pullToTile.Position.x + 1, pullToTile.Position.y + 1, out var rightTile) && rightTile.HasToken;

                return (hasLeftTile, hasRightTile) switch
                {
                    (true, true) => UnityEngine.Random.Range(0, 2) > 0 ? leftTile : rightTile,
                    (true, false) => leftTile,
                    (false, true) => rightTile,
                    _ => null,
                };
            }
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
            _pool = GetComponent<ObjectPool>();
        }
    }
}
