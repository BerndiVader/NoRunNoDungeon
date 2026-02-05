using Godot;
using System;

public class ExplodeGfx : AnimatedSprite
{
    public bool useParticles=true;
    protected int animationLength;
    
    public override void _Ready()
    {
        if(useParticles)
        {
            ExplodeParticles particles=ResourceUtils.particles[(int)PARTICLES.EXPLODE].Instance<ExplodeParticles>();
            particles.Position=Position;
            World.level.AddChild(particles);
        }
        ZIndex=4;
        animationLength=Frames.GetFrameCount(Animation)-1;
        Play();
    }

    public override void _PhysicsProcess(float delta)
    {
        if(Frame==animationLength)
        {
            QueueFree();
            SetPhysicsProcess(false);
        }
    }
}
