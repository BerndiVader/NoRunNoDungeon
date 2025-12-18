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
		QUITTING,
	}
	private static State state;
	private delegate void Goal();
	private static Goal goal;
	private static int delay;
	private static bool quit;

	public static void Start()
	{
		instance=new Worker();
	}

	public Worker() : base()
	{
		placeholders=new ConcurrentStack<WeakReference>();
		SetStatus(State.IDLE);
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

	private static void InstantiatePlaceholder(Placeholder placeholder)
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

	private static void PrepareAndChangeLevel()
	{
		placeholders.Clear();
		World.instance.PrepareAndChangeLevel();
		SetStatus(State.IDLE);
		//Gc();
	}

	private static void Idle()
	{
		delay = 10;
		if (placeholders.TryPop(out WeakReference result) && result.IsAlive && result.Target is Placeholder p)
		{
			InstantiatePlaceholder(p);
			delay = 3;
		}
		OS.DelayMsec(delay);
	}
	
	private static void Quitting()
    {
		OS.DelayMsec(1);
    }

	public static void Stop()
	{
        SetStatus(State.QUITTING);
		quit=true;
		Console.Write("Wait for worker to finish...");
		instance.WaitToFinish();
		while(instance.IsActive())
		{
			Console.Write(".");
			OS.DelayMsec(1);
		}
		Console.WriteLine(" done!");		
	}

	public static void SetStatus(State s)
	{
		switch(s)
		{
			case State.PREPARELEVEL:
				goal=PrepareAndChangeLevel;
				break;
			case State.IDLE:
				goal=Idle;
				break;
			case State.QUITTING:
				goal = Quitting;
				break;
		}
		state=s;
	}

	public static async void Gc()
	{
		await Task.Run(delegate()
		{
			GC.Collect();
		});
	}
}
