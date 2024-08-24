using UnityEngine;

namespace Game
{
    public static class CameraUtils
    {
        public static bool PointInScreen(Vector2 point)
        {
            return ScreenRect.Contains(point);
        }

        public static Rect ScreenRect
        {
            get
            {
                var height = Camera.main.orthographicSize * 2;
                var width = height * Camera.main.aspect;
                var p = (Vector2)Camera.main.transform.position;
                var rect = new Rect(p.x - width / 2f, p.y - height / 2f, width, height);
                return rect;
            }
        }

        public static Vector2 RandomPositionOnScreen(float padding = 0f)
        {
            var height = Camera.main.orthographicSize * 2;
            var width = height * Camera.main.aspect;

            height -= padding * 2;
            width -= padding * 2;

            Vector2 position = (Vector2)Camera.main.transform.position +
                new Vector2(Random.Range(-width / 2f, width / 2f), Random.Range(-height / 2f, height / 2f));
            return position;
        }
    }
}
