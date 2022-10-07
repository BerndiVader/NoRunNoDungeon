using Godot;
using System;
using System.Collections.Generic;

public class Worker : Godot.Thread
{
	public static Worker instance;
	public static Queue<Placeholder>placeholderQueue;
	public static bool prepareLevel,stop;

	public Worker() : base()
	{
		instance=this;
		placeholderQueue=new Queue<Placeholder>();
		prepareLevel=false;
		Start(this,nameof(Runner));
	}

	void Runner(bool run)
	{
		try {
			while(!stop)
			{
				if(prepareLevel)
				{
					prepareLevel=false;
					World.instance.prepareLevel();
				}
				else if(placeholderQueue.Count>0)
				{
					Placeholder p=placeholderQueue.Dequeue() as Placeholder;
					String instancePath=p.placeholder.GetInstancePath();
					ResourceLoader.Load(instancePath);
					p.instantiated=true;
				}
				OS.DelayMsec(5);
			}
		} catch (Exception ex) {
			GD.Print(ex.Message);
		}
	}
}
