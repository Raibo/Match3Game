﻿using System.Collections.Generic;
using UnityEngine;

namespace Hudossay.Match3.Assets.Scripts
{
    public struct MatchingCross
    {
        private Vector2Int _center;
        private List<MatchingGroup> _matchingGroups;
        private TileManager[,] _tiles;

        private const int MaximumSearchDistance = 2;
        private const int MinimumNeighbouringMetches = 2;


        public MatchingCross(Vector2Int center, List<MatchingGroup> matchingGroups, TileManager[,] tiles)
        {
            _center = center;
            _matchingGroups = matchingGroups;
            _tiles = tiles;
        }


        public void GetMatchedTiles(HashSet<TileManager> tilesBuffer)
        {
            tilesBuffer.Clear();

            for (int groupIndex = 0; groupIndex < _matchingGroups.Count; groupIndex++)
                AddMetchesForGroup(_matchingGroups[groupIndex], tilesBuffer);
        }


        private void AddMetchesForGroup(MatchingGroup matchingGroup, HashSet<TileManager> matchBuffer)
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

            for (int i = 1; i <= MaximumSearchDistance; i++)
            {
                if (CheckTileHasGroup(_center + direction * i, matchingGroup))
                    length++;
                else
                    break;
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
