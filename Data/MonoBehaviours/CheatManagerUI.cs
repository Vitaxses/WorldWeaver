using System;
using System.Collections;
using GenericVariableExtension;
using UnityEngine;
using WorldWeaver.Editor;

using static CheatManager;

///  Noclip movement logic adapted from hk-speedrunning/Silksong.DebugMod under MIT
///  https://github.com/hk-speedrunning/Silksong.DebugMod/blob/main/GUIController.cs#L294
///
///  MIT License
///  
///  Copyright (c) 2025 Debug Mod Authors
///  
///  Permission is hereby granted, free of charge, to any person obtaining a copy
///  of this software and associated documentation files (the "Software"), to deal
///  in the Software without restriction, including without limitation the rights
///  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
///  copies of the Software, and to permit persons to whom the Software is
///  furnished to do so, subject to the following conditions:
///  
///  The above copyright notice and this permission notice shall be included in all
///  copies or substantial portions of the Software.
///  
///  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
///  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
///  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
///  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
///  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
///  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
///  SOFTWARE.

namespace WorldWeaver.Data.MonoBehaviours
{
    public class CheatManagerUI : MonoBehaviour
    {
        private MenuStates menuState = MenuStates.Main;

        private bool noClip;
        private Rigidbody2D? heroRigidbody;

        private static GUIStyle? buttonStyle;

        private enum PlayerDataVariableType
        {
            Int,
            Float,
            Bool,
            String
        }

        private string pdVariableName = string.Empty;
        private string pdVariableValue = string.Empty;
        private PlayerDataVariableType pdVariableType = PlayerDataVariableType.Int;

        private string swTargetScene = string.Empty;
        private string swEntryGate = string.Empty;

        private void Update()
        {
            HeroController hero = HeroController.instance;
            InputHandler input = InputHandler.Instance;
            if (hero == null || input == null)
                return;
                
            NoClipUpdate(hero, input);
            HandleKeybinds();
        }

        private void OnGUI()
        {
            if (!IsOpen)
                return;

            SetupGUI();

            switch (menuState)
            {
                case MenuStates.Main:
                    DrawMainMenu();
                    break;

                case MenuStates.Abilities:
                    DrawAbilitiesMenu();
                    break;

                case MenuStates.System:
                    DrawDebugMenu();
                    break;

                case MenuStates.Teleport:
                    DrawTeleportMenu();
                    break;

                case MenuStates.PlayerData:
                    DrawPlayerDataMenu();
                    break;

                case MenuStates.Collectables:
                    DrawItemsMenu();
                    break;

                case MenuStates.Tools:
                    DrawToolsMenu();
                    break;

                case MenuStates.Quests:
                    DrawQuestsMenu();
                    break;

                case MenuStates.Achievements:
                    DrawAchievementsMenu();
                    break;
            }
        }

        private static void SetupGUI()
        {
            if (buttonStyle != null)
                return;

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = FontSize,
                wordWrap = true,
                richText = true
            };
        }

        private static Rect Rect(int y, bool isSmall = false, bool isLeftSide = true)
        {
            if (!isSmall)
                return new(XIndent, YIndent + y * (ButtonHeight + SpaceHeight), ButtonWidth, ButtonHeight);

            return new(isLeftSide ? XIndent : 190 * Multiplier, YIndent + y * (ButtonHeight + SpaceHeight), ButtonWidth / 2, ButtonHeight);
        }
        private static bool Button(string text, int y, bool isSmall = false, bool isLeftSide = true) => GUI.Button(Rect(y, isSmall, isLeftSide), text, buttonStyle);
        private static void Label(string text, int y) => GUI.Label(Rect(y), text, LabelStyle);

