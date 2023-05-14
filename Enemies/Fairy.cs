using Godot;
using System;

public class Fairy : KinematicMonster
{
    private float passedTime;
    private Vector2 offsetPos,lastPos;
    [Export]private Vector2 SinCosSpeed=new Vector2(5f,3.5f);
    [Export]private Vector2 FloatRange=new Vector2(15f,5f);

    public override void _Ready()
    {
        base._Ready();
        passedTime=0f;
        SetProcess(true);
        SetPhysicsProcess(false);
        state=STATE.unknown;
        EmitSignal(STATE.idle.ToString());
    }

    public override void _Process(float delta)
    {
        lastPos=new Vector2(Position);
        goal(delta);
    }

    protected override void idle(float delta)
    {
        fly(delta);
        animationController.FlipH=Position.x>lastPos.x;
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
