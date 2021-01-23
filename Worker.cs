using Godot;
using System;
using System.Collections.Generic;

public class Worker : Godot.Thread
{
    public Queue<Placeholder> placeholderQueue;
    ResourceInteractiveLoader loader;
    public bool prepareLevel,stop;

    public Worker() : base()
    {
        placeholderQueue=new Queue<Placeholder>();
        prepareLevel=false;
        Start(this,nameof(Runner));
    }

    void Runner(bool run)
    {
        while(!stop)
        {
            OS.DelayMsec(5);
            try
            {
                if(prepareLevel)
                {
                    prepareLevel=false;
                    WorldUtils.world.prepareLevel();
                } 
                else if(placeholderQueue.Count>0)
                {
                    Placeholder p=placeholderQueue.Dequeue() as Placeholder;
                    String instancePath=p.placeholder.GetInstancePath();
                    if(!ResourceLoader.HasCached(instancePath))
                    {
                        ResourceLoader.Load(instancePath);
                    }
                    p.instantiated=true;
                }
            }
            catch (Exception e)
            {
                GD.Print(e.Message);
            }
        }
    }
}
