using Godot;
using System;

public class JumpParticles : CPUParticles2D
{
    AnimatedTexture animTex;

    public override void _Ready()
    {
        animTex=(AnimatedTexture)Texture;
        animTex.CurrentFrame=0;
    }

    public override void _PhysicsProcess(float delta)
    {
        animTex.CurrentFrame=Player.instance.animationController.FlipH?1:0;
    }

}
