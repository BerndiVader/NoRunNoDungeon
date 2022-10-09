using Godot;
using System;

public class PhysicsCoin : PhysicsObject
{
    public override void _Ready()
    {
        base._Ready();

        GetNode<Area2D>("Area2D").Connect("body_entered",this,nameof(onBodyEntered));
        GetNode<AnimatedSprite>("AnimatedSprite").Play("default");
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

    }

    public void onBodyEntered(Node body) 
    {
        CoinTakenParticles particles=(CoinTakenParticles)ResourceUtils.particles[(int)PARTICLES.COINTAKENPARTICLES].Instance();
        particles.Position=World.instance.level.ToLocal(GlobalPosition);
        World.instance.level.AddChild(particles);
        QueueFree();
    }

}
