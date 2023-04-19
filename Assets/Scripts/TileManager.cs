using UnityEngine;
using UnityEngine.UI;

namespace Hudossay.Match3.Assets.Scripts
{
    public class TileManager : MonoBehaviour
    {
        public Vector2Int Position;
        public Sprite BackgroundEven;
        public Sprite BackgroundOdd;

        [SerializeField]
        private Image _image;


        public void Init(Vector2Int position)
        {
            Position = position;

            _image.sprite = (Position.x + Position.y) % 2 == 0
                ? BackgroundEven
                : BackgroundOdd;
        }


        private void Reset()
        {
            _image = GetComponent<Image>();
        }
    }
}
