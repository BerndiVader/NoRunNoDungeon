using Godot;
using System;

[Tool]
public class DoorSwitch : Area2D
{
    private static readonly AudioStream SFX=ResourceLoader.Load<AudioStream>("res://sounds/ingame/01_chest_open_4.wav");
    private AudioStreamPlayer2D sfxPlayer;
    private readonly VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();

    [Export] private string SWITCH_ID="";
    [Export] private bool ONE_TIME=false;

    private bool active=false;
    private bool used=false;

    private Tween tween;

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        SetProcess(false);
        SetProcessInput(false);

        if(Engine.EditorHint)
        {
            if(string.IsNullOrEmpty(SWITCH_ID))
            {
                SWITCH_ID=Guid.NewGuid().ToString();
                PropertyListChangedNotify();
            }
            return;
        }

        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);

        sfxPlayer=GetNode<AudioStreamPlayer2D>(nameof(AudioStreamPlayer2D));
        sfxPlayer.Stream=SFX;
        
        tween=GetNode<Tween>(nameof(Tween));
        Connect("body_entered",this,nameof(OnBodyEntered));
        Connect("body_exited",this,nameof(OnBodyExited));
    }

    public override void _PhysicsProcess(float delta)
    {
        if(!tween.IsActive()&&Player.instance.input.JustInteract)
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
        sfxPlayer.Play();
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
