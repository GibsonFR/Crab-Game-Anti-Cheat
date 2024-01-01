//Using
global using BepInEx;
global using BepInEx.IL2CPP;
global using UnityEngine;
global using UnityEngine.UI;
global using UnhollowerRuntimeLib;
global using HarmonyLib;
global using System.Collections.Generic;
global using System;
global using System.IO;
global using System.Runtime.InteropServices;
global using System.Linq;
global using System.Globalization;

namespace GibsonCrabGameAntiCheat
{
    [BepInPlugin("A75EF38C-134F-4BD3-ABD0-C5FBC5EB5C9E", "Gibson Crab Game Anti Cheat", "1.2.0")]
    public class Plugin : BasePlugin
    {
        public override void Load()
        {
            ClassInjector.RegisterTypeInIl2Cpp<FlungDetector>();
            ClassInjector.RegisterTypeInIl2Cpp<ItemDetector>();
            ClassInjector.RegisterTypeInIl2Cpp<Basics>();
            ClassInjector.RegisterTypeInIl2Cpp<MenuManager>();
            ClassInjector.RegisterTypeInIl2Cpp<Selector>();

            Harmony.CreateAndPatchAll(typeof(Plugin));

            Utility.CreateConfigFile();

            Variables.DebugDataCallbacks = new System.Collections.Generic.Dictionary<string, System.Func<string>>();
            MenuFunctions.CheckMenuFileExists();
            MenuFunctions.LoadMenuLayout();
            MenuFunctions.RegisterDefaultCallbacks();

            Log.LogInfo("Mod created by Gibson");
        }


        public class Basics : MonoBehaviour
        {
            public Text text;
            private bool initBasics, initStates;
            private float elapsedBasicsUpdate, elapsedMenu;

            private const string MENU_ON_MSG = "■<color=orange>MenuManager <color=blue>ON</color></color>■";
            private const string MENU_OFF_MSG = "■<color=orange>MenuManager <color=red>OFF</color></color>■";
            private const string NAVIGATION_MSG = "■<color=orange>navigate the menu using the numeric keypad (VER NUM ON)</color>■";
            private const string SELECTION_MSG = "■<color=orange>press 4 or 6 to move forwards or backwards, and 5 to select</color>■";
            private const string SUBMENU_MSG = "■<color=orange>press scrollWheel to exit submenu</color>■";

            void Update()
            {
                elapsedBasicsUpdate += Time.deltaTime;
                elapsedMenu += Time.deltaTime;

                if (Input.GetKeyDown("f7"))
                {
                    Utility.PlayMenuSound();
                    Variables.menuTrigger = !Variables.menuTrigger;
                    Utility.ForceMessage(Variables.menuTrigger ? MENU_ON_MSG : MENU_OFF_MSG);
                    if (Variables.menuTrigger)
                    {
                        Utility.ForceMessage(NAVIGATION_MSG);
                        Utility.ForceMessage(SELECTION_MSG);
                        Utility.ForceMessage(SUBMENU_MSG);
                    }
                }

                if (elapsedMenu >= 0.05f)
                {
                    text.text = Variables.menuTrigger ? MenuFunctions.FormatLayout() : "";
                    Variables.lastOtherPlayerPosition = MultiPlayersFunctions.GetOtherPlayerPosition();
                    elapsedMenu = 0f; // reset the timer
                }

                if (!initStates)
                {
                    Variables.subMenuSelector = -1;
                    Variables.playerIndex = 0;
                    Variables.onButton = false;
                    initBasics = false;
                    initStates = true;
                }

                if (!initBasics)
                {
                    Utility.UpdateBasicsNull();
                    Utility.ReadConfigFile();

                    if (Variables.clientBody != null)
                    {
                        Utility.UpdateBasics();
                        Variables.camera = FindObjectOfType<Camera>();
                        initBasics = true;
                    }
                }
                Variables.clientBody = ClientFunctions.GetPlayerBody();
                Variables.otherPlayerUsername = MultiPlayersFunctions.GetOtherPlayerUsername();

                if (elapsedBasicsUpdate > 0.2f)
                {
                    Utility.UpdateBasicsNull();
                    elapsedBasicsUpdate = 0f;
                }
               
            }
        }

