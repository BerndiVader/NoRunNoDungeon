using Godot;
using System;

public class RunningZombieHurting : RunningZombie
{
    private RayCast2D playerCast;
    private Area2D area;

    public override void _Ready()
    {
        playerCast=GetNode<RayCast2D>("PlayerCast2D");
        playerCast.Enabled=true;
        area=GetNode<Area2D>(nameof(Area2D));
        area.Monitoring=false;
        base._Ready();
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        if(playerCast.Enabled&&playerCast.IsColliding()&&playerCast.GetCollider() is Player)
        {
            OnAttack();
        }

    }

    protected override void OnAttack(Player player=null)
    {
        if(state!=STATE.attack)
        {
            base.OnAttack(player);
            playerCast.Enabled=false;
            area.Connect("body_entered",this,nameof(BodyEntered));
            animationController.Frames.SetAnimationLoop("attack",false);
            animationController.Play("attack");
        }
    }

    protected override void Attack(float delta)
    {
        if(animationController.Frame==10)
        {
            OnDie();
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
        Navigation(delta);
    }

    protected override void FlipH()
    {
        base.FlipH();
		playerCast.Position=FlipX(playerCast.Position);
        playerCast.CastTo=FlipX(playerCast.CastTo);
    }

    public void BodyEntered(Node node)
    {
        if(state==STATE.attack&&node is Player)
        {
            node.EmitSignal("damage",this,1f);
            area.SetDeferred("monitoring",false);
        }
    }

}
