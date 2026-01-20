using Godot;
using System;

[Tool]
public class DoorSwitch : Area2D
{
    [Export] private string switchID="";
    [Export] private bool oneTime=false;

    private static AudioStream sfx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/01_chest_open_4.wav");

    private Tween tween;
    private AudioStreamPlayer2D sfxPlayer=new AudioStreamPlayer2D();
    private bool active=false;
    private bool used=false;

    public override void _Ready()
    {
        if(!Engine.EditorHint) 
        {
            VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
            notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
            AddChild(notifier2D);
        }

        if(Engine.EditorHint&&switchID=="")
        {
            switchID=Guid.NewGuid().ToString();
            PropertyListChangedNotify();
        }

        sfxPlayer.Stream=sfx;
        sfxPlayer.Bus="Sfx";
        sfxPlayer.MaxDistance=500f;
        AddChild(sfxPlayer);
        
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
            if(oneTime&&used)
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
        GetTree().CallGroup(GROUPS.SWITCHABLES.ToString(),nameof(ISwitchable.SwitchCall),switchID);
    }

    private void OnBodyEntered(Node node)
    {
        if(!active&&node is Player)
        {
            if(oneTime&&used)
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
