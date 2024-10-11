using Godot;
using System;

public class FallingPlatform : Platform
{
    [Export] private int TimeSpan=20;
    private int time;
    private bool falling=false;
    private CollisionShape2D area2dShape;
    private float shake=2f,shakeMax=6f,speed=0f;

    public override void _Ready()
    {
        base._Ready();
        time=TimeSpan;
        area2dShape=GetNode<CollisionShape2D>("Area2D/CollisionShape2D2");
        Area2D area2D=GetNode<Area2D>("Area2D");

        area2D.Connect("body_entered",this,nameof(onBodyEntered));
    }

    public override void _PhysicsProcess(float delta)
    {
        if(falling)
        {
            if(time<0) 
            {
                speed=Mathf.Min(speed+=10f,200f);
                MoveLocalY(speed*delta,false);
            } 
            else 
            {
                applyShake();
            }
            time--;
        }
    }

    private void applyShake() 
    {
        shake=Math.Min(shake,shakeMax);
        Vector2 offset=new Vector2((float)MathUtils.randomRange(-shake,shake),(float)MathUtils.randomRange(-shake,shake));
        Position=startPosition+offset;
        shake*=0.9f;
    }

    private void onBodyEntered(Node body) 
    {
        if(!body.IsInGroup(GROUPS.PLAYERS.ToString())&&!body.IsInGroup(GROUPS.LEVEL.ToString())) return;
        if(!falling)
        {
            area2dShape.Position=new Vector2(area2dShape.Position.x,5f);
            falling=true;
        } 
        else if(time<0) 
        {
            delete();
        }
    }

    private void delete()
    {
        BlockParticles blockBreakParticle=(BlockParticles)ResourceUtils.particles[(int)PARTICLES.BLOCK].Instance();
        blockBreakParticle.Position=World.level.ToLocal(GlobalPosition);
        World.level.AddChild(blockBreakParticle);
        QueueFree();
    }

}