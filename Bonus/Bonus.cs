using Godot;
using System;

public class Bonus : Area2D
{
    protected AnimatedSprite animationController;
    protected CollisionShape2D collisionController;
    protected Placeholder parent;
    protected VisibilityNotifier2D notifier2D;

    public override void _Ready()
    {
        if(GetParent().GetType().Name=="Placeholder")
        {
            parent=(Placeholder)GetParent();
            parent.notifier2D.Connect("screen_exited",this,"exitedScreen");
        }
        else
        {
            notifier2D=new VisibilityNotifier2D();
            AddChild(notifier2D);
            notifier2D.Connect("screen_exited",this,"exitedScreen");
        }
        
        animationController=(AnimatedSprite)this.GetNode("AnimatedSprite");
        collisionController=(CollisionShape2D)this.GetNode("CollisionShape2D");
        animationController.Play("default");

    }

    public Vector2 getPosition()
    {
        return parent!=null?parent.Position+Position:Position;
    }

    public void _Free()
    {
        if(parent!=null)
        {
            parent.CallDeferred("queue_free");
        }
        else
        {
            CallDeferred("queue_free");
        }
    }

    void exitedScreen()
    {
        if(parent!=null)
        {
            parent.CallDeferred("queue_free");
        }
        else
        {
            CallDeferred("queue_free");
        }
    }

}
