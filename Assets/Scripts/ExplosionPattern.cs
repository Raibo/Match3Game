using System.Collections.Generic;
using UnityEngine;

namespace Hudossay.Match3.Assets.Scripts
{
    [CreateAssetMenu(fileName = nameof(ExplosionPattern), menuName = "Scriptable Objects/" + nameof(ExplosionPattern))]
    public class ExplosionPattern : ScriptableObject
    {
        public List<Vector2Int> ExplosionPoints;


        public void AddTilesToExplode(Vector2Int center, Tile[,] tiles, HashSet<Tile> pointsBuffer)
        {
            for (int pointIndex = 0; pointIndex < ExplosionPoints.Count; pointIndex++)
            {
                var position = center + ExplosionPoints[pointIndex];

                if (tiles.TryGetValue(position.x, position.y, out var tile))
                    pointsBuffer.Add(tile);
            }
        }


        private void Reset() =>
            ExplosionPoints = new List<Vector2Int>();
    }
}
