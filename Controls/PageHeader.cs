using Popote.Resources.Strings;

namespace Popote.Controls;

// En-tête de page maison (remplace la barre de navigation Shell) : retour éventuel,
// surtitre, titre en Bricolage (titre de niveau 1 pour TalkBack), actions en icônes à droite.
//   <controls:PageHeader Title="Recettes" Eyebrow="Popote">
//       <controls:IconButton Glyph="IconPlus" … />
//   </controls:PageHeader>
[ContentProperty(nameof(Actions))]
public class PageHeader : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(PageHeader), null, propertyChanged: (b, _, n) => ((PageHeader)b)._title.Text = (string?)n);

    public static readonly BindableProperty EyebrowProperty = BindableProperty.Create(
        nameof(Eyebrow), typeof(string), typeof(PageHeader), null,
        propertyChanged: (b, _, n) => { var h = (PageHeader)b; h._eyebrow.Text = (string?)n; h._eyebrow.IsVisible = n is string s && s.Length > 0; });

    public static readonly BindableProperty ShowBackProperty = BindableProperty.Create(
        nameof(ShowBack), typeof(bool), typeof(PageHeader), false, propertyChanged: (b, _, n) => ((PageHeader)b)._back.IsVisible = (bool)n);

    public string? Title { get => (string?)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string? Eyebrow { get => (string?)GetValue(EyebrowProperty); set => SetValue(EyebrowProperty, value); }
    public bool ShowBack { get => (bool)GetValue(ShowBackProperty); set => SetValue(ShowBackProperty, value); }
    public IList<IView> Actions => _actions.Children;

    private readonly Label _title = new() { VerticalOptions = LayoutOptions.Center, LineBreakMode = LineBreakMode.WordWrap, MaxLines = 2 };
    private readonly Label _eyebrow = new() { IsVisible = false };
    private readonly IconButton _back;
    private readonly HorizontalStackLayout _actions = new() { VerticalOptions = LayoutOptions.Center };

    public PageHeader()
    {
        _title.Style = Tokens.Style("Title");
        _eyebrow.Style = Tokens.Style("EyebrowAccent");
        SemanticProperties.SetHeadingLevel(_title, SemanticHeadingLevel.Level1);
        _back = new IconButton { Glyph = "IconBack", Description = AppResources.Action_Back, IsVisible = false };
        _back.Clicked += async (_, _) => await Shell.Current.GoToAsync("..");

        var titles = new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, Children = { _eyebrow, _title } };
        var grid = new Grid
        {
            ColumnDefinitions = { new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto) },
            MinimumHeightRequest = 56,
            Padding = new Thickness(4, 4, 4, 4),
            ColumnSpacing = 4,
        };
        grid.Add(_back, 0);
        grid.Add(titles, 1);
        grid.Add(_actions, 2);
        titles.Margin = new Thickness(12, 0, 0, 0);
        _back.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(IsVisible)) titles.Margin = new Thickness(_back.IsVisible ? 0 : 12, 0, 0, 0);
        };
        Content = grid;
    }
}
