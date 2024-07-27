using UnityEngine;

namespace Game.StateMachine
{
    public class MoveWaypointBotState : MoveBotState
    {

        #region Inspector

        [SerializeField] Transform[] _Waypoints;

        #endregion

        int _waypointIndex;

        bool _firstStart;

        public override Vector2? Target
        {
            get
            {
                if (_Waypoints == null || _Waypoints.Length <= 0)
                    return null;

                return _Waypoints[_waypointIndex].transform.position;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();

            if (!_firstStart)
            {
                _firstStart = true;
                return;
            }

            if (_Waypoints == null || _Waypoints.Length <= 0)
                return;

            _waypointIndex = (_waypointIndex + 1) % _Waypoints.Length;
        }
    }
}
