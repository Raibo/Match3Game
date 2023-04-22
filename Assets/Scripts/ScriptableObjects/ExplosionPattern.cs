using Hudossay.Match3.Assets.Scripts.Extensions;
using Hudossay.Match3.Assets.Scripts.MonoBehaviours;
using System.Collections.Generic;
using UnityEngine;

namespace Hudossay.Match3.Assets.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = nameof(ExplosionPattern), menuName = "Scriptable Objects/" + nameof(ExplosionPattern))]
    public class ExplosionPattern : ScriptableObject
    {
        public List<Vector2Int> ExplosionPoints;


        public void AddTilesToExplode(Vector2Int center, Tile[,] tiles, HashSet<Tile> tilesBuffer)
        {
            for (int pointIndex = 0; pointIndex < ExplosionPoints.Count; pointIndex++)
            {
                var position = center + ExplosionPoints[pointIndex];

                if (tiles.TryGetValue(position.x, position.y, out var tile))
                    tilesBuffer.Add(tile);
            }
        }


        private void Reset() =>
            ExplosionPoints = new();
    }
}
