using m039.UIToolbox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class SamplesTemplateProvider : MonoBehaviour, ListMenu.ITemplateProvider
    {
        public List<(string text, Action onClick)> GetTemplates()
        {
            var buttons = new List<(string, string)>()
            {
                ("GOAP", "GOAPSample"),
                ("Behaviour Tree", "BehaviourTreeSample"),
                ("State Machine", "StateMachineSample"),
                ("Pathfinding", "PathfindingSample"),
                ("Flocking", "FlockingSample")
            };

            var result = new List<(string, Action)>();

            foreach (var (name, scene) in buttons)
            {
                result.Add((name, () => SceneManager.LoadScene(scene)));
            }

            return result;
        }
    }
}
