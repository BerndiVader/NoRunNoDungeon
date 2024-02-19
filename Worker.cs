using Godot;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class Worker : Thread
{
    public static Worker instance;
	public static ConcurrentStack<WeakReference>placeholders;
	public enum State
	{
		IDLE,
		PREPARELEVEL,
	}
	private static State state;
	private delegate void Goal();
	private static Goal goal;
	private static int delay;
	private static bool quit;

	public Worker() : base()
	{
		instance=this;
		placeholders=new ConcurrentStack<WeakReference>();
		setStatus(State.IDLE);
		quit=false;
		Start(this,nameof(Runner));
	}

	private void Runner()
	{
		while(!quit)
		{
			goal();
		}
	}

	private static void instantiatePlaceholder(Placeholder placeholder)
	{
		try
		{
			if(!placeholder.isDisposed&&placeholder.IsInsideTree())
			{
				InstancePlaceholder iPlaceholder=placeholder.GetChild<InstancePlaceholder>(0);
				string instancePath=iPlaceholder.GetInstancePath();
				if(!ResourceLoader.HasCached(instancePath))
				{
					ResourceLoader.Load(instancePath);
				}
				placeholder.CallDeferred("remove_child",iPlaceholder);
				iPlaceholder.Set("position",placeholder.Position);
				World.level.CallDeferred("add_child",iPlaceholder);
				iPlaceholder.CallDeferred("create_instance",false);
				iPlaceholder.CallDeferred("queue_free");
			}
			placeholder.CallDeferred("queue_free");
		}
		catch(Exception e)
		{
			Console.WriteLine(e.GetType().Name);
		}
	}

	private static void prepareAndChangeLevel()
	{
		placeholders.Clear();
		World.instance.prepareAndChangeLevel();
		setStatus(State.IDLE);
		gc();
	}

	private static void idle()
	{
		delay=10;
		if(placeholders.TryPop(out WeakReference result))
		{
			if(result.IsAlive)
			{
				Placeholder p=(Placeholder)result.Target;
				instantiatePlaceholder(p);
				delay=3;
			}
		}
		OS.DelayMsec(delay);
	}

	public static void stop()
	{
		Console.Write("Wait for worker to finish...");
		quit=true;
		instance.WaitToFinish();
		while(instance.IsActive())
		{
			Console.Write(".");
			OS.DelayMsec(1);
		}
		Console.WriteLine(" done!");		
	}

	public static void setStatus(State s)
	{
		switch(s)
		{
			case State.PREPARELEVEL:
				goal=prepareAndChangeLevel;
				break;
			case State.IDLE:
				goal=idle;
				break;
		}
		state=s;
	}

	public static async void gc()
	{
		await Task.Run(delegate()
		{
			GC.Collect();
		});
	}
}
