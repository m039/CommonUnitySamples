using UnityEngine;

namespace Game.FlockingSample
{
    [CreateAssetMenu(fileName = "AlignmentBehaviour", menuName = Consts.RootMenu + "/Flocking/Alignment", order = 1)]
    public class AlignmentBehaviour : FlockingBehaviour
    {
        public override Vector2 CalculateMove(FlockingManager manager, FlockingAgent agent)
        {
            var neighbours = manager.GetNeighbours(agent);
            if (neighbours.Count <= 0)
                return agent.up;

            Vector2 direction = Vector2.zero;

            foreach (var a in neighbours)
            {
                direction += a.up;
            }

            return direction / neighbours.Count;
        }
    }
}
