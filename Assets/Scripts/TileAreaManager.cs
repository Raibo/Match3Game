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
        private List<RectTransform> _generators;
        private Task[] _waitBuffer;

        private readonly Vector2 _generatedTokenDisplacement = new(0f, 200f);

        [Space(15)]
        [SerializeField] private Transform _tilesParent;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private RectTransform _parentRectTransform;
        [SerializeField] private ObjectPool _pool;


        private void Start()
        {
            Init();
            RefillTokens();
        }

        public void Init()
        {
            _tileManagers = new TileManager[GameConfig.Width, GameConfig.Height];
            _waitBuffer = new Task[GameConfig.Width * GameConfig.Height];
            _generators = new List<RectTransform>(GameConfig.Width);

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
                            _generators.Add(rectTroansform);
                    }
            }
        }


        public async Task RefillTokens()
        {
            foreach (var generator in _generators)
            {
                var tokenDefinition = GameConfig.TokenDefinitionOptions.PickRandom(o => o.ProbabilityWeight).TokenDefinition;

                var token = _pool.Rent();
                var tokenManager = token.GetComponent<TokenManager>();
                var tokenRectTransform = token.GetComponent<RectTransform>();

                tokenRectTransform.anchoredPosition = generator.anchoredPosition + _generatedTokenDisplacement;
                tokenManager.SetNewTokenDefinition(tokenDefinition);
                tokenManager.AddTravelDestination(generator.anchoredPosition);

                token.SetActive(true);
            }
        }


        [ContextMenu("Rescale")]
        private void ScaleArea()
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
