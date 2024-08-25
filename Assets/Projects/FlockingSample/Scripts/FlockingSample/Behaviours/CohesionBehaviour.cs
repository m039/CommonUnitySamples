using UnityEngine;

namespace Game.FlockingSample
{
    [CreateAssetMenu(fileName = "CohesionBehaviour", menuName = Consts.RootMenu + "/Flocking/Cohesion", order = 1)]
    public class CohesionBehaviour : FlockingBehaviour
    {
        Vector2 _currentVelocity;

        public float smoothTime = 0.5f;

        public override Vector2 CalculateMove(FlockingManager manager, FlockingAgent agent)
        {
            var neighbours = manager.GetNeighbours(agent);
            if (neighbours.Count <= 0)
                return Vector2.zero;

            var averagePosition = Vector2.zero;

            foreach (var a in neighbours)
            {
                averagePosition += a.position;
            }

            averagePosition /= neighbours.Count;
            averagePosition -= agent.position;
            averagePosition = Vector2.SmoothDamp(agent.up, averagePosition, ref _currentVelocity, smoothTime);

            return averagePosition;
        }
    }
}
