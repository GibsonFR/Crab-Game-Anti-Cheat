using System.Linq;

namespace GibsonCrabGameAntiCheat
{
    internal class Variables
    {
        //Managers
        public static GameManager gameManager;

        //Rigidbody
        public static Rigidbody clientBody;

        //GameObject
        public static GameObject clientObject;

        //ChatBox
        public static ChatBox chatBox;

        //Camera
        public static Camera camera;

        //bool
        public static bool GAC, onButton, onSubButton, menuTrigger;

        //bool[]
        public static bool[] buttonStates = new bool[3];

        //int
        public static int modeId, mapId, menuSelector, subMenuSelector, playerIndex, statusTrigger, menuSpeed = 5, menuSpeedHelperFast, menuSpeedHelper, alertLevel = 0;

        //float
        public static float smoothedSpeed, smoothingFactor = 0.7f, checkFrequency = 0.02f;

        //string
        public static string lastItemName = "null", otherPlayerUsername, layout, displayButton0, displayButton1, displayButton2, customPrecisionFormatTargetPosition = "F1", customPrecisionFormatClientRotation = "F1";

        //string(Path)
        public static string assemblyFolderPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public static string defaultFolderPath = assemblyFolderPath.Replace("\\BepInEx\\plugins", "\\");
        public static string mainFolderPath = @"GibsonAntiCheat\";
        public static string configFilePath = mainFolderPath + @"config\config.txt";
        public static string menuPath = mainFolderPath + @"config\menu.txt";

        //Vector3
        public static Vector3 lastOtherPlayerPosition;

        //List
        public static List<Il2CppSystem.Collections.Generic.Dictionary<ulong, MonoBehaviourPublicCSstReshTrheObplBojuUnique>.Entry> playersList;

        //Dictionary
        public static Dictionary<string, System.Func<string>> DebugDataCallbacks;
        public static Il2CppSystem.Collections.Generic.Dictionary<ulong, PlayerType> activePlayers;

    }
}
