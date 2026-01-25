using Godot;
using System;

public class DirtyZombie : KinematicMonster
{
    private RayCast2D rayCast2D;
    
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

        if(animationPlayer.IsPlaying())
        {
            Position=startOffset+(ANIMATION_OFFSET*animationDirection);
        }

        goal(delta);
    }

    protected override void Navigation(float delta)
    {
        velocity+=FORCE*delta;
        velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);

        int slides = GetSlideCount();
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

    protected override void Idle(float delta)
    {
        Navigation(delta);
    }


    protected override void Passanger(float delta)
    {
        if(!animationPlayer.IsPlaying())
        {
            base.Passanger(delta);
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
