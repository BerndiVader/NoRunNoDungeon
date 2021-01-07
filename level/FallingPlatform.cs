using Godot;
using System;

public class FallingPlatform : Platform
{

    [Export] public int TimeSpan=20;
    int time;

    Area2D area2D;
    CollisionShape2D area2dShape;

    float shake=2f,shakeMax=6f,speed=0f;

    public override void _Ready()
    {
        base._Ready();
        time=TimeSpan;
        area2D=(Area2D)GetNode("Area2D");
        area2dShape=(CollisionShape2D)area2D.GetNode("CollisionShape2D2");
        Connect("tree_exiting",this,nameof(_exitingTree));
    }

    public override void _PhysicsProcess(float delta)
    {
        if(falling)
        {
            if(time<=0) 
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

    void applyShake() 
    {
        shake=Math.Min(shake,shakeMax);
        Vector2 offset=new Vector2(0,0);
        offset.x=(float)MathUtils.randomRange(-shake,shake);
        offset.y=(float)MathUtils.randomRange(-shake,shake);
        Position=startPosition+offset;
        shake*=0.9f;
    }

    public void _on_Area2D_body_entered(Node body) 
    {
        if(!body.IsInGroup("Players")&&!body.IsInGroup("Level")) return;
        if(!falling) 
        {
            Vector2 position=new Vector2(area2dShape.Position);
            position.y=5;
            area2dShape.Position=position;
            falling=true;
        } 
        else if(time<0) 
        {
            if(body.IsInGroup("Level")) 
            {
                    BlockParticles blockBreakParticle=(BlockParticles)ResourceUtils.particles[(int)PARTICLES.BLOCKPARTICLES].Instance();
                    blockBreakParticle.Position=getPosition();
                    WorldUtils.world.level.CallDeferred("add_child",blockBreakParticle);
                    _Free();
                    return;
            }

            Player player=(Player)body;
            if(player.Position.y>Position.y) 
            {
                BlockParticles blockBreakParticle=(BlockParticles)ResourceUtils.particles[(int)PARTICLES.BLOCKPARTICLES].Instance();
                blockBreakParticle.Position=getPosition();
                WorldUtils.world.level.CallDeferred("add_child",blockBreakParticle);
                _Free();
            }
        }
    }

    public void _on_Area2D_body_exited(Node body) 
    {
        if(!body.IsInGroup("Players")) return;
    }

    public void _exitingTree()
    {
        area2D.Monitoring=false;
    }
}