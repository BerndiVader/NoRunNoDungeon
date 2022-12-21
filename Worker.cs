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
	private static Status status;
	private delegate void Goal();
	private static Goal goal;

	public Worker() : base()
	{
		instance=this;
		stop=false;
		setStatus(Status.IDLE);
		placeholders=new ConcurrentStack<WeakReference>();
		Start(this,nameof(Runner));
	}

	private void Runner(bool run)
	{
		while(!stop)
		{
			goal();
			OS.DelayMsec(5);
		}
	}

	private static void instantiatePlaceholder(Placeholder placeholder,InstancePlaceholder iPlaceholder)
	{
		if(placeholder.IsInsideTree())
		{
			placeholder.CallDeferred("remove_child",iPlaceholder);
			iPlaceholder.Set("position",World.level.ToLocal(placeholder.GlobalPosition));
			World.level.CallDeferred("add_child",iPlaceholder);
			iPlaceholder.CallDeferred("create_instance",true);
		}
		iPlaceholder.CallDeferred("queue_free");
		placeholder.CallDeferred("queue_free");
	}

	private static void prepareLevel()
	{
		placeholders.Clear();
		World.instance.prepareLevel();
		setStatus(Status.IDLE);
		gc();
	}

	private static void idle()
	{
		if(placeholders.TryPop(out WeakReference result))
		{
			if(result.IsAlive)
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
	}

	public static void setStatus(Status s)
	{
		status=s;
		switch(status)
		{
			case Status.PREPARELEVEL:
				goal=prepareLevel;
				break;
			case Status.IDLE:
				goal=idle;
				break;
		}
	}

	public static async void gc()
	{
		await Task.Run(delegate()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();			
		});
	}	
}
