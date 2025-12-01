using Godot;
using System;

public class RunningZombieHurting : RunningZombie
{
    private RayCast2D playerCast;
    private Area2D area;

    public override void _Ready()
    {
        base._Ready();
        playerCast=GetNode<RayCast2D>("PlayerCast2D");
        playerCast.Enabled=true;
        area=GetNode<Area2D>(nameof(Area2D));
        area.Monitoring=false;
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        if(playerCast.Enabled&&playerCast.IsColliding()&&playerCast.GetCollider() is Player)
        {
            onAttack();
        }

    }

    protected override void onAttack(Player player = null)
    {
        if(state!=STATE.attack)
        {
            base.onAttack(player);
            playerCast.Enabled=false;
            area.Connect("body_entered",this,nameof(body_entered));
            animationController.Frames.SetAnimationLoop(nameof(attack),false);
            animationController.Play(nameof(attack));
        }
    }

    protected override void attack(float delta)
    {
        if(animationController.Frame==10)
        {
            onDie();
        }
        else if(animationController.Frame==3)
        {
            area.Monitoring=true;
            COOLDOWNER_TIME=0.1f;
            cooldowner=0f;
        }
        else
        {
            if(cooldowner<=0f&&area.Monitoring)
            {
                area.Scale*=1.6f;
                animationController.Scale*=1.4f;
            }
        }

        if(cooldowner<0f)
        {
            cooldowner=COOLDOWNER_TIME;

        }
        cooldowner-=delta;        
    }

    protected override void FlipH()
    {
        base.FlipH();
		playerCast.Position=FlipX(playerCast.Position);
        playerCast.CastTo=FlipX(playerCast.CastTo);
    }

    public void body_entered(Node node)
    {
        if(node is Player)
        {
            node.EmitSignal("damage",this,1f);
            area.SetDeferred("monitoring",false);
        }
    }

}
