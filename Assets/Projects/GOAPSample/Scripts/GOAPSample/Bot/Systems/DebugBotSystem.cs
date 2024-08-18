using m039.Common.Pathfindig;
using System.Text;
using UnityEngine;

namespace Game.GOAPSample
{
    public class DebugBotSystem : CoreBotSystem, IOnCreateEntityEvent, IOnDestoyEntityEvent
    {
        #region Inspector

        [SerializeField]
        TMPro.TMP_Text _DebugInfoText;

        [SerializeField]
        LineRenderer _DebugPathLine;

        #endregion

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            botController.ServiceLocator.Register(this);
            botController.EventBus.Subscribe(this);
        }

        public void DebugPath(Path path)
        {
            if (!CoreGameController.Instance.Blackboard.GetValue(BlackboardKeys.DebugPathfinding))
                return;

            if (path == null)
            {
                _DebugPathLine.positionCount = 0;
            } else
            {
                _DebugPathLine.positionCount = path.vectorPath.Count;
                for (int i = 0; i < path.vectorPath.Count; i++)
                {
                    var p = path.vectorPath[i];
                    _DebugPathLine.SetPosition(i, new Vector3(p.x, p.y, 10));
                }
                _DebugPathLine.startColor = _DebugPathLine.endColor = Color.white;
            }
        }

        public override void Process(float deltaTime)
        {
            base.Process(deltaTime);

            if (botController.Blackboard.ContainsKey(BlackboardKeys.IsInvisible) ||
                !CoreGameController.Instance.Blackboard.GetValue(BlackboardKeys.DebugMode))
            {
                _DebugInfoText.gameObject.SetActive(false);
            } else
            {
                _DebugInfoText.gameObject.SetActive(true);

                var sb = new StringBuilder();

                if (botController.Blackboard.ContainsKey(BlackboardKeys.HasFood))
                {
                    sb.AppendLine("+Food");
                }

                if (botController.Blackboard.ContainsKey(BlackboardKeys.HasWood))
                {
                    sb.AppendLine("+Wood");
                }

                if (botController.Blackboard.ContainsKey(BlackboardKeys.InDanger))
                {
                    sb.AppendLine("<color=\"red\">In Danger</color>");
                }

                if (botController.Blackboard.TryGetValue(BlackboardKeys.Hunger, out var hunger))
                {
                    string number = hunger.ToString("0.0");
                    if (hunger > 0.8f)
                    {
                        number = $"<color=\"red\">{number}</color>";
                    }
                    sb.AppendLine($"Hunger: {number}");
                }

                _DebugInfoText.text = sb.ToString();
            }
        }

        public void OnCreateEntity()
        {
            CoreGameController.Instance.Blackboard.Subscribe(BlackboardKeys.DebugPathfinding, OnDebugPathfindingChanged);
            OnDebugPathfindingChanged();
        }

        public void OnDestroyEntity()
        {
            CoreGameController.Instance.Blackboard.Unsubscribe(BlackboardKeys.DebugPathfinding, OnDebugPathfindingChanged);
        }

        void OnDebugPathfindingChanged()
        {
            _DebugPathLine.gameObject.SetActive(CoreGameController.Instance.Blackboard.GetValue(BlackboardKeys.DebugPathfinding));
        }
    }
}