        private void DrawMainMenu()
        {
            Label("Cheat Manager", 0);

            if (Button("Abilities", 1))
                OpenMenu(MenuStates.Abilities);

            if (Button("System", 2))
                OpenMenu(MenuStates.System);

            if (Button("Teleport", 3))
                OpenMenu(MenuStates.Teleport);

            if (Button("PlayerData", 4))
                OpenMenu(MenuStates.PlayerData);

            if (Button("Items", 5))
                OpenMenu(MenuStates.Collectables);

            if (Button("Tools", 6))
                OpenMenu(MenuStates.Tools);

            if (Button("Quests", 7))
                OpenMenu(MenuStates.Quests);

            if (Button("Achievements", 9))
                OpenMenu(MenuStates.Achievements);

            if (Button($"NoClip: {OnOff(noClip)}", 11, isSmall: true))
                SetNoClip(!noClip);

            if (Button($"Invincibility: {Invincibility}", 11, isSmall: true, isLeftSide: false))
                CycleInvincibility();

            if (Button($"Nail Damage: {NailDamage}", 12))
                if (NailDamage++ > NailDamageStates.InstaKill)
                    NailDamage = 0;
        }

        private void DrawAbilitiesMenu()
        {
            Header("Abilities");

            if (Button($"Damage Self Type: {DamageSelfState}", 1))
                if (DamageSelfState++ > DamageSelfStates.DoubleHit)
                    DamageSelfState = 0;

            if (Button("Damage Self", 2))
            {
                int dmg = DamageSelfState switch
                {
                    DamageSelfStates.SingleHit => 1,
                    DamageSelfStates.DoubleHit => 2,
                    DamageSelfStates.Death => 999,
                    _ => 0
                };

                HeroController.instance?.DamageSelf(dmg);
            }

            if (Button($"Force Stun Enemies: {OnOff(ForceStun)}", 3))
                ForceStun = !ForceStun;

            if (Button($"Force Next Hit Stun", 4))
                ForceNextHitStun = !ForceNextHitStun;

            if (Button($"Frost Damage Disabled: {OnOff(IsFrostDisabled)}", 5))
                IsFrostDisabled = !IsFrostDisabled;

            if (Button($"Silk Drain Disabled: {OnOff(IsSilkDrainDisabled)}", 6))
                IsSilkDrainDisabled = !IsSilkDrainDisabled; // Only used in one fsm action?

            if (Button($"All Abilities", 8))
            {
                var pd = PlayerData.instance;
                if (pd == null)
                    return;

                pd.hasNeedolin = true;
                pd.hasNeedolinMemoryPowerup = true;
                pd.UnlockedFastTravelTeleport = true;

                pd.hasSuperJump = true;
                pd.hasChargeSlash = true;
                pd.hasHarpoonDash = true;

                pd.GetAllPowerups(); // Cloak, Dash, Cling Grip & Faydown
            }
                
            if (Button($"Swift Step", 9))
                PlayerData.instance?.hasDash = !PlayerData.instance.hasDash;

            if (Button($"Cling Grip", 10))
                PlayerData.instance?.hasWalljump = !PlayerData.instance.hasWalljump;

            if (Button($"Needolin", 11))
                PlayerData.instance?.hasNeedolin = !PlayerData.instance.hasNeedolin;

            if (Button($"Elegy of the Deep", 12))
                PlayerData.instance?.hasNeedolinMemoryPowerup = !PlayerData.instance.hasNeedolinMemoryPowerup;

            if (Button($"Beastling Call", 13))
                PlayerData.instance?.UnlockedFastTravelTeleport = !PlayerData.instance.UnlockedFastTravelTeleport;

            if (Button($"Faydown Cloak", 14))
                PlayerData.instance?.hasDoubleJump = !PlayerData.instance.hasDoubleJump;

            if (Button($"Silk Soar", 15))
                PlayerData.instance?.hasSuperJump = !PlayerData.instance.hasSuperJump;

            if (Button($"Needle Strike", 16))
                PlayerData.instance?.hasChargeSlash = !PlayerData.instance.hasChargeSlash;
                
            if (Button($"Drifter's Cloak", 17))
                PlayerData.instance?.hasBrolly = !PlayerData.instance.hasBrolly;

            if (Button($"Clawline", 18))
                PlayerData.instance?.hasHarpoonDash = !PlayerData.instance.hasHarpoonDash;

            BackButton(20);
        }

