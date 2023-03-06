using Godot;
using System;

public abstract class InputController
{
    public abstract bool getLeft();
    public abstract bool getJustLeft();
    public abstract bool getRight();
    public abstract bool getJustRight();
    public abstract bool getUp();
    public abstract bool getJustUp();
    public abstract bool getDown();
    public abstract bool getJustDown();
    public abstract bool getJump();
    public abstract bool getAttack();
    public abstract bool getChange();
    public abstract bool getPause();
    public abstract bool getQuit();
    public abstract bool getJustAccept();
    public abstract void _free();
}
