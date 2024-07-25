using m039.Common;
using UnityEngine;

public class Test1 : CommonMonoBehaviour
{
    public GameObject test;

    [Inject]
    public DITest1 test1;

    void Start()
    {
        Log.Info(this, "test1: " + test1.number1);
    }
}
