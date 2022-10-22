using Godot;
using System;

public class Touch : Sprite
{
    private Stick stick;

    public override void _Ready()
    {
        stick=GetNode<Stick>("Stick");
    }

    public Vector2 getValue()
    {
        return stick.getValue();
    }

}
