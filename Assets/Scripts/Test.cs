using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using context;
using core;
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
        Hoge();
        // Fuga();
    }

    private void Fuga()
    {
        var hogeA = new HogeA();
        hogeA.Monsters = new List<Monster>();
        hogeA.Monsters.Add(new Monster { HP = 10, Attack = 5 });
        hogeA.Monsters.Add(new Monster { HP = 10, Attack = 5 });
        var jsEngine = new Engine()
            .SetValue("log", new Action<object>(x => Log(x.ToString())))
            .SetValue("hoge", hogeA)
            .Execute(@"
log(hoge);
hoge.Monsters.push();
");
        foreach (var m in hogeA.Monsters)
        {
            Debug.Log($"{m.HP}, {m.Attack}");
        }
    }

    private void Hoge()
    {
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
            Log($"system = {system} / {runMethod.Invoke(entities)}");
        }
        PrintEntities(entities);

        var serializeWorldFunc = mainClass.Get("serializeWorld");
        var serializedWorld = serializeWorldFunc.Invoke().AsString();
        Log(serializedWorld);
        var world = World.deserialize(serializedWorld);

        Log($"world.renderer.queue = {world.renderer.queue.length}");
        for (var i = 0; i < world.renderer.queue.length; ++i)
        {
            var q = world.renderer.queue[i];
            Log($"  - {q}");
            Log($"  - {q.GetType()}");
            Log($"  - {((core.Vector2)q).x}, {((core.Vector2)q).y}");
        }
        /*
        Log($"world.renderer.queue = {world.renderer.queue.Length}");
        foreach (var q in world.renderer.queue)
        {
            Log($"  - {q.x}, {q.y}");
        }
        */

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
        public Monster(int hp, int attack)
        {
            HP = hp;
            Attack = attack;
        }

        public Monster()
        {
        }

        public int HP { get; set; }
        public int Attack { get; set; }
    }

    public class HogeA
    {
        public List<Monster> Monsters { get; set; }
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
