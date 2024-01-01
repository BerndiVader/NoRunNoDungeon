using Godot;
using System;

public class Fairy : KinematicMonster
{
    private float passedTime;
    private Vector2 offsetPos;
    [Export]private Vector2 SinCosSpeed=new Vector2(3f,1.5f);
    [Export]private Vector2 FloatRange=new Vector2(5f,5f);

    public override void _Ready()
    {
        base._Ready();
        passedTime=0f;
        state=STATE.unknown;
        EmitSignal(STATE.idle.ToString());
    }

    public override void _PhysicsProcess(float delta)
    {
        float lastX=Position.x;
        goal(delta);
        animationController.FlipH=Position.x>lastX;
    }

    protected override void idle(float delta)
    {
        fly(delta);
    }

    protected override void onIdle()
    {
        onDelay=false;
        if(state!=STATE.idle)
        {
            lastState=state;
            state=STATE.idle;
            animationController.Play("idle");
            goal=idle;
            offsetPos=new Vector2(Position);
        }
    }

    private void fly(float delta)
    {
        passedTime+=delta;
        Position=new Vector2(offsetPos.x+(FloatRange.x*Mathf.Sin(passedTime*SinCosSpeed.x)),offsetPos.y+(FloatRange.y*Mathf.Cos(passedTime*SinCosSpeed.y)));
    }

}
