using System.Collections.Generic;
using UnityEngine;

namespace Hudossay.Match3.Assets.Scripts
{
    [CreateAssetMenu(fileName = nameof(GameConfig), menuName = "Scriptable Objects/" + nameof(GameConfig))]
    public class GameConfig : ScriptableObject
    {
        public int Width;
        public int Height;

        [Space(15)]
        public float TileWidth;
        public float TileHeight;

        [Space(15)]
        public GameObject DefaultTilePrefab;
        public GameObject GeneratorTilePrefab;
        public GameObject BlockedTilePrefab;

        public List<TokenDefinition> TokenDefinitions;
        public List<Vector2Int> BlockedTiles;


        private void Reset()
        {
            Width = 10;
            Height = 10;

            TileWidth = 100f;
            TileHeight = 100f;
        }
    }
}
