using System;
using UnityEngine;

namespace Hudossay.Match3.Assets.Scripts
{
    public class TileAreaManager : MonoBehaviour
    {
        public GameConfig GameConfig;

        private TileManager[,] _tileManagers;

        [HideInInspector] private Transform _transform;
        [HideInInspector] private RectTransform _rectTransform;
        [HideInInspector] private RectTransform _parentRectTransform;


        private void OnEnable() =>
            Init();


        public void Init()
        {
            _tileManagers = new TileManager[GameConfig.Width, GameConfig.Height];

            InitializeTiles();


            void InitializeTiles()
            {
                for (int x = 0; x < GameConfig.Width; x++)
                    for (int y = 0; y < GameConfig.Height; y++)
                    {
                        var position = new Vector2Int(x, y);
                        var prefab = position switch
                        {
                            _ when position.y == GameConfig.Height - 1 => GameConfig.GeneratorTilePrefab,
                            _ when GameConfig.BlockedTiles.Contains(position) => GameConfig.BlockedTilePrefab,
                            _ => GameConfig.DefaultTilePrefab,
                        };

                        var tileObject = Instantiate(prefab, _transform);
                        var tileManager = tileObject.GetComponent<TileManager>();
                        _tileManagers[x, y] = tileManager;

                        var rectTroansform = tileObject.GetComponent<RectTransform>();
                        rectTroansform.anchoredPosition = new Vector2(x * GameConfig.TileWidth, y * GameConfig.TileHeight);
                        tileManager.Init(position);
                    }
            }
        }


        private void ScaleArea()
        {
            var parentSize = _parentRectTransform.sizeDelta;
            _rectTransform.sizeDelta = new Vector2(GameConfig.Width * GameConfig.TileWidth, GameConfig.Height * GameConfig.TileHeight);

            var minimumParentDimension = Math.Min(parentSize.x, parentSize.y);
            var maximumAreaDimansion = Mathf.Max(_rectTransform.sizeDelta.x, _rectTransform.sizeDelta.y);
            var scale = minimumParentDimension / maximumAreaDimansion;

            _rectTransform.localScale = new Vector3(scale, scale, scale);
        }


        private void OnValidate()
        {
            _transform = transform;
            _rectTransform = GetComponent<RectTransform>();
            _parentRectTransform = _transform.parent.GetComponent<RectTransform>();

            ScaleArea();
        }
    }
}
