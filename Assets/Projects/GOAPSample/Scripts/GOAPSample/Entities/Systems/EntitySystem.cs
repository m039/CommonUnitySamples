using Game.BehaviourTreeSample;
using UnityEngine;

namespace Game.GOAPSample
{
    public abstract class EntitySystem : MonoBehaviour
    {
        protected IGameEntity gameEntity;

        public void Init(IGameEntity gameEntity)
        {
            this.gameEntity = gameEntity;
            OnInit();
        }

        public void Deinit()
        {
            OnDeinit();
            this.gameEntity = null;
        }

        protected virtual void OnInit() {}

        protected virtual void OnDeinit() {}

        public virtual void Process(float deltaTime) {}
    }
}
