using Godot;
using HaphyUtils;

namespace TheProject;

public partial class Root : Control
{
	public override void _Ready()
	{
		Log.Configure(pushErrors: false);
		Log.Info("hello world");

		Log.Alert("Update available, maybe consider updating the thing?");
	}

	public override void _Notification(int what)
	{
		if(what == NotificationWMCloseRequest)
		{
			Shutdown();
		}
	}

	public void Shutdown()
	{
		Log.Info("Shutting down...");
		Log.Dispose();
		GetTree().Quit();
	}

	public override void _Process(double delta)
	{

	}
}
