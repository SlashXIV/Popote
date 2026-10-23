using Android.Content.Res;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using Microsoft.Maui.Handlers;
using Popote.Controls;
using AView = Android.Views.View;

namespace Popote;

// Ajustements Android des contrôles natifs, appelés depuis MauiProgram :
// - champs (Entry, Editor, Picker) sans soulignement ni fond Material : c'est Field qui dessine le cadre ;
// - NumberLabel en chiffres tabulaires (fonction OpenType « tnum ») ; TickerLabel sans mise en page ;
// - rôle TalkBack des contrôles maison (A11y.Role) et activation par double toucher.
public static class PopoteHandlers
{
    public static void Configure()
    {
        EntryHandler.Mapper.AppendToMapping("PopoteBare", (h, _) => Bare(h.PlatformView));
        EditorHandler.Mapper.AppendToMapping("PopoteBare", (h, _) => Bare(h.PlatformView));
        PickerHandler.Mapper.AppendToMapping("PopoteBare", (h, _) => Bare(h.PlatformView));

        LabelHandler.Mapper.AppendToMapping("PopoteTabular", (h, v) =>
        {
            if (v is NumberLabel) h.PlatformView.FontFeatureSettings = "tnum";
        });
        LabelHandler.Mapper.AppendToMapping(nameof(TickerLabel.Value), (h, v) =>
        {
            if (v is not TickerLabel ticker) return;
            var tv = h.PlatformView;
            // Largeur min = max : setText n'appelle plus requestLayout, seulement invalidate (même hauteur).
            if (tv.Width > 0 && tv.MinWidth != tv.Width) tv.SetWidth(tv.Width);
            tv.Text = ticker.Value;
        });

        // Rejoué après chaque mise à jour de la sémantique MAUI, qui peut poser son propre délégué.
        ViewHandler.ViewMapper.AppendToMapping(nameof(IView.Semantics), ApplyRole);
    }

    private static void Bare(AView view)
    {
        view.Background = null;
        view.BackgroundTintList = ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
    }

    private static void ApplyRole(IViewHandler handler, IView view)
    {
        if (view is not BindableObject b || handler.PlatformView is not AView platform) return;
        var role = A11y.GetRole(b);
        if (role == A11yRole.None) return;
        var current = ViewCompat.GetAccessibilityDelegate(platform);
        if (current is RoleDelegate) return;
        ViewCompat.SetAccessibilityDelegate(platform, new RoleDelegate(current, b, role));
        platform.Focusable = true;
    }

    private sealed class RoleDelegate : AccessibilityDelegateCompat
    {
        private readonly AccessibilityDelegateCompat? _inner;
        private readonly WeakReference<BindableObject> _view;
        private readonly A11yRole _role;

        public RoleDelegate(AccessibilityDelegateCompat? inner, BindableObject view, A11yRole role)
        {
            _inner = inner;
            _view = new WeakReference<BindableObject>(view);
            _role = role;
        }

        public override void OnInitializeAccessibilityNodeInfo(AView? host, AccessibilityNodeInfoCompat? info)
        {
            if (_inner is not null) _inner.OnInitializeAccessibilityNodeInfo(host, info);
            else base.OnInitializeAccessibilityNodeInfo(host, info);
            if (info is null) return;

            info.ClassName = _role switch
            {
                A11yRole.CheckBox => "android.widget.CheckBox",
                A11yRole.Switch => "android.widget.ToggleButton",
                A11yRole.Radio => "android.widget.RadioButton",
                _ => "android.widget.Button",
            };
            info.Clickable = true;
            info.AddAction(AccessibilityNodeInfoCompat.AccessibilityActionCompat.ActionClick);
            if (_role is A11yRole.CheckBox or A11yRole.Switch or A11yRole.Radio && _view.TryGetTarget(out var v))
            {
                info.Checkable = true;
                info.Checked = A11y.GetIsChecked(v);
            }
        }

        public override bool PerformAccessibilityAction(AView? host, int action, Android.OS.Bundle? args)
        {
            if (action == AccessibilityNodeInfoCompat.ActionClick && _view.TryGetTarget(out var v) && v is IActivatable a)
            {
                a.Activate();
                return true;
            }
            return _inner?.PerformAccessibilityAction(host, action, args) ?? base.PerformAccessibilityAction(host, action, args);
        }
    }
}
