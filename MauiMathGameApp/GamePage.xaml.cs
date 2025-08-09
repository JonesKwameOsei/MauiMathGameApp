using System.Threading.Tasks;
using MauiMathGameApp.Services;

namespace MauiMathGameApp;

public partial class GamePage : ContentPage
{
  private readonly GameHistoryService _gameHistoryService = new();

  public string GameType { get; set; }
  int firstNumber = 0;
  int secondNumber = 0;
  int score = 0;
  int totalQuestions;
  int gamesLeft;

  public GamePage(string gameType, int numberOfQuestions)
  {
    InitializeComponent();
    GameType = gameType;
    totalQuestions = numberOfQuestions; // Set the total number of questions based on the input
    gamesLeft = totalQuestions; // Initialize the number of games left to the total questions
    BindingContext = this; // aasigning the binding context to the current instance of this class

    Title = $"{GameType} Game"; // Set the title of the page based on the game type

    CreateNewQuestions(); // Call the method to create new questions in the constructor
  }

  // A method to create questions based on the game type
  private void CreateNewQuestions()
  {
    string gameOperand = GameType switch
    {
      "Addition" => "+",
      "Subtraction" => "-",
      "Multiplication" => "*",
      "Division" => "÷",
      _ => ""
    };

    // Create a random number generator
    Random random = new Random();

    // Generate two random numbers based on the game type
    firstNumber = GameType != "Division" ? random.Next(1, 9) : random.Next(1, 99);
    secondNumber = GameType != "Division" ? random.Next(1, 9) : random.Next(1, 99);

    if (GameType == "Division")
    {
      while (firstNumber < secondNumber || firstNumber % secondNumber != 0)
      {
        firstNumber = random.Next(1, 99);
        secondNumber = random.Next(1, 99);
      }
    }

    // Set the question text based on the game type and operand
    QuestionLabel.Text = $"{firstNumber} {gameOperand} {secondNumber}";
  }

  // A method to submit the answer
  private async void OnAnswerSubmitted(object sender, EventArgs eventArgs)
  {
    // validate input: not empty and is a valid integer (convert input (string) into an integer)
    if (string.IsNullOrWhiteSpace(AnswerEntry.Text) || !int.TryParse(AnswerEntry.Text, out int answer))
    {
      AnswerLabel.Text = "Please enter a valid number.";
      return; // Exit the method if the input is invalid
    }
    var isCorrect = false;

    // Check the answer based on the game type use 
    switch (GameType)
    {
      case "Addition":
        isCorrect = answer == firstNumber + secondNumber;
        break;

      case "Subtraction":
        isCorrect = answer == firstNumber - secondNumber;
        break;

      case "Multiplication":
        isCorrect = answer == firstNumber * secondNumber;
        break;

      case "Division":
        isCorrect = answer == firstNumber / secondNumber;
        break;
    }
    ProcessAnswer(isCorrect);

    // Decrease the number of games left after each question
    gamesLeft--;

    // Clear the answer entry field for the next question
    AnswerEntry.Text = string.Empty;
    AnswerEntry.Focus(); // Set focus back to the answer entry field

    // Hide feedback card for next question
    await Task.Delay(1500); // Show feedback for 1.5 seconds
    FeedbackCard.IsVisible = false;

    // Check if there are any games left
    if (gamesLeft > 0)
      CreateNewQuestions();
    else
      GameOver();
  }

  private async void GameOver()
  {
    // Hide the question area and show game over section
    QuestionArea.IsVisible = false;
    GameOverSection.IsVisible = true;
    
    // Calculate percentage and create encouraging message
    double percentage = (double)score / totalQuestions * 100;
    string performanceMessage = percentage switch
    {
      >= 90 => "🌟 Outstanding! You're a math superstar!",
      >= 75 => "🎉 Excellent work! Keep it up!",
      >= 60 => "👍 Good job! You're improving!",
      >= 40 => "💪 Nice try! Practice makes perfect!",
      _ => "🎯 Keep practicing! You'll get better!"
    };
    
    GameOverLabel.Text = $"{performanceMessage}\n\nScore: {score} out of {totalQuestions} ({percentage:F0}%)";

    // * Save game results
    try
    {
      await _gameHistoryService.SaveGameAsync(GameType, score, totalQuestions);
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"Failed to save game: {ex.Message}");
    }
  }

  private async void ProcessAnswer(bool isCorrect)
  {
    // use ternery operator to set the score
    score += isCorrect ? 1 : 0;

    // Display the result with improved styling
    AnswerLabel.Text = isCorrect ? "✅ Correct! Great job!" : "❌ Incorrect! Try again next time.";
    AnswerLabel.TextColor = isCorrect ? Color.FromArgb("#A3BE8C") : Color.FromArgb("#BF616A");
    
    // Show the feedback card
    FeedbackCard.IsVisible = true;
    
    // Auto-scroll to make feedback card visible
    await Task.Delay(100); // Small delay to ensure card is rendered
    await MainScrollView.ScrollToAsync(FeedbackCard, ScrollToPosition.MakeVisible, true);
  }

  private async void OnBackToMenu(object sender, EventArgs e)
  {
    bool confirm = await DisplayAlert(
        "Confirm",
        "Are you sure you want to return to the main menu?",
        "Yes",
        "No"
    );

    if (confirm)
    {
      // Return to the main menu by popping to root page
      await Navigation.PopToRootAsync();
    }
  }
  // Pseudocode plan:
  // 1. When navigating from MainMenuPage to GamePage, use Navigation.PushAsync with animation.
  // 2. When returning from GamePage to MainMenuPage, use Navigation.PopToRootAsync with animation.
  // 3. Optionally, add a fade or scale animation to the page's content on appearing/disappearing for a smoother effect.
  // 4. Override OnAppearing and OnDisappearing in GamePage to animate the content area.
  protected override async void OnAppearing()
  {
    base.OnAppearing();
    // Fade in the main content area for a smooth transition
    if (QuestionArea != null)
    {
      QuestionArea.Opacity = 0;
      await QuestionArea.FadeTo(1, 400, Easing.CubicIn);
    }
  }

  protected override async void OnDisappearing()
  {
    // Fade out the main content area before navigating away
    if (QuestionArea != null)
    {
      await QuestionArea.FadeTo(0, 200, Easing.CubicOut);
    }
    base.OnDisappearing();
  }
}