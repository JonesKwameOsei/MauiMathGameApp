using System.Text.Json.Serialization;

namespace MauiMathGameApp.Models
{
    /// <summary>
    /// Represents the mathematical operations available in the game
    /// </summary>
    public enum GameOperation
    {
        Addition,
        Subtraction,
        Multiplication,
        Division
    }

    /// <summary>
    /// Represents a completed math game with results and metadata
    /// </summary>
    public class Game
    {
        /// <summary>
        /// Unique identifier for the game session
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The type of mathematical operation played
        /// </summary>
        [JsonPropertyName("type")]
        public GameOperation Type { get; set; }

        /// <summary>
        /// Number of correct answers achieved
        /// </summary>
        [JsonPropertyName("score")]
        public int Score { get; set; }

        /// <summary>
        /// Total number of questions asked in the game
        /// </summary>
        [JsonPropertyName("totalQuestions")]
        public int TotalQuestions { get; set; }

        /// <summary>
        /// When the game was completed
        /// </summary>
        [JsonPropertyName("datePlayed")]
        public DateTime DatePlayed { get; set; }

        /// <summary>
        /// Calculated percentage score (Score / TotalQuestions * 100)
        /// </summary>
        [JsonIgnore]
        public double PercentageScore => TotalQuestions > 0 ? (double)Score / TotalQuestions * 100 : 0;

        /// <summary>
        /// Human-readable display of the game type
        /// </summary>
        [JsonIgnore]
        public string GameTypeDisplay => Type.ToString();
    }
}
