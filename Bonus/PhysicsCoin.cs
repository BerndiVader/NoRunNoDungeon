using Godot;
using System;

public class PhysicsCoin : PhysicsObject
{
    AnimatedSprite animationController;
    Area2D area2D;

    public override void _Ready()
    {
        base._Ready();

        area2D=(Area2D)GetNode("Area2D");
        area2D.Connect("body_entered",this,nameof(onBodyEntered));

        animationController=(AnimatedSprite)GetNode("AnimatedSprite");
        animationController.Play("default");
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

    }

    public void onBodyEntered(Node body) 
    {
        CoinTakenParticles particles=(CoinTakenParticles)ResourceUtils.particles[(int)PARTICLES.COINTAKENPARTICLES].Instance();
        particles.Position=WorldUtils.world.level.ToLocal(GlobalPosition);
        WorldUtils.world.level.AddChild(particles);
        CallDeferred("queue_free");
    }

}