        public class MenuManager : MonoBehaviour
        {
            DateTime lastActionTime = DateTime.Now;

            private const int WAIT = 150;
            // Importation des fonctions de la librairie user32.dll
            [DllImport("user32.dll")]
            private static extern short GetKeyState(int nVirtKey);
            [DllImport("user32.dll")]
            private static extern short GetAsyncKeyState(int vKey);
            [DllImport("user32.dll")]
            private static extern int GetSystemMetrics(int nIndex);
            const int VK_RBUTTON = 0x02;
            void Update()
            {

                TimeSpan elapsed = DateTime.Now - lastActionTime;
                HandleMenuDisplays();
                HandleMenuActions(elapsed);
                HandleMenuSpeedHelper(elapsed);

                if (Input.GetMouseButtonDown(2))
                {
                    Variables.onSubButton = false;
                    Variables.onButton = false;
                    Variables.subMenuSelector = -1;
                }

            }

            private void HandleMenuDisplays()
            {
                Variables.displayButton0 = MenuFunctions.HandleMenuDisplay(0, () => "Flung Detector", () => MenuFunctions.DisplayButtonState(0)) + MenuFunctions.GetSelectedFlungDetectorParam();
                Variables.displayButton1 = MenuFunctions.HandleMenuDisplay(1, () => "Item Detector", () => MenuFunctions.DisplayButtonState(1));
                Variables.displayButton2 = MenuFunctions.HandleMenuDisplay(2, () => "Fast Fire Detector", () => MenuFunctions.DisplayButtonState(2));
            }

            private void HandleMenuActions(TimeSpan elapsed)
            {
                bool moletteH = false;
                bool moletteB = false;

                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll > 0f && Variables.menuTrigger)
                {
                    moletteH = true;
                }
                else if (scroll < 0f && Variables.menuTrigger)
                {
                    moletteB = true;
                }

                if (!Variables.menuTrigger || elapsed.TotalMilliseconds < WAIT)
                {
                    if (!moletteB && !moletteH)
                        return;
                }

                Variables.menuSpeedHelperFast = 0;

                bool f5KeyPressed = moletteH;
                bool f6KeyPressed = GetAsyncKeyState(VK_RBUTTON) < 0;
                bool f7KeyPressed = moletteB;
                if (f5KeyPressed || f6KeyPressed || f7KeyPressed)
                {
                    UpdateMenuSpeed(elapsed);
                    HandleKeyActions(f5KeyPressed, f6KeyPressed, f7KeyPressed);
                    lastActionTime = DateTime.Now;
                }
            }

            private void HandleMenuSpeedHelper(TimeSpan elapsed)
            {
                if (elapsed.TotalMilliseconds >= WAIT + Variables.menuSpeedHelperFast)
                {
                    if (Variables.menuSpeedHelper > 0)
                        Variables.menuSpeedHelper -= 1;
                    Variables.menuSpeedHelperFast += WAIT;
                }
            }

            private void UpdateMenuSpeed(TimeSpan elapsed)
            {
                if (elapsed.TotalMilliseconds <= 200)
                    Variables.menuSpeedHelper += 2;
                if (Variables.menuSpeedHelper > 8)
                    Variables.menuSpeed = 5;
                else
                    Variables.menuSpeed = 1;

                Utility.PlayMenuSound();

            }

            private void HandleKeyActions(bool f5KeyPressed, bool f6KeyPressed, bool f7KeyPressed)
            {
                if (f5KeyPressed)
                {
                    HandleF5KeyPressed();
                }

                if (f6KeyPressed)
                {
                    HandleF6KeyPressed();
                }

                if (f7KeyPressed)
                {
                    HandleF7KeyPressed();
                }
            }

