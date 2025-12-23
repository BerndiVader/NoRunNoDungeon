using Godot;
using System;

public class FallingHammer : Area2D
{
    private RayCast2D raycast;
    private Tween tween;
    private bool active=false;
    private float startRotation;

    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);

        raycast=GetNode<RayCast2D>(nameof(RayCast2D));

        tween=new Tween();
        AddChild(tween);

        startRotation=Rotation;
        Start();
    }

    public override void _PhysicsProcess(float delta)
    {
        if(active&&raycast.IsColliding())
        {
            Stop();
        }
    }

    private void Start()
    {
        active=true;
        tween.InterpolateProperty(this,"rotation",Rotation,0f,2f,Tween.TransitionType.Circ,Tween.EaseType.In);
        tween.Start();   
    }

    private void Stop()
    {
        tween.StopAll();
        active=false;
        Rotation=Rotation;
        tween.InterpolateProperty(this,"rotation",Rotation,startRotation,1f,Tween.TransitionType.Quad,Tween.EaseType.Out);
        tween.Start();
    }

    private void OnBackComplete()
    {
        Start();
    }


}