        private void DrawDebugMenu()
        {
            Header("Debug");

            if (Button($"Field Access Optimisers: {OnOff(UseFieldAccessOptimisers)}", 1))
                UseFieldAccessOptimisers = !UseFieldAccessOptimisers;

            if (Button($"Disable Async Scene Load: {OnOff(DisableAsyncSceneLoad)}", 2))
                DisableAsyncSceneLoad = !DisableAsyncSceneLoad;

            if (Button($"Async Save Load: {OnOff(UseAsyncSaveFileLoad)}", 3))
                UseAsyncSaveFileLoad = !UseAsyncSaveFileLoad;

            if (Button($"Tasks For JSON: {OnOff(UseTasksForJsonConversion)}", 4))
                UseTasksForJsonConversion = !UseTasksForJsonConversion;

            if (Button($"Force Currency Counters: {OnOff(ForceCurrencyCountersAppear)}", 5))
                ForceCurrencyCountersAppear = !ForceCurrencyCountersAppear;

            if (Button($"Dialogue Debug: {OnOff(IsDialogueDebugEnabled)}", 6))
                IsDialogueDebugEnabled = !IsDialogueDebugEnabled;

            if (Button($"World Rumble Disabled: {OnOff(IsWorldRumbleDisabled)}", 7))
                IsWorldRumbleDisabled = !IsWorldRumbleDisabled;

            if (Button($"Fast Text: {OnOff(IsTextPrintSkipEnabled)}", 8))
                IsTextPrintSkipEnabled = !IsTextPrintSkipEnabled;

            Label($"Saving: {OnOff(AllowSaving)}", 10);

            if (Button($"Async Save Load: {OnOff(UseAsyncSaveFileLoad)}", 11))
            {
                UseAsyncSaveFileLoad = !UseAsyncSaveFileLoad;
            }

            BackButton(13);
        }

        private void DrawTeleportMenu()
        {
            Header("Respawn");
            Label($"Respawn scene: '{PlayerData.instance?.respawnScene}'", 1);
            Label($"Respawn marker: '{PlayerData.instance?.respawnMarkerName}'", 2);

            if (Button("Respawn", 3))
                GameManager.instance?.ReadyForRespawn(false);
                
            if (Button("Hazard Respawn", 4))
                GameManager.instance?.HazardRespawn();

            Header("Teleport", 6);

            Label("Target Scene", 7);
            swTargetScene = GUI.TextField(Rect(8), swTargetScene);
            
            Label("Target Scene Entry Gate", 9);
            swEntryGate = GUI.TextField(Rect(10), swEntryGate);

            if (string.IsNullOrEmpty(swTargetScene))
            {
                BackButton(12);
                return;
            }

            if (Button("Load Scene", 11))
                StartCoroutine(LoadScene());

            BackButton(13);
        }

