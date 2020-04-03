using System;
using Esprima.Ast;
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
        logText.text = $"{text}\n{logText.text}";
    }
}
