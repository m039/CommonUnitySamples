using m039.Common;
using UnityEngine;

public class DIProvider : MonoBehaviour, IDependencyProvider
{
    [Provide]
    DITest1 GetTest1()
    {
        var test1 = (DITest1)ScriptableObject.CreateInstance<DITest1>();
        test1.number1 = 11;
        test1.number2 = 12;
        return test1;
    }

}
