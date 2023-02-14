using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    bool show_console = false;
    bool show_help;
    bool godmode = false, idkfa = false;
    string input;

    [SerializeField]
    private Sprite eshells_sprite, invincibility_sprite;

    //Command declarations
    public static DebugCommand HELP, NOCLIP, GOD, KILL, KILLALL, IDKFA;
    public static DebugCommand<int> SETHEALTH, SETARMOR;
    public static DebugCommand<string> KILLTYPE, GIVE;

    public List<object> command_list;

    //various burps whenever the player picks up a small item (indices 2-4)
    void PlaySmallPickupSound()
    {
        int index = Random.Range(2, 5);
        AudioUtils.InstanceVoice(index, transform.position, this, null, false, 1f, Random.Range(0.5f, 1.0f));
    }

    //big burp whenever the player picks up a big item (indices 5-6), has a rare chance to play the stock burp sound (index 7)
    void PlayBigPickupSound()
    {
        int index = Random.Range(5, 7);
        if (Random.Range(0, 100) == 0)
        {
            index = 7;
            AudioUtils.InstanceVoice(index, transform.position, this);
        }
        else
            AudioUtils.InstanceVoice(index, transform.position, this, null, false, 1f, Random.Range(0.5f, 1.0f));
    }

    //plays whenever the player picks up a big health at low hp
    void PlayHealthPickupCritical()
    {
        AudioUtils.InstanceVoice(8, transform.position, this);
    }

    void PlayPickupShotgunSound()
    {
        AudioUtils.InstanceVoice(1, transform.position, this);
    }

    public bool ShowConsole
    {
        get { return show_console; }
    }

    public void ToggleConsole()
    {
        show_console = !show_console;

        if (show_console)
        {
            Time.timeScale = 0.0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            Time.timeScale = 1.0f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void Awake()
    {
        //define commands
        HELP = new DebugCommand("help", "Shows list of all available commands", "help", () =>
        {
            show_help = true;
        });

        NOCLIP = new DebugCommand("noclip", "You're a ghost!", "noclip", () =>
        {
            GetComponent<PlayerMovement>().ToggleNoclip();
        });

        GOD = new DebugCommand("god", "Nano-machines son!", "god", () =>
        {
            godmode = !godmode;
            PlayerStats stats = GetComponent<PlayerStats>();

            if (godmode)
            {
                stats.PowerupTime = float.MinValue;
                stats.Invincible = true;
            }
            else
            {
                stats.PowerupTime = 0.0f;
                stats.Invincible = false;
            }
        });

        SETHEALTH = new DebugCommand<int>("set_health", "Sets health to a specific value", "set_health <VALUE>", (int hp) =>
        {
            GetComponent<PlayerStats>().HP = hp;
        });

        SETARMOR = new DebugCommand<int>("set_resistance", "Sets resistance to a specific value", "set_resistance <VALUE>", (int res) =>
        {
            GetComponent<PlayerStats>().AP = res;
        });

        KILL = new DebugCommand("kill", "Kys :)", "kill", () =>
        {
            Debug.Log("occurring");
            GetComponent<PlayerStats>().TakeDamage(int.MaxValue);
        });

        KILLTYPE = new DebugCommand<string>("kill_type", "Kills enemies of specific type... or yourself :)", "kill_type <TYPE>", (string type) =>
        {
            if(type == "player" || type == "me")
            {
                GetComponent<PlayerStats>().TakeDamage(int.MaxValue);
                return;
            }

            //find all enemies in the game and check their typing, kill them if the right typing was provided
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            bool killed = false;
            for(int i=0; i<enemies.Length; i++)
            {
                BaseEnemyBehavior behavior = enemies[i].GetComponent<BaseEnemyBehavior>();
                if(behavior.EnemyTag == type)
                {
                    behavior.TakeDamage(int.MaxValue);
                    killed = true;
                }
            }

            if (!killed)
                GetComponent<PlayerStats>().AnnounceText = $"No enemies of type: \"{type}\" were found in the level!";
        });

        KILLALL = new DebugCommand("kill_all", "Mass murder!", "kill_all", () =>
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            for(int i=0; i<enemies.Length; i++)
            {
                enemies[i].GetComponent<BaseEnemyBehavior>().TakeDamage(int.MaxValue);
            }
        });

        GIVE = new DebugCommand<string>("give", "Gives a specified powerup", "give <POWERUP>", (string powerup) =>
        {
            PlayerStats stats = GetComponent<PlayerStats>();
            switch (powerup)
            {
                case "small_hp":
                    stats.HP += 10;
                    stats.AnnounceText = "Picked up pizza slice!";
                    PlaySmallPickupSound();
                    break;
                case "big_hp":
                    stats.HP += 50;
                    stats.AnnounceText = "Picked up a pizza pie!";
                    if (stats.HP > 20)
                        PlayBigPickupSound();
                    else
                        PlayHealthPickupCritical();
                    break;
                case "armor":
                    if (stats.AP < 100)
                    {
                        stats.AP += 20;
                        stats.AnnounceText = "Picked up a cold one!";
                        PlaySmallPickupSound();
                    }
                    break;
                case "explosive_rounds":
                    if (stats.PowerupTime <= 0)
                    {
                        stats.PowerupTime = 60;
                        stats.EShells = true;
                        stats.PowerUpSprite = eshells_sprite;
                        stats.AnnounceText = "Picked up explosive rounds!";
                        stats.face.OneTimeAnimationDriver(4);
                        PlayBigPickupSound();
                    }
                    break;
                case "invincibility":
                    if (stats.PowerupTime <= 0)
                    {
                        stats.PowerupTime = 60;
                        stats.Invincible = true;
                        stats.PowerUpSprite = invincibility_sprite;
                        stats.AnnounceText = "Picked up some sort of doo-hickey?";
                        stats.face.OneTimeAnimationDriver(4);
                        PlayBigPickupSound();
                    }
                    break;
                case "shotgun":
                    stats.HasShotgun = true;
                    PlayPickupShotgunSound();
                    break;
            }
        });

        IDKFA = new DebugCommand("IDKFA", "Gives god mode and explosive shells, it's time to frag!", "IDKFA", () =>
        {
            idkfa = !idkfa;
            PlayerStats stats = GetComponent<PlayerStats>();

            if (idkfa)
            {
                stats.PowerupTime = float.MinValue;
                stats.Invincible = true;
                stats.EShells = true;
            }
            else
            {
                stats.PowerupTime = 0.0f;
                stats.Invincible = false;
                stats.EShells = false;
            }
        });

        //add all commands to the list
        command_list = new List<object>()
        {
            HELP, NOCLIP, GOD, SETHEALTH, SETARMOR, KILL, KILLTYPE, KILLALL, GIVE, IDKFA
        };
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            ToggleConsole();
            input = "";
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            HandleInput();
            input = "";
        }
    }

    Vector2 scroll;

    private void OnGUI()
    {
        if (!show_console) return;

        float y = 0.0f;

        GUIStyle style = new GUIStyle();
        style.fontSize = 30;
        style.normal.textColor = Color.white;

        if (show_help)
        {
            GUI.Box(new Rect(0, y, Screen.width, 200), "");

            Rect viewport = new Rect(0, 0, Screen.width - 30, 200 * command_list.Count);

            scroll = GUI.BeginScrollView(new Rect(0, y + 5f, Screen.width, 190), scroll, viewport);

            for(int i=0; i<command_list.Count; i++)
            {
                DebugCommandBase command = command_list[i] as DebugCommandBase;

                string label = $"{command.command_format} - {command.command_description}";

                Rect labelRect = new Rect(4, 40 * i, viewport.width - 100, 40);

                GUI.Label(labelRect, label, style);
            }

            GUI.EndScrollView();

            y += 200;
        }

        GUI.Box(new Rect(0, y, Screen.width, 60), "");
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        input = GUI.TextField(new Rect(10.0f, y + 10.0f, Screen.width - 20.0f, 40.0f), input, int.MaxValue, style);
    }

    private void HandleInput()
    {
        string[] properties = input.Split(' ');

        for(int i=0;i <command_list.Count; i++)
        {
            DebugCommandBase command_base = command_list[i] as DebugCommandBase;

            if (properties[0] == command_base.command_id)
            {
                if(command_list[i] as DebugCommand != null)
                {
                    //Cast to this type and invoke the command
                    (command_list[i] as DebugCommand).Invoke();
                }
                else if(command_list[i] as DebugCommand<int> != null)
                {
                    (command_list[i] as DebugCommand<int>).Invoke(int.Parse(properties[1]));
                }
                else if(command_list[i] as DebugCommand<string> != null)
                {
                    (command_list[i] as DebugCommand<string>).Invoke(properties[1]);
                }
            }
        }
    }
}
