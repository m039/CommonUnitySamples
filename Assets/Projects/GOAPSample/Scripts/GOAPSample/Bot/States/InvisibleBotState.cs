using Game.BehaviourTreeSample;
using Game.StateMachineSample;
using UnityEngine;

namespace Game.GOAPSample
{
    public class InvisibleBotState : CoreBotState
    {
        #region Inspector

        [SerializeField]
        Transform[] _Visuals;

        #endregion

        public override void OnEnter()
        {
            base.OnEnter();

            foreach (var v in _Visuals)
            {
                v.gameObject.SetActive(false);
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            foreach (var v in _Visuals)
            {
                v.gameObject.SetActive(true);
            }
        }
    }
}
