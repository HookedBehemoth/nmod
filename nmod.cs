/*
 * Copyright (c) 2021-2022 HookedBehemoth
 *
 * This program is free software; you can redistribute it and/or modify it
 * under the terms and conditions of the GNU General Public License,
 * version 3, as published by the Free Software Foundation.
 *
 * This program is distributed in the hope it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for
 * more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using BepInEx.Configuration;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

using VRC.Core;
using VRC.SDKBase;

using ActionMenuDriver = MonoBehaviourPublicObGaObAc1ObAcBoCoObUnique;
using ActionMenuOpener = MonoBehaviourPublicSiObSiCaBoSiAcObBo1Unique;
using ActionMenuType = MonoBehaviourPublicSiObSiCaBoSiAcObBo1Unique.EnumNPublicSealedvaLeRi3vUnique;
using SelectedOutline = MonoBehaviourPublicInLi1MeHaInMeRe1MeUnique;
using RoomManager = MonoBehaviourPublicBoApSiApBoObStApBo1Unique;
using Il2CppSystem.Collections.Generic;
using VRCMotionState = MonoBehaviourPublicLaSiBoSiChBoObVeBoSiUnique;
using TextMeshProText = TextMeshProUGUIPublicLo_lLa_cBoOb_c_p1InUnique;
using UnityEngine.Playables;

namespace NMod;

public class PluginComponent : MonoBehaviour
{
    public void Start()
    {
        VM.Logger.LogInfo("NMod loaded!");
        StartCoroutine(WaitForUiManager().WrapToIl2Cpp());
        StartCoroutine(SpamPropsOnPlayer().WrapToIl2Cpp());
    }

    public void Update()
    {
        Flight.Update();
        ESP.Update();
    }

    public class ESP
    {
        private static bool _ESPEnabled = false;

        private static float timer;

        public static bool ESPEnabled
        {
            get
            {
                return _ESPEnabled;
            }
            set
            {
                _ESPEnabled = value;
                ApplyState();
            }
        }

        private static void ApplyState()
        {
            foreach (var rootObject in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (rootObject.name.Contains("VRCPlayer[Remote]"))
                {
                    try {
                        Transform child = rootObject.transform.Find("SelectRegion");
                        var renderer = child.GetComponent<Renderer>();
                        SelectedOutline.Method_Internal_Static_Void_Renderer_Boolean_PDM_0(renderer, _ESPEnabled);
                    } catch {}
                }
            }
        }
        public static void Update()
        {
            timer += Time.deltaTime;
            if (!(timer < 3f))
            {
                timer -= 3f;
                ApplyState();
            }
        }
    }
    internal class Flight
    {
        public static bool FlightEnabled = false;
        private static GameObject localPlayer;
        private static VRCMotionState motionState;
        // private static InputStateController stateController;
        private static CharacterController characterController;
        private static Vector3 originalGravity;
        public static VRCPlayerApi stuckPlayer = null;
        public static int stuckPlayerPos = 0;
        public static bool propSpam = false;
        public static VRCPlayerApi spammedPlayer = null;

        public static bool NoclipEnabled
        {
            get
            {
                return !characterController.enabled;
            }
            set
            {
                characterController.enabled = !value;
            }
        }

        private static MonoBehaviour1PublicOb_pOb_sBo_p_sGaObStUnique LocalPlayer
        {
            get
            {
                return MonoBehaviour1PublicOb_pOb_sBo_p_sGaObStUnique.field_Internal_Static_MonoBehaviour1PublicOb_pOb_sBo_p_sGaObStUnique_0;
            }
        }

        public static void Update()
        {
            if (RoomManager.field_Internal_Static_ApiWorld_0 == null || LocalPlayer == null)
            {
                return;
            }
            if (localPlayer == null)
            {
                localPlayer = LocalPlayer.gameObject;
                if (localPlayer == null)
                {
                    return;
                }
                if (motionState == null)
                {
                    motionState = localPlayer.GetComponent<VRCMotionState>();
                }
                // if (stateController == null)
                // {
                //     stateController = localPlayer.GetComponent<InputStateController>();
                // }
                if (characterController == null)
                {
                    characterController = localPlayer.GetComponent<CharacterController>();
                }
            }
            if (FlightEnabled)
            {
                if (Physics.gravity != Vector3.zero)
                {
                    originalGravity = Physics.gravity;
                    Physics.gravity = Vector3.zero;
                }
                var val = VRCPlayerApi.AllPlayers.Find((Il2CppSystem.Predicate<VRCPlayerApi>)(x => x.isLocal));
                if (!val.IsValid())
                {
                    return;
                }
                Vector3 val2 = Vector3.zero;
                if (IsXrPresent)
                {
                    var driver = ActionMenuExtra.GetDriver();
                    float num = Time.deltaTime * val.GetRunSpeed();
                    if (!driver.GetLeftOpener().isOpen())
                    {
                        val2 += localPlayer.transform.forward * Input.GetAxis("Vertical") * num;
                        val2 += localPlayer.transform.right * Input.GetAxis("Horizontal") * num;
                    }
                    if (!driver.GetRightOpener().isOpen())
                    {
                        val2 += new Vector3(0f, Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickVertical") * num);
                    }
                }
                else
                {
                    float num2 = Time.deltaTime * (Input.GetKey((KeyCode)304) ? val.GetRunSpeed() : val.GetWalkSpeed());
                    val2 += Camera.main.transform.forward * Input.GetAxis("Vertical") * num2;
                    val2 += Camera.main.transform.right * Input.GetAxis("Horizontal") * num2;
                    if (Input.GetKey((KeyCode)113))
                    {
                        val2 -= new Vector3(0f, num2);
                    }
                    if (Input.GetKey((KeyCode)101))
                    {
                        val2 += new Vector3(0f, num2);
                    }
                }
                if (NoclipEnabled)
                {
                    localPlayer.transform.position += val2;
                }
                else
                {
                    localPlayer.transform.position += new Vector3(0f, val2.y);
                }
                if (motionState != null)
                {
                    motionState.Reset();
                }
                // if (stateController != null && !NoclipEnabled)
                // {
                //     stateController.ResetLastPosition();
                // }
            }
            else if (originalGravity != Vector3.zero)
            {
                Physics.gravity = originalGravity;
                originalGravity = Vector3.zero;
            }
            try {
                if (Flight.stuckPlayer != null) {
                    var self = VRCPlayerApi.AllPlayers.Find((Il2CppSystem.Predicate<VRCPlayerApi>)(x => x.isLocal));

                    if (stuckPlayerPos == 0) {
                        //self.TeleportTo(stuckPlayer.GetPosition(), self.GetRotation());
                        self.gameObject.transform.position = stuckPlayer.GetPosition();
                        self.gameObject.transform.rotation = self.GetRotation();
                    } else {
                        Vector3 stuckPlayerForward = stuckPlayer.GetBoneTransform(UnityEngine.HumanBodyBones.Head).forward;
                        stuckPlayerForward.x = stuckPlayerForward.x * stuckPlayerPos;
                        stuckPlayerForward.z = stuckPlayerForward.z * stuckPlayerPos;
                        stuckPlayerForward.y = 0;
                        //self.TeleportTo(stuckPlayer.GetPosition() + stuckPlayerForward, self.GetRotation());
                        self.gameObject.transform.position = stuckPlayer.GetPosition() + stuckPlayerForward;
                        self.gameObject.transform.rotation = self.GetRotation();
                    }
                }
            } catch {
                Flight.stuckPlayer = null;
            }
        }

        public static bool? _isXrPresent = null;

        public static bool IsXrPresent
        {
            get
            {
                if (_isXrPresent is bool present)
                {
                    return present;
                }

                var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
                SubsystemManager.GetInstances(xrDisplaySubsystems);
                foreach (var xrDisplay in xrDisplaySubsystems)
                {
                    if (xrDisplay.running)
                    {
                        return (_isXrPresent = true).Value;
                    }
                }
                return (_isXrPresent = false).Value;
            }
        }
    }
    // public override void OnInitializeMelon()
    //     => MelonCoroutines.Start(WaitForUiManager());

    private static System.Collections.IEnumerator WaitForUiManager()
    {
        /* Wait for VRCUiManager init */
        while (GameObject.Find("Canvas_MainMenu(Clone)/Container/Wing_Right") == null)
            yield return new WaitForSeconds(1f);

        GameObject flightButton = CreateButton("Flight", GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_QM_Launchpad/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickLinks_A/LayoutGuide"), () =>
        {
            Flight.FlightEnabled = !Flight.FlightEnabled;
            if (Flight.FlightEnabled == false) Flight.NoclipEnabled = false;
        });
        flightButton.transform.localPosition = new Vector3(-300, -300, 0);

        GameObject noclipButton = CreateButton("Noclip", GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_QM_Launchpad/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickLinks_A/LayoutGuide"), () =>
        {
            Flight.NoclipEnabled = !Flight.NoclipEnabled;
            if (Flight.FlightEnabled == false) Flight.NoclipEnabled = false;
        });
        noclipButton.transform.localPosition = new Vector3(0, -300, 0);

        GameObject espButton = CreateButton("ESP", GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_QM_Launchpad/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickLinks_A/LayoutGuide"), () =>
        {
            ESP.ESPEnabled = !ESP.ESPEnabled;
        });
        espButton.transform.localPosition = new Vector3(300, -300, 0);

        //CreateButton("Purchase all (Fix it!!!)", GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_QM_Launchpad/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickLinks_A"), EconomyUdonBehaviourPath.PurchaseAll);

        GameObject autosexButton = CreateButton("Auto-Sex", GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_QM_Launchpad/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickLinks_A/LayoutGuide"), () => { AutoMeshToggle(); });
        autosexButton.transform.localPosition = new Vector3(0, -500, 0);

        CreateMainMenu();
    }
    private static System.Collections.IEnumerator SpamPropsOnPlayer()
    {
        while (true) {
            if (Flight.propSpam && Flight.spammedPlayer != null)
            {
                try {
                    TeleportPickups(Flight.spammedPlayer);
                }
                catch {
                    Flight.stuckPlayerPos = 0;
                    Flight.propSpam = false;
                }
                yield return new WaitForSeconds(0.25f);
            } else {
                yield return new WaitForSeconds(1f);
            }
        }
    }
    public static void CreateMainMenu()
    {
        //TELEPORT
        GameObject RightExploreMenu = GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Right/Container/InnerContainer/Explore/ScrollRect/Viewport/VerticalLayoutGroup");
        ClearMenu(RightExploreMenu);
        RightExploreMenu.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().m_ChildControlHeight = true;
        CreateButton("Reload Player List", GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Right/Container/InnerContainer/Explore/ScrollRect/Viewport/VerticalLayoutGroup"), CreateTeleportButton);
        CreateSeparator(RightExploreMenu);

        //CHEATS
        GameObject LeftExploreMenu = GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Left/Container/InnerContainer/Explore/ScrollRect/Viewport/VerticalLayoutGroup");
        ClearMenu(LeftExploreMenu);
        LeftExploreMenu.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().m_ChildControlHeight = true;
        CreateButton("Cheats", LeftExploreMenu, CreateCheatsMenu);
        CreateButton("Among Us", LeftExploreMenu, CreateAmongUsMenu);
        CreateButton("Murder", LeftExploreMenu, CreateMurderMenu);
        CreateButton("Sticky Stuckle", LeftExploreMenu, CreateStickyMenu);
        CreateButton("Pickups", LeftExploreMenu, CreatePickupMenu);
        CreateButton("Toggles", LeftExploreMenu, TogglesMenu);

        //Useless fluff at the bottom so we don't stop execution if the game gets updated and shit breaks
        GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_QM_Launchpad/Header_H1/LeftItemContainer/Text_Title").GetComponent<TextMeshProText>().prop_String_0 = VM.menuText.Value + " v" + MyPluginInfo.PLUGIN_VERSION;
        GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_QM_Launchpad/Header_H1/LeftItemContainer/Text_Title").GetComponent<TextMeshProText>().color = new Color(255, 0, 0, 255);
        GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Right/Container/InnerContainer/Explore/WngHeader_H1/LeftItemContainer/Text_Title").GetComponent<TextMeshProText>().prop_String_0 = "Teleport";
        GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Right/Container/InnerContainer/WingMenu/ScrollRect/Viewport/VerticalLayoutGroup/Button_Explore/Container/Text_QM_H3").GetComponent<TextMeshProText>().prop_String_0 = "Teleport";
        GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Left/Container/InnerContainer/Explore/WngHeader_H1/LeftItemContainer/Text_Title").GetComponent<TextMeshProText>().prop_String_0 = "Elite Hacks";
        GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Left/Container/InnerContainer/WingMenu/ScrollRect/Viewport/VerticalLayoutGroup/Button_Explore/Container/Text_QM_H3").GetComponent<TextMeshProText>().prop_String_0 = "Elite Hacks";
        //Big menu wings are stupid and useless and I'm not adding buttons to the menus twice
        GameObject.Find("Canvas_MainMenu(Clone)/Container/Wing_Right").active = false;
        GameObject.Find("Canvas_MainMenu(Clone)/Container/Wing_Left").active = false;
    }
    public static void ClearMenu(GameObject ourMenu)
    {
        for (int i = ourMenu.transform.childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(ourMenu.transform.GetChild(i).gameObject);
        }
    }
    public static void CreateTeleportButton()
    {
        GameObject ourMenu = GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Right/Container/InnerContainer/Explore/ScrollRect/Viewport/VerticalLayoutGroup");
        ClearMenu(ourMenu);
        CreateButton("Reload Player List", ourMenu, CreateTeleportButton);
        CreateSeparator(ourMenu);
        var self = VRCPlayerApi.AllPlayers.Find((Il2CppSystem.Predicate<VRCPlayerApi>)(x => x.isLocal));

        foreach (var player in GetPlayersSorted())
        {
            if (player.displayName != self.displayName) {
                CreateButton(player.displayName, ourMenu, () =>
                {
                    self.TeleportTo(player.GetPosition(), player.GetRotation());
                });
            }
        }
    }
    public static void CreateStickyMenu()
    {
        GameObject LeftExploreMenu = GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Left/Container/InnerContainer/Explore/ScrollRect/Viewport/VerticalLayoutGroup");
        ClearMenu(LeftExploreMenu);
        CreateButton("Back", LeftExploreMenu, CreateMainMenu);
        CreateButton("Stop Being a Shticky", LeftExploreMenu, () =>
        {
            Flight.stuckPlayer = null;
        });
        CreateButton("On Top", LeftExploreMenu, () =>
        {
            Flight.stuckPlayerPos = 0;
        });
        CreateButton("In front", LeftExploreMenu, () =>
        {
            Flight.stuckPlayerPos = 2;
        });
        CreateButton("Behind", LeftExploreMenu, () =>
        {
            Flight.stuckPlayerPos = -3;
        });
        CreateButton("Reload Player List", LeftExploreMenu, CreateStickyMenu);
        CreateSeparator(LeftExploreMenu);
        var self = VRCPlayerApi.AllPlayers.Find((Il2CppSystem.Predicate<VRCPlayerApi>)(x => x.isLocal));

        foreach (var player in GetPlayersSorted())
        {
            if (player.displayName != self.displayName) {
                CreateButton(player.displayName, LeftExploreMenu, () =>
                {
                    Flight.stuckPlayer = player;
                });
            }
        }
    }
    public static void TogglesMenu()
    {
        GameObject LeftExploreMenu = GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Left/Container/InnerContainer/Explore/ScrollRect/Viewport/VerticalLayoutGroup");
        ClearMenu(LeftExploreMenu);
        CreateButton("Back", LeftExploreMenu, CreateMainMenu);
        CreateButton("Auto-Sex All", LeftExploreMenu, () => {AutoMeshToggle();});
        CreateButton("Reload Player List", LeftExploreMenu, TogglesMenu);
        CreateSeparator(LeftExploreMenu);
        var self = VRCPlayerApi.AllPlayers.Find((Il2CppSystem.Predicate<VRCPlayerApi>)(x => x.isLocal));
        foreach (var player in GetPlayersSorted())
        {
            if (player.displayName != self.displayName) {
                CreateButton(player.displayName, LeftExploreMenu, () =>
                {
                    ClearMenu(LeftExploreMenu);
                    CreateButton("Back", LeftExploreMenu, TogglesMenu);
                    CreateButton("Toggle Animator", LeftExploreMenu, () =>
                    {
                        var ourAnimator = GameObject.Find(player.gameObject.name + "/ForwardDirection/Avatar").GetComponentsInChildren<Animator>();
                        ourAnimator[0].enabled = !ourAnimator[0].enabled;
                    });
                    CreateButton("Meshes", LeftExploreMenu, () =>
                    {
                        CreateMeshToggleMenu(LeftExploreMenu, player);
                    });
                    CreateButton("Blendshapes (WIP)", LeftExploreMenu, () =>
                    {
                        ClearMenu(LeftExploreMenu);
                        CreateButton("Back", LeftExploreMenu, TogglesMenu);
                        var ourSkinnedMeshes = GameObject.Find(player.gameObject.name + "/ForwardDirection/Avatar").GetComponentsInChildren<SkinnedMeshRenderer>(true);
                        foreach (var skinnedMesh in ourSkinnedMeshes)
                        {
                            if (skinnedMesh.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount > 0) {
                                CreateButton(skinnedMesh.name, LeftExploreMenu, () =>
                                {
                                    ClearMenu(LeftExploreMenu);
                                    CreateButton("Back", LeftExploreMenu, TogglesMenu);
                                    CreateSeparator(LeftExploreMenu);
                                    var skinnedMeshRenderer = skinnedMesh.GetComponent<SkinnedMeshRenderer>();
                                    for (int i = 0; i <= skinnedMeshRenderer.sharedMesh.blendShapeCount - 1; i++) {
                                        CreateButton(skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i) + " (" + skinnedMeshRenderer.GetBlendShapeWeight(i) + ")", LeftExploreMenu, () =>
                                        {
                                            // Behemoth, why doesn't this work but the same thing works fine in unityexplorer??
                                            skinnedMeshRenderer.SetBlendShapeWeight(i,100f);
                                        });
                                    }
                                });
                            }
                        }
                    });
                    CreateButton("Parameters (WIP)", LeftExploreMenu, () =>
                    {
                        ClearMenu(LeftExploreMenu);
                        CreateButton("Back", LeftExploreMenu, TogglesMenu);
                        CreateSeparator(LeftExploreMenu);
                        var playerGameobject = GameObject.Find(player.gameObject.name + "/ForwardDirection");
                        CreateSeparator(LeftExploreMenu, "--- Bool ---");
                        for (int i = 0; i <= playerGameobject.GetComponentsInChildren<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>()[0].GetExpressionParameterCount() - 1; i++) {   
                            var ourParameter = playerGameobject.GetComponentsInChildren<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>()[0].GetExpressionParameter(i);
                            if (ourParameter.valueType == VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType.Bool) {
                                CreateButton(ourParameter.name + " (true)", LeftExploreMenu, () =>
                                {
                                    foreach (var animator in player.gameObject.GetComponentsInChildren<UnityEngine.Animator>())
                                    {
                                        animator.SetBoolString(ourParameter.name, true);
                                    }
                                });
                                CreateButton(ourParameter.name + " (false)", LeftExploreMenu, () =>
                                {
                                    foreach (var animator in player.gameObject.GetComponentsInChildren<UnityEngine.Animator>())
                                    {
                                        animator.SetBoolString(ourParameter.name, false);
                                    }
                                });
                            }
                        }
                    });
                });
            }
        }

    }
    public static void CreateMeshToggleMenu(GameObject LeftExploreMenu, VRCPlayerApi player, string removeThisButton = null)
    {
        ClearMenu(LeftExploreMenu);
        CreateButton("Back", LeftExploreMenu, TogglesMenu);
        CreateButton("Auto-Sex", LeftExploreMenu, () => {AutoMeshToggle(player.displayName);});
        CreateSeparator(LeftExploreMenu, "- SkinnedMeshes -");

        var ourSkinnedMeshes = GameObject.Find(player.gameObject.name + "/ForwardDirection/Avatar").GetComponentsInChildren<SkinnedMeshRenderer>(true);
        var ourMeshes = GameObject.Find(player.gameObject.name + "/ForwardDirection/Avatar").GetComponentsInChildren<MeshRenderer>(true);
        var skinnedMeshNames = new List<string>();
        var meshNames = new List<string>();
        foreach (var skinnedMesh in ourSkinnedMeshes)
        {
            skinnedMeshNames.Add(skinnedMesh.gameObject.name);
        }
        foreach (var mesh in ourMeshes)
        {
            meshNames.Add(mesh.gameObject.name);
        }
        skinnedMeshNames.Sort();
        for (int i = 0; i <= skinnedMeshNames.Count - 1; i++) {
            foreach (var skinnedMesh in ourSkinnedMeshes)
            {
                if (skinnedMesh.gameObject.name == skinnedMeshNames[i] && skinnedMesh.gameObject.name != removeThisButton)
                {
                    CreateMeshToggleButton(LeftExploreMenu, skinnedMesh.gameObject, player);
                }
            }
        }
        meshNames.Sort();
        CreateSeparator(LeftExploreMenu, "--- Meshes ---");
        for (int i = 0; i <= meshNames.Count - 1; i++) {
            foreach (var mesh in ourMeshes)
            {
                if (mesh.gameObject.name == meshNames[i] && mesh.gameObject.name != removeThisButton)
                {
                    CreateMeshToggleButton(LeftExploreMenu, mesh.gameObject, player);
                }
            }
        }
    }
    public static void CreateMeshToggleButton(GameObject LeftExploreMenu, GameObject mesh, VRCPlayerApi player)
    {
        string ignoreID = "3298479235"; //CHICKEN JOCKEY!
        CreateButton(mesh.name + " (" + mesh.active + ")", LeftExploreMenu, () =>
        {
            GameObject newMesh = GameObject.Instantiate(mesh);
            newMesh.name = mesh.name;
            newMesh.transform.parent = mesh.transform.parent;
            newMesh.active = !newMesh.active;
            mesh.name = ignoreID; //destroying the game object doesn't instantly remove it unfortunately
            GameObject.Destroy(mesh, 0);
            CreateMeshToggleMenu(LeftExploreMenu, player, ignoreID);
        });
    }
    public static void AutoMeshToggle(string specificPlayer = null)
    {
        string[] hideKeywords = {"shirt", "pants", "socks", "bra", "panties", "garters", "hoodie", "tank", "shorts", "pasties", "sweater", "shoes", "skirt", "bandage", "dress", "strap", "scarf", "garter", "choker", "collar", "necklace", "bikini", "bodysuit", "top", "panty", "stocking", "harness", "turtleneck", "vans", "fishnet", "bag", "blous", "corset", "tights", "thong", "jeans", "fishnets", "jacket", "belt", "bunnysuit", "bottom","uniform", "underwear", "yamakaze", "cloth", "thighhigh", "skukumizu", "jacket", "parka", "stayhome"};
        string[] showKeywords = {"penis", "cock", "dildo", "strap-on", "strapon", "skin", "nipple", "フタ", "dick", "xxx", "lewd", "18+", "sex", "boob", "nipless"};
        var self = VRCPlayerApi.AllPlayers.Find((Il2CppSystem.Predicate<VRCPlayerApi>)(x => x.isLocal));
        foreach (var player in VRCPlayerApi.AllPlayers)
        {
            if (player.displayName != self.displayName)
            {
                if (specificPlayer != null && player.displayName != specificPlayer) continue;
                var ourSkinnedMeshes = GameObject.Find(player.gameObject.name + "/ForwardDirection/Avatar").GetComponentsInChildren<SkinnedMeshRenderer>(true);
                var ourMeshes = GameObject.Find(player.gameObject.name + "/ForwardDirection/Avatar").GetComponentsInChildren<MeshRenderer>(true);
                foreach (var skinnedMesh in ourSkinnedMeshes)
                {
                    foreach (string x in hideKeywords)
                    {
                        if (skinnedMesh.name.IndexOf(x, 0, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            try {
                            GameObject newMesh = GameObject.Instantiate(skinnedMesh.gameObject);
                            newMesh.name = skinnedMesh.name;
                            newMesh.transform.parent = skinnedMesh.transform.parent;
                            newMesh.active = newMesh.active = false;
                            GameObject.Destroy(skinnedMesh.gameObject, 0);
                            } catch{}
                        }
                    }
                    foreach (string x in showKeywords)
                    {
                        if (skinnedMesh.name.IndexOf(x, 0, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            try {
                            GameObject newMesh = GameObject.Instantiate(skinnedMesh.gameObject);
                            newMesh.name = skinnedMesh.name;
                            newMesh.transform.parent = skinnedMesh.transform.parent;
                            newMesh.active = newMesh.active = true;
                            GameObject.Destroy(skinnedMesh.gameObject, 0);
                            } catch{}
                        }
                    }
                }
                foreach (var mesh in ourMeshes)
                {
                    foreach (string x in hideKeywords)
                    {
                        if (mesh.name.IndexOf(x, 0, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            try {
                            GameObject newMesh = GameObject.Instantiate(mesh.gameObject);
                            newMesh.name = mesh.name;
                            newMesh.transform.parent = mesh.transform.parent;
                            newMesh.active = newMesh.active = false;
                            GameObject.Destroy(mesh.gameObject, 0);
                            } catch{}
                        }
                    }
                    foreach (string x in showKeywords)
                    {
                        if (mesh.name.IndexOf(x, 0, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            try {
                            GameObject newMesh = GameObject.Instantiate(mesh.gameObject);
                            newMesh.name = mesh.name;
                            newMesh.transform.parent = mesh.transform.parent;
                            newMesh.active = newMesh.active = true;
                            GameObject.Destroy(mesh.gameObject, 0);
                            } catch{}
                        }
                    }
                }
            }
        }
        
    }
    public static void CreatePickupMenu()
    {
        GameObject LeftExploreMenu = GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Left/Container/InnerContainer/Explore/ScrollRect/Viewport/VerticalLayoutGroup");
        ClearMenu(LeftExploreMenu);
        CreateButton("Back", LeftExploreMenu, CreateMainMenu);
        if (Flight.spammedPlayer == null)
            CreateButton("SPAM (Select a Player!)", LeftExploreMenu, CreatePickupMenu);
        else if (Flight.propSpam)
            CreateButton("SPAMMING! (" + Flight.spammedPlayer.displayName + ")", LeftExploreMenu, () => {
                Flight.propSpam = !Flight.propSpam;
                CreatePickupMenu();
            });
        else
            CreateButton("SPAM (" + Flight.spammedPlayer.displayName + ")", LeftExploreMenu, () => {
                Flight.propSpam = !Flight.propSpam;
                CreatePickupMenu();
            });
        CreateButton("Reload Player List", LeftExploreMenu, CreatePickupMenu);
        CreateSeparator(LeftExploreMenu);
        foreach (var player in GetPlayersSorted())
        {
            CreateButton(player.displayName, LeftExploreMenu, () =>
            {
                Flight.spammedPlayer = player;
                if (Flight.propSpam) return;
                TeleportPickups(player);
            });
        }
    }
    public static void TeleportPickups(VRCPlayerApi player)
    {
        var pickups = UnityEngine.Object.FindObjectsOfType<VRC_Pickup>();
        var self = VRCPlayerApi.AllPlayers.Find((Il2CppSystem.Predicate<VRCPlayerApi>)(x => x.isLocal));
        try {
            GameObject posObj = new GameObject();
            Destroy(posObj, 10f);
            foreach (var pickup in pickups) {
                if (pickup == null) continue;
                VRC.SDKBase.Networking.SetOwner(self, pickup.gameObject);
                Vector3 playerPosition = player.GetPosition();
                Vector3 targetRandomPosition = UnityEngine.Random.onUnitSphere*(player.GetAvatarEyeHeightAsMeters()*0.75f);
                targetRandomPosition.x = targetRandomPosition.x + playerPosition.x;
                targetRandomPosition.y = targetRandomPosition.y + playerPosition.y + (player.GetAvatarEyeHeightAsMeters() / 2) + 0.4f;
                targetRandomPosition.z = targetRandomPosition.z + playerPosition.z;
                posObj.transform.position = targetRandomPosition;
                try {
                    // if the map disables this we can do it anyway
                    pickup.gameObject.GetComponent<VRC.SDK3.Components.VRCObjectSync>().SetGravity(true);
                    pickup.gameObject.GetComponent<VRC.SDK3.Components.VRCObjectSync>().TeleportTo(posObj.transform);
                } catch {}
            }
        }catch {
            foreach (var pickup in pickups) {
                if (pickup == null) continue;
                VRC.SDKBase.Networking.SetOwner(self, pickup.gameObject);
                try {
                    // if the map disables this we can do it anyway
                    pickup.gameObject.GetComponent<VRC.SDK3.Components.VRCObjectSync>().SetGravity(true);
                    pickup.gameObject.GetComponent<VRC.SDK3.Components.VRCObjectSync>().TeleportTo(player.gameObject.transform);
                } catch {}
            }
        }
    }
    public static void CreateCheatsMenu()
    {
        var player = Networking.LocalPlayer;
        GameObject LeftExploreMenu = GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Left/Container/InnerContainer/Explore/ScrollRect/Viewport/VerticalLayoutGroup");
        ClearMenu(LeftExploreMenu);
        CreateButton("Back", LeftExploreMenu, CreateMainMenu);
        CreateSeparator(LeftExploreMenu);
        CreateButton("Increase Jump Height", LeftExploreMenu, () =>
        {
            player.SetJumpImpulse(player.GetJumpImpulse() + 1);
        });
        CreateButton("Decrease Jump Height", LeftExploreMenu, () =>
        {
            player.SetJumpImpulse(player.GetJumpImpulse() - 1);
        });
        CreateButton("Increase Run Speed", LeftExploreMenu, () =>
        {
            player.SetRunSpeed(player.GetRunSpeed() + 1);
        });
        CreateButton("Decrease Run Speed", LeftExploreMenu, () =>
        {
            player.SetRunSpeed(player.GetRunSpeed() - 1);
        });
        CreateButton("Increase Walk Speed", LeftExploreMenu, () =>
        {
            player.SetWalkSpeed(player.GetWalkSpeed() + 1);
        });
        CreateButton("Decrease Walk Speed", LeftExploreMenu, () =>
        {
            player.SetWalkSpeed(player.GetWalkSpeed() - 1);
        });
        CreateButton("Increase Strafe Speed", LeftExploreMenu, () =>
        {
            player.SetStrafeSpeed(player.GetStrafeSpeed() + 1);
        });
        CreateButton("Decrease Strafe Speed", LeftExploreMenu, () =>
        {
            player.SetStrafeSpeed(player.GetStrafeSpeed() - 1);
        });
        CreateButton("Increase Gravity Strength", LeftExploreMenu, () =>
        {
            player.SetGravityStrength(player.GetGravityStrength() + 1);
        });
        CreateButton("Decrease Gravity Strength", LeftExploreMenu, () =>
        {
            player.SetGravityStrength(player.GetGravityStrength() - 1);
        });
    }
    public static void CreateAmongUsMenu()
    {
        GameObject LeftExploreMenu = GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Left/Container/InnerContainer/Explore/ScrollRect/Viewport/VerticalLayoutGroup");
        ClearMenu(LeftExploreMenu);
        CreateButton("Back", LeftExploreMenu, CreateMainMenu);
        CreateSeparator(LeftExploreMenu);

        var node_list = GameObject.Find("Game Logic/Player Nodes");

        if (node_list == null)
        {
            CreateButton("The Imposter is Sus!", LeftExploreMenu, CreateAmongUsMenu);
            return;
        }
        else
        {
            CreateButton("Reload Player List", LeftExploreMenu, CreateAmongUsMenu);
            CreateButton("Break/Fix Game Logic", LeftExploreMenu, () =>
            {
                GameObject.Find("Game Logic/Lobby Area Bounds").active = !GameObject.Find("Game Logic/Lobby Area Bounds").active;
                GameObject.Find("Game Logic/Game Area Bounds").active = !GameObject.Find("Game Logic/Game Area Bounds").active;
                GameObject.Find("Game Logic/Vent Area Bounds").active = !GameObject.Find("Game Logic/Vent Area Bounds").active;
                GameObject.Find("Game Logic/Ejected Area Bounds").active = !GameObject.Find("Game Logic/Ejected Area Bounds").active;
                GameObject.Find("Game Logic/Lobby Spawns").active = !GameObject.Find("Game Logic/Lobby Spawns").active;
                GameObject.Find("Game Logic/Meeting doors").active = !GameObject.Find("Game Logic/Meeting doors").active;
                GameObject.Find("Game Logic/Error Handler").active = !GameObject.Find("Game Logic/Error Handler").active;
            });
            foreach (var node in node_list.GetComponentsInChildren<VRC.Udon.UdonBehaviour>())
            {
                var program = node._program;

                /* Get playername from heap */
                var symbol_table = program.SymbolTable;
                var heap = program.Heap;
                var player_api_ptr = symbol_table.GetAddressFromSymbol("playerApi");
                var ptr = heap.GetHeapVariable(player_api_ptr);
                if (ptr == null)
                    continue;
                var player = new VRCPlayerApi(ptr.Pointer);

                CreateButton(player.displayName, LeftExploreMenu, () =>
                {
                    for (int i = LeftExploreMenu.transform.childCount - 1; i >= 0; i--)
                    {
                        GameObject.Destroy(LeftExploreMenu.transform.GetChild(i).gameObject);
                    }
                    CreateButton("Back", LeftExploreMenu, CreateMainMenu);
                    CreateSeparator(LeftExploreMenu);

                    foreach (var entry in node._eventTable)
                    {
                        CreateButton(entry.Key, LeftExploreMenu, () =>
                        {
                            node.RunProgram(entry.Value[0]);
                        });
                    }
                });
            }
        }
    }
    public static void CreateMurderMenu()
    {
        GameObject LeftExploreMenu = GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Left/Container/InnerContainer/Explore/ScrollRect/Viewport/VerticalLayoutGroup");
        ClearMenu(LeftExploreMenu);
        CreateButton("Back", LeftExploreMenu, CreateMainMenu);
        CreateSeparator(LeftExploreMenu);

        var unlockableWeapons = GameObject.Find("Game Logic/Weapons/Unlockables");
        var normalWeapons = GameObject.Find("Game Logic/Weapons");
        var snakeBox = GameObject.Find("Game Logic/Snakes/SnakeDispenser");
        var cameraItem = GameObject.Find("Game Logic/Polaroids Unlock Camera/FlashCamera");
        var cursedBomb = GameObject.Find("Game Logic/Skulls Unlock CursedBomb/CursedBomb");

        if (unlockableWeapons == null || normalWeapons == null)
        {
            CreateButton("You are not a murderer", LeftExploreMenu, CreateMurderMenu);
        }
        else
        {
            var self = VRCPlayerApi.AllPlayers.Find((Il2CppSystem.Predicate<VRCPlayerApi>)(x => x.isLocal));
            CreateButton("Toggle Doors", LeftExploreMenu, () =>
            {
                GameObject.Find("Environment/Doors").active = !GameObject.Find("Environment/Doors").active;
            });

            CreateButton("Break Game Logic", LeftExploreMenu, () =>
            {
                GameObject.Find("Game Logic/Error Handler").active = !GameObject.Find("Game Logic/Error Handler").active;
                GameObject.Find("Game Logic/Lobby Spawns").active = !GameObject.Find("Game Logic/Lobby Spawns").active;
            });

            for (int i = 0; i < unlockableWeapons.transform.childCount; i++)
            {
                Transform ourWeapon = unlockableWeapons.transform.GetChild(i);
                CreateButton("Spawn " + ourWeapon.name, LeftExploreMenu, () =>
                {
                    ourWeapon.gameObject.active = false;
                    Vector3 ourPosition = self.GetPosition();
                    ourWeapon.GetComponents<VRC.Udon.UdonBehaviour>()[1].enabled = false;
                    ourWeapon.position = new Vector3(ourPosition.x, ourPosition.y + 0.01f, ourPosition.z);
                    ourWeapon.gameObject.active = true;
                });
            }
            for (int i = 0; i < normalWeapons.transform.childCount; i++)
            {
                Transform ourWeapon = normalWeapons.transform.GetChild(i);
                if (ourWeapon.name != "Unlockables" && ourWeapon.name != "Pickup reset point")
                {
                    CreateButton("Spawn " + ourWeapon.name, LeftExploreMenu, () =>
                    {
                        ourWeapon.gameObject.active = false;
                        Vector3 ourPosition = self.GetPosition();
                        ourWeapon.GetComponents<VRC.Udon.UdonBehaviour>()[1].enabled = false;
                        ourWeapon.position = new Vector3(ourPosition.x, ourPosition.y + 0.02f, ourPosition.z);
                        ourWeapon.gameObject.active = true;
                    });
                }
            }

            CreateButton("Spawn Camera", LeftExploreMenu, () =>
            {
                cameraItem.gameObject.active = false;
                Vector3 ourPosition = self.GetPosition();
                cameraItem.GetComponents<VRC.Udon.UdonBehaviour>()[1].enabled = false;
                cameraItem.transform.position = new Vector3(ourPosition.x, ourPosition.y + 0.02f, ourPosition.z);
                cameraItem.gameObject.active = true;
            });

            CreateButton("Spawn Snake Box", LeftExploreMenu, () =>
            {
                Vector3 ourPosition = self.GetPosition();
                snakeBox.transform.position = new Vector3(ourPosition.x, ourPosition.y + 0.02f, ourPosition.z);
            });

            CreateButton("Spawn CursedBomb (Halloween)", LeftExploreMenu, () =>
            {
                cursedBomb.gameObject.active = false;
                Vector3 ourPosition = self.GetPosition();
                cursedBomb.GetComponents<VRC.Udon.UdonBehaviour>()[1].enabled = false;
                cursedBomb.transform.position = new Vector3(ourPosition.x, ourPosition.y + 0.02f, ourPosition.z);
                cursedBomb.gameObject.active = true;
            });
        }
    }
    public static GameObject CreateButton(string buttonName, GameObject ButtonParent, System.Action buttonAction)
    {
        GameObject myButton = GameObject.Instantiate(GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_QM_Launchpad/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickLinks_A/LayoutGuide/Button_Avatars/"));
        myButton.name = "Button_" + System.Guid.NewGuid().ToString(); //Name the buttons something specific internally
        myButton.transform.SetParent(ButtonParent.transform, false);
        GameObject.Destroy(myButton.GetComponents<Component>()[^1]); //prevents the button from opening home menu dialog. It's always last on the list of components, at least I assume!
        GameObject.Find(myButton.name + "/Icons").active = false; //remove icon
        GameObject.Find(myButton.name + "TextLayoutParent/Text_H4").GetComponent<TextMeshProText>().prop_String_0 = buttonName;
        var ev = new UnityEngine.UI.Button.ButtonClickedEvent();
        var cb = (UnityAction)(System.Action)(() => buttonAction());
        ev.AddListener(cb);
        myButton.GetComponent<ButtonPublicIMoveHandlerIEventSystemHandlerIPointerDownHandlerIPointerUpHandlerIPointerEnterHandlerIPointerExitHandlerISelectHandlerIDeselectHandlerOb_tIm_iStOb_t_sBo_dUnique>().onClick = ev;
        return myButton;
    }
    public static GameObject CreateSeparator(GameObject Parent, string text = "------------")
    {
        GameObject mySeparator = GameObject.Instantiate(GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Right/Container/InnerContainer/Friends/Panel_Wing_ScrollRect_Labeled/Viewport/VerticalLayoutGroup/Header_Wing_H3"));
        mySeparator.name = "Separator_" + System.Guid.NewGuid().ToString(); //Name the separators something specific internally
        mySeparator.transform.SetParent(Parent.transform, false);
        GameObject.Find(mySeparator.name + "Text_H3").GetComponent<TextMeshProText>().prop_String_0 = text;
        return mySeparator;
    }
    public static List<VRCPlayerApi> GetPlayersSorted()
    {
        var playerNames = new List<string>();
        var ourPlayers = new List<VRCPlayerApi>();
        var allPlayers = VRCPlayerApi.AllPlayers;
        foreach (var player in allPlayers)
        {
            playerNames.Add(player.displayName);
        }
        playerNames.Sort();
        for (int i = 0; i <= playerNames.Count - 1; i++) {
            foreach (var player in allPlayers)
            {
                if (player.displayName == playerNames[i])
                {
                    ourPlayers.Add(player);
                }
            }
        }
        return ourPlayers;
    }
}

internal static class ActionMenuExtra
{
    public static bool isOpen(this ActionMenuOpener actionMenuOpener)
    {
        return actionMenuOpener.field_Private_Boolean_0; //only bool on action menu opener, shouldnt change
    }

    public static ActionMenuType GetActionMenuType(this ActionMenuOpener opener)
    {
        return opener.field_Public_EnumNPublicSealedvaLeRi3vUnique_0;
    }

    public static ActionMenuDriver GetDriver()
    {
        return ActionMenuDriver.field_Public_Static_MonoBehaviourPublicObGaObAc1ObAcBoCoObUnique_0;
    }

    public static ActionMenuOpener GetLeftOpener(this ActionMenuDriver actionMenuDriver)
    {
        //var opener = actionMenuDriver.field_Public_MonoBehaviourPublicCaObAc1BoSiBoObObObUnique_0;
        var opener = actionMenuDriver.field_Public_MonoBehaviourPublicSiObSiCaBoSiAcObBo1Unique_0;
        if (opener.GetActionMenuType() ==
            ActionMenuType.Left)
            return opener;
        return actionMenuDriver.field_Public_MonoBehaviourPublicSiObSiCaBoSiAcObBo1Unique_1;
    }

    public static ActionMenuOpener GetRightOpener(this ActionMenuDriver actionMenuDriver)
    {
        var opener = actionMenuDriver.field_Public_MonoBehaviourPublicSiObSiCaBoSiAcObBo1Unique_1;
        if (opener.GetActionMenuType() == ActionMenuType.Right)
            return opener;
        return actionMenuDriver.field_Public_MonoBehaviourPublicSiObSiCaBoSiAcObBo1Unique_0;
    }

    public static ActionMenuOpener GetActionMenuOpener()
    {
        var driver = GetDriver();
        if (!driver.GetLeftOpener().isOpen() &&
            driver.GetRightOpener().isOpen())
            return driver.GetRightOpener();

        if (driver.GetLeftOpener().isOpen() &&
            !driver.GetRightOpener().isOpen())
            return driver.GetLeftOpener();

        return null;
    }
}

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("VRChat.exe")]
public class VM : BasePlugin
{
    public static ConfigEntry<string> menuText;
    public static ManualLogSource Logger { get; private set; }
    public override void Load()
    {
        Logger = Log;
        AddComponent<PluginComponent>();
        menuText = Config.Bind(     "General",      // The section under which the option is shown
                                    "menuText",  // The key of the configuration option in the configuration file
                                    "N MENU", // The default value
                                    "Text to show on the hand menu"); // Description of the option to show in the config file
    }
}
