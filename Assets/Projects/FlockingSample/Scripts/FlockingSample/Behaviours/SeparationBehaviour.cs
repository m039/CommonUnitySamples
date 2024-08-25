using UnityEngine;

namespace Game.FlockingSample
{
    [CreateAssetMenu(fileName = "SeparationBehaviour", menuName = Consts.RootMenu + "/Flocking/Separation", order = 1)]
    public class SeparationBehaviour : FlockingBehaviour
    {
        public override Vector2 CalculateMove(FlockingManager manager, FlockingAgent agent)
        {
            Vector2 point = Vector2.zero;
            var count = 0;
            var neighbours = manager.GetNeighbours(agent);

            foreach (var a in neighbours)
            {
                var v = a.position - agent.position;
                if (v.magnitude < agent.bodyRadius)
                {
                    point += -v;
                    count++;
                }
            }

            if (count == 0)
            {
                return point;
            }

            return point / count * manager.separationCoeff;
        }
    }
}
