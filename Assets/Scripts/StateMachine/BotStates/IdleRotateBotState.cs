using UnityEngine;

namespace Game.StateMachine
{
    public class IdleRotateBotState : IdleBotState
    {
        #region Inspector

        [SerializeField] float _RotationSpeed = 10;

        #endregion

        public override void OnUpdate()
        {
            base.OnUpdate();

            var eulerAngles = botController.transform.rotation.eulerAngles;
            eulerAngles.z += _RotationSpeed * Time.deltaTime;
            botController.transform.rotation = Quaternion.Euler(eulerAngles);
        }

        public override void OnExit()
        {
            base.OnExit();

            botController.transform.rotation = Quaternion.identity;
        }
    }
}
