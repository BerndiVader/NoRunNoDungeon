using Godot;
using System;

public class Intro : Node
{
	private static InputController input;
	private static readonly AudioStreamMP3 MUSIC=ResourceLoader.Load<AudioStreamMP3>("res://sounds/title/Dark Age 11 Heroic Victory - 1.mp3");
	private readonly AudioStreamPlayer2D MUSIC_PLAYER=new AudioStreamPlayer2D();
	private bool onOptions=false;
	
	public override void _Ready()
	{
		MUSIC_PLAYER.Bus="Master";
		MUSIC_PLAYER.Stream=MUSIC;
		MUSIC_PLAYER.Position=new Vector2(256f,146f);
		AddChild(MUSIC_PLAYER);
		MUSIC_PLAYER.Play();

		AddChild(ResourceUtils.camera.Instance<PlayerCamera>());		
		input=ResourceUtils.GetInputController(this);
		GetTree().CurrentScene=this;
		GetTree().Paused=false;

	}

	public override void _PhysicsProcess(float delta)
	{
		if(onOptions)
        {
			if(GetNodeOrNull<OptionsUI>("Options")!=null||GetNodeOrNull<InstructionsUI>("Instructions")!=null)
			{
            	return;
            }
			GetNode<RichTextLabel>(nameof(RichTextLabel)).Visible=true;
			onOptions=false;
			input=ResourceUtils.GetInputController(this);
        }

		if(input.Jump())
		{
			input.Free();
			World.ChangeScene(ResourceUtils.world);
		}
		else if(input.Change())
        {
			input.Free();
			OptionsUI options=BaseUI.OptionsPack.Instance<OptionsUI>();
			options.Name="Options";
			AddChild(options);
			onOptions=true;
			GetNode<RichTextLabel>(nameof(RichTextLabel)).Visible=false;
        }
		else if(input.Attack())
		{
			input.Free();
			InstructionsUI instructions=BaseUI.InstructionsPack.Instance<InstructionsUI>();
			instructions.Name="Instructions";
			AddChild(instructions);
			onOptions=true;
			GetNode<RichTextLabel>(nameof(RichTextLabel)).Visible=false;
		}
		else if(input.Quit())
		{
			input.Free();
			input=null;
			World.Quit();
		}
	}

	public override void _Notification(int what)
	{
		if(what==MainLoop.NotificationWmQuitRequest)
		{
			Worker.Stop();
		}
		base._Notification(what);
	}

}
