using Godot;
using System;

[Tool]
public class DoorSwitch : Area2D
{
    private static readonly AudioStream SFX=ResourceLoader.Load<AudioStream>("res://sounds/ingame/01_chest_open_4.wav");
    private readonly AudioStreamPlayer2D SFX_PLAYER=new AudioStreamPlayer2D();

    [Export] private string SWITCH_ID="";
    [Export] private bool ONE_TIME=false;

    private bool active=false;
    private bool used=false;

    private Tween tween;

    public override void _Ready()
    {
        if(!Engine.EditorHint) 
        {
            VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
            notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
            AddChild(notifier2D);
        }

        if(Engine.EditorHint&&SWITCH_ID=="")
        {
            SWITCH_ID=Guid.NewGuid().ToString();
            PropertyListChangedNotify();
        }

        SFX_PLAYER.Stream=SFX;
        SFX_PLAYER.Bus="Sfx";
        SFX_PLAYER.MaxDistance=ResourceUtils.MAX_SFX_DISTANCE;
        AddChild(SFX_PLAYER);
        
        tween=GetNode<Tween>(nameof(Tween));
        Connect("body_entered",this,nameof(OnBodyEntered));
        Connect("body_exited",this,nameof(OnBodyExited));

        SetPhysicsProcess(false);
        SetProcess(false);
        SetProcessInput(false);
    }

    public override void _PhysicsProcess(float delta)
    {
        if(!tween.IsActive()&&World.instance.input.Change())
        {
            if(ONE_TIME&&used)
            {
                SetPhysicsProcess(false);
                return;
            }
            Interact();
            used=true;
        }
    }

    private void Interact()
    {
        SFX_PLAYER.Play();
        RotationDegrees=-40f;
        tween.InterpolateProperty(this,"rotation_degrees",-40f,40f,0.3f, Tween.TransitionType.Sine,Tween.EaseType.InOut);
        tween.InterpolateProperty(this,"rotation_degrees",40f,-40f,0.3f, Tween.TransitionType.Sine,Tween.EaseType.InOut,0.3f);
        tween.Start();
        GetTree().CallGroup(GROUPS.SWITCHABLES.ToString(),nameof(ISwitchable.SwitchCall),SWITCH_ID);
    }

    private void OnBodyEntered(Node node)
    {
        if(!active&&node is Player)
        {
            if(ONE_TIME&&used)
            {
                return;
            }
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

}
