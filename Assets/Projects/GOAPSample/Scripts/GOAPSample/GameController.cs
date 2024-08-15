using Game.BehaviourTreeSample;
using m039.Common.BehaviourTrees;
using m039.Common.Blackboard;
using m039.Common.DependencyInjection;
using m039.Common.GOAP;
using m039.Common.StateMachine;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.GOAPSample
{
    public class GameController : CoreGameController
    {
        static readonly Collider2D[] s_Buffer = new Collider2D[16];

        [Inject]
        readonly GameUI _ui;

        [Inject]
        readonly WorldGenerator _worldGenerator;

        IGameEntity _selectedBot;

        void Start()
        {
            _ui.onRegenerate += _worldGenerator.GenerateWorld;
            _worldGenerator.GenerateWorld();
        }

        void Update()
        {
            ProcessInput();
            ProcessBotInfo();
        }

        void ProcessInput()
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var count = Physics2D.OverlapCircleNonAlloc(position, 0.1f, s_Buffer);
                IGameEntity bot = null;

                for (int i = 0; i < count; i++)
                {
                    if (s_Buffer[i].GetComponentInParent<IGameEntity>() is IGameEntity gameEntity)
                    {
                        if (bot == null && gameEntity.type == GameEntityType.Bot)
                        {
                            bot = gameEntity;
                            break;
                        }
                    }
                }

                void unselectCurrentBot()
                {
                    _selectedBot?.locator.Get<BlackboardBase>().Remove(BlackboardKeys.Selection);
                    _selectedBot = null;
                }

                if (_selectedBot != null && _selectedBot == bot)
                {
                    unselectCurrentBot();
                }
                else if (bot != null)
                {
                    unselectCurrentBot();
                    _selectedBot = bot;
                    _selectedBot.locator.Get<BlackboardBase>().SetValue(BlackboardKeys.Selection, true);
                }
            }
        }

        void ProcessBotInfo()
        {
            if (_selectedBot == null)
            {
                _ui.botInfo.gameObject.SetActive(false);
                return;
            }
            _ui.botInfo.gameObject.SetActive(true);

            object getValue(BlackboardEntry value)
            {
                var v = value.GetValue();
                if (v is ICollection collection)
                {
                    return $"{v.GetType().Name}[{collection.Count}]";
                }
                else if (v is IGameEntity gameEntity)
                {
                    return gameEntity.name;
                }
                return v;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"<b>{_selectedBot.name}</b>");
            if (_selectedBot.locator.TryGet(out BlackboardBase blackboard) && blackboard is GameBlackboard gameBlackboard)
            {
                sb.AppendLine("Blackboard:");
                foreach (var entities in gameBlackboard)
                {
                    sb.AppendLine($"  {entities.Key}: {getValue(entities.Value)}");
                }
            }
            if (_selectedBot.locator.TryGet(out StateMachine stateMachine))
            {
                sb.AppendLine();
                sb.AppendLine("StateMachine:");
                string currentStateName;
                if (stateMachine.CurrentState is MonoBehaviour monoBahaviour)
                {
                    currentStateName = monoBahaviour.name;
                }
                else
                {
                    currentStateName = stateMachine.CurrentState?.ToString();
                }
                sb.AppendLine($"  CurrentState: {currentStateName}");
            }
            if (_selectedBot.locator.TryGet(out Agent agent))
            {
                sb.AppendLine();
                sb.AppendLine("GOAP:");
                agent.Print(2, sb);
            }
            _ui.botInfo.text = sb.ToString();
        }
    }
}
