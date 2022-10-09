using Godot;
using System;
using System.Collections.Concurrent;

public class Worker : Godot.Thread
{
	public static Worker instance;
	public static ConcurrentStack<WeakReference>placeholders;
	public static bool prepareLevel,stop;

	public Worker() : base()
	{
		instance=this;
		placeholders=new ConcurrentStack<WeakReference>();
		prepareLevel=false;
		Start(this,nameof(Runner));
	}

	private void Runner(bool run)
	{
		while(!stop)
		{
			if(prepareLevel)
			{
				placeholders.Clear();
				World.instance.prepareLevel();
				prepareLevel=false;
			}
			else if(placeholders.Count>0)
			{
				if(placeholders.TryPop(out WeakReference result))
				{
					Placeholder p=(Placeholder)result.Target;
					InstancePlaceholder placeholder=p.GetChild<InstancePlaceholder>(0);
					String instancePath=placeholder.GetInstancePath();
					if(!ResourceLoader.HasCached(instancePath))
					{
						ResourceLoader.Load(instancePath);
					}
					instantiatePlaceholder(p,placeholder);
				}
			}
			OS.DelayMsec(5);
		}
	}

	private void instantiatePlaceholder(Placeholder p,InstancePlaceholder placeholder)
	{
		p.CallDeferred("remove_child",placeholder);
		World.instance.level.CallDeferred("add_child",placeholder);
		placeholder.Set("position",World.instance.level.ToLocal(p.GlobalPosition));
		placeholder.CallDeferred("create_instance",false);
		placeholder.CallDeferred("queue_free");
		p.CallDeferred("queue_free");
	}
}
