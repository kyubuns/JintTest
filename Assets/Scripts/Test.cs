using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using context;
using Esprima.Ast;
using haxe.root;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Runtime;
using Jint.Runtime.Interop;
using UnityEngine;
using UnityEngine.UI;
using world;
using Debug = UnityEngine.Debug;

public class Test : MonoBehaviour
{
    [SerializeField] private Text logText = default;
    private bool logStop = false;

    public void Start()
    {
        logText.text = "Init...";
        Log("Test.Start");

        var readText = File.ReadAllText("/Users/kyubuns/code/Aprot/Haxe/bin/main.js");
        // var readText = ((TextAsset) Resources.Load("main")).text;

        Debug.Log(readText);
        var jsEngine = new Engine()
            .SetValue("log", new Action<object>(x => Log(x.ToString())))
            .Execute(readText);

        var mainClass = jsEngine.GetValue("Main");
        var initFanc = mainClass.Get("init");
        var entities = initFanc.Invoke();

        PrintEntities(entities);

        var getListFunc = mainClass.Get("getList");
        var systems = getListFunc.Invoke();
        Log($"systems = {systems}");

        foreach (var system in systems.AsArray())
        {
            var runMethod = system.Get("run");
            Log($"system = {system} / {runMethod.Invoke(null, entities)}");
        }
        PrintEntities(entities);

        Log("Test.Finish");
    }

    private void PrintEntities(JsValue entities)
    {
        Log("PrintEntities");
        foreach (var entity in entities.AsArray())
        {
            Log("entity");
            foreach (var property in entity.AsObject().GetOwnProperties())
            {
                Log($"  - {property.Key}");
                foreach (var ownProperty in property.Value.Value.AsObject().GetOwnProperties())
                {
                    Log($"    - {ownProperty.Key} = {ownProperty.Value.Value}");
                }
            }
            Log($"  ----");
        }
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
