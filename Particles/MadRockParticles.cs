using Godot;
using System;

public class MadRockParticles : Particles
{
    public override void _Ready()
    {
        SetProcess(false);
    }

    public override void _Process(float delta) 
    {
        if(!Emitting) 
        {
            SetProcess(false);
            QueueFree();
        }
    }

    public override void _EnterTree()
    {
        Emitting=false;
        OneShot=true;
        Connect("tree_entered",this,nameof(onTreeEntered));
        base._EnterTree();
    }

    private void onTreeEntered()
    {
        Emitting=true;
        SetProcess(true);
    }

}
