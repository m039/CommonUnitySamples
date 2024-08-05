using m039.Common;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public class DebugTargetBotSystem : BotSystem
    {
        #region Inspector

        [SerializeField]
        LineRenderer _LineRenderer;

        #endregion

        bool _enabled;

        private void OnEnable()
        {
            _enabled = true;
        }

        private void OnDisable()
        {
            _enabled = false;
        }

        void Update()
        {
            if (_LineRenderer == null)
            {
                return;
            }

            if (botController == null ||
                !botController.Blackboard.TryGetValue(BlackboardKeys.Target, out var target) ||
                !botController.ServiceLocator.TryGet<IGameEntity>(out var gameEntity))
            {
                _LineRenderer.positionCount = 0;
                return;
            }

            if (!CoreGameController.Instance.Blackboard.GetValue(BlackboardKeys.DebugMode, false))
            {
                _LineRenderer.positionCount = 0;
                return;
            }

            if (_enabled)
            {
                _LineRenderer.positionCount = 2;
                _LineRenderer.SetPosition(0, ((Vector3)gameEntity.position).With(z: 200f));
                _LineRenderer.SetPosition(1, ((Vector3)target.position).With(z: 200f));
            } else
            {
                _LineRenderer.positionCount = 0;
            }
        }
    }
}
