using Client.Networking.Core;
using Client.IO;

namespace Client;

public partial class App : Application
{
	public App()
	{
		Task.Run(SocketCore.Start);

		InitializeComponent();

		MainPage = new MainPage();
	}
}
