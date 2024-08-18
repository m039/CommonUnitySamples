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
        static readonly public BlackboardKey<float> DestinationThreshold = new("destination_threshold");
        static readonly public BlackboardKey<IGameEntity> Target = new("target");
        static readonly public BlackboardKey<int> EatenFood = new("eaten_food");
        static readonly public BlackboardKey<int> TypeClass = new("type_class");
        static readonly public BlackboardKey<Queue<System.Action>> ExpertActions = new("expert_actions");
        static readonly public BlackboardKey<Queue<System.Action>> ExpertLateActions = new("expert_late_actions");
        static readonly public BlackboardKey<BlackboardBase> GroupBlackboard = new("group_blackboard");
        static readonly public BlackboardKey<bool> DebugMode = new("debug_mode");
        static readonly public BlackboardKey<bool> Selection = new("selection");

        // GOAP Sample
        static readonly public BlackboardKey<bool> IsFacingLeft = new("is_facing_left");
        static readonly public BlackboardKey<bool> HasWood = new("has_wood");
        static readonly public BlackboardKey<bool> IsChoping = new("is_choping");
        static readonly public BlackboardKey<bool> IsInvisible = new("is_invisible");
        static readonly public BlackboardKey<bool> IsLit = new("is_lit");
        static readonly public BlackboardKey<List<IGameEntity>> Childs = new("childs");
        static readonly public BlackboardKey<int> MaxChilds = new("max_childs");
        static readonly public BlackboardKey<IGameEntity> House = new("house");
        static readonly public BlackboardKey<IGameEntity> Bonfire = new("bonfire");
        static readonly public BlackboardKey<float> Tiredness = new("tiredness");
        static readonly public BlackboardKey<HashSet<IGameEntity>> InsideBots = new("inside_bots");
        static readonly public BlackboardKey<Vector2> Entrance = new("entrance");
        static readonly public BlackboardKey<int> WoodCount = new("wood_count");
        static readonly public BlackboardKey<int> FoodCount = new("food_count");
        static readonly public BlackboardKey<bool> NotInterrupt = new("not_interrupt");
        static readonly public BlackboardKey<float> Hunger = new("hunger");
        static readonly public BlackboardKey<bool> HasFood = new("has_food");
        static readonly public BlackboardKey<bool> InDanger = new("in_danger");
        static readonly public BlackboardKey<float> MoveSpeedMultiplier = new("move_speed_multiplier");
        static readonly public BlackboardKey<bool> DebugPathfinding = new("debug_pathfinding");

    }
}
