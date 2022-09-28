using Godot;
using System;

public class Bonus : Area2D
{
    protected AnimatedSprite animationController;
    protected CollisionShape2D collisionController;
    protected VisibilityNotifier2D notifier2D;

    public override void _Ready()
    {
        notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",this,"exitedScreen");
        AddChild(notifier2D);
        
        animationController=(AnimatedSprite)this.GetNode("AnimatedSprite");
        collisionController=(CollisionShape2D)this.GetNode("CollisionShape2D");
        animationController.Play("default");

    }

    void exitedScreen()
    {
        CallDeferred("queue_free");
    }

}
