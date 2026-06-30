using Microsoft.Extensions.DependencyInjection;

namespace Popote;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		// L'app part sur une identité claire (cf. docs/design-system.md) :
		// on force le thème clair quel que soit le réglage système.
		UserAppTheme = AppTheme.Light;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}