using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.FlockingSample
{
    [CreateAssetMenu(fileName = "StayOnScreenBehaviour", menuName = Consts.RootMenu + "/Flocking/StayOnScreen", order = 1)]
    public class StayOnScreenBehaviour : FlockingBehaviour
    {
        [Range(0f, 1f)]
        [SerializeField]
        float _padding = 0f;

        public override Vector2 CalculateMove(FlockingManager manager, FlockingAgent agent)
        {
            var move = Vector2.zero;
            float t;

            var rect = CameraUtils.ScreenRect;
            var size = Mathf.Min(rect.width, rect.height) * _padding / 2f;
            var halfWidth = rect.width / 2f;
            var halfHeight = rect.height / 2f;

            var dx = agent.position.x - rect.center.x;
            
            if (Mathf.Abs(dx) > halfWidth - size)
            {
                t = Mathf.Abs(dx) / halfWidth;
                move.x -= dx * t * t;
            }

            var dy = agent.position.y - rect.center.y;
            if (Mathf.Abs(dy) > halfHeight - size)
            {
                t = Mathf.Abs(dy) / halfHeight;
                move.y -= dy * t * t;
            }
            
            return move;
        }
    }
}
