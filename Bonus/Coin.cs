using Godot;
using System;

public class Coin : Bonus
{
    public override void _Ready()
    {
        base._Ready();
        Connect("body_entered",this,"enteredBody");
    }

    public void enteredBody(Node body) 
    {
        CoinTakenParticles particles=(CoinTakenParticles)ResourceUtils.particles[(int)PARTICLES.COINTAKENPARTICLES].Instance();
        particles.Position=WorldUtils.world.level.ToLocal(GlobalPosition);
        WorldUtils.world.level.AddChild(particles);
        CallDeferred("queue_free");
    }

}
