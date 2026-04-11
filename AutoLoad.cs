using System.Linq;
using Godot;

public class AutoLoad : Node
{
    public override void _Ready()
    {

        if(OS.GetCmdlineArgs().Contains<string>("--headless"))
        {
            QueueFree();
            GetTree().Quit();
        }
        else
        {
            GameSettings.Init();
            GameSettings.current.SetAll(this);

            Worker.Start();
            ResourceUtils.Init();
            World.Init(GetTree().Root);
            QueueFree();
        }

    }

}
