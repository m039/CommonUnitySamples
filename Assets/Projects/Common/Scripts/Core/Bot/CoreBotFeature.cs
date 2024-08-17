using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public abstract class CoreBotFeature : MonoBehaviour
    {
        protected CoreBotController botController;

        public virtual void Init(CoreBotController botController)
        {
            this.botController = botController;
            OnInit();
        }

        public virtual void Deinit()
        {
            OnDeinit();
        }

        protected abstract void OnInit();

        protected abstract void OnDeinit();
    }
}
