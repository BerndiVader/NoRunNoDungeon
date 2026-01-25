using Godot;
using System;

public class DirtyZombie : KinematicMonster
{
    private RayCast2D rayCast2D;
    private bool justJumped=false;
    
    public override void _Ready()
    {
        base._Ready();

        animationPlayer=GetNode<AnimationPlayer>(nameof(AnimationPlayer));
        animationPlayer.Connect("animation_started",this,nameof(OnAnimationPlayerStarts));
        animationPlayer.Connect("animation_finished",this,nameof(OnAnimationPlayerEnded));

        rayCast2D=GetNode<RayCast2D>(nameof(RayCast2D));
        rayCast2D.Enabled=true;

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
        velocity+=FORCE*delta;
        velocity=MoveAndSlideWithSnap(velocity,justJumped?Vector2.Zero:snap,Vector2.Up,false,4,0.785398f,true);
        justJumped=false;

        int slides=GetSlideCount();
        if(slides>0)
        {
            for(int i=0;i<slides;i++)
            {
                if(GetSlideCollision(i).Collider is Platform platform)
                {
                    velocity.x=platform.CurrentSpeed.x;
                }
                else
                {
                    velocity=StopX(velocity,delta);
                }
            }
        }
        else
        {
            velocity=StopX(velocity,delta);
        }
    }

    protected override void Passanger(float delta)
    {
        if(!animationPlayer.IsPlaying())
        {
            base.Passanger(delta);
        }
    }

    protected override void OnDamage(Node2D node=null,float amount=0)
    {
        if(state!=STATE.damage&&state!=STATE.die)
        {
            base.OnDamage(node,amount);
            if(GlobalPosition.x-node.GlobalPosition.x<0)
            {
                animationDirection=-1;
            }
            justJumped=true;
            velocity+=new Vector2(200f*animationDirection,-50f);
            animationPlayer.Play("HIT");
        }        
    }


    protected override void FlipH()
    {
		animationController.FlipH^=true;
		collisionController.Position=FlipX(collisionController.Position);
        staticBody.Position=FlipX(staticBody.Position);
        rayCast2D.Position=FlipX(rayCast2D.Position);
        rayCast2D.CastTo=FlipX(rayCast2D.CastTo);
		facing=Facing();
    }

}
