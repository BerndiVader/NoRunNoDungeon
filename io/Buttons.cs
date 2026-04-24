using Godot;
using System;

public class Buttons : Node2D
{
    public XButton jump,attack,change,accept;
    public override void _Ready()
    {
        jump=(XButton)GetNode("A");
        attack=(XButton)GetNode("0");
        change=(XButton)GetNode("X");
        accept=(XButton)GetNode("O");
        SetPhysicsProcess(false);
        SetProcessInput(false);
        SetProcess(false);
    }

}
