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
        animationController=(AnimatedSprite)GetNode("AnimatedSprite");

        area2D.Connect("body_entered",this,nameof(onBodyEntered));
        animationController.Play("default");
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

    }

    public void onBodyEntered(Node body) 
    {
        if(body.IsInGroup("Players")) 
        {
            CoinTakenParticles particles=(CoinTakenParticles)ResourceUtils.particles[1].Instance();
            particles.Position=parent.Position+Position;
            WorldUtils.world.level.AddChild(particles);
            parent.CallDeferred("queue_free");
        }
    }

}
