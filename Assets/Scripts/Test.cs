using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using Esprima.Ast;
using Jint;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class Test : MonoBehaviour
{
    [SerializeField] private Text logText = default;
    private bool logStop = false;

    public IEnumerator Start()
    {
        logText.text = "Init...";
        Log("Test.Start");

        var engine = new Engine()
            .SetValue("log", new Action<object>(x => Log(x.ToString())));

        engine.Execute(@"
            function hello() { 
                log('Hello ECMA Script!');
            };

            hello();
        ");

        var add = new Engine()
            .Execute("function add(a, b) { return a + b; }")
            .GetValue("add");

        Log($"1 + 2 = {add.Invoke(1, 2)}");
        Log($"5 + 7 = {add.Invoke(5, 7)}");

        var nanka = new Nanka
        {
            MonsterA = new Monster { HP = 10, Attack = 3 },
            MonsterB = new Monster { HP = 20, Attack = 5 },
        };

        new Engine()
            .SetValue("log", new Action<object>(x => Log(x.ToString())))
            .SetValue("nanka", nanka)
            .Execute(@"
                log(`nanka.MonsterA.HP = ${nanka.MonsterA.HP}`);
                log(`nanka.MonsterA.HP -= 5`);
                nanka.MonsterA.HP -= 5;
            ");

        Log(nanka.MonsterA.HP.ToString());

        yield return new WaitForSeconds(1);

        // var readText = File.ReadAllText("/Users/kyubuns/code/Aprot/Haxe/bin/main.js");
        var readText = ((TextAsset) Resources.Load("main")).text;
        Debug.Log(readText);
        var jsEngine = new Engine()
            .SetValue("log", new Action<object>(x => Log(x.ToString())))
            .Execute(readText);

        logStop = true;
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < 10000; ++i)
        {
            var mainClass = jsEngine.GetValue("Main");
            var getListFunc = mainClass.Get("getList");
            var systems = getListFunc.Invoke();
            Log($"systems = {systems}");

            foreach (var system in systems.AsArray())
            {
                Log($"system = {system} / {system.Get("run").Invoke()}");
            }
        }
        stopwatch.Stop();
        logStop = false;
        Log($"{stopwatch.ElapsedMilliseconds}ms");

        Log("Test.Finish");
    }

    public class Nanka
    {
        public Monster MonsterA { get; set; }
        public Monster MonsterB { get; set; }
    }

    public class Monster
    {
        public int HP { get; set; }
        public int Attack { get; set; }
    }

    private void Log(string text)
    {
        if (logStop) return;
        Debug.Log(text);
        logText.text = $"{text}\n{logText.text}";
    }
}
