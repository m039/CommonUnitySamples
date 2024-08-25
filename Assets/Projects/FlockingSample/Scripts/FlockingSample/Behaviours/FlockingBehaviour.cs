using UnityEngine;

namespace Game.FlockingSample
{
    public abstract class FlockingBehaviour : ScriptableObject
    {
        public abstract Vector2 CalculateMove(FlockingManager manager, FlockingAgent agent);
    }
}
