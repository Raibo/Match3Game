﻿using Hudossay.Match3.Assets.Scripts.SupportStructures;
using System.Collections.Generic;
using UnityEngine;

namespace Hudossay.Match3.Assets.Scripts.ScriptableObjects
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
        public ExplosionPattern OnClickExplosion;

        [Space(15)]
        public GameObject RegularTilePrefab;
        public GameObject GeneratorTilePrefab;
        public GameObject BlockedTilePrefab;

        [Space(15)]
        public Vector2 GeneratedTokensDisplacement;
        public float TokenSpeed;
        public float GenerationDelayDelta;
        public float GenerationDelayMax;
        public float GenerationDelayTimeout;
        public List<TokenDefinitionOption> TokenDefinitionOptions;
        public List<Vector2Int> BlockedTiles;


        private void Reset()
        {
            Width = 10;
            Height = 10;

            TileWidth = 100f;
            TileHeight = 100f;

            TokenSpeed = 800f;
            GenerationDelayDelta = 0.05f;
            GenerationDelayMax = 0.2f;
            GenerationDelayTimeout = 1f;

            TokenDefinitionOptions = new();
            BlockedTiles = new();
        }
    }
}