            private void HandleF5KeyPressed()
            {
                if (!Variables.onButton)
                    Variables.menuSelector = Variables.menuSelector > 0 ? Variables.menuSelector - 1 : Variables.buttonStates.Length - 1;
                else if (!Variables.onSubButton)
                {
                    switch (Variables.menuSelector)
                    {
                        case 0:
                            Variables.subMenuSelector = Variables.subMenuSelector > 0 ? Variables.subMenuSelector - 1 : 2;
                            break;
                        default:
                            return;
                    }
                }
                else
                {
                    switch (Variables.menuSelector)
                    {
                        case 0:
                            if (Variables.subMenuSelector == 0)
                            {
                                Variables.checkFrequency = Variables.checkFrequency - 0.01f * Variables.menuSpeed;
                                if (Variables.checkFrequency < 0)
                                    Variables.checkFrequency = 0;
                            }
                            if (Variables.subMenuSelector == 1)
                            {
                                Variables.alertLevel = Variables.alertLevel - 1 * Variables.menuSpeed;
                                if (Variables.alertLevel < 0)
                                    Variables.alertLevel = 0;
                                break;
                            }
                            break;
                        default:
                            return;
                    }
                }
            }

            private void HandleF7KeyPressed()
            {
                if (!Variables.onButton)
                    Variables.menuSelector = Variables.menuSelector < Variables.buttonStates.Length - 1 ? Variables.menuSelector + 1 : 0;
                else if (!Variables.onSubButton)
                {
                    switch (Variables.menuSelector)
                    {
                        case 0:
                            Variables.subMenuSelector = Variables.subMenuSelector < 2 ? Variables.subMenuSelector + 1 : 0;
                            break;
                        default:
                            return;
                    }
                }
                else
                {
                    switch (Variables.menuSelector)
                    {
                        case 0:
                            if (Variables.subMenuSelector == 0)
                            {
                                Variables.checkFrequency = Variables.checkFrequency < 1 ? Variables.checkFrequency + 0.01f * Variables.menuSpeed : 1;
                            }
                            if (Variables.subMenuSelector == 1)
                            {
                                Variables.alertLevel = Variables.alertLevel < 2 ? Variables.alertLevel + 1 * Variables.menuSpeed : 2;
                            }
                            break;
                        default:
                            return;
                    }
                }
            }

            public static void HandleF6KeyPressed()
            {
                if (Variables.menuSelector < Variables.buttonStates.Length)
                {
                    MenuFunctions.ExecuteSubMenuAction();

                    if (!Variables.onButton && IsSpecialMenu(Variables.menuSelector))
                        Variables.subMenuSelector = 0;

                    bool previousButtonState = Variables.buttonStates[Variables.menuSelector];
                    if (!IsSpecialMenu(Variables.menuSelector))
                    {
                        Variables.buttonStates[Variables.menuSelector] = !previousButtonState;
                        Variables.onButton = Variables.buttonStates[Variables.menuSelector];
                    }
                    else
                    {
                        Variables.onButton = true;
                    }

                    if (!IsSpecialMenu(Variables.menuSelector))
                        Variables.onButton = false;
                }
            }
            private static bool IsSpecialMenu(int menuSelector)
            {
                return menuSelector == 0;
            }
        }

        public class Selector : MonoBehaviour
        {
            float elapsed;

