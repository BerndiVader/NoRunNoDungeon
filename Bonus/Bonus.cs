using Godot;
using System;

public class Bonus : Area2D
{
    protected AnimatedSprite animationController;
    protected CollisionShape2D collisionController;
    protected VisibilityNotifier2D notifier2D;

    protected Node2D parent;

    public override void _Ready()
    {
        parent=(Node2D)GetParent();
        
        animationController=(AnimatedSprite)this.GetNode("AnimatedSprite");
        collisionController=(CollisionShape2D)this.GetNode("CollisionShape2D");
        animationController.Play("default");

        notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",parent,"exitedScreen");
        AddChild(notifier2D);
    }

}
