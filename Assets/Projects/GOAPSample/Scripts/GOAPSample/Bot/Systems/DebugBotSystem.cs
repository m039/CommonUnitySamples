using Game.BehaviourTreeSample;
using m039.Common.Pathfindig;
using System.Text;
using UnityEngine;

namespace Game.GOAPSample
{
    public class DebugBotSystem : CoreBotSystem
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
        }

        public void DebugPath(Path path)
        {
            if (path == null ||
                !CoreGameController.Instance.Blackboard.GetValue(BlackboardKeys.DebugPathfinding))
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
    }
}
