using Godot;

public class Gate : Area2D,ISwitchable
{
    private static readonly AudioStream TELEPORT_FX=ResourceLoader.Load<AudioStream>("res://sounds/ingame/12_Player_Movement_SFX/88_Teleport_02.wav");
    private static readonly AudioStreamMP3 CLOSE_FX=ResourceLoader.Load<AudioStreamMP3>("res://sounds/ingame/06_door_close_1.mp3");

    private const string ID="companion";

    private enum STYLE
    {
        STEEL,
        WOOD,
    }
    private enum TYPE
    {
        ENTRY,
        EXIT,
        EXIT_WITH_0Y,
        EXIT_WITH_DY
    }

    [Export] private string companionID="";
    [Export] private TYPE type=TYPE.ENTRY;
    [Export] private STYLE style=STYLE.WOOD;
    [Export] private bool closed=false;
    [Export] private Gamestate changeStateTo=Gamestate.BONUS;
    [Export] private bool oneTime=true;
    [Export] private bool oneWay=false;
    [Export] private string switchID="";
    [Export] private Godot.Collections.Dictionary<string,object> LEVEL_SETTINGS=new Godot.Collections.Dictionary<string,object>()
    {
        {"Use",false},
        {"Dir",Vector2.Zero},
        {"Speed",-1.0f},
        {"Zoom",-1.0f},
    };

    private bool active=false;
    private bool used=false;
    private Vector2 restorePosition=Vector2.Zero;
    private Settings settings;
    private Gamestate gamestate;
    private AnimatedSprite sprite;

    public override void _Ready()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);

        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);

        AddToGroup(GROUPS.SWITCHABLES.ToString());

        Connect("body_entered",this,nameof(OnBodyEntered));
        Connect("body_exited",this,nameof(OnBodyExited));

        if(changeStateTo!=Gamestate.KEEP)
        {
            oneTime=true;
        }

        sprite=GetNode<AnimatedSprite>(nameof(AnimatedSprite));
        sprite.Animation=style.ToString();
        sprite.Frame=closed?4:0;
    }

    public override void _PhysicsProcess(float delta)
    {
        if(active)
        {
            if(World.instance.input.Change())
            {
                active=false;
                SetPhysicsProcess(active);
                if(oneTime&&used)
                {
                    closed=true;
                    sprite.Play();
                    return;
                }
                used=true;
                if(type==TYPE.ENTRY)
                {
                    if(changeStateTo!=Gamestate.KEEP)
                    {
                        gamestate=World.state;
                        World.instance.SetGamestate(changeStateTo);
                        restorePosition=World.level.Position;
                    }
                }
                else if(oneTime)
                {
                    closed=true;
                    sprite.Play();
                }
                if((bool)LEVEL_SETTINGS["Use"])
                {
                    settings=new Settings(World.level,Vector2.Zero,(float)LEVEL_SETTINGS["Speed"],(float)LEVEL_SETTINGS["Zoom"]);
                    settings.autoRestore=World.level.settings.autoRestore;
                    settings.restoreToDefault=false;
                    settings.Set();
                }
                GetTree().CallGroup(GROUPS.SWITCHABLES.ToString(),nameof(TeleportCall),ID+companionID,GetInstanceId());
            }
        }
    }


    private void OnBodyEntered(Node node)
    {
        if(!closed&&!active&&node is Player)
        {
            active=true;
            SetPhysicsProcess(active);
        }
    }

    private void OnBodyExited(Node node)
    {
        if(active&&node is Player)
        {
            active=false;
            SetPhysicsProcess(active);
        }
    }

    public void TeleportCall(string id,ulong instance)
    {
        if(instance!=GetInstanceId()&&id==ID+companionID)
        {
            SfxPlayer teleportFx=new SfxPlayer
            {
                Position=Position,
                Stream=TELEPORT_FX
            };
            World.level.AddChild(teleportFx);

            if(changeStateTo!=Gamestate.KEEP)
            {
                if(oneTime&&used)
                {
                    closed=true;
                    sprite.Frame=0;
                    sprite.Play();
                }
                TeleportLevel();
            }
            else
            {
                if(oneTime)
                {
                    closed=true;
                    sprite.Frame=0;
                    sprite.Play();
                    SfxPlayer closefx=new SfxPlayer
                    {
                        Stream=CLOSE_FX,
                        Position=Position
                    };
                    World.level.AddChild(closefx);
                }
                TeleportPlayer();
            }
        }
    }

    private void TeleportLevel()
    {
        Player.instance.Teleport(true);
        Vector2 offset=World.RESOLUTION/2-Renderer.instance.ToLocal(GlobalPosition);
        Vector2 targetPosition=restorePosition!=Vector2.Zero?restorePosition:World.level.Position+offset;

        if(type==TYPE.EXIT_WITH_0Y)
        {
            targetPosition.y=0f;
        }

        SceneTreeTween tween=GetTree().CreateTween();
        tween.TweenProperty(World.level,"position",targetPosition,0.1f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
        tween.TweenCallback(this,nameof(TeleportPlayer));
    }

    private void TeleportPlayer()
    {
        Player.instance.GlobalPosition=GlobalPosition;
        Player.instance.Teleport(false);

        if(oneWay)
        {
            sprite.Frame=0;
            sprite.Play();
            SfxPlayer closefx=new SfxPlayer
            {
                Stream=CLOSE_FX,
                Position=Position
            };
            World.level.AddChild(closefx);

        }

        if(type==TYPE.ENTRY&&changeStateTo!=Gamestate.KEEP)
        {
            World.instance.SetGamestate(gamestate);
        }

        if((bool)LEVEL_SETTINGS["Use"])
        {
            if(settings!=null)
            {
                settings.Restore();
            }
            else
            {
                World.level.DEFAULT_SETTING.Restore();
            }
        }
    }

    public void SwitchCall(string id)
    {
        if(id==switchID)
        {
            switch(closed)
            {
                case true:
                    sprite.Frame=sprite.Frames.GetFrameCount(style.ToString());
                    sprite.Play(null,true);
                    closed=false;
                    break;
                case false:
                    sprite.Frame=0;
                    sprite.Play();
                    closed=true;
                    break;
            }
            SfxPlayer closefx=new SfxPlayer
            {
                Stream=CLOSE_FX,
                Position=Position
            };
            World.level.AddChild(closefx);            
        }
    }
}
