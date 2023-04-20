using System;
using UnityEngine;
using UnityEngine.UI;

namespace Hudossay.Match3.Assets.Scripts
{
    public class TileManager : MonoBehaviour
    {
        public Vector2Int Position;
        public bool IsGenerator;
        public bool IsBlocked;
        public bool CanAcceptDiagonal;

        [Space(15)]
        public Sprite BackgroundEven;
        public Sprite BackgroundOdd;

        [SerializeField]
        private Image _image;


        public void Init(Vector2Int position, TileManager[,] tileManagers)
        {
            Position = position;

            _image.sprite = (Position.x + Position.y) % 2 == 0
                ? BackgroundEven
                : BackgroundOdd;

            CanAcceptDiagonal = CheckShouldAcceptDiagonal();


            bool CheckShouldAcceptDiagonal()
            {
                if (IsBlocked)
                    return false;

                var upperTileBlocked = CheckTile(position.x, position.y + 1, tm => tm.IsBlocked);

                var upperLeftTileDiagonal = CheckTile(position.x - 1, position.y + 1, tm => tm.CanAcceptDiagonal || tm.IsBlocked);
                var upperMiddleTileDiagonal = CheckTile(position.x, position.y + 1, tm => tm.CanAcceptDiagonal || tm.IsBlocked);
                var upperRightTileDiagonal = CheckTile(position.x + 1, position.y + 1, tm => tm.CanAcceptDiagonal || tm.IsBlocked);

                return upperTileBlocked || (upperLeftTileDiagonal && upperMiddleTileDiagonal && upperRightTileDiagonal);
            }


            bool CheckTile(int x, int y, Func<TileManager, bool> func) =>
                tileManagers.TryGetValue(x, y, out var tileManager) && func(tileManager);
        }


        private void Reset()
        {
            _image = GetComponent<Image>();
        }
    }
}
