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
        notifier2D.Connect("screen_exited",this,nameof(onExitedScreen));
        AddChild(notifier2D);
        
        animationController=GetNode<AnimatedSprite>("AnimatedSprite");
        collisionController=GetNode<CollisionShape2D>("CollisionShape2D");
        animationController.Play("default");

    }

    void onExitedScreen()
    {
        QueueFree();
    }

}
