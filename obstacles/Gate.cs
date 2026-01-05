using System;
using Godot;

public class Gate : Area2D
{
    private enum TYPE
    {
        ENTRY,
        EXIT,
    }

    [Export] private string companionID="";
    [Export] private TYPE type=TYPE.ENTRY;

    private bool active=false;
    private bool closed=false;
    private const string ID="companion";
    private Vector2 restorePosition=Vector2.Zero;
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

        sprite=GetNode<AnimatedSprite>(nameof(AnimatedSprite));
        sprite.Play("open");
    }

    public override void _PhysicsProcess(float delta)
    {
        if(active)
        {
            if(World.instance.input.JustUp())
            {

                if(type==TYPE.ENTRY)
                {
                    World.instance.SetGamestate(Gamestate.BONUS);
                    restorePosition=World.level.Position;
                }
                if(type==TYPE.EXIT)
                {
                    World.instance.RestoreGamestate();
                }
                GetTree().CallGroup(GROUPS.SWITCHABLES.ToString(),nameof(ISwitchable.SwitchCall),ID+companionID,GetInstanceId());
                closed=true;
                sprite.Play("close");
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
            TeleportLevel();
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
    }



}
