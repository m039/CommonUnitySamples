using m039.Common.DependencyInjection;
using UnityEngine;

namespace Game.FlockingSample
{
    public class GameController : CoreGameController
    {
        #region Inspector

        [SerializeField]
        TMPro.TMP_Text _FPSCounter;

        #endregion

        [Inject]
        FlockingManager _flocking;

        float _fpsTimer = 0;

        void Start()
        {
            _flocking.CreateAgents();
        }

        void Update()
        {
            ProcessDebug();
        }

        void ProcessDebug()
        {
            _fpsTimer -= Time.deltaTime;
            if (_fpsTimer < 0)
            {
                _FPSCounter.text = string.Format("FPS: {0,3:f2}", 1 / Time.deltaTime);
                _fpsTimer = 0.1f;
            }
        }
    }
}
