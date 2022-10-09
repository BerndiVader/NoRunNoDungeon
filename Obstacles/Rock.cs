using Godot;
using System;

public class Rock : PhysicsObject
{
    public override void _PhysicsProcess(float delta)
    {
        Vector2 force=new Vector2(0,GRAVITY);

        velocity+=GetFloorVelocity()*delta;
        velocity+=force*delta;

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
