namespace MauiMathGameApp;

public partial class MainPage : ContentPage
{
    // Remove the field declaration for SelectedGameTypeLabel to avoid ambiguity.
    // The label should be defined in XAML with x:Name="SelectedGameTypeLabel" and accessed via the generated partial class.

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnGameChosen(object sender, EventArgs e)
    {
        if (e is TappedEventArgs tappedEventArgs && tappedEventArgs.Parameter is string selectedGameType)
        {
            Navigation.PushAsync(new GameSetUpPage(selectedGameType));
        }
    }

    private void OnViewPreviousGamesChosen(object sender, EventArgs e)
    {
        Navigation.PushAsync(new PreviousGames());
    }
}
