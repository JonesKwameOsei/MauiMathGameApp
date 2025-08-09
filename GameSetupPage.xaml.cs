namespace MauiMathGameApp;

public partial class GameSetupPage : ContentPage
{
    private readonly string? gameType;

    public GameSetupPage(string selectedGameType)
    {
        InitializeComponent();
        gameType = selectedGameType;
        GameTypeLabel.Text = $"Selected Game Type: {gameType}";
    }

    private async void OnStartGameClicked(object sender, EventArgs e)
    {
        if (!int.TryParse(QuestionsEntry.Text, out int numberOfQuestions) || numberOfQuestions <= 0)
        {
            await DisplayAlert("Error", "Please enter a valid number of questions.", "OK");
            return;
        }

        await Navigation.PushAsync(new GamePage(gameType, numberOfQuestions));
    }
}