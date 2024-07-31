using m039.Common.Blackboard;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public interface IGameEntity
    {
        int id { get; }

        Vector2 position { get; set; }

        void OnCreate(Blackboard blackboard);

        void OnDestroy();
    }
}
