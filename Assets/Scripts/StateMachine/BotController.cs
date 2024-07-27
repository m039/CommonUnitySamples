using m039.Common;
using System.Linq;
using UnityEngine;

namespace Game
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

        StateMachine StateMachine { get; } = new();

        void Awake()
        {
            foreach (var botState in GetComponentsInChildren<BotState>())
            {
                botState.Init(this);
            }

            StartPosition = transform.position;

            StateMachine.AddAnyTransition(_IdleState, () => Input.GetKeyDown(KeyCode.R));
            StateMachine.AddAnyTransition(_PatrolState, () => Input.GetKeyDown(KeyCode.P));
            StateMachine.AddAnyTransition(_WanderState, () => Input.GetKeyDown(KeyCode.W));
            StateMachine.SetState(_IdleState);
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
