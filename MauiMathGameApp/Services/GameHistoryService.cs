using MauiMathGameApp.Models;
using System.Text.Json;

namespace MauiMathGameApp.Services
{
  public class GameHistoryService
  {
    /// <summary>
    /// Service responsible for persisting and retrieving game history data
    /// </summary>
    private const string GAME_HISTORY_KEY = "game_history";
    private static int _nextId = 1;

    /// <summary>
    ///   Saves a completed game to persistent storage
    /// </summary>
    /// <param name="gameType">The type of math operation played</param>
    /// <param name="score">Number pf correct answers</param>
    /// <param name="totalQuestions">Total questions asked</param>
    /// <returns></returns>
    public async Task SaveGameAsync(string gameType, int score, int totalQuestions)
    {
      // * Convert starting game type to enum && Safe enum conversion
      if (!Enum.TryParse<GameOperation>(gameType, out GameOperation operation))
      {
        throw new ArgumentException($"Invalid game type: {gameType}");
      }

      // * Create game result
      Game game = new Game
      {
        Id = _nextId++,
        Type = operation,
        Score = score,
        TotalQuestions = totalQuestions,
        DatePlayed = DateTime.Now
      };

      // * Get existing games
      var games = await GetGameHistoryAsync();

      // * Add new game
      games.Add(game);

      // * Save back to preferences && Convert objects to JSON
      var json = JsonSerializer.Serialize(games);
      Preferences.Set(GAME_HISTORY_KEY, json);
    }

    /// <summary>
    /// Retrieves all saved games from storage
    /// </summary>
    /// <returns>List of games ordered by most recent first</returns>
    public async Task<List<Game>> GetGameHistoryAsync()
    {
      await Task.Delay(1); // Make it truly async for future database migration
      var json = Preferences.Get(GAME_HISTORY_KEY, string.Empty);

      if (string.IsNullOrEmpty(json))
      {
        return new List<Game>();
      }

      try
      {
        var games = JsonSerializer.Deserialize<List<Game>>(json) ?? new List<Game>();

        // * Update next ID to avoid conflicts
        if (games.Any())
        {
          _nextId = games.Max(g => g.Id) + 1;
        }

        // * Return most recent first
        return games.OrderByDescending(g => g.DatePlayed).ToList();
      }
      catch (JsonException)
      {
        // * If JSON is corrupted, return empty list and reset storage
        Preferences.Remove(GAME_HISTORY_KEY);
        return new List<Game>();
      }
    }

    /// <summary>
    /// Deletes a specific game from history
    /// </summary>
    /// <param name="gameId">The ID of the game to delete</param>
    public async Task DeleteGameAsync(int gameId)
    {
      var games = await GetGameHistoryAsync();
      var gameToRemove = games.FirstOrDefault(g => g.Id == gameId);
      
      if (gameToRemove != null)
      {
        games.Remove(gameToRemove);
        
        // Save updated list back to preferences
        var json = JsonSerializer.Serialize(games);
        Preferences.Set(GAME_HISTORY_KEY, json);
      }
    }

    /// <summary>
    /// Clears all game history (useful for testing or testing or user preference)
    /// </summary>
    public void ClearHistory()
    {
      Preferences.Remove(GAME_HISTORY_KEY);
      _nextId = 1;
    }

    /// <summary>
    /// Gets basic statistics about game history using LINQ
    /// </summary>
    /// <returns>Statistics about total games, average score, etc.</returns>
    public async Task<GameStatistics> GetStatisticsAsync()
    {
      var games = await GetGameHistoryAsync();

      return new GameStatistics
      {
        TotalGames = games.Count,
        AverageScore = games.Any() ? games.Average(g => g.PercentageScore) : 0,
        BestScore = games.Any() ? games.Max(g => g.PercentageScore) : 0,
        FavouriteGameType = games.GroupBy(g => g.Type)
                               .OrderByDescending(g => g.Count())
                               .FirstOrDefault()?.Key.ToString() ?? "None"
      };
    }
  }

  /// <summary>
  /// Statistics summary for game history
  /// </summary>
  public class GameStatistics
  {
    public int TotalGames { get; set; }
    public double AverageScore { get; set; }
    public double BestScore { get; set; }
    public string FavouriteGameType { get; set; } = string.Empty;
  }
}