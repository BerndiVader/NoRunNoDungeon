using Godot;
using System;

public class Shopkeeper : KinematicMonster
{

    public override void _Ready()
    {
        base._Ready();

		SetSpawnFacing();
        OnIdle();
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
		goal(delta);
    }

    protected override void Idle(float delta)
    {
        Navigation(delta);
    }

    protected override void OnDamage(Node2D node=null,float amount=0f)
    {
        if(state!=STATE.damage&&state!=STATE.die)
        {
            return;
        }

    }



}
