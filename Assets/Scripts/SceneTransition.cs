using UnityEngine.SceneManagement;

public static class SceneTransition
{
    public static string PackId { get; set; }
    public static string SongId { get; set; }
    public static int DifficultyId { get; set; }

    public static string SongFolder => $"{PackId}/{SongId}";

    public static bool HasPendingReturn { get; set; }

    public static void GoToGame(string packId, string songId, int difficultyId)
    {
        PackId = packId;
        SongId = songId;
        DifficultyId = difficultyId;
        HasPendingReturn = false;
        SceneManager.LoadScene("GameScene");
    }

    public static void GoToMenu(string packId, string songId, int difficultyId)
    {
        PackId = packId;
        SongId = songId;
        DifficultyId = difficultyId;
        HasPendingReturn = true;
        SceneManager.LoadScene("MenuScene");
    }
}