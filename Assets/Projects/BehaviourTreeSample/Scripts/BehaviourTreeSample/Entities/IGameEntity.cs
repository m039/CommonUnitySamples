using m039.Common;
using m039.Common.Blackboard;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public enum GameEntityType
    {
        Bot = 0,
        Food = 1,
        House = 2,
        Bonfire = 3,
        Forest = 4,
        Tree = 5,
        Glade = 6,
        Mushroom = 7
    }

    public interface IGameEntity
    {
        int id { get; }

        Vector2 position { get; set; }

        GameEntityType type { get; }

        int typeClass { get; }

        ServiceLocator locator { get; }

        float spawnRadius { get; }

        void OnCreate(BlackboardBase blackboard);

        void OnDestroy();

        bool IsAlive { get; set; }

        string name { get; }
    }

    public static class GameEntityExt {
        public static void Destroy(this IGameEntity gameEntity) {
            CoreGameController.Instance.ServiceLocator.Get<IGameEntityFactory>().Destroy(gameEntity);
        }
    }
}
