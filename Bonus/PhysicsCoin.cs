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

    public void onBodyEntered(Node body) 
    {
        if(body.Name.Equals("Player"))
        {
            CoinTakenParticles particles=(CoinTakenParticles)ResourceUtils.particles[(int)PARTICLES.COINTAKEN].Instance();
            particles.Position=World.level.ToLocal(GlobalPosition);
            World.level.AddChild(particles);
            QueueFree();
        }
    }

}
