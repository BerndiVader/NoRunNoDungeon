using Godot;
using System;

public class Placeholder : Node2D
{
    public VisibilityNotifier2D notifier2D;
    InstancePlaceholder placeholder;
    public Node2D instance;
    bool instantiated;

    public override void _Ready()
    {
        instantiated=false;

        SetPhysicsProcess(false);
        SetProcessInput(false);
        SetProcess(true);

        notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_entered",this,nameof(enteredScreen));
        AddChild(notifier2D);

        placeholder=(InstancePlaceholder)GetChild(0);
    }

    public override void _Process(float delta)
    {
        if(instantiated)
        {
            SetProcess(false);
            instance=(Node2D)placeholder.CreateInstance(true);
        }
    }

    public void enteredScreen()
    {
        if(placeholder==null)
        {
            placeholder=(InstancePlaceholder)GetChild(0);
        }

        System.Threading.Thread thread=new System.Threading.Thread(()=>instantiatePlaceholder(this));
        thread.Start();

    }

    void exitedScreen()
    {
        CallDeferred("queue_free");
    }

    public static void instantiatePlaceholder(Placeholder p)
    {
        String instancePath=p.placeholder.GetInstancePath();
        if(!ResourceLoader.HasCached(instancePath))
        {
            ResourceLoader.Load(instancePath);
        }
        p.instantiated=true;
    }

}
