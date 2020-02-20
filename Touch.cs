using Godot;
using System;

public class Touch : Sprite
{
    Stick stick;

    public override void _Ready()
    {
        stick=(Stick)GetNode("Stick");
    }

    public Vector2 getValue()
    {
        return stick.getValue();
    }

}
