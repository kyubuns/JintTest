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

        var world = new world.World();
        world.input = new context.Input();
        world.renderer = new context.Renderer();
        world.renderer.queue = new Array<object>();
        world.time = new context.Time();

        Debug.Log(readText);
        var jsEngine = new Engine()
            .SetValue("aprot", new JsAprot(world))
            .SetValue("log", new Action<object>(x => Log(x.ToString())))
            .Execute(readText);

        var mainClass = jsEngine.GetValue("Main");
        var initFanc = mainClass.Get("init");
        var entities = initFanc.Invoke();

        PrintEntities(entities);
        PrintWorld(world);

        var getListFunc = mainClass.Get("getList");
        var systems = getListFunc.Invoke();
        Log($"systems = {systems}");

        foreach (var system in systems.AsArray())
        {
            var runMethod = system.Get("run");
            Log($"system = {system} / {runMethod.Invoke(entities)}");
        }
        PrintEntities(entities);
        PrintWorld(world);

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

    private void PrintWorld(world.World world)
    {
        Log($"world.renderer.queue = {world.renderer.queue.length}");
        for (var i = 0; i < world.renderer.queue.length; ++i)
        {
            var element = world.renderer.queue[i];
            Log($"{element.GetType()}");
            core.Vector2
            var vector2 = (core.Vector2) element;
            Log($"  - {vector2.x}, {vector2.y}");
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

public class JsAprot
{
    public Native native;

    public JsAprot(world.World world)
    {
        native = new Native(world);
    }
}

public class Native
{
    public world.World world;

    public Native(world.World world)
    {
        this.world = world;
    }
}
