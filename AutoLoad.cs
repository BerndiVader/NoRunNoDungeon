using Godot;

public class AutoLoad : Node
{
    public override void _Ready()
    {
        foreach(string arg in OS.GetCmdlineArgs())
        {
            if(arg=="--headless")
			{
				QueueFree();
				GetTree().Quit();
				return;
			}
        }

        GameSettings.Init();
        GameSettings.current.SetAll(this);

        Worker.Start();
        ResourceUtils.Init();
        World.Init(GetTree().Root);
        QueueFree();
    }

}
