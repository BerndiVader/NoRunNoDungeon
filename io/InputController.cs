using Godot;
using System;

public abstract class InputController
{
    public abstract bool Left();
    public abstract bool JustLeft();
    public abstract bool Right();
    public abstract bool JustRight();
    public abstract bool Up();
    public abstract bool JustUp();
    public abstract bool Down();
    public abstract bool JustDown();
    public abstract bool Jump();
    public abstract bool Attack();
    public abstract bool Change();
    public abstract bool Pause();
    public abstract bool Quit();
    public abstract bool JustAccept();
    public abstract void Free();
}
