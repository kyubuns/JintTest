using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    [SerializeField] private Text logText = default;

    public void Start()
    {
        logText.text = "Init...";
        Log("Test.Start");
        Log("Test.Finish");
    }

    private void Log(string text)
    {
        logText.text = $"{text}\n{logText.text}";
    }
}
