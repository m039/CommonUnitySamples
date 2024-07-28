using UnityEngine;

namespace Game
{
    public abstract class CoreBotBrain : MonoBehaviour
    {
        public virtual void Init(CoreBotController botController)
        {
        }

        public abstract void Think();

        public virtual void FixedThink()
        {
        }
    }
}
