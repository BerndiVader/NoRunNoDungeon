using Godot;
using System;

public class ExplodeGfx : AnimatedSprite
{
    public bool useParticles=true;
    
    public override void _Ready()
    {
        if(useParticles)
        {
            ExplodeParticles particles=ResourceUtils.particles[(int)PARTICLES.EXPLODE].Instance<ExplodeParticles>();
            particles.Position=Position;
            World.level.AddChild(particles);
        }
        ZIndex=4;

        SetProcessInput(false);
        SetProcess(false);
        SetPhysicsProcess(false);        

        Connect("animation_finished",this,nameof(OnFinish));
        Play();
    }

    protected void OnFinish()
    {
        CallDeferred("queue_free");
    }
}
