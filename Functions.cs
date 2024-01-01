
namespace GibsonCrabGameAntiCheat
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
    public class Utility
    {
        public static string FindChildren(GameObject parent, string path)
        {
            Transform target = parent.transform.Find(path);
            if (target != null && target.childCount > 0)
            {
                return target.GetChild(0).name;
            }
            else
            {
                return "null";
            }
        }
        public static void UpdateBasics()
        {
            if (Variables.clientBody == null) return;
            Variables.clientBody = ClientFunctions.GetPlayerBody();
            Variables.clientObject = ClientFunctions.GetPlayerObject();
        }
        public static void UpdateBasicsNull()
        {
            Variables.gameManager = GameObject.Find("/GameManager (1)").GetComponent<GameManager>();
            Variables.playersList = Variables.gameManager.activePlayers.entries.ToList();
            Variables.activePlayers = Variables.gameManager.activePlayers;
            Variables.mapId = GameData.GetMapId();
            Variables.modeId = GameData.GetModeId();
            Variables.chatBox = ChatBox.Instance;
           
        }
        public static void ReadConfigFile()
        {
            string[] lines = System.IO.File.ReadAllLines(Variables.configFilePath);
            Dictionary<string, string> config = new Dictionary<string, string>();
            CultureInfo cultureInfo = new CultureInfo("fr-FR");
            float resultFloat;
            bool resultBool;
            bool parseSuccess;
            int resultInt;

            foreach (string line in lines)
            {
                string[] parts = line.Split('=');
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    config[key] = value;
                }
            }
        }
        public static void CreateConfigFile()
        {
            string path = Path.Combine("GibsonAntiCheat", "config");
            Directory.CreateDirectory(path);

            string configFilePath = Path.Combine(path, "config.txt");

            Dictionary<string, string> configDefaults = new Dictionary<string, string>
            {
                {"version", "v1.2.0"},
            };

            Dictionary<string, string> currentConfig = new Dictionary<string, string>();

            // If the file exists, read current config
            if (File.Exists(configFilePath))
            {
                string[] lines = File.ReadAllLines(configFilePath);

                foreach (string line in lines)
                {
                    string[] keyValue = line.Split('=');
                    if (keyValue.Length == 2)
                    {
                        currentConfig[keyValue[0]] = keyValue[1];
                    }
                }
            }

            // Merge current config with defaults
            foreach (KeyValuePair<string, string> pair in configDefaults)
            {
                if (!currentConfig.ContainsKey(pair.Key))
                {
                    currentConfig[pair.Key] = pair.Value;
                }
            }

            // Save merged config
            using (StreamWriter sw = File.CreateText(configFilePath))
            {
                foreach (KeyValuePair<string, string> pair in currentConfig)
                {
                    sw.WriteLine(pair.Key + "=" + pair.Value);
                }
            }
        }
        public static void PlayMenuSound()
        {
            if (Variables.clientBody == null) return;
            PlayerInventory.Instance.woshSfx.pitch = 5 * Variables.menuSpeed;
            PlayerInventory.Instance.woshSfx.Play();
            
        }
        public static void SendMessage(string message)
        {
            Variables.chatBox.SendMessage(message);
        }
        public static void ForceMessage(string message) //modif
        {
            Variables.chatBox.ForceMessage(message);
        }
        public static bool DoesFunctionCrash(Action function)
        {
            try
            {
                function.Invoke();
                return false;
            }
            catch (Exception ex)
            {
                // La fonction a généré une exception
                return true;
            }
        }
    }
    public class ItemsUsageTracker
    {
        public static GameObject FindPlayerObjectFromWeapon(GameObject currentObject)
        {
            while (currentObject != null && currentObject.name != "OnlinePlayer(Clone)")
            {
                currentObject = currentObject.transform.parent?.gameObject;
            }
            return currentObject?.name == "OnlinePlayer(Clone)" ? currentObject : null;
        }
        public static class MeleeWeaponUsageTracker
        {
            private static readonly Dictionary<string, DateTime> lastMeleeWeaponUsage = new Dictionary<string, DateTime>();

            public static string GetMessageForMeleeWeaponUse(string username, int playerNumber, string itemName)
            {
                DateTime currentTime = DateTime.Now;
                if (lastMeleeWeaponUsage.TryGetValue(username, out DateTime lastUseTime))
                {
                    TimeSpan timeDifference = currentTime - lastUseTime;
                    lastMeleeWeaponUsage[username] = currentTime;
                    int normalSpeed = 0;

                    if (itemName != "null")
                        Variables.lastItemName = itemName;

                    switch (Variables.lastItemName)
                    {
                        case "Bat(Clone)":
                            normalSpeed = 980;
                            break;
                        case "Katana(Clone)":
                            normalSpeed = 730;
                            break;
                        case "Knife(Clone)":
                            normalSpeed = 730;
                            break;
                        case "MetalPipe(Clone)":
                            normalSpeed = 1180;
                            break;
                        case "Stick(Clone)":
                            normalSpeed = 650;
                            break;
                        case "Bomb(Clone)":
                            normalSpeed = 730;
                            break;
                        default:
                            normalSpeed = 0;
                            break;
                    }

                    if (timeDifference.TotalMilliseconds <= normalSpeed)
                        return $"<color=red>[GAC]</color> [C] FastFire [{Variables.lastItemName.Replace("(Clone)", "")}] | #{playerNumber} {username} | last use: {timeDifference.TotalMilliseconds.ToString("F1")} ms";
                    else
                        return "null";
                }
                else
                {
                    lastMeleeWeaponUsage[username] = currentTime;
                    return "null";
                }
            }
        }
        public static class GunUsageTracker
        {
            private static readonly Dictionary<string, DateTime> lastGunUsage = new Dictionary<string, DateTime>();

            public static string GetMessageForGunUse(string username, int playerNumber)
            {
                DateTime currentTime = DateTime.Now;
                if (lastGunUsage.TryGetValue(username, out DateTime lastUseTime))
                {
                    TimeSpan timeDifference = currentTime - lastUseTime;
                    lastGunUsage[username] = currentTime;

                    if (timeDifference.TotalMilliseconds <= 130)
                        return $"<color=red>[GAC]</color>  [C] FastFire [Gun] | #{playerNumber} {username} | last use: {timeDifference.TotalMilliseconds.ToString("F1")} ms";
                    else
                        return "null";
                }
                else
                {
                    lastGunUsage[username] = currentTime;
                    return "null";
                }
            }
        }

        public static class SnowballUsageTracker
        {
            private static readonly Dictionary<string, DateTime> lastSnowballUsage = new Dictionary<string, DateTime>();

            public static string GetMessageForSnowballUse(string username, int playerNumber)
            {
                DateTime currentTime = DateTime.Now;
                if (lastSnowballUsage.TryGetValue(username, out DateTime lastUseTime))
                {
                    TimeSpan timeDifference = currentTime - lastUseTime;
                    lastSnowballUsage[username] = currentTime;

                    if (timeDifference.TotalMilliseconds <= 480)
                        return $"<color=red>[GAC]</color> [C] FastFire [Snowball] | #{playerNumber} {username} | last use: {timeDifference.TotalMilliseconds.ToString("F1")} ms";
                    else
                        return "null";
                }
                else
                {
                    lastSnowballUsage[username] = currentTime;
                    return "null";
                }
            }
        }
    }

    public class MenuFunctions
    {
        public static void CheckMenuFileExists()
        {
            string menuContent = "\t\r\n\tPosition : [POSITION]  |  Speed : [SPEED]  |  Rotation : [ROTATION]\t\t<b> \r\n\r\n\t______________________________________________________________________</b>\r\n\r\n\r\n\t<b><color=orange>[OTHERPLAYER]</color></b>  |  Position: [OTHERPOSITION]  |  Speed : [OTHERSPEED] | Selecteur :  [SELECTEDINDEX] | <b>Status : [STATUS]</b> \r\n\r\n\t\t\t\r\n\t\r\n\r\n\t______________________________________________________________________\r\n\r\n\t\t\r\n     <b>[MENUBUTTON0]\r\n\r\n\t[MENUBUTTON1]\r\n\r\n\t[MENUBUTTON2]</b>";

            if (System.IO.File.Exists(Variables.menuPath))
            {
                string currentContent = System.IO.File.ReadAllText(Variables.menuPath, System.Text.Encoding.UTF8);


                if (currentContent != menuContent)
                {
                    System.IO.File.WriteAllText(Variables.menuPath, menuContent, System.Text.Encoding.UTF8);
                }
            }
            else
            {
                // Si le fichier n'existe pas, créez-le avec le contenu fourni
                System.IO.File.WriteAllText(Variables.menuPath, menuContent, System.Text.Encoding.UTF8);
            }
        }
        public static void RegisterDataCallbacks(System.Collections.Generic.Dictionary<string, System.Func<string>> dict)
        {
            foreach (System.Collections.Generic.KeyValuePair<string, System.Func<string>> pair in dict)
            {
                Variables.DebugDataCallbacks.Add(pair.Key, pair.Value);
            }
        }
        
        public static void LoadMenuLayout()
        {
            Variables.layout = System.IO.File.ReadAllText(Variables.menuPath, System.Text.Encoding.UTF8);
        }
        public static void RegisterDefaultCallbacks()
        {
            RegisterDataCallbacks(new System.Collections.Generic.Dictionary<string, System.Func<string>>(){
                {"POSITION", ClientFunctions.GetPlayerPositionAsString},
                {"SPEED", ClientFunctions.GetPlayerSpeedAsString},
                {"ROTATION", ClientFunctions.GetPlayerRotationAsString},
                {"SELECTEDINDEX", () => Variables.playerIndex.ToString()},
                {"OTHERPLAYER", MultiPlayersFunctions.GetOtherPlayerUsername},
                {"OTHERPOSITION", MultiPlayersFunctions.GetOtherPlayerPositionAsString},
                {"OTHERSPEED", MultiPlayersFunctions.GetOtherPlayerSpeed},
                {"STATUS", MultiPlayersFunctions.GetStatus},
                {"MENUBUTTON0",() => Variables.displayButton0},
                {"MENUBUTTON1",() => Variables.displayButton1},
                {"MENUBUTTON2",() => Variables.displayButton2},
            });
        }

        public static string DisplayButtonState(int index)
        {
            if (Variables.buttonStates[index])
                return "<b><color=red>ON</color></b>";
            else
                return "<b><color=blue>OFF</color></b>";
        }
        public static string FormatLayout()
        {
            string formatted = Variables.layout;
            foreach (System.Collections.Generic.KeyValuePair<string, System.Func<string>> pair in Variables.DebugDataCallbacks)
            {
                formatted = formatted.Replace("[" + pair.Key + "]", pair.Value());
            }
            return formatted;
        }
        public static string HandleMenuDisplay(int buttonIndex, Func<string> getButtonLabel, Func<string> getButtonSpecificData)
        {
            string buttonLabel = getButtonLabel();

            if (Variables.menuSelector != buttonIndex)
            {
                return $" {buttonLabel} <b>{getButtonSpecificData()}</b>";
            }

            if (!Variables.buttonStates[buttonIndex])
            {
                return $"■<color=yellow>{buttonLabel}</color>■  <b>{getButtonSpecificData()}</b>";
            }
            else
            {
                return $"<color=red>■</color><color=yellow>{buttonLabel}</color><color=red>■</color>  <b>{getButtonSpecificData()}</b>";
            }
        }
        public static string GetSelectedFlungDetectorParam()
        {
            if (Variables.menuSelector == 0)
            {
                switch (Variables.subMenuSelector)
                {
                    case 0:
                        if (Variables.onSubButton)
                            return "  |  " + $"<color=red>■</color><color=orange>Check Frequency : {Variables.checkFrequency.ToString("F2")}</color><color=red>■</color>" + $"  |  Alert Level" + $"  |  Flung Detector Status";
                        else
                            return "  |  " + $"■<color=orange>Check Frequency : {Variables.checkFrequency.ToString("F2")}</color>■" + $"  |  Alert Level" + $"  |  Flung Detector Status";
                    case 1:
                        if (Variables.onSubButton)
                            return "  |  " + $"Check Frequency" + $"  |  <color=red>■</color><color=orange>Alert Level : {Variables.alertLevel.ToString()}</color><color=red>■</color>" + $"  |  Flung Detector Status";
                        else
                            return "  |  " + $"Check Frequency" + $"  |  ■<color=orange>Alert Level : {Variables.alertLevel.ToString()}</color>■" + $"  |  Flung Detector Status";
                    case 2:
                        return "  |  " + $"Check Frequency" + $"  |  Alert Level" + $"  |  ■<color=orange>Flung Dector Status : {Variables.buttonStates[0].ToString()}</color>■";
                    default:
                        return "";
                }
            }
            else
                return "";
        }
        public static void ExecuteSubMenuAction()
        {
            if (!Variables.onButton)
            {
                var selectors = (Variables.menuSelector, Variables.subMenuSelector);

                switch (selectors)
                {
                    case (40, -1):
                        break;
                }
            }
            if (Variables.onButton)
            {
                var selectors = (Variables.menuSelector, Variables.subMenuSelector);

                switch (selectors)
                {
                    case (0, 0):
                        Variables.onSubButton = !Variables.onSubButton;
                        break;
                    case (0, 1):
                        Variables.onSubButton = !Variables.onSubButton;
                        break;
                    case (0, 2):
                        Variables.buttonStates[0] = !Variables.buttonStates[0];

                        if (Variables.buttonStates[0])
                            Utility.ForceMessage("■<color=yellow>(FD))Flung Detector ON</color>■");
                        else
                            Utility.ForceMessage("■<color=yellow>(FD)Flung Detector OFF</color>■");
                        break;
                }
            }
        }
    }
    public class ClientFunctions
    {
        public static Rigidbody GetPlayerBody()
        {
            return GameObject.Find("/Player") == null ? null : GameObject.Find("/Player").GetComponent<Rigidbody>();
        }
        public static GameObject GetPlayerObject()
        {
            return GameObject.Find("/Player");
        }
        public static PlayerType GetPlayerManager()
        {
            if (Variables.clientBody == null) return null;
            return GetPlayerObject().GetComponent<PlayerType>();
        }
        public static Rigidbody GetPlayerBodySafe()
        {
            if (Variables.clientBody == null)
            {
                Variables.clientBody = GetPlayerBody();
            }
            return Variables.clientBody;
        }
        public static Camera GetCameraSafe()
        {
            if (Variables.clientBody != null)
            {
                return Variables.camera;
            }
            else
            {
                return null;
            }
        }
        public static string GetPlayerRotationAsString()
        {
            return Variables.clientBody == null ? "(0,0,0)" : GetCameraSafe().transform.rotation.eulerAngles.ToString();
        }
        public static string GetPlayerPositionAsString()
        {
            return Variables.clientBody == null ? "0.0" : Variables.clientBody.transform.position.ToString();
        }
        public static string GetPlayerSpeedAsString()
        {
            Vector3 velocity = Vector3.zero;
            if (Variables.clientBody != null)
                velocity = new UnityEngine.Vector3(Variables.clientBody.velocity.x, 0f, Variables.clientBody.velocity.z);
            return velocity.magnitude.ToString("0.00");
        }
    }
    public static class MultiPlayersFunctions
    {
        private static System.Random random = new System.Random();

        public static Rigidbody GetOtherPlayerBody()
        {
            Rigidbody rb = null;

            bool result = Utility.DoesFunctionCrash(() => {
                Variables.gameManager.activePlayers.entries.ToList()[Variables.playerIndex].value.GetComponent<Rigidbody>();
            });

            if (result)
            {
                rb = null;
            }
            else
            {
                rb = Variables.gameManager.activePlayers.entries.ToList()[Variables.playerIndex].value.GetComponent<Rigidbody>();
            }

            return rb;
        }
        public static Rigidbody GetOtherPlayerBody(int u)
        {
            Rigidbody rb = null;

            bool result = Utility.DoesFunctionCrash(() => {
                Variables.gameManager.activePlayers.entries.ToList()[u].value.GetComponent<Rigidbody>();
            });

            if (result)
            {
                rb = null;
            }
            else
            {
                rb = Variables.gameManager.activePlayers.entries.ToList()[u].value.GetComponent<Rigidbody>();
            }

            return rb;
        }
        public static string GetOtherPlayerUsername()
        {
            try
            {
                var activePlayersList = Variables.gameManager.activePlayers.entries.ToList();
                var otherPlayerBody = GetOtherPlayerBody();

                if (GetOtherPlayerBody().transform.position == Vector3.zero)
                    return "<color=red>N/A</color>";

                return otherPlayerBody == null ? "<color=red>N/A</color>" : "#" + activePlayersList[Variables.playerIndex].value.playerNumber.ToString() + " " + activePlayersList[Variables.playerIndex].value.username;
            }
            catch { }
            return "<color=red>N/A</color>";

        }
        public static string GetOtherPlayerPositionAsString()
        {
            var otherPlayerBody = GetOtherPlayerBody();

            return otherPlayerBody == null
                ? Vector3.zero.ToString(Variables.customPrecisionFormatTargetPosition)
                : otherPlayerBody.position.ToString(Variables.customPrecisionFormatTargetPosition);
        }
        public static Vector3 GetOtherPlayerPosition()
        {
            var otherPlayerBody = GetOtherPlayerBody();

            return otherPlayerBody == null
                ? Vector3.zero
                : otherPlayerBody.position;
        }
        public static string GetOtherPlayerSpeed() 
        {
            Vector3 pos = GetOtherPlayerPosition();
            double distance = Vector3.Distance(pos, Variables.lastOtherPlayerPosition);
            double speedDouble = distance / 0.05f;
            Variables.smoothedSpeed = (float)((Variables.smoothedSpeed * Variables.smoothingFactor + (1 - Variables.smoothingFactor) * speedDouble) * 1.005);
            return Variables.smoothedSpeed.ToString("F1");
        }
        public static string GetStatus()
        {
            string mode = GameData.GetGameModeName();

            if (Variables.smoothedSpeed > 45 && mode != "Race")
            {
                Variables.statusTrigger += 1;
                if (Variables.statusTrigger >= 5 && Variables.statusTrigger < 25)
                    return "CHEAT (or Sussy Slope)";
                Variables.statusTrigger = 0;
                return "";
            }
            else if (Variables.smoothedSpeed > 30 && mode != "Race")
            {
                Variables.statusTrigger += 1;
                if (Variables.statusTrigger >= 5 && Variables.statusTrigger < 25)
                    return "FAST";
                Variables.statusTrigger = 0;
                return "";
            }
            else if (Variables.smoothedSpeed > 21)
            {
                if (Variables.statusTrigger < 5)
                    Variables.statusTrigger += 1;
                if (Variables.statusTrigger > 5)
                    Variables.statusTrigger -= 1;
                if (Variables.statusTrigger >= 5)
                    return "MOONWALK";
                return "";
            }
            else if (Variables.smoothedSpeed > 5)
            {
                if (Variables.statusTrigger < 5)
                    Variables.statusTrigger += 1;
                if (Variables.statusTrigger > 5)
                    Variables.statusTrigger -= 1;
                if (Variables.statusTrigger >= 5)
                    return "MOVING";
                return "";
            }
            else if (Variables.smoothedSpeed <= 5)
            {
                if (Variables.statusTrigger < 5)
                    Variables.statusTrigger += 1;
                if (Variables.statusTrigger > 5)
                    Variables.statusTrigger -= 1;
                if (Variables.statusTrigger >= 5)
                    return "IDLE";
                return "";
            }

            if (Variables.statusTrigger > 0)
                Variables.statusTrigger -= 1;
            return "";
        }
    }
}
