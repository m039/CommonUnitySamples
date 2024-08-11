using Game.BehaviourTreeSample;
using m039.Common.Blackboard;

namespace Game.GOAPSample
{
    public class MoveToBotStrategy : MoveBotStrategy
    {
        public MoveToBotStrategy(CoreBotController botController, BlackboardKey<IGameEntity> key, bool inSpawnRadiusRange = true)
            : base(
                  botController,
                  () => botController.Blackboard.GetValue(key).position,
                  inSpawnRadiusRange ? () => botController.Blackboard.GetValue(key).spawnRadius : null
                  )
        {
        }
    }
}
