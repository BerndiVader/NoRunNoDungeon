using Godot;
using System;

public class Touch : Sprite
{
    Stick stick;

    public override void _Ready()
    {
        stick=GetNode("Stick") as Stick;
    }

    public Vector2 getValue()
    {
        return stick.getValue();
    }

}
