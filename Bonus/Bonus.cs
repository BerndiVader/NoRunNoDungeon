using Godot;
using System;

public class Bonus : Area2D
{
    protected AnimatedSprite animationController;
    protected CollisionShape2D collisionController;
    protected VisibilityNotifier2D visibleNotifier;

    public override void _Ready()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);

        animationController=(AnimatedSprite)this.GetNode("AnimatedSprite");
        collisionController=(CollisionShape2D)this.GetNode("CollisionShape2D");
        animationController.Play("default");
        visibleNotifier=new VisibilityNotifier2D();
        CallDeferred("add_child",visibleNotifier);
        visibleNotifier.Connect("screen_entered",this,nameof(_onScreenEntered));
        visibleNotifier.Connect("screen_exited",this,nameof(_onScreenExited));
    }

    public void _onScreenEntered() 
    {
        SetProcess(true);
        SetPhysicsProcess(true);
    }

    public void _onScreenExited() 
    {
        SetProcess(false);
        SetPhysicsProcess(false);
        CallDeferred("queue_free");
    }

}
