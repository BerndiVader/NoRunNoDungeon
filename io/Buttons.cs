using Godot;
using System;

public class Buttons : Node2D
{
    public XButton jump,x,o,c;
    public override void _Ready()
    {
        jump=(XButton)GetNode("A");
        x=(XButton)GetNode("X");
        o=(XButton)GetNode("O");
        c=(XButton)GetNode("0");
        SetPhysicsProcess(false);
        SetProcessInput(false);
        SetProcess(false);
    }

}
