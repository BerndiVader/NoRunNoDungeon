using Godot;
using System;

public class Rock : PhysicsObject
{
    private StaticBody2D collider;
    public override void _Ready()
    {
        base._Ready();
        collider=GetNode<StaticBody2D>("StaticBody2D");
        AddToGroup(GROUPS.OBSTACLES.ToString());
        collider.AddToGroup(GROUPS.OBSTACLES.ToString());

    }
    public override void _PhysicsProcess(float delta)
    {
        velocity+=GRAVITY*delta;

        KinematicCollision2D collision=MoveAndCollide(velocity*delta);

        if(collision!=null) 
        {
            Node2D node=(Node2D)collision.Collider;
            velocity=velocity.Bounce(collision.Normal);

            if(node.IsInGroup(GROUPS.PLATFORMS.ToString()))
            {
                Platform collider=(Platform)node;
                velocity.x+=collider.CurrentSpeed.x*0.5f;
            }
        }
    }

}
