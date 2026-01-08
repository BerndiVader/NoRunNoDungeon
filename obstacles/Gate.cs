using System;
using System.Data.SqlTypes;
using Godot;

public class Gate : Area2D
{
    private enum STYLE
    {
        STEEL,
        WOOD,
    }
    private enum TYPE
    {
        ENTRY,
        EXIT,
    }

    [Export] private string companionID="";
    [Export] private TYPE type=TYPE.ENTRY;
    [Export] private STYLE style=STYLE.WOOD;
    [Export] private bool closed=false;
    [Export] private Gamestate changeStateTo=Gamestate.BONUS;
    [Export] private bool oneTime=true;

    private bool active=false;
    private bool used=false;
    private const string ID="companion";
    private Vector2 restorePosition=Vector2.Zero;
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
            if(World.instance.input.JustUp())
            {
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
                GetTree().CallGroup(GROUPS.SWITCHABLES.ToString(),nameof(ISwitchable.SwitchCall),ID+companionID,GetInstanceId());
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

    public void SwitchCall(string id,ulong instance)
    {
        if(instance!=GetInstanceId()&&id==ID+companionID)
        {
            if(changeStateTo!=Gamestate.KEEP)
            {
                if(oneTime&&used)
                {
                    closed=true;
                    sprite.Play();
                }
                TeleportLevel();
            }
            else
            {
                if(oneTime)
                {
                    closed=true;
                    sprite.Play();
                }
                TeleportPlayer();
            }
        }
    }

    private void TeleportLevel()
    {
        Player.instance.onTeleport=true;
        Vector2 offset=World.RESOLUTION/2-Renderer.instance.ToLocal(GlobalPosition);
        Vector2 targetPosition=restorePosition!=Vector2.Zero?restorePosition:World.level.Position+offset;

        SceneTreeTween tween=GetTree().CreateTween();
        tween.TweenProperty(World.level,"position",targetPosition,0.1f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
        tween.TweenCallback(this,nameof(TeleportPlayer));
    }

    private void TeleportPlayer()
    {
        Player.instance.GlobalPosition=GlobalPosition;
        Player.instance.Visible=true;
        Player.instance.onTeleport=false;       
        if(type==TYPE.ENTRY&&changeStateTo!=Gamestate.KEEP)
        {
            World.instance.SetGamestate(gamestate);
        }
    }



}
