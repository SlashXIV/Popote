namespace Popote.Motion;

// Briques d'entrée réutilisables. Chacune respecte MotionTokens.Level :
// Full = mouvement complet, Reduced = fondu court, None = état final immédiat.
public static class Entrances
{
    // L'élément se pose : fondu + montée de 12, décalé selon son rang (8 au plus).
    public static Task RiseInAsync(this VisualElement view, int index = 0, uint baseDelayMs = 0)
    {
        view.CancelAnimations();
        switch (MotionTokens.Level)
        {
            case MotionLevel.None:
                view.Opacity = 1; view.TranslationY = 0;
                return Task.CompletedTask;
            case MotionLevel.Reduced:
                view.TranslationY = 0; view.Opacity = 0;
                return RunAfterAsync(view, 0, () => view.FadeToAsync(1, MotionTokens.ReducedMs, Easing.Linear));
            default:
                view.Opacity = 0; view.TranslationY = MotionTokens.RiseDistance;
                var delay = baseDelayMs + (uint)Math.Min(index, MotionTokens.MaxStaggered) * MotionTokens.ItemStaggerMs;
                return RunAfterAsync(view, delay, () => Task.WhenAll(
                    view.FadeToAsync(1, MotionTokens.EnterMs, MotionTokens.Enter),
                    view.TranslateToAsync(0, 0, MotionTokens.EnterMs, MotionTokens.Enter)));
        }
    }

    // Une photo se pose : 1,04 → 1 en fondu (Full), fondu court (Reduced), rien (None).
    public static Task SettleInAsync(this VisualElement view)
    {
        view.CancelAnimations();
        switch (MotionTokens.Level)
        {
            case MotionLevel.None:
                view.Opacity = 1; view.Scale = 1;
                return Task.CompletedTask;
            case MotionLevel.Reduced:
                view.Scale = 1; view.Opacity = 0;
                return RunAfterAsync(view, 0, () => view.FadeToAsync(1, MotionTokens.ReducedMs, Easing.Linear));
            default:
                view.Opacity = 0; view.Scale = MotionTokens.SettleScale;
                return RunAfterAsync(view, 0, () => Task.WhenAll(
                    view.FadeToAsync(1, MotionTokens.EnterMs, MotionTokens.Enter),
                    view.ScaleToAsync(1, MotionTokens.TransitionMs, MotionTokens.Enter)));
        }
    }

    // Retour visuel de pression (Full seulement) : 0,97 puis retour, sans bloquer la commande.
    public static async Task PressAsync(this VisualElement view)
    {
        if (MotionTokens.Level != MotionLevel.Full) return;
        view.CancelAnimations();
        await view.ScaleToAsync(MotionTokens.PressScale, MotionTokens.PressMs, MotionTokens.Enter);
        await view.ScaleToAsync(1, MotionTokens.PressMs * 2, MotionTokens.Release);
    }

    // Le bol « déborde » : trois gonflements puis repos (Full) ; un fondu court (Reduced) ; rien (None).
    public static async Task OverflowAsync(this VisualElement view)
    {
        view.CancelAnimations();
        view.Scale = 1; view.Opacity = 1;
        switch (MotionTokens.Level)
        {
            case MotionLevel.None:
                return;
            case MotionLevel.Reduced:
                view.Opacity = 0.4;
                await view.FadeToAsync(1, MotionTokens.ReducedMs, Easing.Linear);
                return;
            default:
                for (var i = 0; i < MotionTokens.OverflowCount; i++)
                {
                    // ScaleToAsync renvoie true si l'animation a été annulée (page quittée) : on s'arrête.
                    if (await view.ScaleToAsync(MotionTokens.OverflowScale, MotionTokens.PressMs * 2, MotionTokens.Release)) break;
                    if (await view.ScaleToAsync(1, MotionTokens.EnterMs / 2, MotionTokens.Enter)) break;
                }
                view.Scale = 1;
                return;
        }
    }

    // Une vue pas encore attachée (page poussée, au moment de OnAppearing) finirait ses
    // animations aussitôt : on attend qu'elle soit chargée avant d'animer.
    private static async Task RunAfterAsync(VisualElement view, uint delayMs, Func<Task> action)
    {
        if (!view.IsLoaded)
        {
            var loaded = new TaskCompletionSource();
            void OnLoaded(object? sender, EventArgs e) { view.Loaded -= OnLoaded; loaded.TrySetResult(); }
            view.Loaded += OnLoaded;
            await loaded.Task;
        }
        if (delayMs > 0) await Task.Delay((int)delayMs);
        await action();
    }
}

// Attache le retour de pression à un bouton : <Button.Behaviors><motion:PressScaleBehavior /></Button.Behaviors>
public sealed class PressScaleBehavior : Behavior<Button>
{
    protected override void OnAttachedTo(Button bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.Pressed += OnPressed;
    }

    protected override void OnDetachingFrom(Button bindable)
    {
        bindable.Pressed -= OnPressed;
        base.OnDetachingFrom(bindable);
    }

    private static void OnPressed(object? sender, EventArgs e)
    {
        if (sender is VisualElement v) _ = v.PressAsync();
    }
}
