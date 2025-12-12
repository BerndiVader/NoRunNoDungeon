using Godot;
using System;

[Tool]
public class DoorSwitch : Area2D
{
    [Export] private string switchID="";
    private Tween tween;

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

        AddUserSignal(STATE.damage.ToString());
        Connect(STATE.damage.ToString(),this,nameof(OnDamage));
        
        tween=GetNode<Tween>(nameof(Tween));
    }

    private void OnDamage(Player player=null, int amount=0)
    {
        if(!tween.IsActive()) 
        {
            RotationDegrees=-40f;
            tween.InterpolateProperty(this,"rotation_degrees",-40f,40f,0.3f, Tween.TransitionType.Sine,Tween.EaseType.InOut);
            tween.InterpolateProperty(this,"rotation_degrees",40f,-40f,0.3f, Tween.TransitionType.Sine,Tween.EaseType.InOut,0.3f);
            tween.Start();
            GetTree().CallGroup(GROUPS.DOORS.ToString(),nameof(HiddenDoor.SwitchCall),switchID);
        }
    }

}
