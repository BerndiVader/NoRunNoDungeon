using Godot;
using System;

public class BossTestTree : WalkingTree,IFraming
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

    public void Framing()
    {
        PlayerCamera.instance.Framing(this);
    }

    public override void _ExitTree()
    {
        if(IsInstanceValid(PlayerCamera.instance))
        {
            PlayerCamera.instance.StopFraming(this);
        }
        base._ExitTree();
    }
}
