using Game.BehaviourTreeSample;
using m039.Common;
using m039.Common.Blackboard;
using m039.Common.DependencyInjection;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Game.GOAPSample
{
    public class GameController : CoreGameController
    {
        [Inject]
        readonly GameUI _ui;

        [Inject]
        readonly WorldGenerator _worldGenerator;

        private void Start()
        {
            _ui.onRegenerate += _worldGenerator.GenerateWorld;
            _worldGenerator.GenerateWorld();
        }
    }
}
