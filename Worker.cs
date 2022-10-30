using Godot;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class Worker : Thread
{
    public static Worker instance;
	public static ConcurrentStack<WeakReference>placeholders;
	public static bool stop;
	public enum Status
	{
		IDLE=0,
		PREPARELEVEL=1,
	}
	public static Status status;

	public Worker() : base()
	{
		instance=this;
		stop=false;
		status=Status.IDLE;
		placeholders=new ConcurrentStack<WeakReference>();
		Start(this,nameof(Runner));
	}

	private void Runner(bool run)
	{
		while(!stop)
		{
			switch(status)
			{
				case Status.PREPARELEVEL:
				{
					placeholders.Clear();
					World.instance.prepareLevel();
					status=Status.IDLE;
					gc();
					break;
				}
				case Status.IDLE:
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
					break;
				}
			}
			OS.DelayMsec(5);
		}
	}

	private void instantiatePlaceholder(Placeholder placeholder,InstancePlaceholder iPlaceholder)
	{
		if(placeholder.IsInsideTree())
		{
			placeholder.CallDeferred("remove_child",iPlaceholder);
			iPlaceholder.Set("position",World.instance.level.ToLocal(placeholder.GlobalPosition));
			World.instance.level.CallDeferred("add_child",iPlaceholder);
			iPlaceholder.CallDeferred("create_instance",true);
		}
		iPlaceholder.CallDeferred("queue_free");
		placeholder.CallDeferred("queue_free");
	}

	public async void gc()
	{
		await Task.Run(delegate()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();			
		});
	}	
}
