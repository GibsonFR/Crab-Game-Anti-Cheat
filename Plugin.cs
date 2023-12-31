//Using
global using BepInEx;
global using BepInEx.IL2CPP;
global using UnityEngine;
global using UnityEngine.UI;
global using UnhollowerRuntimeLib;
global using HarmonyLib;
global using System.Collections.Generic;
global using System;
using System.Linq;
using System.Timers;

namespace GibsonAntiCheat
{
    [BepInPlugin("A75EF38C-134F-4BD3-ABD0-C5FBC5EB5C9E", "Gibson Anti Cheat", "1.1.0")]
    public class Plugin : BasePlugin
    {
        public override void Load()
        {
            ClassInjector.RegisterTypeInIl2Cpp<FlungDetector>();
            ClassInjector.RegisterTypeInIl2Cpp<ItemChecker>();
            ClassInjector.RegisterTypeInIl2Cpp<Basics>();

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }


        public class Basics : MonoBehaviour
        {
            private bool init;
            private float elapsed;
            void Update()
            {
                if (!Variables.GAC) return;
                elapsed += Time.deltaTime;

                if (!init)
                {
                    Variables.clientBody = null;

                    Variables.gameManager = GameObject.Find("/GameManager (1)").GetComponent<GameManager>();
                    Variables.clientBody = GameObject.Find("/Player").GetComponent<Rigidbody>();
                    if (Variables.clientBody != null)
                        init = true;
                }

                if (elapsed > 1)
                {
                    Variables.playersList = Variables.gameManager.activePlayers.entries.ToList();
                    Variables.modeId = GameData.GetModeId();
                    elapsed = 0f;
                }
                

            }
        }

        public class FlungDetector : MonoBehaviour
        {
            private bool message;
            private const float updateInterval = 0.02f;
            private float elapsed = 0f;
            private Dictionary<string, PlayerData> playersData = new Dictionary<string, PlayerData>();

            void Update()
            {
                if (!Variables.GAC) return;
                elapsed += Time.deltaTime;

                if (elapsed > updateInterval)
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

                if (playerData.DirectionChanges >= 1 && playerData.DirectionChanges < 4 && !message)
                {
                    if (Vector3.Distance(playerData.ActualPosition, playerData.LastPosition) > 2.5f)
                    {
                        Chatbox.Instance.ForceMessage("<color=red>[GAC] </color>[P] " + prob + " | [C] Flung |#" + playerData.PlayerId.ToString() + "  " + playerData.PlayerName);
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
        }

        public class ItemChecker : MonoBehaviour
        {
            float elapsed;

            void Update()
            {
                if (!Variables.GAC) return;
                elapsed += Time.deltaTime;

                if (elapsed > 1)
                {
                    foreach (GameObject obj in FindObjectsOfType<GameObject>())
                    {
                        if (obj.name == "OnlinePlayer(Clone)")
                        {
                            string itemName = FindChildren(obj, "ItemOrbit/ItemParent");
                            var player = obj.GetComponent<MonoBehaviourPublicCSstReshTrheObplBojuUnique>();

                            if (IsIllegalItem(itemName, Variables.modeId))
                                Chatbox.Instance.ForceMessage($"<color=red>[GAC]</color>  [C] Illegal Item [{itemName.Replace("(Clone)", "")}] | #{player.playerNumber} {player.username}");
                            
                        }
                    }

                    elapsed = 0;
                }
            }

            public string FindChildren(GameObject parent, string chemin)
            {
                Transform target = parent.transform.Find(chemin);
                if (target != null && target.childCount > 0)
                {
                    return target.GetChild(0).name;
                }
                else
                {
                    return "null";
                }
            }

            public bool IsIllegalItem(string itemName, int modeId)
            {
                if (itemName == "Snowball(Clone)") return false;

                if ((itemName == "Pistol(Clone)" ||
                     itemName == "Katana(Clone)" || itemName == "Knife(Clone)" ||
                     itemName == "MetalPipe(Clone)") && modeId != 7)
                {
                    return true;
                }

                if (itemName == "Bomb(Clone)" && modeId != 6)
                    return true;

                if ((itemName == "Bat(Clone)" && (modeId != 7 && modeId != 2)) ||
                    (itemName == "Stick(Clone)" && (modeId != 7 && modeId != 4)))
                {
                    return true;
                }

                switch (itemName)
                {
                    case "Rifle(Clone)":
                    case "Revolver(Clone)":
                    case "DoubleBarrelShotgun(Clone)":
                    case "Milk(Clone)":
                    case "PizzaSteve(Clone)":
                    case "Grenade(Clone)":
                        return true;
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(Chatbox), nameof(Chatbox.AppendMessage))]
        [HarmonyPostfix]
        static void OnReceiveMessage(Chatbox __instance, ulong __0, string __1, string __2)
        {
            if (__1 == "!GAC")
            {
                Variables.GAC = !Variables.GAC;

                if (Variables.GAC)
                    Chatbox.Instance.ForceMessage("<color=red>[GAC]</color> ON!");
                else
                    Chatbox.Instance.ForceMessage("<color=red>[GAC]</color> Off!");


            }            
        }


        [HarmonyPatch(typeof(GameUI), "Awake")]
        [HarmonyPostfix]
        public static void UIAwakePatch(GameUI __instance)
        {
            GameObject menuObject = new GameObject();
            Text text = menuObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 16;
            text.raycastTarget = false;

            FlungDetector flungDetector = menuObject.AddComponent<FlungDetector>();
            ItemChecker itemChecker = menuObject.AddComponent<ItemChecker>();
            Basics basics = menuObject.AddComponent<Basics>();

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