using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.FlockingSample
{
    [CreateAssetMenu(fileName = "StayOnScreenBehaviour", menuName = Consts.RootMenu + "/Flocking/StayOnScreen", order = 1)]
    public class StayOnScreenBehaviour : FlockingBehaviour
    {
        public override Vector2 CalculateMove(FlockingManager manager, FlockingAgent agent)
        {
            var rect = CameraUtils.ScreenRect;
            var radius = Mathf.Min(rect.width, rect.height);
            var centerOffset = rect.center - agent.position;
            var t = centerOffset.magnitude / radius;
            if (t < 0.9f)
            {
                return Vector2.zero;
            }

            return centerOffset * t * t;
        }
    }
}
