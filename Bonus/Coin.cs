using Godot;
using System;

public class Coin : Bonus
{
    public override void _Ready()
    {
        base._Ready();
        Connect("body_entered",this,nameof(onBodyEntered));
    }

    public void onBodyEntered(Node body) 
    {
        if(body.IsInGroup("Players")) 
        {
            CoinTakenParticles particles=(CoinTakenParticles)ResourceUtils.particles[1].Instance();
            particles.Position=Position;
            WorldUtils.world.level.AddChild(particles);
            CallDeferred("queue_free");
        }
    }

}
