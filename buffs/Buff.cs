using Godot;
using System;

public abstract class Buff : Sprite
{

    protected WeakReference<Buff>weakRef;

    public abstract void Replace(Buff buff);
    public abstract void Apply();

}
