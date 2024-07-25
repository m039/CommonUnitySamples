using UnityEngine;

[System.Serializable]
public class DITest1 : ScriptableObject, IDITest1
{
    public int number1 { get; set; }
    public int number2; 
}

public interface IDITest1
{
    public int number1 { get; }
}
