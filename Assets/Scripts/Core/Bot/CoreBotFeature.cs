using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public abstract class CoreBotFeature : MonoBehaviour
    {
        public abstract void Init(CoreBotController botController);

        public abstract void Deinit(CoreBotController botController);
    }
}
