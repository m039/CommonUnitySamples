using m039.Common;
using m039.Common.Blackboard;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public enum GameEntityType
    {
        Bot = 0,
        Food = 1
    }

    public interface IGameEntity
    {
        int id { get; }

        Vector2 position { get; set; }

        GameEntityType type { get; }

        ServiceLocator locator { get; }

        void OnCreate(Blackboard blackboard);

        void OnDestroy();

        bool IsAlive { get; set; }
    }

    public static class GameEntityExt {
        public static void Destroy(this IGameEntity gameEntity) {
            CoreGameController.Instance.ServiceLocator.Get<IGameEntityFactory>().Destroy(gameEntity);
        }
    }
}
