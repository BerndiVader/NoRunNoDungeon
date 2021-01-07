using Godot;
using System;

public class PlaceholderScanner : Area2D
{
    public override void _Ready()
    {
        Connect("body_entered",this,nameof(onBodyEntered));
    }

    void onBodyEntered(Node node)
    {
        GD.Print(node.Name);
    }

}
