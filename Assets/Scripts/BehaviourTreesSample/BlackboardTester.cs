using m039.Common.Blackboard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.BlackboardSample
{
    public class BlackboardTester : MonoBehaviour
    {
        m039.Common.Blackboard.Blackboard blackboard = new();

        [SerializeField]
        BlackboardData _BlackboardData;

        private void Awake()
        {
            blackboard.SetValues(_BlackboardData);
            var key22 = new BlackboardKey("key22");
            if (blackboard.TryGetValue(key22, out int key22Value))
            {
                Debug.Log("key22 => " + key22Value);
            } else
            {
                Debug.Log("Can't find value.");
            }
        }
    }
}
