using UnityEngine;

namespace Game
{
    public static class PhysicUtils
    {
        public static bool OverlapCircles(Vector2 point1, float radius1, Vector2 point2, float radius2)
        {
            return Vector2.Distance(point1, point2) - radius1 - radius2 < 0;
        }
    }
}
