using Godot;
using System;

public class FallingPlatform : Platform
{
    [Export] public int TimeSpan=20;
    int time;
    bool falling=false;
    CollisionShape2D area2dShape;
    float shake=2f,shakeMax=6f,speed=0f;

    public override void _Ready()
    {
        base._Ready();
        time=TimeSpan;
        area2dShape=GetNode("Area2D/CollisionShape2D2") as CollisionShape2D;
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
        if(!body.IsInGroup(GROUPS.PLAYERS.ToString())&&!body.IsInGroup(GROUPS.LEVEL.ToString())) return;
        if(!falling) 
        {
            area2dShape.Position=new Vector2(area2dShape.Position.x,5f);
            falling=true;
        } 
        else if(time<0) 
        {
            if(body.IsInGroup(GROUPS.LEVEL.ToString())) 
            {
                    delete();
                    return;
            }

            Player player=(Player)body;
            if(player.GlobalPosition.y>GlobalPosition.y) 
            {
                delete();
            }
        }
    }

    void delete()
    {
        BlockParticles blockBreakParticle=(BlockParticles)ResourceUtils.particles[(int)PARTICLES.BLOCKPARTICLES].Instance();
        blockBreakParticle.Position=WorldUtils.world.level.ToLocal(GlobalPosition);
        WorldUtils.world.level.CallDeferred("add_child",blockBreakParticle);
        CallDeferred("queue_free");
    }

    public void _on_Area2D_body_exited(Node body) 
    {
        if(!body.IsInGroup(GROUPS.LEVEL.ToString())) return;
    }

}