        private IEnumerator LoadScene()
        {
            IsOpen = false;
            if (GameManager.instance.IsGamePaused())
                yield return GameManager.instance.PauseGameToggle(false);

            GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo()
            {
                SceneName = swTargetScene,
                EntryGateName = swEntryGate
            });
        }

        private void DrawPlayerDataMenu()
        {
            Header("PlayerData");

            if (Button("Add Health", 1))
                HeroController.instance?.AddHealth(1);
            
            if (Button("Add Mask", 2))
                HeroController.instance?.AddToMaxHealth(1);

            if (Button("Add Silk", 3))
                HeroController.instance?.AddSilk(1, true);

            if (Button("Add Spool", 4))
                HeroController.instance?.AddToMaxSilk(1);

            if (Button("Add Silk Heart", 5))
                HeroController.instance?.AddToMaxSilkRegen(1);

            Label("Variable Name", 7);
            pdVariableName = GUI.TextField(Rect(8), pdVariableName);
            
            Label("Value", 9);
            pdVariableValue = GUI.TextField(Rect(10), pdVariableValue);

            if (Button($"Type: {pdVariableType}", 11) && pdVariableType++ > PlayerDataVariableType.Bool)
                pdVariableType = 0;

            if (string.IsNullOrEmpty(pdVariableName) || string.IsNullOrEmpty(pdVariableValue))
            {
                BackButton(13);
                return;
            }

            if (Button("Set Variable", 12))
                SetPlayerDataVariable();

            if (Button("Get Variable", 13))
                Debug.Log($"[CheatManagerUI] Result: {GetPlayerDataVariable()}");

            BackButton(15);
        }

        private void SetPlayerDataVariable()
        {
            if (string.IsNullOrWhiteSpace(pdVariableName))
                return;

            object value;

            switch (pdVariableType)
            {
                case PlayerDataVariableType.Int:
                    if (!int.TryParse(pdVariableValue, out int intValue))
                        return;

                    value = intValue;
                    break;

                case PlayerDataVariableType.Float:
                    if (!float.TryParse(pdVariableValue, out float floatValue))
                        return;

                    value = floatValue;
                    break;

                case PlayerDataVariableType.Bool:
                    if (!bool.TryParse(pdVariableValue, out bool boolValue))
                        return;

                    value = boolValue;
                    break;

                case PlayerDataVariableType.String:
                    value = pdVariableValue;
                    break;

                default:
                    return;
            }

            PlayerData.instance?.SetVariable(pdVariableName, value, value.GetType());
        }

        private object? GetPlayerDataVariable()
        {
            if (string.IsNullOrWhiteSpace(pdVariableName))
                return null;

            Type? type = pdVariableType switch
            {
                PlayerDataVariableType.Int => typeof(int),
                PlayerDataVariableType.Float => typeof(float),
                PlayerDataVariableType.Bool => typeof(bool),
                PlayerDataVariableType.String => typeof(string),
                
                _ => null
            };

            return PlayerData.instance?.GetVariable(pdVariableName, type);
        }

        private void DrawItemsMenu()
        {
            Header("Items");

            // Needs to be expanded
            if (Button($"All Items", 1))
            {
                var pd = PlayerData.instance;
                if (pd == null)
                    return;

                pd.HasMelodyArchitect = true;
                pd.HasMelodyConductor = true;
                pd.HasMelodyLibrarian = true;

                pd.HasSlabKeyA = true;
                pd.HasSlabKeyB = true;
                pd.HasSlabKeyC = true;

                CollectableItemManager.GetItemByName("Coral Heart").Collect();
                CollectableItemManager.GetItemByName("Flower Heart").Collect();
                CollectableItemManager.GetItemByName("Hunter Heart").Collect();
                CollectableItemManager.GetItemByName("Clover Heart").Collect();

                CollectableItemManager.GetItemByName("White Flower").Collect();
                
                CollectableItemManager.GetItemByName("Ward Key").Collect();
                CollectableItemManager.GetItemByName("Ward Boss Key").Collect();
                CollectableItemManager.GetItemByName("Architect Key").Collect();
                CollectableItemManager.GetItemByName("Dock Key").Collect();
                CollectableItemManager.GetItemByName("Belltown House Key").Collect();
                CollectableItemManager.GetItemByName("Craw Summons").Collect();
                
                CollectableItemManager.GetItemByName("Silk Grub").Collect(10);
                CollectableItemManager.GetItemByName("Crest Socket Unlocker").Collect(32);
                CollectableItemManager.GetItemByName("Simple Key").Collect(10);
                CollectableItemManager.GetItemByName("Pale_Oil").Collect(10);
                CollectableItemManager.GetItemByName("Tool Metal").Collect(10);
                
                pd.CollectedHeartClover = true;
                pd.CollectedHeartCoral = true;
                pd.CollectedHeartFlower = true;
                pd.CollectedHeartHunter = true;

                pd.hasQuill = true;
            }

            BackButton(3);
        }

        private void DrawToolsMenu()
        {
            Header("Tools");

            if (Button("Unlock All Tools", 1))
                ToolItemManager.UnlockAllTools();

            if (Button("Unlock All Crests", 2))
                ToolItemManager.UnlockAllCrests();

            if (Button("Replenish Tools", 3))
                ToolItemManager.TryReplenishTools(true, ToolItemManager.ReplenishMethod.Bench);

            if (Button($"Change Equips Anywhere: {OnOff(CanChangeEquipsAnywhere)}", 4))
                CanChangeEquipsAnywhere = !CanChangeEquipsAnywhere;

            BackButton(6);
        }

        private void DrawQuestsMenu()
        {
            Header("Quests");

            if (Button($"Show All Quest Board Quests: {OnOff(ShowAllQuestBoardQuest)}", 1))
                ShowAllQuestBoardQuest = !ShowAllQuestBoardQuest;

            if (Button($"Complete All Active Quests", 2))
                foreach (FullQuestBase allFullQuest in QuestManager.GetAllFullQuests())
                    if (allFullQuest.IsAccepted || allFullQuest.IsCompleted)
                        allFullQuest.SilentlyComplete();

            BackButton(4);
        }

        private void DrawAchievementsMenu()
        {
            Header("Achievements");

            if (Button($"Always Award: {OnOff(AlwaysAwardAchievement)}", 1))
                AlwaysAwardAchievement = !AlwaysAwardAchievement;

            if (Button("Award All Achievements", 2))
                GameManager.instance?.achievementHandler.AwardAllAchievements();

            /*

            ShowAllCompletionIcons is not used because it also checks IsCheatsEnabled

            if (Button($"Show All Completion Icons: {OnOff(ShowAllCompletionIcons)}", 3))
                ShowAllCompletionIcons = !ShowAllCompletionIcons;

            */

            BackButton(4);
        }

        private static void Header(string text, int y = 0) => Label(text, y);

        private void BackButton(int y)
        {
            if (Button("< Back", y))
                menuState = MenuStates.Main;
        }

        private void OpenMenu(MenuStates state) => menuState = state;

        private static void CycleInvincibility()
        {
            Invincibility++;
            if (Invincibility == InvincibilityStates.TestInvincible) 
                Invincibility++;

            if (Invincibility > InvincibilityStates.PreventDeath)
                Invincibility = 0;

            PlayerData.instance?.isInvincible = Invincibility != InvincibilityStates.Off;
        }

        private void SetNoClip(bool value)
        {
            noClip = value;

            if (!noClip)
                heroRigidbody?.constraints &= ~RigidbodyConstraints2D.FreezePosition;
        }

        /// <summary>
        /// Noclip movement logic adapted from hk-speedrunning/Silksong.DebugMod under MIT:
        /// https://github.com/hk-speedrunning/Silksong.DebugMod/blob/main/GUIController.cs#L294
        /// </summary>
        /// <param name="hero"></param>
        /// <param name="input"></param>
        private void NoClipUpdate(HeroController hero, InputHandler input)
        {
            if (!noClip)
                return;
            
            Vector3 offset = Vector3.zero;
            float speed = Input.GetKey(KeyCode.LeftShift) ? 40f : 20f;
            float distance = speed * Time.deltaTime;

            if (input.inputActions.Left.IsPressed)
                offset += Vector3.left * distance;

            if (input.inputActions.Right.IsPressed)
                offset += Vector3.right * distance;

            if (input.inputActions.Up.IsPressed)
                offset += Vector3.up * distance;

            if (input.inputActions.Down.IsPressed)
                offset += Vector3.down * distance;

            if (heroRigidbody == null)
                heroRigidbody = hero.GetComponent<Rigidbody2D>();

            if (hero.transitionState == GlobalEnums.HeroTransitionState.WAITING_TO_TRANSITION)
            {
                hero.transform.position += offset;
                heroRigidbody.constraints |= RigidbodyConstraints2D.FreezePosition;
            }
            else
                heroRigidbody.constraints &= ~RigidbodyConstraints2D.FreezePosition;
        }

        private void HandleKeybinds()
        {
            if (Input.GetKeyDown(KeyCode.Keypad1) || (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.L)))
            {
                IsOpen = !IsOpen;
            }

            if (Input.GetKeyDown(WorldWeaverSettings.Instance.PlayBootstrapInvincibilityKey))
                CycleInvincibility();

            if (Input.GetKeyDown(WorldWeaverSettings.Instance.PlayBootstrapNoClipKey))
                SetNoClip(!noClip);
        }

        private static string OnOff(bool value)
        {
            return value ? "On" : "Off";
        }
    }
}