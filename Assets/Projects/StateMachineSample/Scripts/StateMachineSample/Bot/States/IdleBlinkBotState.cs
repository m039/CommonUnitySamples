using System;
using UnityEngine;

namespace Game.StateMachineSample
{
    public class IdleBlinkBotState : IdleBotState
    {
        #region Inspector

        [SerializeField] Color _ColorMin = Color.white;

        [SerializeField] Color _ColorMax = Color.white;

        [SerializeField] float _Speed = 1f;

        #endregion

        [NonSerialized]
        float _timer = 0;

        public override void OnEnter()
        {
            base.OnEnter();

            _timer = 0;
            SetColor(1);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            _timer += Time.deltaTime;
            SetColor(Mathf.Cos(_timer * _Speed));
        }

        public override void OnExit()
        {
            base.OnExit();

            botController.EventBus.Raise<ISetColorEvent>(a => a.SetColor(Color.white));
        }

        void SetColor(float value)
        {
            var color = Color.Lerp(_ColorMin, _ColorMax, value);
            // WARNING: Could be a performance issue.
            botController.EventBus.Raise<ISetColorEvent>(a => a.SetColor(color));
        }
    }
}
