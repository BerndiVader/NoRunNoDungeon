using Godot;
using System;

public class JumpParticles : CPUParticles2D
{
    private AnimatedTexture animTex;

    public override void _Ready()
    {
        SetPhysicsProcess(true);
        SetProcess(false);
        SetProcessInput(false);

        animTex=(AnimatedTexture)Texture;
        animTex.CurrentFrame=0;
    }

    public override void _PhysicsProcess(float delta)
    {
        bool flip=Player.instance.AnimationController.FlipH;
        int frame=flip?1:0;
        if(animTex.CurrentFrame!=frame)
        {
            animTex.CurrentFrame=frame;
        }
    }

    public void Start(bool flipped)
    {
        animTex.CurrentFrame=flipped?1:0;
        Emitting=false;
        Emitting=true;
    }

    public void Stop()
    {
        Emitting=false;
    }

}