            void Update()
            {
                elapsed += Time.deltaTime;

                if (Input.GetKey("left") && Variables.playerIndex > 0 && elapsed >= 0.2f)
                {
                    Utility.PlayMenuSound();
                    Variables.playerIndex -= 1;
                    Il2CppSystem.Collections.Generic.Dictionary<ulong, PlayerType> activePlayers = Variables.gameManager.activePlayers;
                    Variables.smoothedSpeed = 0;
                    ChatBox.Instance.ForceMessage("<color=green>Selecteur = " + Variables.playerIndex.ToString());
                    Variables.lastOtherPlayerPosition = activePlayers.entries.ToList()[Variables.playerIndex].value.transform.position;
                    elapsed = 0f;
                }
                if (Input.GetKey("right") && Variables.playerIndex < 40 && elapsed >= 0.2f)
                {
                    Utility.PlayMenuSound();
                    Il2CppSystem.Collections.Generic.Dictionary<ulong, PlayerType> activePlayers = Variables.gameManager.activePlayers;

                    if (Variables.playerIndex < activePlayers.count - 1)
                        Variables.playerIndex += 1;
                    else if (Variables.playerIndex > activePlayers.count)
                        Variables.playerIndex = 0;
                    Variables.smoothedSpeed = 0;
                    ChatBox.Instance.ForceMessage("<color=green>Selecteur = " + Variables.playerIndex.ToString());
                    Variables.lastOtherPlayerPosition = activePlayers.entries.ToList()[Variables.playerIndex].value.transform.position;
                    elapsed = 0f;
                }
            }
        }
        public class PlayerData
        {
            public string PlayerName { get; set; }
            public Vector3 ActualPosition { get; set; }
            public Vector3 LastPosition { get; set; }
            public int Actualisations { get; set; }
            public int PlayerId { get; set; }
            public Vector3 Direction { get; set; }
            public int DirectionChanges { get; set; }
        }

        public class FlungDetector : MonoBehaviour
        {
            private bool message;
            private float elapsed = 0f;
            private Dictionary<string, PlayerData> playersData = new Dictionary<string, PlayerData>();

            void Update()
            {
                if (!Variables.buttonStates[0]) return;
                elapsed += Time.deltaTime;

                if (elapsed > Variables.checkFrequency)
                {
                    GetAlivePlayersData();
                    elapsed = 0f;
                }
            }

            private void GetAlivePlayersData()
            {
                foreach (var player in Variables.playersList)
                {
                    var playerValue = player?.value;
                    if (playerValue != null && !playerValue.dead)
                    {
                        string playerName = playerValue.username.Trim();
                        if (!playersData.TryGetValue(playerName, out var playerData))
                        {
                            playerData = new PlayerData { PlayerName = playerName };
                            playersData.Add(playerName, playerData);
                        }

                        UpdatePlayerData(playerData, playerValue);
                    }
                }
            }

            private void UpdatePlayerData(PlayerData playerData, PlayerType playerValue)
            {
                playerData.Actualisations += 1;
                playerData.LastPosition = playerData.ActualPosition;
                playerData.ActualPosition = playerValue.transform.position;
                playerData.PlayerId = playerValue.playerNumber;
                playerData.Direction = (playerData.ActualPosition - playerData.LastPosition).normalized;

                CheckPlayerMovement(playerData);
                CheckPlayerDirectionChanges(playerData);
            }

            private void CheckPlayerMovement(PlayerData playerData)
            {
                float angle = Vector3.Angle(playerData.LastPosition, playerData.Direction);
                if (playerData.LastPosition != Vector3.zero && (angle >= 170 && angle <= 190))
                {
                    playerData.DirectionChanges++;
                    playerData.Actualisations = 0;
                    message = false;
                }
                else if (playerData.Actualisations > 30)
                {
                    if (playerData.DirectionChanges > 0)
                        playerData.DirectionChanges = 0;
                    playerData.Actualisations = 0;
                    message = false;
                }
            }

            private void CheckPlayerDirectionChanges(PlayerData playerData)
            {
                string prob = GetProbabilityLevel(playerData.DirectionChanges);

                if (playerData.DirectionChanges > Variables.alertLevel && playerData.DirectionChanges < 4 && !message)
                {
                    if (Vector3.Distance(playerData.ActualPosition, playerData.LastPosition) > 2.5f)
                    {
                        Utility.ForceMessage("<color=red>[GAC] </color>[P] " + prob + " | [C] Flung |#" + playerData.PlayerId.ToString() + "  " + playerData.PlayerName);
                        message = true;
                    }
                    else
                        playerData.DirectionChanges--;
                }
            }

