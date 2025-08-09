using System.Collections.ObjectModel;
using MauiMathGameApp.Models;
using MauiMathGameApp.Services;

namespace MauiMathGameApp;

public partial class PreviousGames : ContentPage
{
  private readonly GameHistoryService _gameHistoryService = new();
  public ObservableCollection<Game> GameHistory { get; set; } = new();

  public PreviousGames()
  {
    InitializeComponent();
    BindingContext = this; // Enable data binding
  }

  protected override async void OnAppearing()
  {
    base.OnAppearing();
    await LoadGameHistoryAsync();
  }

  /// <summary>
  /// Loads game history from storage and updates the UI
  /// </summary>
  private async Task LoadGameHistoryAsync()
  {
    try
    {
      // * Get games from service
      var games = await _gameHistoryService.GetGameHistoryAsync();

      // * Clear and repopulate collection
      GameHistory.Clear();
      foreach (var game in games)
      {
        GameHistory.Add(game);
      }

      // * Update statistics
      await UpdateStatisticsAsync();

      // * Handling empty state
      UpdateEmptyState();
    }
    catch (Exception ex)
    {
      await DisplayAlert("Error", $"Failed to load game history: {ex.Message}", "OK");
    }
  }


  /// <summary>
  /// Updates the statistics labels with current data
  /// </summary>
  private async Task UpdateStatisticsAsync()
  {
    try
    {
      var stats = await _gameHistoryService.GetStatisticsAsync();

      TotalGamesLabel.Text = $"Total Games Played: {stats.TotalGames}";
      AverageScoreLabel.Text = $"Average Score: {stats.AverageScore:F1}%";
      BestScoreLabel.Text = $"Best Score: {stats.BestScore:F1}%";
      FavouriteGameLabel.Text = $"Favorite Game: {stats.FavouriteGameType}";
    }
    catch (Exception ex)
    {
      // Handle statistics error gracefully
      System.Diagnostics.Debug.WriteLine($"Statistics error: {ex.Message}");
      TotalGamesLabel.Text = "Statistics unavailable";
      AverageScoreLabel.Text = "";
      BestScoreLabel.Text = "";
      FavouriteGameLabel.Text = "";
    }
  }

  /// <summary>
  /// Shows/hides empty state message based on game count
  /// </summary>
  private void UpdateEmptyState()
  {
    bool hasGames = GameHistory.Count > 0;

    // * Show games list and hide empty message when games exist
    EmptyStateLabel.IsVisible = !hasGames;
  }


  /// <summary>
  /// Handles individual game deletion
  /// </summary>
  private async void OnDeleteGameClicked(object sender, EventArgs e)
  {
    if (sender is Button button && button.CommandParameter is Game gameToDelete)
    {
      bool confirm = await DisplayAlert(
          "Delete Game",
          $"Are you sure you want to delete this {gameToDelete.GameTypeDisplay} game?",
          "Delete",
          "Cancel"
      );

      if (confirm)
      {
        try
        {
          // Remove from collection (UI updates automatically)
          GameHistory.Remove(gameToDelete);
          
          // Update the service/storage (you'll need to add this method)
          await _gameHistoryService.DeleteGameAsync(gameToDelete.Id);
          
          // Update UI state
          UpdateEmptyState();
          await UpdateStatisticsAsync();
          
          await DisplayAlert("Success", "Game deleted successfully!", "OK");
        }
        catch (Exception ex)
        {
          await DisplayAlert("Error", $"Failed to delete game: {ex.Message}", "OK");
        }
      }
    }
  }

  /// <summary>
  /// Refresh the page data
  /// </summary>
  /// <returns></returns>
  public async Task RefreshAsync()
  {
    await LoadGameHistoryAsync();
  }
}