using Godot;
using System;

public class BossTestTree : WalkingTree
{
    public override void _PhysicsProcess(float delta)
    {
        goal(delta);
        Framing();
    }

    protected override void OnPassanger(Player player=null)
    {
        if(state!=STATE.passanger)
        {
			base.OnDamage(player,1f);
            animationPlayer.Play("PASSANGER");
        }
    }
    
}
