using Godot;
using System;

public class ExplodeGfx : AnimatedSprite
{
    protected int animationLength;
    public override void _Ready()
    {
        ExplodeParticles particles=ResourceUtils.particles[(int)PARTICLES.EXPLODE].Instance<ExplodeParticles>();
        particles.Position=Position;
        World.level.AddChild(particles);
        ZIndex=4;
        animationLength=this.Frames.GetFrameCount(this.Animation)-1;
        Play();
    }

    public override void _PhysicsProcess(float delta)
    {
        if(Frame==animationLength)
        {
            CallDeferred("queue_free");
            SetPhysicsProcess(false);
        }
    }
}
