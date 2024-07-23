using m039.Common;
using UnityEngine;

public class Test1 : CommonMonoBehaviour
{
    public GameObject test;

    void Start()
    {
        Log.Info(this, "HELLO: " + test);

        Coroutines.WaitForSeconds(3, () => Log.Info<Test1>("Alarm"));
    }
}
