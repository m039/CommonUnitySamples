using Game.BehaviourTreeSample;
using m039.Common.Blackboard;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public static class BlackboardKeys 
    {
        // Behaviour Tree Sample
        static readonly public BlackboardKey<Vector3> StartPosition = new("start_position");
        static readonly public BlackboardKey<float> MoveSpeed = new("move_speed");
        static readonly public BlackboardKey<int> Id = new("id");
        static readonly public BlackboardKey<Vector2> Position = new("position");
        static readonly public BlackboardKey<Vector3> Destination = new("destination");
        static readonly public BlackboardKey<IGameEntity> Target = new("target");
        static readonly public BlackboardKey<int> EatenFood = new("eaten_food");
        static readonly public BlackboardKey<int> TypeClass = new("type_class");
        static readonly public BlackboardKey<Queue<System.Action>> ExpertActions = new("expert_actions");
        static readonly public BlackboardKey<Queue<System.Action>> ExpertLateActions = new("expert_late_actions");
        static readonly public BlackboardKey<BlackboardBase> GroupBlackboard = new("group_blackboard");
        static readonly public BlackboardKey<bool> DebugMode = new("debug_mode");
        static readonly public BlackboardKey<bool> Selection = new("selection");

        // GOAP Sample
    }
}