            private string GetProbabilityLevel(int directionChanges)
            {
                return directionChanges switch
                {
                    1 => "Low",
                    2 => "Moderate",
                    3 => "High",
                    _ => "High",
                };
            }
        }
        public class ItemDetector : MonoBehaviour
        {
            float elapsed;

            void Update()
            {
                if (!Variables.buttonStates[1]) return;
                elapsed += Time.deltaTime;

                if (elapsed > 1)
                {
                    foreach (GameObject obj in FindObjectsOfType<GameObject>())
                    {
                        if (obj.name == "OnlinePlayer(Clone)")
                        {
                            string itemName = Utility.FindChildren(obj, "ItemOrbit/ItemParent");
                            var player = obj.GetComponent<MonoBehaviourPublicCSstReshTrheObplBojuUnique>();

                            if (IsIllegalItem(itemName, Variables.modeId))
                                Utility.ForceMessage($"<color=red>[GAC]</color>  [C] Illegal Item [{itemName.Replace("(Clone)", "")}] | #{player.playerNumber} {player.username}");

                        }
                    }

                    elapsed = 0;
                }
            }
            public bool IsIllegalItem(string itemName, int modeId)
            {
                switch (itemName)
                {
                    case "Snowball(Clone)":
                        return false;
                    case "Rifle(Clone)":
                        return true;
                    case "Pistol(Clone)":
                        if (modeId != 7) return true;
                        else return false;
                    case "Revolver(Clone)":
                        return true;
                    case "DoubleBarrelShotgun(Clone)":
                        return true;
                    case "Bat(Clone)":
                        switch (modeId)
                        {
                            case 7 or 2: return false;
                            default: return true;
                        }
                    case "Bomb(Clone)":
                        if (modeId != 6) return true;
                        else return false;
                    case "Katana(Clone)":
                        if (modeId != 7) return true;
                        else return false;
                    case "Knife(Clone)":
                        if (modeId != 7) return true;
                        else return false;
                    case "MetalPipe(Clone)":
                        if (modeId != 7) return true;
                        else return false;
                    case "Stick(Clone)":
                        switch (modeId)
                        {
                            case 7 or 4: return false;
                            default: return true;
                        }
                    case "Milk(Clone)":
                        return true;
                    case "PizzaSteve(Clone)":
                        return true;
                    case "Grenade(Clone)":
                        return true;
                    default: return false;
                }

            }
        }

        [HarmonyPatch(typeof(MonoBehaviour2PublicGathObauTrgumuGaSiBoUnique), nameof(MonoBehaviour2PublicGathObauTrgumuGaSiBoUnique.AllUse))]
        [HarmonyPostfix]
        public static void OnSnowballUse(MonoBehaviour2PublicGathObauTrgumuGaSiBoUnique __instance)
        {
            if (!Variables.buttonStates[2]) return;
            GameObject playerObject = ItemsUsageTracker.FindPlayerObjectFromWeapon(__instance.gameObject);
            if (playerObject != null)
            {
                var playerComponent = playerObject.GetComponent<MonoBehaviourPublicCSstReshTrheObplBojuUnique>();
                if (playerComponent != null)
                {
                    string username = playerComponent.username;
                    int playerNumber = playerComponent.playerNumber;
                    string message = ItemsUsageTracker.SnowballUsageTracker.GetMessageForSnowballUse(username, playerNumber);
                    if (message != "null")
                        Utility.ForceMessage(message);
                }
            }
        }

