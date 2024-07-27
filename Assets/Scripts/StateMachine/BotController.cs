using m039.Common.StateMachine;
using System.Linq;
using UnityEngine;

namespace Game.StateMachine
{
    public class BotController : MonoBehaviour
    {
        #region Inspector

        public float MoveSpeed = 10f;

        [SerializeField] IdleBotState _IdleState;

        [SerializeField] WanderBotState _WanderState;

        [SerializeField] PatrolBotState _PatrolState;

        [SerializeField] SpriteRenderer _Renderer;

        #endregion

        public Vector2 StartPosition { get; private set; }

        m039.Common.StateMachine.StateMachine StateMachine { get; } = new();

        void Awake()
        {
            foreach (var botState in GetComponentsInChildren<BotState>())
            {
                botState.Init(this);
            }

            StartPosition = transform.position;

            var idleState = _IdleState;
            var patrolState = _PatrolState;
            var wanderState = _WanderState;

            StateMachine.AddAnyTransition(idleState, () => Input.GetKeyDown(KeyCode.R));
            StateMachine.AddAnyTransition(patrolState, () => Input.GetKeyDown(KeyCode.P));
            StateMachine.AddAnyTransition(wanderState, () => Input.GetKeyDown(KeyCode.W));
            StateMachine.SetState(idleState);
        }

        void OnGUI()
        {
            if (StateMachine.CurrentState is MonoBehaviourState state) {
                string getName(IState state)
                {
                    if (state is MonoBehaviourState mbs)
                    {
                        return mbs.gameObject.name;
                    } else {
                        return state.GetType().Name;
                    }
                }

                GUI.Label(new Rect(10, 10, 1000, 200), "State: " +  string.Join(" => ", state.GetHierarchicalStates().Select(getName)));
            }
        }

        public void SetColor(Color color)
        {
            _Renderer.color = color;
        }

        void Update()
        {
            StateMachine.Update();
        }

        void FixedUpdate()
        {
            StateMachine.FixedUpdate();
        }
    }
}
