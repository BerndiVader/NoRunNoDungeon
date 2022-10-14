using Godot;
using System;

public class Coin : Bonus
{
    public override void _Ready()
    {
        base._Ready();
        Connect("body_entered",this,nameof(onEnteredBody));
    }

    public void onEnteredBody(Node body) 
    {
        if(body.Name.Equals("Player"))
        {
            CoinTakenParticles particles=(CoinTakenParticles)ResourceUtils.particles[(int)PARTICLES.COINTAKENPARTICLES].Instance();
            particles.Position=World.instance.level.ToLocal(GlobalPosition);
            World.instance.level.AddChild(particles);
            QueueFree();
        }
    }

}
