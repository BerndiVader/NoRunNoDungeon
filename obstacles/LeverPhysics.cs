using Godot;
using System;

public class LeverPhysics : PhysicsBlock
{
    private static readonly AudioStream SFX=ResourceLoader.Load<AudioStream>("res://sounds/ingame/01_chest_open_4.wav");
    private AudioStreamPlayer2D sfxPlayer;

    [Export] private string SWITCH_ID="";
    [Export] private bool ONE_TIME=false;

    private bool active=false;
    private bool used=false;

    private Tween tween;
    private Area2D area2D;

    public override void _Ready()
    {
        base._Ready();

        SetProcess(false);
        SetProcessInput(false);

        sfxPlayer=GetNode<AudioStreamPlayer2D>(nameof(AudioStreamPlayer2D));
        sfxPlayer.Stream=SFX;

        area2D=GetNode<Area2D>(nameof(Area2D));
        area2D.Connect("body_entered",this,nameof(OnBodyEntered));
        area2D.Connect("body_exited",this,nameof(OnBodyExited));
        
        tween=GetNode<Tween>(nameof(Tween));
    }

    public override void _PhysicsProcess(float delta)
    {
        Navigation(delta);
        
        if(active&&!tween.IsActive()&&Player.instance.input.JustInteract)
        {
            if(ONE_TIME&&used)
            {
                return;
            }

            Interact();
            used=true;
        }
    }

    private void Interact()
    {
        sfxPlayer.Play();
        area2D.RotationDegrees=-40f;
        tween.InterpolateProperty(area2D,"rotation_degrees",-40f,40f,0.3f, Tween.TransitionType.Sine,Tween.EaseType.InOut);
        tween.InterpolateProperty(area2D,"rotation_degrees",40f,-40f,0.3f, Tween.TransitionType.Sine,Tween.EaseType.InOut,0.3f);
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
        }
    }

    private void OnBodyExited(Node node)
    {
        if(active&&node is Player)
        {
            active=false;
        }
    }

}

