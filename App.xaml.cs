using Popote.Motion;
using Popote.Services;

namespace Popote;

public partial class App : Application
{
	public App(SettingsService settings)
	{
		InitializeComponent();

		// Thème (Système par défaut), Ambiance et vibrations enregistrés (page Réglages).
		settings.ApplyAll();
		RequestedThemeChanged += (_, _) => ApplySystemBars();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());
		window.Created += (_, _) => ApplySystemBars();
		return window;
	}

	// Retour au premier plan : le réglage « Supprimer les animations » a pu changer.
	protected override void OnResume()
	{
		base.OnResume();
		MotionTokens.RefreshSystemScale();
	}

	private void ApplySystemBars()
	{
#if ANDROID
		SystemBars.Apply(RequestedTheme == AppTheme.Dark);
#endif
	}
}
