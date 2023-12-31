namespace GibsonAntiCheat
{
    public class GameData
    {
        public static LobbyManager GetLobbyManager()
        {
            return LobbyManager.Instance;
        }
        public static int GetMapId()
        {
            return GetLobbyManager().map.id;
        }
        public static string GetGameModeName()
        {
            return UnityEngine.Object.FindObjectOfType<LobbyManager>().gameMode.modeName.ToString();
        }
        public static int GetModeId()
        {
            return GetLobbyManager().gameMode.id;
        }
    }
}