        [HarmonyPatch(typeof(MonoBehaviour2PublicObauTrSiVeSiGahiUnique), nameof(MonoBehaviour2PublicObauTrSiVeSiGahiUnique.AllUse))]
        [HarmonyPostfix]
        public static void OnMeleeUse(MonoBehaviour2PublicObauTrSiVeSiGahiUnique __instance)
        {
            if (!Variables.buttonStates[2]) return;
            GameObject playerObject = ItemsUsageTracker.FindPlayerObjectFromWeapon(__instance.gameObject);
            if (playerObject != null)
            {
                var playerComponent = playerObject.GetComponent<MonoBehaviourPublicCSstReshTrheObplBojuUnique>();
                if (playerComponent != null)
                {
                    string username = playerComponent.username;
                    int playerNumber = playerComponent.playerNumber;
                    string itemName = "null";
                    try
                    {
                        itemName = Utility.FindChildren(playerObject, "ItemOrbit/ItemParent");
                    }
                    catch { }

                    string message = ItemsUsageTracker.MeleeWeaponUsageTracker.GetMessageForMeleeWeaponUse(username, playerNumber, itemName);
                    if (message != "null")
                        Utility.ForceMessage(message);
                }
            }
        }

        [HarmonyPatch(typeof(MonoBehaviour2PublicTrguGamubuGaSiBoSiUnique), nameof(MonoBehaviour2PublicTrguGamubuGaSiBoSiUnique.AllUse))]
        [HarmonyPostfix]
        public static void OnGunUse(MonoBehaviour2PublicTrguGamubuGaSiBoSiUnique __instance)
        {
            if (!Variables.buttonStates[2]) return;
            GameObject playerObject = ItemsUsageTracker.FindPlayerObjectFromWeapon(__instance.gameObject);
            if (playerObject != null)
            {
                var playerComponent = playerObject.GetComponent<MonoBehaviourPublicCSstReshTrheObplBojuUnique>();
                if (playerComponent != null)
                {
                    string username = playerComponent.username;
                    int playerNumber = playerComponent.playerNumber;

                    string message = ItemsUsageTracker.GunUsageTracker.GetMessageForGunUse(username, playerNumber);
                    if (message != "null")
                        Utility.ForceMessage(message);
                }
            }
        }

        [HarmonyPatch(typeof(ChatBox), nameof(ChatBox.AppendMessage))]
        [HarmonyPostfix]
        static void OnReceiveMessage(ChatBox __instance, ulong __0, string __1, string __2)
        {
        }

        [HarmonyPatch(typeof(GameUI), "Awake")]
        [HarmonyPostfix]
        public static void UIAwakePatch(GameUI __instance)
        {
            GameObject menuObject = new GameObject();
            Text text = menuObject.AddComponent<Text>();
            text.font = (Font)Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.supportRichText = true;
            text.raycastTarget = false;

            Basics basics = menuObject.AddComponent<Basics>();
            basics.text = text;

            FlungDetector flungDetector = menuObject.AddComponent<FlungDetector>();
            ItemDetector itemChecker = menuObject.AddComponent<ItemDetector>();
            MenuManager menuManager = menuObject.AddComponent<MenuManager>();
            Selector selector = menuObject.AddComponent<Selector>();    

            menuObject.transform.SetParent(__instance.transform);
            menuObject.transform.localPosition = new UnityEngine.Vector3(menuObject.transform.localPosition.x, -menuObject.transform.localPosition.y, menuObject.transform.localPosition.z);
            RectTransform rt = menuObject.GetComponent<RectTransform>();
            rt.pivot = new UnityEngine.Vector2(0, 1);
            rt.sizeDelta = new UnityEngine.Vector2(1920, 1080);
        }

        //Anticheat Bypass 
        [HarmonyPatch(typeof(EffectManager), "Method_Private_Void_GameObject_Boolean_Vector3_Quaternion_0")]
        [HarmonyPatch(typeof(LobbyManager), "Method_Private_Void_0")]
        [HarmonyPatch(typeof(MonoBehaviourPublicVesnUnique), "Method_Private_Void_0")]
        [HarmonyPatch(typeof(LobbySettings), "Method_Public_Void_PDM_2")]
        [HarmonyPatch(typeof(MonoBehaviourPublicTeplUnique), "Method_Private_Void_PDM_32")]
        [HarmonyPrefix]
        public static bool Prefix(System.Reflection.MethodBase __originalMethod)
        {
            return false;
        }
    }
}