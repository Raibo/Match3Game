using Hudossay.Match3.Assets.Scripts.Extensions;
using Hudossay.Match3.Assets.Scripts.MonoBehaviours;
using Hudossay.Match3.Assets.Scripts.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;

namespace Hudossay.Match3.Assets.Scripts.SupportStructures
{
    public readonly struct MatchingCross
    {
        private readonly Vector2Int _center;
        private readonly List<MatchingGroup> _matchingGroups;
        private readonly Tile[,] _tiles;

        private const int MaximumSearchDistance = 2;
        private const int MinimumNeighbouringMetches = 2;


        public MatchingCross(Vector2Int center, List<MatchingGroup> matchingGroups, Tile[,] tiles)
        {
            _center = center;
            _matchingGroups = matchingGroups;
            _tiles = tiles;
        }


        public void GetMatchedTiles(HashSet<Tile> tilesBuffer)
        {
            tilesBuffer.Clear();

            for (int groupIndex = 0; groupIndex < _matchingGroups.Count; groupIndex++)
                AddMetchesForGroup(_matchingGroups[groupIndex], tilesBuffer);
        }


        private void AddMetchesForGroup(MatchingGroup matchingGroup, HashSet<Tile> matchBuffer)
        {
            var leftVector = new Vector2Int(-1, 0);
            var rightVector = new Vector2Int(1, 0);
            var upVector = new Vector2Int(0, 1);
            var downVector = new Vector2Int(0, -1);

            var leftLength = GetMatchingLength(leftVector, matchingGroup);
            var rightLength = GetMatchingLength(rightVector, matchingGroup);
            var upLength = GetMatchingLength(upVector, matchingGroup);
            var downLength = GetMatchingLength(downVector, matchingGroup);

            if (leftLength + rightLength < MinimumNeighbouringMetches)
            {
                leftLength = 0;
                rightLength = 0;
            }

            if (downLength + upLength < MinimumNeighbouringMetches)
            {
                downLength = 0;
                upLength = 0;
            }

            if (leftLength + rightLength + downLength + upLength == 0)
                return;

            for (int x = -leftLength; x <= rightLength; x++)
                matchBuffer.Add(_tiles[_center.x + x, _center.y]);

            for (int y = -downLength; y <= upLength; y++)
                matchBuffer.Add(_tiles[_center.x, _center.y + y]);
        }


        private int GetMatchingLength(Vector2Int direction, MatchingGroup matchingGroup)
        {
            int length = 0;

            for (int distance = 1; distance <= MaximumSearchDistance; distance++)
            {
                if (!CheckTileHasGroup(_center + direction * distance, matchingGroup))
                    break;

                length++;
            }

            return length;
        }


        private bool CheckTileHasGroup(Vector2Int position, MatchingGroup group)
        {
            if (!_tiles.TryGetValue(position.x, position.y, out var tileToCheck))
                return false;

            return tileToCheck.IsSettled && tileToCheck.Token.TokenDefinition.MatchingGroups.Contains(group);
        }
    }
}
