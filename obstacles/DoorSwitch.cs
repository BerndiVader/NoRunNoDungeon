using Godot;
using System;
using System.ComponentModel.Design;

[Tool]
public class DoorSwitch : Area2D
{
    [Export] private string switchID="";
    private Tween tween;
    private bool active=false;

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
        
        tween=GetNode<Tween>(nameof(Tween));      
        tween.Connect("tween_all_completed",this,nameof(OnTweensCompleted));

        Connect("body_entered",this,nameof(OnBodyEntered));
        Connect("body_exited",this,nameof(OnBodyExited));

        SetPhysicsProcess(active);
        SetProcess(false);
        SetProcessInput(false);
    }

    public override void _PhysicsProcess(float delta)
    {
        if(!active&&World.instance.input.Change())
        {
            OnInteract();
        }
    }

    private void OnTweensCompleted()
    {
        active=false;
    }

    private void OnInteract(Player player=null,float amount=0f)
    {
        if(!tween.IsActive()) 
        {
            active=true;
            RotationDegrees=-40f;
            tween.InterpolateProperty(this,"rotation_degrees",-40f,40f,0.3f, Tween.TransitionType.Sine,Tween.EaseType.InOut);
            tween.InterpolateProperty(this,"rotation_degrees",40f,-40f,0.3f, Tween.TransitionType.Sine,Tween.EaseType.InOut,0.3f);
            tween.Start();
            GetTree().CallGroup(GROUPS.SWITCHABLES.ToString(),nameof(ISwitchable.SwitchCall),switchID);
        }
    }

    private void OnBodyEntered(Node node)
    {
        if(node is Player)
        {
            SetPhysicsProcess(true);
        }
    }

    private void OnBodyExited(Node node)
    {
        if(node is Player)
        {
            SetPhysicsProcess(false);
        }
    }

}
