using Godot;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class Worker : Godot.Thread
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

	private void instantiatePlaceholder(Placeholder p,InstancePlaceholder placeholder)
	{
		p.CallDeferred("remove_child",placeholder);
		placeholder.Set("position",World.instance.level.ToLocal(p.GlobalPosition));
		World.instance.level.CallDeferred("add_child",placeholder);
		placeholder.CallDeferred("replace_by_instance");
		p.CallDeferred("queue_free");
	}

	public async void gc()
	{
		await Task.Run(() => 
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();			
		});
	}	
}
