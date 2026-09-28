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
    [Export] private Gamestate changeStateTo=Gamestate.BOSS;
    [Export] private bool oneTime=true;
    [Export] private bool oneWay=false;
    [Export] private string switchID="";
    [Export] private Godot.Collections.Dictionary<string,object> LEVEL_SETTINGS=Settings.DEFAULT_LEVEL_SETTINGS.Duplicate();

    private bool active=false;
    private bool used=false;
    private Vector2 restorePosition=Vector2.Zero;
    private Settings settings,restoreSettings;
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

        if(Settings.Usable(LEVEL_SETTINGS))
        {
            Settings.Populate(LEVEL_SETTINGS);
            settings=new Settings(World.level,LEVEL_SETTINGS);
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if(!active)
        {
            return;
        }

        if(Player.instance.input.JustInteract)
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

            if(settings!=null)
            {
                restoreSettings=World.level.settings;
                settings.Set();
            }

            Dust dust=ResourceUtils.dust.Instance<Dust>();
            dust.type=Dust.TYPE.DISAPPEAR;
            dust.Position=World.level.ToLocal(Player.instance.GlobalPosition);
            World.level.AddChild(dust);

            GetTree().CallGroup(GROUPS.SWITCHABLES.ToString(),nameof(TeleportCall),ID+companionID,GetInstanceId());
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
            Renderer.instance.PlaySfx(TELEPORT_FX,GlobalPosition);

            if(changeStateTo!=Gamestate.KEEP)
            {
                if(oneTime&&used)
                {
                    closed=true;
                    sprite.Frame=0;
                    sprite.Play();
                    Renderer.instance.PlaySfx(CLOSE_FX,GlobalPosition);
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
                    Renderer.instance.PlaySfx(CLOSE_FX,GlobalPosition);
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

        Dust dust=ResourceUtils.dust.Instance<Dust>();
        dust.type=Dust.TYPE.APPEAR;
        dust.Position=World.level.ToLocal(Player.instance.GlobalPosition);
        World.level.AddChild(dust);        

        Player.instance.Teleport(false);

        if(oneWay)
        {
            sprite.Frame=0;
            sprite.Play();
            Renderer.instance.PlaySfx(CLOSE_FX,GlobalPosition);
        }

        if(type==TYPE.ENTRY&&changeStateTo!=Gamestate.KEEP)
        {
            World.instance.SetGamestate(gamestate);
        }

        if(Settings.Usable(LEVEL_SETTINGS))
        {
            if(restoreSettings!=null&&!settings.restoreToDefault)
            {
                restoreSettings.Set();
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
            Renderer.instance.PlaySfx(CLOSE_FX,GlobalPosition);
        }
    }
}
