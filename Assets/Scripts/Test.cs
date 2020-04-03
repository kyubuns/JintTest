using System;
using Jint;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    [SerializeField] private Text logText = default;

    public void Start()
    {
        logText.text = "Init...";
        Log("Test.Start");

        var engine = new Engine()
                .SetValue("log", new Action<object>(x => Log(x.ToString())))
            ;

        engine.Execute(@"
            function hello() { 
                log('Hello ECMA Script!');
            };

            hello();
        ");

        Log("Test.Finish");
    }

    private void Log(string text)
    {
        logText.text = $"{text}\n{logText.text}";
    }
}
