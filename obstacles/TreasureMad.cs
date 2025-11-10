using Godot;
using System;

public class TreasureMad : PhysicsObject
{
    private StaticBody2D collider;
    public override void _Ready()
    {
        base._Ready();

        GetNode<AnimatedSprite>(nameof(AnimatedSprite)).Playing = true; 

        collider = GetNode<StaticBody2D>(nameof(StaticBody2D));
        AddToGroup(GROUPS.OBSTACLES.ToString());
        collider.AddToGroup(GROUPS.OBSTACLES.ToString());

    }
    public override void _PhysicsProcess(float delta)
    {
        velocity+=GRAVITY*delta;

        KinematicCollision2D collision=MoveAndCollide(velocity*delta);

        if(collision!=null) 
        {
            Node2D node = (Node2D)collision.Collider;
            velocity=velocity.Bounce(Vector2.Up);

            if(node.IsInGroup(GROUPS.PLATFORMS.ToString()))
            {
                Platform collider=(Platform)node;
                velocity.x+=collider.CurrentSpeed.x*0.5f;
            }
        }
    }

}
