using Godot;
using System;

public class JumpParticles : CPUParticles2D
{
    private AnimatedTexture animTex;

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        SetProcess(false);
        SetProcessInput(false);

        animTex=(AnimatedTexture)Texture;
        animTex.CurrentFrame=0;
    }

    public void Start(bool flipped)
    {
        animTex.CurrentFrame=flipped?1:0;
        Emitting=true;
    }

    public void Stop()
    {
        Emitting=false;
    }

}
