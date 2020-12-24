using Godot;
using System;

public abstract class InputController
{
    public abstract bool getLeft();
    public abstract bool getRight();
    public abstract bool getUp();
    public abstract bool getDown();
    public abstract bool getJump();
    public abstract bool getAttack();
    public abstract bool getPause();
    public abstract bool getQuit();
    public abstract void _free();
}
