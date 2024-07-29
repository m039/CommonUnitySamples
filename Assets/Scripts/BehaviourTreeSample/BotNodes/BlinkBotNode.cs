using m039.Common.BehaviourTrees;
using System;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public class BlinkBotNode : MonoBehaviour, INode
    {
        #region Inspector

        [SerializeField] Color _ColorMin = Color.white;

        [SerializeField] Color _ColorMax = Color.white;

        [SerializeField] float _Speed = 1f;

        #endregion

        CoreBotController _botController;

        public void Init(CoreBotController botController)
        {
            _botController = botController;
        }

        void INode.Reset()
        {
            _timer = 0;
            SetColor(1);
        }

        public Status Process()
        {
            _timer += Time.deltaTime;
            SetColor(Mathf.Cos(_timer * _Speed));
            return Status.Running;
        }

        [NonSerialized]
        float _timer = 0;

        void SetColor(float value)
        {
            var color = Color.Lerp(_ColorMin, _ColorMax, value);
            // WARNING: Could be a performance issue.
            _botController.EventBus.Raise<ISetColorEvent>(a => a.SetColor(color));
        }
    }
}
