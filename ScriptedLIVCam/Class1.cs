using Il2CppExitGames.Client.Photon;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppLIV.SDK.Unity;
using Il2CppPhoton.Pun;
using Il2CppPhoton.Realtime;
using Il2CppRUMBLE.Interactions.InteractionBase;
using Il2CppRUMBLE.MoveSystem;
using Il2CppRUMBLE.Players;
using Il2CppRUMBLE.Utilities;
using Il2CppTMPro;
using MelonLoader;
using RumbleModdingAPI;
using RumbleModUI;
using ScriptedLIVCam;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static RumbleModdingAPI.Calls;
using static ScriptedLIVCam.MarkerPoint;

[assembly: MelonInfo(typeof(ScriptedLCMain), "ScriptedLIVCam", "0.0.1", "TacoSlayer36")]
[assembly: MelonGame("Buckethead Entertainment", "RUMBLE")]
[assembly: MelonColor(255, 255, 248, 231)]
[assembly: MelonAuthorColor(255, 255, 248, 231)]
[assembly: VerifyLoaderVersion(0, 6, 6, true)]

namespace ScriptedLIVCam
{
    public static class BuildInfo
    {
        public const string ModName = "ScriptedLIVCam";
        public const string ModVersion = "0.0.1";
        public const string Description = "Make the LIV camera follow custom paths";
        public const string Author = "TacoSlayer36";
        public const string Company = "";
    }

    public class ScriptedLCMain : MelonMod
    {
        Mod Mod = new Mod();

        public static ScriptedLCMain Instance { get; set; } = new ScriptedLCMain();


        private bool hasFlatLand = false;

        public GameObject? CurvesParent;
        public List<Curve> Curves = new List<Curve>();
        private List<Curve> curvesToSave = new List<Curve>();
        private Curve? currentCurve;
        private Curve? storedSelectedCurve;
        public GameObject livObject;

        private List<CurveFile> undoStates = new List<CurveFile>();
        private int statesUndone = 0;
        private int maxUndoStates = 10;
        private bool isStateChanging = false;
        private bool isUndoRedoOperation = false;

        private string directoryPath = "UserData/ScriptedLIVCam/SavedSplines";
        private string otherSettingsPath = "UserData/ScriptedLIVCam/OtherSettings.txt";

        public int dontShowOnLIVLayer = LayerMask.NameToLayer("PlayerController");

        private GameObject? modParentObject;

        private string scene = string.Empty;

        private const byte eventNumber = 32;
        private static RaiseEventOptions eventOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.All };

        private bool init = false;
        
        private bool sceneWasLoaded = false;

        private bool curveEditingEnabled = true;
        private bool isMinimized = false;

        public bool currentTriggerState;
        public bool previousTriggerState;

        public bool currentGripState = false;
        public bool previousGripState = false;
        public bool currentMainPrimaryState = false;
        public bool currentMainSecondaryState = false;
        public bool currentOffPrimaryState = false;

        private List<KeyCode> playKeyCodes = new List<KeyCode>();
        private List<KeyCode> stopKeyCodes = new List<KeyCode>();
        private bool settingPlayKey = false;
        private bool settingStopKey = false;

        private float triggerThreshold = 0.2f;
        private float gripThreshold = 0.5f;
        private float dragThreshold = 0.06f;
        private float pointGripRadius = 0.12f;

        public string EmptyField = "---";

        private ControlPoint? activeRawPoint;
        private SelectedControl? selectedControl;
        public MarkerPoint? ActiveRawMarker;
        public MarkerPoint? SelectedMarker;

        private List<MarkerPoint> passedPauseMarkers = new List<MarkerPoint>();

        public float cameraRoll = 0f;
        public float cameraSpeed = 0f;
        public float cameraFOV = 0f;

        public float staticFOV = 60f;
        public float staticRoll = 0f;

        private GameObject? markerPreview;

        Transform rightHandTransform = new Transform();
        Transform leftHandTransform = new Transform();

        Transform mainHandTransform = new Transform();
        Transform offHandTransform = new Transform();

        Vector3 prevMainHandPos = new Vector3();
        Quaternion prevMainHandRot = new Quaternion();

        private bool swapMainHand = false;
        private bool snapPosition = false;
        private bool snapRotation = false;
        private bool cameraAudio = false;
        private bool hideTags = false;
        private float snapRange = 0.1f;
        private bool editingCurve = false;
        private bool useLegacyCam = false;

        private GameObject? localHealthBar;

        public Material? curveMaterial;

        private GameObject? scLCBlueprints;
        private GameObject? scriptMenuBlueprint;
        private GameObject? cameraBlueprint;

        private Material dofMaterial;
        private float focusDistance = 10f;
        private float blurStrength = 1f;

        public GameObject? pauseMarkerBlueprint;
        public GameObject? rollMarkerBlueprint;
        public GameObject? fovMarkerBlueprint;
        public GameObject? speedMarkerBlueprint;

        private GameObject? scriptMenu;
        private GameObject? livCameraIndicator;

        private GameObject? RotTargetObject;
        private GameObject? FocusTargetObject;

        private GameObject? Maximized;
        private GameObject? Minimized;

        private GameObject? UISelectMenu;
        private GameObject? UIEditMenu;
        private GameObject? UIMarkersMenu;
        private GameObject? UIStaticMenu;
        private GameObject? UINetworkMenu;
        private GameObject? UIFileMenu;
        private GameObject? UISettingsMenu;

        private List<Image> UIImageComponents = new List<Image>();

        private List<GameObject> UISelectedIDTexts = new List<GameObject>();
        private GameObject UISelectedOwnerText;

        private GameObject? UISelectedRotTarget;
        private GameObject? UISelectedFocusTarget;
        private GameObject? UICurveDuration;
        private GameObject? UICurveDurationPauses;
        private GameObject? UIOrderPosText;
        private bool modifyingCurveDuration = false;

        private GameObject? UIIsHidden;
        private GameObject? UIIsEnabled;
        private GameObject? UIIsLoop;
        private GameObject? UIIsStatic;

        private List<GameObject?> UIToolButtons = new List<GameObject?>();
        private GameObject? UIMoveCurveTool;
        private GameObject? UIPenTool;
        private GameObject? UIPenTool2;
        private GameObject? UIAnchorTool;
        private GameObject? UISelectMarkerTool;
        private GameObject? UIPauseMarkerTool;
        private GameObject? UIRollMarkerTool;
        private GameObject? UIDeleteMarkerTool;
        private GameObject? UIFovMarkerTool;
        private GameObject? UISpeedMarkerTool;
        private GameObject? UIMoveStaticTool;
        private GameObject? UIGrabTool;
        private GameObject? UISelectMarkerTool2;

        private GameObject? UIStaticFOV;
        private bool modifyingStaticFOV = false;
        private GameObject? UIStaticRoll;
        private bool modifyingStaticRoll = false;

        private List<GameObject> UIfileOptions = new List<GameObject>();
        private List<string> fileNames = new List<string>();
        private int selectedFileOption = 0;
        private int filePage = 0;

        private GameObject? UILoadInGym;
        private GameObject? UILoadInPark;
        private GameObject? UILoadInRing;
        private GameObject? UILoadInPit;

        private List<string> loadInGym = new List<string>();
        private List<string> loadInPark = new List<string>();
        private List<string> loadInRing = new List<string>();
        private List<string> loadInPit = new List<string>();

        private GameObject? UIRequestIDText;
        private GameObject? UIRequestOwnerText;
        private CurveFile curveSendRequest;

        private GameObject? UISwapHandsText;
        private GameObject? UISnapPosText;
        private GameObject? UISnapRotText;
        private GameObject? UICameraAudioText;
        private GameObject? UIHideTagsText;

        private Tool selectedTool = Tool.Pen;

        private enum Tool
        {
            Select,
            Pen,
            Anchor,
            SelectMarker,
            AddPauseMarker,
            AddRollMarker,
            AddFOVMarker,
            AddSpeedMarker,
            DeleteMarker,
            MoveStatic,
            GrabStatic
        }

        private List<Tool> addMarkerTools = new List<Tool> { Tool.AddPauseMarker, Tool.AddRollMarker, Tool.AddFOVMarker, Tool.AddSpeedMarker };

        private SlideValue? slidingValue;

        private LIV liv;

        private bool isPointingAtCanvas = false;
        private GameObject? uiPointer;

        private bool isCameraAnimating = false;
        private bool isPlayingAll = false;
        private bool isCameraPaused = false;
        private float cameraLineOffset = 0f;

        private bool isCameraStatic = false;
        private Vector3 staticCameraPos = Vector3.zero;
        private Quaternion staticCameraRot = Quaternion.identity;
        private float staticCameraFOV = 60f;
        private float staticCameraRoll = 0f;

        private AudioListener playerAudioListener;
        private AudioListener camAudioListener;
        private RecordingCamera legacyCamRC;
        private AudioListener legacyCamAudioListener;
        private GameObject legacyCam;

        public override void OnLateInitializeMelon()
        {
            UI.instance.UI_Initialized += OnUIInit;
            Calls.onMapInitialized += SceneLoaded;
            Calls.onMyModsGathered += checkMods;
            Calls.onPlayerSpawned += playerJoined;

            Instance = this;

            curveMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            curveMaterial.color = Color.white;
            curveMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        private void checkMods()
        {
            hasFlatLand = Mods.findOwnMod("FlatLand", "1.6.0", false);
        }

        public void playerJoined()
        {
            if (sceneWasLoaded)
            {
                MelonCoroutines.Start(setTagsVisibility(!hideTags));
            }
        }

        private IEnumerator listenForFlatLandButton()
        {
            yield return new WaitForSeconds(1);
            GameObject.Find("FlatLand/FlatLandButton/Button").GetComponent<InteractionButton>().onPressed.AddListener(new Action(() =>
            {
                MelonCoroutines.Start(toFlatLand());
            }));
            yield break;
        }

        private IEnumerator toFlatLand()
        {
            yield return new WaitForSeconds(1f);
            modParentObject.active = true;
        }

        public void OnUIInit()
        {
            Mod.ModName = BuildInfo.ModName;
            Mod.ModVersion = BuildInfo.ModVersion;
            Mod.SetFolder("ScriptedLivCam");
            Mod.AddDescription("Description", "", BuildInfo.Description, new Tags { IsSummary = true });

            Mod.AddToList("Spline Editing", false, 0, "Set to enable the menu and all spline editing", new Tags());

            Mod.GetFromFile();
            Mod.ModSaved += Save;
            Save();

            UI.instance.AddMod(Mod);
        }

        private void closeMenu()
        {
            if (scriptMenu != null)
            {
                scriptMenu.SetActive(false);
            }
            if (uiPointer != null)
            {
                uiPointer.SetActive(false);
            }
        }

        private void SaveOtherSettings()
        {
            var settings = new
            {
                LoadInGym = loadInGym,
                LoadInPark = loadInPark,
                LoadInRing = loadInRing,
                LoadInPit = loadInPit
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(settings, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(otherSettingsPath, json);
        }

        private void LoadOtherSettings()
        {
            if (!File.Exists(otherSettingsPath))
            {
                SaveOtherSettings();
            }

            string json = File.ReadAllText(otherSettingsPath);
            var settings = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);

            loadInGym = ((IEnumerable<string>)settings.LoadInGym.ToObject<List<string>>()).Where(file => File.Exists(Path.Combine(directoryPath, file))).ToList();
            loadInPark = ((IEnumerable<string>)settings.LoadInPark.ToObject<List<string>>()).Where(file => File.Exists(Path.Combine(directoryPath, file))).ToList();
            loadInRing = ((IEnumerable<string>)settings.LoadInRing.ToObject<List<string>>()).Where(file => File.Exists(Path.Combine(directoryPath, file))).ToList();
            loadInPit = ((IEnumerable<string>)settings.LoadInPit.ToObject<List<string>>()).Where(file => File.Exists(Path.Combine(directoryPath, file))).ToList();

            SaveOtherSettings();
        }

        public void Save()
        {
            curveEditingEnabled = (bool)Mod.Settings[1].SavedValue;

            if (curveEditingEnabled && scriptMenu != null)
            {
                scriptMenu.SetActive(true);
            }
            else
            {
                closeMenu();
            }
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            if (sceneName == "Loader" || sceneName == string.Empty)
            {
                return;
            }
            Curves.Clear();
            currentCurve = null;
            stopAnimation();

            sceneWasLoaded = false;
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            scene = sceneName;
        }

        public void SceneLoaded()
        {
            sceneWasLoaded = true;

            if (!init && scene == "Gym")
            {
                if (livObject == null)
                {
                    livObject = new GameObject("LIV");
                    liv = livObject.AddComponent<LIV>();
                    GameObject.DontDestroyOnLoad(livObject);
                }

                instantiateBlueprints();

                PhotonNetwork.NetworkingClient.EventReceived += (Action<EventData>)OnEvent;

                init = true;
            }

            if (hasFlatLand && scene == "Gym")
            {
                MelonCoroutines.Start(listenForFlatLandButton());
            }

            if (scene == "Loader")
            {
                return;
            }

            localHealthBar = Calls.Players.GetLocalHealthbarGameObject();
            setTagsVisibility(!hideTags);

            if (modParentObject == null)
            {
                modParentObject = new GameObject("ScriptedLIVCam");
            }

            if (modParentObject != null)
            {
                if (CurvesParent == null)
                {
                    CurvesParent = new GameObject("CurvesParent");
                    CurvesParent.transform.SetParent(modParentObject.transform, true);
                }

                if (livCameraIndicator == null)
                {
                    livCameraIndicator = GameObject.Instantiate(cameraBlueprint);
                    livCameraIndicator.transform.SetParent(modParentObject.transform, true);
                    livCameraIndicator.SetActive(false);
                    livCameraIndicator.transform.GetChild(0).gameObject.layer = dontShowOnLIVLayer;
                }

                if (livCameraIndicator != null)
                {
                    MelonCoroutines.Start(setUpAudioListeners());
                }

                //if (legacyCamRC == null)
                //{
                //    legacyCam = GameObject.Find("RecordingCamera");
                //}

                if (scriptMenu == null)
                {
                    scriptMenu = GameObject.Instantiate(scriptMenuBlueprint);

                    if (scriptMenu != null)
                    {
                        scriptMenu.name = "ScriptMenu";
                        scriptMenu.transform.SetParent(modParentObject.transform, true);
                        scriptMenu.transform.GetChild(0).gameObject.layer = dontShowOnLIVLayer;
                        scriptMenu.SetActive(curveEditingEnabled);

                        Maximized = scriptMenu.transform.GetChild(0).GetChild(0).gameObject;
                        Minimized = scriptMenu.transform.GetChild(0).GetChild(1).gameObject;

                        UISelectMenu = Maximized.transform.GetChild(3).gameObject;
                        UIEditMenu = Maximized.transform.GetChild(4).gameObject;
                        UIMarkersMenu = Maximized.transform.GetChild(5).gameObject;
                        UIStaticMenu = Maximized.transform.GetChild(6).gameObject;
                        UINetworkMenu = Maximized.transform.GetChild(7).gameObject;
                        UIFileMenu = Maximized.transform.GetChild(8).gameObject;
                        UISettingsMenu = Maximized.transform.GetChild(9).gameObject;

                        UISelectedIDTexts.Clear();

                        UISelectedIDTexts.Add(UISelectMenu.transform.GetChild(3).GetChild(2).gameObject);
                        UISelectedIDTexts.Add(UIEditMenu.transform.GetChild(1).GetChild(0).gameObject);
                        UISelectedIDTexts.Add(UIMarkersMenu.transform.GetChild(0).GetChild(0).gameObject);
                        UISelectedIDTexts.Add(UINetworkMenu.transform.GetChild(1).GetChild(0).gameObject);

                        UIImageComponents.Clear();
                        UIImageComponents.AddRange(scriptMenu.transform.GetComponentsInChildren<Image>());

                        UISelectedOwnerText = UINetworkMenu.transform.GetChild(1).GetChild(1).gameObject;

                        UIIsEnabled = UISelectMenu.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetChild(0).gameObject;
                        UIIsHidden = UISelectMenu.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetChild(1).GetChild(0).gameObject;
                        UIIsLoop = UIMarkersMenu.transform.GetChild(7).GetChild(0).GetChild(1).GetChild(0).gameObject;
                        UIIsStatic = UIStaticMenu.transform.GetChild(0).GetChild(0).GetChild(1).gameObject;

                        UISelectedRotTarget = UIMarkersMenu.transform.GetChild(6).GetChild(2).gameObject;
                        //UISelectedFocusTarget = UIEditMenu.transform.GetChild(6).GetChild(2).gameObject;
                        UICurveDuration = UIMarkersMenu.transform.GetChild(4).GetChild(1).GetChild(0).gameObject;
                        UICurveDurationPauses = UIMarkersMenu.transform.GetChild(4).GetChild(4).gameObject;

                        UIOrderPosText = UIEditMenu.transform.GetChild(3).GetChild(0).GetChild(0).gameObject;

                        UIToolButtons.Clear();
                        UIToolButtons.Add(UIMoveCurveTool = UIEditMenu.transform.GetChild(0).GetChild(0).gameObject);
                        UIToolButtons.Add(UIPenTool = UIEditMenu.transform.GetChild(0).GetChild(1).gameObject);
                        UIToolButtons.Add(UIPenTool2 = Maximized.transform.GetChild(10).GetChild(1).gameObject);
                        UIToolButtons.Add(UIAnchorTool = UIEditMenu.transform.GetChild(0).GetChild(2).gameObject);
                        UIToolButtons.Add(UISelectMarkerTool = UIMarkersMenu.transform.GetChild(1).GetChild(0).gameObject);
                        UIToolButtons.Add(UIPauseMarkerTool = UIMarkersMenu.transform.GetChild(1).GetChild(1).gameObject);
                        UIToolButtons.Add(UIRollMarkerTool = UIMarkersMenu.transform.GetChild(1).GetChild(2).gameObject);
                        UIToolButtons.Add(UIDeleteMarkerTool = UIMarkersMenu.transform.GetChild(2).GetChild(0).gameObject);
                        UIToolButtons.Add(UIFovMarkerTool = UIMarkersMenu.transform.GetChild(2).GetChild(1).gameObject);
                        UIToolButtons.Add(UISpeedMarkerTool = UIMarkersMenu.transform.GetChild(2).GetChild(2).gameObject);
                        UIToolButtons.Add(UIMoveStaticTool = UIStaticMenu.transform.GetChild(3).GetChild(0).gameObject);
                        UIToolButtons.Add(UIGrabTool = UIStaticMenu.transform.GetChild(3).GetChild(1).gameObject);
                        UIToolButtons.Add(UISelectMarkerTool2 = Maximized.transform.GetChild(10).GetChild(2).gameObject);

                        UIStaticFOV = UIStaticMenu.transform.GetChild(1).GetChild(1).GetChild(0).gameObject;
                        UIStaticRoll = UIStaticMenu.transform.GetChild(2).GetChild(1).GetChild(0).gameObject;

                        UIfileOptions.Clear();
                        for (int i = 0; i < 8; i++)
                        {
                            UIfileOptions.Add(UIFileMenu.transform.GetChild(2).GetChild(i).gameObject);
                        }

                        UIRequestIDText = UINetworkMenu.transform.GetChild(3).GetChild(0).gameObject;
                        UIRequestOwnerText = UINetworkMenu.transform.GetChild(3).GetChild(1).gameObject;

                        UISwapHandsText = UISettingsMenu.transform.GetChild(0).GetChild(0).GetChild(1).GetChild(0).gameObject;
                        UISnapPosText = UISettingsMenu.transform.GetChild(0).GetChild(0).GetChild(1).GetChild(0).gameObject;
                        UISnapRotText = UISettingsMenu.transform.GetChild(1).GetChild(0).GetChild(1).GetChild(0).gameObject;
                        UICameraAudioText = UISettingsMenu.transform.GetChild(10).GetChild(0).GetChild(1).GetChild(0).gameObject;
                        UIHideTagsText = UISettingsMenu.transform.GetChild(11).GetChild(0).GetChild(1).GetChild(0).gameObject;

                        UILoadInGym = UIFileMenu.transform.GetChild(5).GetChild(0).gameObject;
                        UILoadInPark = UIFileMenu.transform.GetChild(5).GetChild(1).gameObject;
                        UILoadInRing = UIFileMenu.transform.GetChild(5).GetChild(2).gameObject;
                        UILoadInPit = UIFileMenu.transform.GetChild(5).GetChild(3).gameObject;

                        disableAllMenus();
                        UISelectMenu.SetActive(true);

                        if (isMinimized)
                        {
                            Maximized.SetActive(false);
                            Minimized.SetActive(true);
                        }

                        RefreshFileList();
                        UpdateFileUI();
                    }
                }
            }

            stopStatic();

            // Load corresponding files based on the scene name
            List<string> filesToLoad = scene switch
            {
                "Gym" => loadInGym,
                "Park" => loadInPark,
                "Map0" => loadInRing,
                "Map1" => loadInPit,
                _ => new List<string>()
            };

            foreach (var fileName in filesToLoad)
            {
                string filePath = Path.Combine(directoryPath, fileName);
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    CurveFile curveFile = Newtonsoft.Json.JsonConvert.DeserializeObject<CurveFile>(json);
                    appendCurves(curveFile);
                }
            }
        }

        private System.Collections.IEnumerator setUpAudioListeners()
        {
            yield return new WaitForSeconds(2f);
            playerAudioListener = Calls.Managers.GetPlayerManager().LocalPlayer.Controller.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<AudioListener>();

            camAudioListener = livCameraIndicator.AddComponent<AudioListener>();
            camAudioListener.enabled = false;
            camAudioListener.tag = "ScriptedLIVCam";
            camAudioListener.velocityUpdateMode = playerAudioListener.velocityUpdateMode;

            //legacyCamAudioListener = legacyCam.AddComponent<AudioListener>();
            //legacyCamAudioListener.enabled = false;
            //legacyCamAudioListener.tag = "LegacyCam";
            //legacyCamAudioListener.velocityUpdateMode = playerAudioListener.velocityUpdateMode;

            //legacyCamRC = legacyCam.GetComponent<RecordingCamera>();
        }

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code == eventNumber)
            {
                string received = photonEvent.CustomData.ToString();
                char option = received[0];
                string code = received.Substring(1, 8);
                string json = received.Substring(9);

                if (option == 'R') // Someone has requested a curve to be updated
                {
                    List<Curve> curvesToSave = new List<Curve>();
                    Curve curveToCheck = GetCurveByID(code);

                    if (curveToCheck == null) return;

                    curvesToSave.Add(curveToCheck);
                    if (curveToCheck.RotTarget != null)
                        curvesToSave.Add(curveToCheck.RotTarget);

                    CurveFile curveFile = new CurveFile
                    {
                        FileName = "",
                        Owner = Calls.Managers.GetPlayerManager().LocalPlayer.Data.GeneralData.PlayFabMasterId,
                        Curves = curvesToSave
                    };

                    string strToSend = Newtonsoft.Json.JsonConvert.SerializeObject(curveFile, Newtonsoft.Json.Formatting.Indented,
                    new Newtonsoft.Json.JsonSerializerSettings
                    {
                        ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                    });

                    PhotonNetwork.RaiseEvent(eventNumber, "U" + "--------" + strToSend, eventOptions, SendOptions.SendReliable);
                }
                else
                {
                    CurveFile asFile = Newtonsoft.Json.JsonConvert.DeserializeObject<CurveFile>(json);
                    if (asFile == null) return;

                    if (option == 'N') // No additional options; curves will only update if needed
                    {
                        foreach (Curve curve in Curves)
                        {
                            if (curve.Id == asFile.Curves[0].Id)
                            {
                                overwriteCurves(asFile); // Curve is overwritten if it already exists
                                return;
                            }
                        }

                        curveSendRequest = asFile; // Curve is prompted to be accepted if it does not already exist
                    }

                    if (option == 'U') // Curve has been requested and therefore already exists
                    {
                        overwriteCurves(asFile);
                    }

                    updateTexts();
                }
            }
        }

        private void instantiateBlueprints()
        {
            scLCBlueprints = LoadAssetBundle("ScriptedLIVCam.assets.scriptedlivcam", "ScLCBlueprints");

            scriptMenuBlueprint = scLCBlueprints.transform.GetChild(0).gameObject;
            cameraBlueprint = scLCBlueprints.transform.GetChild(1).gameObject;
            rollMarkerBlueprint = scLCBlueprints.transform.GetChild(2).gameObject;
            fovMarkerBlueprint = scLCBlueprints.transform.GetChild(3).gameObject;
            pauseMarkerBlueprint = scLCBlueprints.transform.GetChild(4).gameObject;
            speedMarkerBlueprint = scLCBlueprints.transform.GetChild(5).gameObject;

            MeshRenderer mr = cameraBlueprint.transform.GetChild(0).GetComponent<MeshRenderer>();
            mr.material = curveMaterial;
            mr.material.color = Color.green;

            GameObject.DontDestroyOnLoad(scLCBlueprints);
            scLCBlueprints.SetActive(false);
        }

        public override void OnUpdate()
        {
            //if (Input.GetKeyDown(KeyCode.P))
            //{
            //    //Camera cambera = GameObject.Find("RecordingCamera").GetComponent<Camera>();
            //    Camera cambera = liv.render.cameraInstance;

            //    cambera.depthTextureMode = DepthTextureMode.Depth;

            //    int blurResolution = Mathf.NextPowerOfTwo(Mathf.Max(Screen.width, Screen.height));

            //    RenderTexture tempTexture = new RenderTexture(blurResolution, blurResolution, 0);
            //    RenderTexture finalTexture = new RenderTexture(blurResolution, blurResolution, 0);

            //    tempTexture.filterMode = FilterMode.Bilinear;
            //    finalTexture.filterMode = FilterMode.Bilinear;

            //    dofMaterial = scriptMenu.GetComponent<Renderer>().material;
            //    dofMaterial.hideFlags = HideFlags.HideAndDontSave;

            //    dofMaterial.SetFloat("_BokehRadius", 4f);
            //    dofMaterial.SetFloat("_FocusDistance", 10f);
            //    dofMaterial.SetFloat("_FocusRange", 1f);

            //    CommandBuffer commandBuffer = new CommandBuffer();

            //    commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, tempTexture, dofMaterial, 0);
            //    commandBuffer.Blit(tempTexture, finalTexture, dofMaterial, 1);

            //    commandBuffer.Blit(finalTexture, BuiltinRenderTextureType.CameraTarget);

            //    cambera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);

            //    tempTexture.Release();
            //    finalTexture.Release();
            //}

            //if (Input.GetKeyDown(KeyCode.S))
            //{
            //    currentCurve.ScaleCurve(Vector3.one * 0.5f);
            //}

            //if (Input.GetKeyDown(KeyCode.R))
            //{
            //    currentCurve.RotateCurve(new Vector3(10f, 20f, 30f));
            //}

            //if (Input.GetKeyDown(KeyCode.P))
            //{
            //    MelonLogger.Msg(Newtonsoft.Json.JsonConvert.SerializeObject(curveSendRequest, Newtonsoft.Json.Formatting.Indented,
            //        new Newtonsoft.Json.JsonSerializerSettings
            //        {
            //            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            //        }));
            //}

            if (init)
            {
                // Get player hand transforms
                var playerManager = Calls.Managers.GetPlayerManager();
                if (playerManager != null && playerManager.LocalPlayer != null && playerManager.LocalPlayer.Controller != null)
                {
                    var controllerTransform = playerManager.LocalPlayer.Controller.transform;
                    if (controllerTransform.childCount > 1)
                    {
                        rightHandTransform = controllerTransform.GetChild(1).GetChild(2);
                        leftHandTransform = controllerTransform.GetChild(1).GetChild(1);

                        if (rightHandTransform != null && leftHandTransform != null)
                        {
                            mainHandTransform = rightHandTransform;
                            offHandTransform = leftHandTransform;
                        }
                    }
                }
            }

            // Staticize camera
            if (isCameraStatic)
            {
                Quaternion noRollRotation = Quaternion.LookRotation(staticCameraRot * Vector3.forward, Vector3.up);
                Quaternion combinedRotation = noRollRotation * Quaternion.Euler(0, 0, staticCameraRoll);
                moveCamera(staticCameraPos, combinedRotation, staticCameraFOV);
            }

            // Animate camera
            if (isCameraAnimating && currentCurve != null)
            {
                if (!currentCurve.IsEnabled)
                {
                    goToNextCurve();
                }

                float durationInSeconds = currentCurve.duration / 1000f;

                detectFullPauses();

                if (currentOffPrimaryState)
                {
                    isCameraPaused = false;
                }

                if (!isCameraPaused)
                {
                    cameraLineOffset += Time.deltaTime * (1 / durationInSeconds);
                }

                float adjustedCamLineOffset = currentCurve.GetAdjustedPosition(cameraLineOffset);

                if (adjustedCamLineOffset <= 1f)
                {
                    Vector3 RotTargetPos;
                    Vector3 FocusTargetPos;
                    Vector3 tangent;
                    Vector3 pos = currentCurve.CurvePosToWorldPos(adjustedCamLineOffset, 20, out tangent);
                    Quaternion lookRotation = Quaternion.LookRotation(tangent);
                    float fov = 0f;

                    livCameraIndicator.SetActive(true);

                    // Create camera rotation target dot if needed
                    if (RotTargetObject == null)
                    {
                        RotTargetObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        RotTargetObject.GetComponent<SphereCollider>().enabled = false;
                        RotTargetObject.name = "CamRotTarget";
                        RotTargetObject.transform.localScale = Vector3.one * 0.03f;
                        RotTargetObject.layer = dontShowOnLIVLayer;
                        MeshRenderer mr = RotTargetObject.GetComponent<MeshRenderer>();
                        mr.material = curveMaterial;
                        mr.material.color = new Color(1f, 0.6f, 0.6f);
                        RotTargetObject.transform.SetParent(modParentObject.transform, true);
                    }

                    // Create camera focus target dot if needed
                    if (FocusTargetObject == null)
                    {
                        FocusTargetObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        FocusTargetObject.GetComponent<SphereCollider>().enabled = false;
                        FocusTargetObject.name = "CamFocusTarget";
                        FocusTargetObject.transform.localScale = Vector3.one * 0.03f;
                        FocusTargetObject.layer = dontShowOnLIVLayer;
                        MeshRenderer mr = RotTargetObject.GetComponent<MeshRenderer>();
                        mr.material = curveMaterial;
                        mr.material.color = new Color(1f, 1f, 0.6f);
                        FocusTargetObject.transform.SetParent(modParentObject.transform, true);
                    }

                    // Only rotate camera if there's a rotation target (otherwise, it rotates to the tangent)
                    if (currentCurve.RotTarget != null)
                    {
                        RotTargetPos = currentCurve.RotTarget.CurvePosToWorldPos(currentCurve.RotTarget.GetAdjustedPosition(adjustedCamLineOffset), 20, out tangent);
                        lookRotation = Quaternion.LookRotation(RotTargetPos - pos);

                        RotTargetObject.SetActive(true);
                        RotTargetObject.transform.position = RotTargetPos;
                    }

                    // Only apply depth of field if there's a focus target (otherwise, focus is global)
                    if (currentCurve.FocusTarget != null)
                    {
                        FocusTargetPos = currentCurve.FocusTarget.CurvePosToWorldPos(currentCurve.FocusTarget.GetAdjustedPosition(adjustedCamLineOffset), 20, out tangent);

                        FocusTargetObject.SetActive(true);
                        FocusTargetObject.transform.position = FocusTargetPos;
                    }

                    cameraRoll = interpolateBetweenMarkers(MarkerPoint.MarkerType.Roll);
                    cameraFOV = interpolateBetweenMarkers(MarkerPoint.MarkerType.FOV);

                    Quaternion rollRotation = Quaternion.AngleAxis(cameraRoll, lookRotation * Vector3.forward);
                    Quaternion rot = rollRotation * lookRotation;
                    fov = cameraFOV;

                    moveCamera(pos, rot, fov);
                }
                else if (currentCurve.IsLoop)
                {
                    cameraLineOffset = 0f;
                    passedPauseMarkers.Clear();
                }
                else if (!isPlayingAll)
                {
                    stopAnimation();
                }
                else
                {
                    goToNextCurve();
                }
            }

            currentTriggerState = Calls.ControllerMap.RightController.GetTrigger() > triggerThreshold;
            currentGripState = Calls.ControllerMap.RightController.GetGrip() > gripThreshold;
            currentMainPrimaryState = Calls.ControllerMap.RightController.GetPrimary() == 1f;
            currentMainSecondaryState = Calls.ControllerMap.RightController.GetSecondary() == 1f;
            currentOffPrimaryState = Calls.ControllerMap.LeftController.GetPrimary() == 1f;

            if (swapMainHand)
            {
                if (rightHandTransform != null && leftHandTransform != null)
                {
                    mainHandTransform = leftHandTransform;
                    offHandTransform = rightHandTransform;
                }

                currentTriggerState = Calls.ControllerMap.LeftController.GetTrigger() > triggerThreshold;
                currentGripState = Calls.ControllerMap.LeftController.GetGrip() > gripThreshold;
                currentMainPrimaryState = Calls.ControllerMap.LeftController.GetPrimary() == 1f;
                currentMainSecondaryState = Calls.ControllerMap.LeftController.GetSecondary() == 1f;
                currentOffPrimaryState = Calls.ControllerMap.RightController.GetPrimary() == 1f;
            }

            if (settingPlayKey || settingStopKey)
            {
                if (Input.anyKeyDown)
                {
                    foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
                    {
                        if (Input.GetKeyDown(key))
                        {
                            if (settingPlayKey && playKeyCodes.Count < 4 && !playKeyCodes.Contains(key))
                            {
                                playKeyCodes.Add(key);
                            }
                            if (settingStopKey && stopKeyCodes.Count < 4 && !playKeyCodes.Contains(key))
                            {
                                stopKeyCodes.Add(key);
                            }
                        }
                    }
                }
            }

            // Good luck navigating If Hell
            if (curveEditingEnabled)
            {
                if (scriptMenu != null)
                {
                    if (offHandTransform != null)
                    {
                        scriptMenu.transform.position = offHandTransform.position;
                        scriptMenu.transform.rotation = offHandTransform.rotation * Quaternion.Euler(50f, 0f, 0f);
                    }

                    if (mainHandTransform != null)
                    {
                        PerformRaycast(mainHandTransform);
                    }
                }

                // On trigger up
                if (!currentTriggerState && previousTriggerState)
                {
                    addUndoState();
                    isUndoRedoOperation = false;
                    buttonUnclick();
                }

                // On grip up
                if (!currentGripState && previousGripState)
                {
                    addUndoState();
                    isUndoRedoOperation = false;
                }

                // Value sliding
                if (slidingValue != null)
                {
                    if (modifyingCurveDuration)
                    {
                        currentCurve.duration = (int)slidingValue.Slide(mainHandTransform.position);
                        updateTexts();
                    }

                    if (modifyingStaticFOV)
                    {
                        staticCameraFOV = (int)slidingValue.Slide(mainHandTransform.position);
                        updateTexts();
                    }

                    if (modifyingStaticRoll)
                    {
                        staticCameraRoll = (int)slidingValue.Slide(mainHandTransform.position) % 360;
                        updateTexts();
                    }
                }

                // Only editing curve if you've selected a curve that exists
                editingCurve = currentCurve != null && Curves.Contains(currentCurve);

                if (editingCurve)
                {
                    if (!isPointingAtCanvas && !currentCurve.IsHidden && !isMinimized)
                    {
                        if (selectedTool == Tool.Select)
                        {
                            selectTool();
                        }

                        if (selectedTool == Tool.Pen)
                        {
                            penTool();
                        }

                        if (selectedTool == Tool.Anchor)
                        {
                            anchorTool();
                        }

                        if (addMarkerTools.Contains(selectedTool))
                        {
                            addMarkerTool(selectedTool);
                        }
                        else
                        {
                            GameObject.Destroy(markerPreview);
                            markerPreview = null;
                        }

                        if (addMarkerTools.Contains(selectedTool) || selectedTool == Tool.SelectMarker || selectedTool == Tool.DeleteMarker)
                        {
                            MarkerTool();
                        }

                        if (selectedTool == Tool.DeleteMarker)
                        {
                            DeleteMarkerTool();
                        }
                    }

                    foreach (var curve in Curves)
                    {
                        if (!curve.IsHidden)
                            curve.Render(curve == currentCurve);
                    }

                    if (mainHandTransform != null)
                    {
                        SelectControl(mainHandTransform.position);
                    }
                }

                if (!isPointingAtCanvas && !isMinimized)
                {
                    if (selectedTool == Tool.MoveStatic)
                    {
                        MoveStaticTool();
                    }

                    if (selectedTool == Tool.GrabStatic)
                    {
                        GrabStaticTool();
                    }
                }
            }

            previousTriggerState = currentTriggerState;
            previousGripState = currentGripState;

            if (mainHandTransform != null)
            {
                prevMainHandPos = mainHandTransform.position;
                prevMainHandRot = mainHandTransform.rotation;
            }
        }

        private void stopStatic()
        {
            isCameraStatic = false;
            updateTexts();

            if (cameraAudio)
            {
                try
                {
                    playerAudioListener.enabled = true;
                    camAudioListener.enabled = false;
                    //legacyCamAudioListener.enabled = false;
                }
                catch { }

                //legacyCamRC.enabled = true;
            }
        }

        private void playAnimation()
        {
            if (currentCurve != null)
            {
                isCameraAnimating = true;
                stopStatic();
                cameraLineOffset = 0f;
                passedPauseMarkers.Clear();

                if (cameraAudio)
                {
                    try
                    {
                        playerAudioListener.enabled = false;
                        if (useLegacyCam)
                        {
                            legacyCamAudioListener.enabled = true;
                        }
                        else
                        {
                            camAudioListener.enabled = true;
                        }
                    }
                    catch { }
                }

                if (useLegacyCam)
                {
                    legacyCamRC.enabled = false;
                }
            }

            if (playKeyCodes != null)
            {
                SimulateKeyCombo(playKeyCodes);
            }
        }

        private void stopAnimation()
        {
            if (livCameraIndicator != null)
            {
                livCameraIndicator.SetActive(false);
            }

            if (RotTargetObject != null)
            {
                RotTargetObject.SetActive(false);
            }

            if (FocusTargetObject != null)
            {
                FocusTargetObject.SetActive(false);
            }

            isCameraAnimating = false;

            passedPauseMarkers.Clear();
            isCameraPaused = false;
            isPlayingAll = false;

            if (cameraAudio)
            {
                try
                {
                    playerAudioListener.enabled = true;
                    camAudioListener.enabled = false;
                    //legacyCamAudioListener.enabled = false;
                }
                catch { }
            }
            
            //legacyCamRC.enabled = true;
        }

        private void detectFullPauses()
        {
            foreach (MarkerPoint marker in currentCurve.Markers)
            {
                if (marker.Type == MarkerType.Pause && marker.Value == -1)
                {
                    if (!passedPauseMarkers.Contains(marker) && marker.CurvePos <= currentCurve.GetAdjustedPosition(cameraLineOffset))
                    {
                        passedPauseMarkers.Add(marker);
                        isCameraPaused = true;
                    }
                }
            }
        }

        public void SimulateKeyCombo(List<KeyCode> recordedKeys)
        {
            //// Convert Unity KeyCode to InputSimulator VirtualKeyCode
            //List<VirtualKeyCode> virtualKeys = new List<VirtualKeyCode>();
            //foreach (KeyCode key in recordedKeys)
            //{
            //    virtualKeys.Add((VirtualKeyCode)System.Enum.Parse(typeof(VirtualKeyCode), key.ToString()));
            //}

            //// Simulate the key combination
            //sim.Keyboard.ModifiedKeyStroke(
            //    virtualKeys.GetRange(0, virtualKeys.Count - 1), // Modifier keys
            //    virtualKeys[virtualKeys.Count - 1]              // Final key
            //);
        }

        private void addUndoState()
        {
            if (isStateChanging || isUndoRedoOperation) return;

            CurveFile curveFile = new CurveFile
            {
                Curves = Curves.Select(curve => new Curve(curve.Id, curve.RotTargetID, curve.FocusTargetID, curve.Owner)
                {
                    ControlPoints = curve.ControlPoints.Select(point => new ControlPoint(curve, point.WorldPos, point.Handle1, point.Handle2, point.Type)).ToList(),
                    Markers = curve.Markers.Select(marker => new MarkerPoint(marker.CurveID, marker.CurvePos, marker.Type, marker.Value)).ToList(),
                    IsHidden = curve.IsHidden,
                    IsEnabled = curve.IsEnabled,
                    IsLoop = curve.IsLoop,
                    duration = curve.duration
                }).ToList(),
                SelectedCurveID = currentCurve?.Id
            };

            if (undoStates.Count > 0 && undoStates.Last().Equals(curveFile))
            {
                return;
            }

            undoStates.Add(curveFile);

            if (undoStates.Count > maxUndoStates)
            {
                undoStates.RemoveAt(0);
            }

            statesUndone = 0;
        }

        public float GetDefaultMarkerValue(MarkerPoint.MarkerType type)
        {
            float defaultValue = 0f;

            switch (type)
            {
                case MarkerType.Pause:
                    defaultValue = 0f;
                    break;
                case MarkerType.Roll:
                    defaultValue = 0f;
                    break;
                case MarkerType.FOV:
                    defaultValue = 60f;
                    break;
                case MarkerType.Speed:
                    defaultValue = 0f;
                    break;
            }

            return defaultValue;
        }

        private void moveCamera(Vector3 translation, Quaternion rotation, float fov)
        {
            if (useLegacyCam)
            {
                legacyCamRC.transform.position = translation;
                legacyCamRC.transform.rotation = rotation;
                legacyCamRC.GetComponent<Camera>().fieldOfView = fov;
            }
            else if (liv != null && liv.render != null)
            {
                liv.render.SetPose(translation, rotation, fov, false);
            }

            if (livCameraIndicator != null)
            {
                livCameraIndicator.transform.position = translation;
                livCameraIndicator.transform.rotation = rotation;
                livCameraIndicator.transform.localScale = new Vector3(60f / fov, 60f / fov, fov / 60f) * 0.1f;
            }
        }

        private float interpolateBetweenMarkers(MarkerPoint.MarkerType type)
        {
            MarkerPoint nextMarker = currentCurve.GetNextMarker(currentCurve.GetAdjustedPosition(cameraLineOffset), type);
            MarkerPoint prevMarker = currentCurve.GetPrevMarker(currentCurve.GetAdjustedPosition(cameraLineOffset), type);

            float value = 0f;

            if (prevMarker == null)
            {
                prevMarker = new MarkerPoint(currentCurve, 0f, type);
                prevMarker.Value = GetDefaultMarkerValue(type);
            }

            if (nextMarker == null)
            {
                nextMarker = new MarkerPoint(currentCurve, 1f, type);
                nextMarker.Value = prevMarker.Value;
            }

            float distanceBetweenMarkers = nextMarker.CurvePos - prevMarker.CurvePos;
            float camDistanceFromPrev = currentCurve.GetAdjustedPosition(cameraLineOffset) - prevMarker.CurvePos;
            float t = distanceBetweenMarkers != 0 ? camDistanceFromPrev / distanceBetweenMarkers : 0;

            switch (type)
            {
                case MarkerPoint.MarkerType.Roll:
                    value = Lerp(prevMarker.Value, nextMarker.Value, t);
                    break;
                case MarkerPoint.MarkerType.FOV:
                    value = Lerp(prevMarker.Value, nextMarker.Value, t);
                    break;
            }

            return value;
        }

        public float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }

        private void goToNextCurve()
        {
            if (Curves.IndexOf(currentCurve) == Curves.Count - 1)
            {
                selectCurve(storedSelectedCurve);
                stopAnimation();
                return;
            }

            int nextIndex = Curves.IndexOf(currentCurve) + 1;
            selectCurve(nextIndex);

            cameraLineOffset = 0f;
            passedPauseMarkers.Clear();
        }

        public Vector3 Snap(Vector3 input)
        {
            return input;

            if (!currentMainPrimaryState || !currentMainSecondaryState)
            {
                return input;
            }

            List<Vector3> snappers = new List<Vector3>();

            foreach (Curve curve in Curves)
            {
                foreach (ControlPoint controlPoint in curve.ControlPoints)
                {
                    snappers.Add(controlPoint.WorldPos);

                    if (currentCurve == curve)
                    {
                        snappers.Add(controlPoint.Handle1);
                        snappers.Add(controlPoint.Handle2);
                    }    
                }
            }

            float closestDistance = float.MaxValue;
            Vector3 snappedValue = input;

            foreach (Vector3 snapper in snappers)
            {
                float dist = Vector3.Distance(input, snapper);

                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    snappedValue = snapper;
                }
            }

            return snappedValue;
        }

        private void selectTool()
        {
            // On grip hold
            if (currentGripState)
            {
                if (!currentMainPrimaryState && !currentMainSecondaryState)
                    currentCurve.OffsetCurve(mainHandTransform.position - prevMainHandPos);

                if (currentMainPrimaryState && !currentMainSecondaryState)
                    currentCurve.OffsetCurve((mainHandTransform.position - prevMainHandPos) * 2f);

                if (!currentMainPrimaryState && currentMainSecondaryState)
                    currentCurve.OffsetCurve((mainHandTransform.position - prevMainHandPos) * 0.5f);

                if (currentMainPrimaryState && currentMainSecondaryState)
                    currentCurve.OffsetCurve((mainHandTransform.position - prevMainHandPos) * 10f);
            }
        }

        private void penTool()
        {
            // On trigger down
            if (currentTriggerState && !previousTriggerState)
            {
                if (selectedControl == null)
                {
                    activeRawPoint = currentCurve.AddPoint(mainHandTransform.position);
                }
                else
                {
                    if (selectedControl.IsHandle == false)
                    {
                        currentCurve.RemovePoint(selectedControl.ControlPoint);

                        if (isCameraAnimating)
                        {
                            if (currentCurve.ControlPoints.Count < 2)
                            {
                                stopAnimation();
                            }
                        }
                    }
                }
            }

            // On trigger hold
            if (currentTriggerState)
            {
                if (activeRawPoint != null)
                {
                    if (Vector3.Distance(mainHandTransform.position, activeRawPoint.WorldPos) > dragThreshold)
                    {
                        activeRawPoint.SetType(ControlPoint.ControlPointType.Symmetric);
                        activeRawPoint.Handle2 = mainHandTransform.position;

                        foreach (MarkerPoint marker in currentCurve.Markers)
                        {
                            marker.SetPosition(marker.CurvePos);
                        }
                    }
                }
            }

            // On trigger up
            if (!currentTriggerState && previousTriggerState)
            {
                activeRawPoint = null;
                currentCurve.UpdatePoints();
            }

            // On grip hold
            if (selectedControl != null && currentGripState)
            {
                if (selectedControl.IsHandle)
                {
                    if (selectedControl.HandleIndex == 0)
                    {
                        selectedControl.ControlPoint.Handle1 = Snap(mainHandTransform.position);
                    }
                    else
                    {
                        selectedControl.ControlPoint.Handle2 = Snap(mainHandTransform.position);
                    }
                }
                else
                {
                    selectedControl.ControlPoint.SetPosition(Snap(mainHandTransform.position));
                }

                foreach (MarkerPoint marker in currentCurve.Markers)
                {
                    marker.SetPosition(marker.CurvePos);
                }
            }

            // On grip up
            if (!currentGripState && previousGripState)
            {
                currentCurve.UpdatePoints();
            }
        }

        private void anchorTool()
        {
            // On trigger down
            if (currentTriggerState && !previousTriggerState)
            {
                if (selectedControl != null && selectedControl.IsHandle == false)
                {
                    activeRawPoint = selectedControl.ControlPoint;
                }

                if (selectedControl != null && selectedControl.IsHandle == true)
                {
                    selectedControl.ControlPoint.SetType(ControlPoint.ControlPointType.Corner);
                }

            }

            // On trigger hold
            if (currentTriggerState)
            {
                if (activeRawPoint != null)
                {
                    activeRawPoint.SetType(ControlPoint.ControlPointType.Symmetric);
                    activeRawPoint.Handle2 = mainHandTransform.position;

                    foreach (MarkerPoint marker in currentCurve.Markers)
                    {
                        marker.SetPosition(marker.CurvePos);
                    }
                }
            }

            // On trigger up
            if (!currentTriggerState && previousTriggerState)
            {
                activeRawPoint = null;
                currentCurve.UpdatePoints();
            }

            // On grip hold
            if (selectedControl != null && currentGripState)
            {
                selectedControl.ControlPoint.SetType(ControlPoint.ControlPointType.Free);

                if (selectedControl.IsHandle)
                {
                    if (selectedControl.HandleIndex == 0)
                    {
                        selectedControl.ControlPoint.Handle1 = mainHandTransform.position;
                    }
                    else
                    {
                        selectedControl.ControlPoint.Handle2 = mainHandTransform.position;
                    }
                }

                foreach (MarkerPoint marker in currentCurve.Markers)
                {
                    marker.SetPosition(marker.CurvePos);
                }
            }

            // On grip up
            if (!currentGripState && previousGripState)
            {
                currentCurve.UpdatePoints();
            }
        }

        private void addMarkerTool(Tool selectedTool)
        {
            float closestCurvePos = currentCurve.WorldPosToCurvePos(mainHandTransform.position);
            Vector3 closestWorldPos = currentCurve.CurvePosToWorldPos(closestCurvePos, 20, out _);
            float distanceToCurve = Vector3.Distance(mainHandTransform.position, closestWorldPos);

            MarkerPoint.MarkerType markerType = MarkerPoint.MarkerType.Pause;
            markerType = toolToMarkerType(selectedTool);

            // Hover add preview
            if (distanceToCurve <= pointGripRadius * 2)
            {
                if (!currentTriggerState)
                {
                    if (markerPreview == null)
                    {
                        markerPreview = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        markerPreview.GetComponent<SphereCollider>().enabled = false;
                        markerPreview.transform.localScale = Vector3.one * 0.03f * 0.75f * -1f;
                        markerPreview.transform.parent = modParentObject.transform;
                        markerPreview.layer = dontShowOnLIVLayer;
                        MeshRenderer mr = markerPreview.GetComponent<MeshRenderer>();
                        mr.material = curveMaterial;
                        mr.material.color = Color.yellow;

                        markerPreview.transform.position = closestWorldPos;
                    }
                    else
                    {
                        markerPreview.transform.position = closestWorldPos;
                    }
                }
            }
            else
            {
                if (markerPreview != null)
                {
                    GameObject.Destroy(markerPreview);
                    markerPreview = null;
                }
            }

            // On trigger down
            if (currentTriggerState && !previousTriggerState)
            {
                if (SelectedMarker == null)
                {
                    if (markerPreview != null)
                    {
                        ActiveRawMarker = currentCurve.AddMarker(markerPreview.transform.position, markerType);
                        GameObject.Destroy(markerPreview);
                        markerPreview = null;
                    }
                }
            }
        }

        private void MarkerTool()
        {
            // On trigger down
            if (currentTriggerState && !previousTriggerState)
            {
                if (SelectedMarker != null)
                {
                    ActiveRawMarker = SelectedMarker;
                }
            }

            // On trigger hold
            if (currentTriggerState)
            {
                if (ActiveRawMarker != null)
                {
                    if (slidingValue != null)
                    {
                        ActiveRawMarker.setValue(slidingValue.Slide(mainHandTransform.position));

                        if (ActiveRawMarker.Type == MarkerType.Pause)
                        {
                            updateTexts();
                        }
                    }

                    if (Vector3.Distance(mainHandTransform.position, ActiveRawMarker.WorldPos) > dragThreshold)
                    {
                        if (slidingValue == null)
                        {
                            if (ActiveRawMarker.Type == MarkerPoint.MarkerType.Roll)
                            {
                                slidingValue = new SlideValue(ActiveRawMarker.Value, int.MinValue, int.MaxValue, 1000, mainHandTransform.position.y);
                            }

                            if (ActiveRawMarker.Type == MarkerPoint.MarkerType.FOV)
                            {
                                slidingValue = new SlideValue(ActiveRawMarker.Value, 1, int.MaxValue, 100, mainHandTransform.position.y);
                            }

                            if (ActiveRawMarker.Type == MarkerPoint.MarkerType.Speed)
                            {
                                slidingValue = new SlideValue(ActiveRawMarker.Value, 0, 1, 10, mainHandTransform.position.y);
                            }

                            if (ActiveRawMarker.Type == MarkerPoint.MarkerType.Pause)
                            {
                                slidingValue = new SlideValue(ActiveRawMarker.Value, -1, int.MaxValue, 100000, mainHandTransform.position.y);
                            }
                        }
                    }
                }
            }

            // On trigger up
            if (!currentTriggerState && previousTriggerState)
            {
                ActiveRawMarker = null;
                slidingValue = null;
            }

            // On grip hold
            if (currentGripState)
            {
                if (SelectedMarker != null)
                {
                    SelectedMarker.SetPosition(currentCurve.WorldPosToCurvePos(mainHandTransform.position));
                }
            }
        }

        private void DeleteMarkerTool()
        {
            // On trigger down
            if (currentTriggerState && !previousTriggerState)
            {
                if (SelectedMarker != null)
                {
                    currentCurve.RemoveMarker(SelectedMarker);
                }
            }
        }

        private void MoveStaticTool()
        {
            // On grip hold
            if (currentGripState)
            {
                if (!currentMainPrimaryState && !currentMainSecondaryState)
                    staticCameraPos += mainHandTransform.position - prevMainHandPos;

                if (currentMainPrimaryState && !currentMainSecondaryState)
                    staticCameraPos += (mainHandTransform.position - prevMainHandPos) * 2f;

                if (!currentMainPrimaryState && currentMainSecondaryState)
                    staticCameraPos += (mainHandTransform.position - prevMainHandPos) * 0.5f;

                if (currentMainPrimaryState && currentMainSecondaryState)
                    staticCameraPos += (mainHandTransform.position - prevMainHandPos) * 10f;

                Quaternion deltaRotation = Quaternion.Inverse(prevMainHandRot) * mainHandTransform.rotation;
                staticCameraRot = staticCameraRot * deltaRotation;
            }
        }

        private void GrabStaticTool()
        {
            // On grip hold
            if (currentGripState)
            {
                staticCameraPos = mainHandTransform.position + (mainHandTransform.forward) * 0.1f;
                staticCameraRot = mainHandTransform.rotation;
            }
        }

        private MarkerPoint.MarkerType toolToMarkerType(Tool tool)
        {
            MarkerPoint.MarkerType markerType = MarkerPoint.MarkerType.Pause;

            switch (tool)
            {
                case Tool.AddPauseMarker:
                    markerType = MarkerPoint.MarkerType.Pause;
                    break;
                case Tool.AddRollMarker:
                    markerType = MarkerPoint.MarkerType.Roll;
                    break;
                case Tool.AddFOVMarker:
                    markerType = MarkerPoint.MarkerType.FOV;
                    break;
                case Tool.AddSpeedMarker:
                    markerType = MarkerPoint.MarkerType.Speed;
                    break;
            }

            return markerType;
        }

        public void renderLine(GameObject gameObject, List<Vector3> points)
        {
            if (points == null || points.Count < 2)
            {
                return;
            }

            if (gameObject.GetComponent<MeshFilter>() == null)
            {
                gameObject.AddComponent<MeshFilter>();
            }

            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            Mesh mesh = new Mesh();
            meshFilter.mesh = mesh;

            Vector3[] vertices = points.ToArray();
            int[] indices = new int[(points.Count - 1) * 2];

            for (int i = 0; i < points.Count - 1; i++)
            {
                indices[i * 2] = i;
                indices[i * 2 + 1] = i + 1;
            }

            mesh.vertices = vertices;
            mesh.SetIndices(indices, MeshTopology.Lines, 0);
            mesh.RecalculateBounds();
        }

        private void PerformRaycast(Transform handTransform)
        {
            Ray ray = new Ray(handTransform.position, handTransform.forward);
            RaycastHit[] hits = Physics.RaycastAll(ray);

            isPointingAtCanvas = false;

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.transform.IsChildOf(scriptMenu.transform))
                {
                    isPointingAtCanvas = true;
                    if (uiPointer == null)
                    {
                        uiPointer = new GameObject("UIPointer");
                        MeshRenderer mr = uiPointer.AddComponent<MeshRenderer>();
                        mr.material = curveMaterial;
                        mr.material.color = Color.green;
                        uiPointer.transform.SetParent(modParentObject.transform, true);
                        uiPointer.layer = dontShowOnLIVLayer;
                    }

                    uiPointer.SetActive(true);
                    renderLine(uiPointer, new List<Vector3> { handTransform.position, hit.point });
                }

                if (currentTriggerState && !previousTriggerState)
                {
                    buttonClick(hit.collider.name);
                }
            }

            if (!isPointingAtCanvas && uiPointer != null)
            {
                uiPointer.SetActive(false);
            }
        }

        public void buttonClick(string objectName)
        {
            if (objectName == "UndoCollider")
            {
                isUndoRedoOperation = true;
                Undo();
            }

            if (objectName == "RedoCollider")
            {
                isUndoRedoOperation = true;
                Redo();
            }

            if (objectName == "CloseCollider")
            {
                curveEditingEnabled = false;
                Mod.Settings[1].Value = false;
                closeMenu();
            }

            if (objectName == "MinimizeCollider")
            {
                Maximized.SetActive(false);
                Minimized.SetActive(true);
                isMinimized = true;
            }

            if (objectName == "MaximizeCollider")
            {
                Maximized.SetActive(true);
                Minimized.SetActive(false);
                isMinimized = false;
            }

            if (objectName == "CreateCurveCollider")
            {
                stopAnimation();
                currentCurve = AddCurve();
                selectCurve(currentCurve);
                selectedTool = Tool.Pen;
            }

            if (objectName == "CreateCircleCollider")
            {
                addCircleCurve();
            }

            if (objectName == "SelectionTabCollider")
            {
                disableAllMenus();
                UISelectMenu.SetActive(true);
            }

            if (objectName == "EditTabCollider")
            {
                disableAllMenus();
                UIEditMenu.SetActive(true);
            }

            if (objectName == "MarkersTabCollider")
            {
                disableAllMenus();
                UIMarkersMenu.SetActive(true);
            }

            if (objectName == "StaticTabCollider")
            {
                disableAllMenus();
                UIStaticMenu.SetActive(true);
            }

            if (objectName == "NetworkTabCollider")
            {
                disableAllMenus();
                UINetworkMenu.SetActive(true);
            }

            if (objectName == "FileTabCollider")
            {
                disableAllMenus();
                UIFileMenu.SetActive(true);
            }

            if (objectName == "SettingsTabCollider")
            {
                disableAllMenus();
                UISettingsMenu.SetActive(true);
            }

            if (Curves.Count > 1)
            {
                if (objectName == "NextCurveCollider")
                {
                    if (currentCurve != null)
                    {
                        selectCurve((Curves.IndexOf(currentCurve) + 1) % Curves.Count);
                    }
                    else
                    {
                        selectCurve(Curves.Count - 1);
                    }
                }

                if (objectName == "PreviousCurveCollider")
                {
                    if (currentCurve != null)
                    {
                        int previousIndex = Curves.IndexOf(currentCurve) - 1;
                        if (previousIndex < 0)
                        {
                            previousIndex = Curves.Count - 1;
                        }
                        selectCurve(previousIndex);
                    }
                    else
                    {
                        selectCurve(Curves.Count - 1);
                    }
                }
            }

            if (Curves.Count >= 1)
            {
                if (objectName == "PlayButtonCollider")
                {
                    if (currentCurve.IsEnabled)
                    {
                        playAnimation();
                    }
                }

                if (objectName == "PlayAllButtonCollider")
                {
                    storedSelectedCurve = currentCurve;
                    selectCurve(Curves[0]);

                    isPlayingAll = true;
                    playAnimation();
                }

                if (objectName == "StopButtonCollider")
                {
                    stopAnimation();

                    if (stopKeyCodes != null)
                    {
                        SimulateKeyCombo(stopKeyCodes);
                    }
                }
            }

            if (objectName == "LoadCollider")
            {
                loadCurves(readFromFile());
            }

            if (objectName == "AppendCollider")
            {
                appendCurves(readFromFile());
            }

            if (objectName == "OverwriteCollider")
            {
                overwriteCurves(readFromFile());
            }

            if (currentCurve != null)
            {
                if (objectName == "HideCollider")
                {
                    if (currentCurve.IsHidden)
                    {
                        currentCurve.IsHidden = false;
                    }
                    else
                    {
                        currentCurve.IsHidden = true;
                        currentCurve.Unrender();
                    }

                    updateTexts();
                }

                if (objectName == "EnableCollider")
                {
                    currentCurve.IsEnabled = !currentCurve.IsEnabled;
                    updateTexts();
                }

                if (objectName == "LoopCollider")
                {
                    currentCurve.IsLoop = !currentCurve.IsLoop;
                    updateTexts();
                }

                if (objectName == "ReverseCurveCollider")
                {
                    currentCurve.ControlPoints.Reverse();
                    foreach (ControlPoint controlPoint in currentCurve.ControlPoints)
                    {
                        Vector3 temp = controlPoint.Handle1;
                        controlPoint.Handle1 = controlPoint.Handle2;
                        controlPoint.Handle2 = temp;
                    }

                    foreach (MarkerPoint marker in currentCurve.Markers)
                    {
                        marker.SetPosition(marker.CurvePos);
                    }
                }

                if (objectName == "DeleteCurveCollider")
                {
                    stopAnimation();

                    currentCurve.Unrender();

                    foreach (Curve curve in Curves)
                    {
                        if (curve.RotTarget == currentCurve)
                        {
                            curve.RotTarget = null;
                        }
                    }

                    Curves.Remove(currentCurve);
                    GameObject.Destroy(currentCurve.curveObject);

                    if (Curves.Count > 0)
                    {
                        selectCurve(Curves.Count - 1);
                    }
                    else
                    {
                        selectCurve(null);
                    }
                }

                if (objectName == "NextCurve2Collider")
                {
                    List<Curve> targetCurves = GetAvailableTargetCurves();
                    Curve selectedTarget;

                    if (currentCurve.RotTarget != null)
                    {
                        selectedTarget = targetCurves[(targetCurves.IndexOf(currentCurve.RotTarget) + 1) % targetCurves.Count];
                    }
                    else
                    {
                        selectedTarget = targetCurves[1];
                    }

                    currentCurve.RotTarget = selectedTarget;
                    if (selectedTarget != null)
                    {
                        currentCurve.RotTargetID = selectedTarget.Id;
                    }

                    UISelectedRotTarget.GetComponent<TextMeshProUGUI>().text = selectedTarget != null ? selectedTarget.Id : EmptyField;
                }

                if (objectName == "PreviousCurve2Collider")
                {
                    List<Curve> targetCurves = GetAvailableTargetCurves();
                    Curve selectedTarget;

                    if (currentCurve.RotTarget != null)
                    {
                        int previousIndex = targetCurves.IndexOf(currentCurve.RotTarget) - 1;
                        if (previousIndex < 0)
                        {
                            previousIndex = Curves.Count - 1;
                        }
                        selectedTarget = targetCurves[previousIndex];
                    }
                    else
                    {
                        selectedTarget = targetCurves[targetCurves.Count - 1];
                    }

                    currentCurve.RotTarget = selectedTarget;
                    if (selectedTarget != null)
                    {
                        currentCurve.RotTargetID = selectedTarget.Id;
                    }

                    UISelectedRotTarget.GetComponent<TextMeshProUGUI>().text = selectedTarget != null ? selectedTarget.Id : EmptyField;
                }

                if (objectName == "NextCurve3Collider")
                {
                    List<Curve> targetCurves = GetAvailableTargetCurves();
                    Curve selectedTarget;

                    if (currentCurve.FocusTarget != null)
                    {
                        selectedTarget = targetCurves[(targetCurves.IndexOf(currentCurve.FocusTarget) + 1) % targetCurves.Count];
                    }
                    else
                    {
                        selectedTarget = targetCurves[1];
                    }

                    currentCurve.FocusTarget = selectedTarget;
                    if (selectedTarget != null)
                    {
                        currentCurve.FocusTargetID = selectedTarget.Id;
                    }

                    UISelectedFocusTarget.GetComponent<TextMeshProUGUI>().text = selectedTarget != null ? selectedTarget.Id : EmptyField;
                }

                if (objectName == "PreviousCurve2Collider")
                {
                    List<Curve> targetCurves = GetAvailableTargetCurves();
                    Curve selectedTarget;

                    if (currentCurve.FocusTarget != null)
                    {
                        int previousIndex = targetCurves.IndexOf(currentCurve.FocusTarget) - 1;
                        if (previousIndex < 0)
                        {
                            previousIndex = Curves.Count - 1;
                        }
                        selectedTarget = targetCurves[previousIndex];
                    }
                    else
                    {
                        selectedTarget = targetCurves[targetCurves.Count - 1];
                    }

                    currentCurve.FocusTarget = selectedTarget;
                    if (selectedTarget != null)
                    {
                        currentCurve.FocusTargetID = selectedTarget.Id;
                    }

                    UISelectedFocusTarget.GetComponent<TextMeshProUGUI>().text = selectedTarget != null ? selectedTarget.Id : EmptyField;
                }

                if (objectName == "SelectionToolCollider")
                {
                    selectedTool = Tool.Select;
                    updateToolButtons();
                }

                if (objectName == "PenToolCollider")
                {
                    selectedTool = Tool.Pen;
                    updateToolButtons();
                }

                if (objectName == "AnchorToolCollider")
                {
                    selectedTool = Tool.Anchor;
                    updateToolButtons();
                }

                if (objectName == "SetDurationButtonCollider")
                {
                    slidingValue = new SlideValue(currentCurve.duration, 1, int.MaxValue, 10000, mainHandTransform.position.y);
                    modifyingCurveDuration = true;
                }

                if (objectName == "ResetDurationButtonCollider")
                {
                    currentCurve.duration = 5000;
                    updateTexts();
                }

                if (objectName == "IncrementXCollider")
                {
                    if (!currentMainPrimaryState && !currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.right * 0.1f);
                    else if (currentMainPrimaryState && !currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.right * 0.2f);
                    else if (!currentMainPrimaryState && currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.right * 0.01f);
                    else if (currentMainPrimaryState && currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.right * 1f);
                }

                if (objectName == "DecrementXCollider")
                {
                    if (!currentMainPrimaryState && !currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.left * 0.1f);
                    else if (currentMainPrimaryState && !currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.left * 0.2f);
                    else if (!currentMainPrimaryState && currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.left * 0.01f);
                    else if (currentMainPrimaryState && currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.left * 1f);
                }

                if (objectName == "IncrementYCollider")
                {
                    if (!currentMainPrimaryState && !currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.up * 0.1f);
                    if (currentMainPrimaryState && !currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.up * 0.2f);
                    if (!currentMainPrimaryState && currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.up * 0.01f);
                    if (currentMainPrimaryState && currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.up * 1f);
                }

                if (objectName == "DecrementYCollider")
                {
                    if (!currentMainPrimaryState && !currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.down * 0.1f);
                    if (currentMainPrimaryState && !currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.down * 0.2f);
                    if (!currentMainPrimaryState && currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.down * 0.01f);
                    if (currentMainPrimaryState && currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.down * 1f);
                }

                if (objectName == "IncrementZCollider")
                {
                    if (!currentMainPrimaryState && !currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.forward * 0.1f);
                    if (currentMainPrimaryState && !currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.forward * 0.2f);
                    if (!currentMainPrimaryState && currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.forward * 0.01f);
                    if (currentMainPrimaryState && currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.forward * 1f);
                }

                if (objectName == "DecrementZCollider")
                {
                    if (!currentMainPrimaryState && !currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.back * 0.1f);
                    if (currentMainPrimaryState && !currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.back * 0.2f);
                    if (!currentMainPrimaryState && currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.back * 0.01f);
                    if (currentMainPrimaryState && currentMainSecondaryState)
                        currentCurve.OffsetCurve(Vector3.back * 1f);
                }

                if (objectName == "SelectMarkerToolCollider")
                {
                    selectedTool = Tool.SelectMarker;
                    updateToolButtons();
                }

                if (objectName == "AddPauseMarkerToolCollider")
                {
                    selectedTool = Tool.AddPauseMarker;
                    updateToolButtons();
                }

                if (objectName == "AddRollMarkerToolCollider")
                {
                    selectedTool = Tool.AddRollMarker;
                    updateToolButtons();
                }

                if (objectName == "AddFOVMarkerToolCollider")
                {
                    selectedTool = Tool.AddFOVMarker;
                    updateToolButtons();
                }

                if (objectName == "AddSpeedMarkerToolCollider")
                {
                    selectedTool = Tool.AddSpeedMarker;
                    updateToolButtons();
                }

                if (objectName == "DeleteMarkerToolCollider")
                {
                    selectedTool = Tool.DeleteMarker;
                    updateToolButtons();
                }

                if (objectName == "SaveCurveCollider")
                {
                    curvesToSave.Clear();
                    curvesToSave.Add(currentCurve);
                    if (currentCurve.RotTarget != null)
                        curvesToSave.Add(currentCurve.RotTarget);

                    writeToFile(false);
                    RefreshFileList();
                    UpdateFileUI();
                }

                if (objectName == "SaveAllCurvesCollider")
                {
                    curvesToSave.Clear();
                    curvesToSave.AddRange(Curves);

                    writeToFile(false);
                    RefreshFileList();
                    UpdateFileUI();
                }

                if (objectName == "UploadCollider")
                {
                    List<Curve> curvesToSave = new List<Curve>();
                    curvesToSave.Add(currentCurve);
                    if (currentCurve.RotTarget != null)
                        curvesToSave.Add(currentCurve.RotTarget);

                    CurveFile curveFile = new CurveFile
                    {
                        FileName = "",
                        Owner = Calls.Managers.GetPlayerManager().LocalPlayer.Data.GeneralData.PlayFabMasterId,
                        Curves = curvesToSave,
                        SelectedCurveID = currentCurve?.Id
                    };

                    string strToSend = Newtonsoft.Json.JsonConvert.SerializeObject(curveFile, Newtonsoft.Json.Formatting.Indented,
                    new Newtonsoft.Json.JsonSerializerSettings
                    {
                        ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                    });

                    PhotonNetwork.RaiseEvent(eventNumber, "N" + "--------" + strToSend, eventOptions, SendOptions.SendReliable);
                }

                if (objectName == "DownloadCollider")
                {
                    PhotonNetwork.RaiseEvent(eventNumber, "R" + currentCurve.Id + "--", eventOptions, SendOptions.SendReliable);
                }

                if (objectName == "AcceptCollider")
                {
                    if (curveSendRequest != null)
                    {
                        overwriteCurves(curveSendRequest);
                        curveSendRequest = null;
                        updateTexts();
                    }
                }

                if (objectName == "RejectCollider")
                {
                    curveSendRequest = null;
                    updateTexts();
                }

                if (objectName == "OrderLeftCollider")
                {
                    int currentIndex = Curves.IndexOf(currentCurve);
                    if (currentIndex > 0)
                    {
                        // Swap the current curve with the previous one
                        Curve temp = Curves[currentIndex - 1];
                        Curves[currentIndex - 1] = currentCurve;
                        Curves[currentIndex] = temp;
                    }

                    updateTexts();
                }

                if (objectName == "OrderRightCollider")
                {
                    int currentIndex = Curves.IndexOf(currentCurve);
                    if (currentIndex < Curves.Count - 1)
                    {
                        // Swap the current curve with the next one
                        Curve temp = Curves[currentIndex + 1];
                        Curves[currentIndex + 1] = currentCurve;
                        Curves[currentIndex] = temp;
                    }

                    updateTexts();
                }
            }

            if (objectName == "SaveStaticCollider")
            {
                writeToFile(true);
                RefreshFileList();
                UpdateFileUI();
            }

            if (objectName == "StaticCollider")
            {
                if (!isCameraStatic)
                {
                    stopAnimation();
                    livCameraIndicator.SetActive(true);
                    isCameraStatic = true;

                    if (cameraAudio)
                    {
                        try
                        {
                            playerAudioListener.enabled = false;
                            if (useLegacyCam)
                            {
                                legacyCamAudioListener.enabled = true;
                            }
                            else
                            {
                                camAudioListener.enabled = true;
                            }
                        }
                        catch { }
                    }
                }
                else
                {
                    livCameraIndicator.SetActive(false);
                    isCameraStatic = false;
                }

                updateTexts();
            }

            if (objectName == "SetFOVCollider")
            {
                slidingValue = new SlideValue(staticCameraFOV, 1, int.MaxValue, 100, mainHandTransform.position.y);
                modifyingStaticFOV = true;
            }

            if (objectName == "SetRollCollider")
            {
                slidingValue = new SlideValue(staticCameraRoll, int.MinValue, int.MaxValue, 1000, mainHandTransform.position.y);
                modifyingStaticRoll = true;
            }

            if (objectName == "MoveStaticToolCollider")
            {
                selectedTool = Tool.MoveStatic;
                updateToolButtons();
            }

            if (objectName == "GrabStaticToolCollider")
            {
                selectedTool = Tool.GrabStatic;
                updateToolButtons();
            }

            if (objectName == "RefreshFilesCollider")
            {
                RefreshFileList();
                UpdateFileUI();
            }

            if (objectName == "NextFilesCollider")
            {
                if ((filePage + 1) * UIfileOptions.Count < fileNames.Count)
                {
                    filePage++;
                    UpdateFileUI();
                }
            }

            if (objectName == "PrevFilesCollider")
            {
                if (filePage > 0)
                {
                    filePage--;
                    UpdateFileUI();
                }
            }

            if (objectName.Contains("FileOptionCollider"))
            {
                selectedFileOption = int.Parse(objectName.Replace("FileOptionCollider", ""));
                UpdateFileUI();

                for (int i = 0; i < UIfileOptions.Count; i++)
                {
                    if (i == selectedFileOption)
                    {
                        UIfileOptions[i].transform.GetChild(0).GetComponent<Image>().color = new Color(0.337f, 0.604f, 0.337f, 1.0f);
                    }
                    else
                    {
                        UIfileOptions[i].transform.GetChild(0).GetComponent<Image>().color = new Color(0.094f, 0.094f, 0.094f, 1.0f);
                    }
                }
            }

            if (objectName == "LoadInGymCollider")
            {
                ToggleLoadInList(loadInGym, UILoadInGym);
            }

            if (objectName == "LoadInParkCollider")
            {
                ToggleLoadInList(loadInPark, UILoadInPark);
            }

            if (objectName == "LoadInRingCollider")
            {
                ToggleLoadInList(loadInRing, UILoadInRing);
            }

            if (objectName == "LoadInPitCollider")
            {
                ToggleLoadInList(loadInPit, UILoadInPit);
            }

            if (objectName == "SwapHandsCollider")
            {
                swapMainHand = !swapMainHand;
                updateTexts();
            }

            if (objectName == "SnapPositionCollider")
            {
                snapPosition = !snapPosition;
                updateTexts();
            }

            if (objectName == "SnapRotationCollider")
            {
                snapRotation = !snapRotation;
                updateTexts();
            }

            if (objectName == "CameraAudioCollider")
            {
                if (cameraAudio)
                {
                    cameraAudio = false;
                    playerAudioListener.enabled = true;
                    camAudioListener.enabled = false;
                }
                else
                {
                    cameraAudio = true;
                }
                updateTexts();
            }

            if (objectName == "HideTagsCollider")
            {
                hideTags = !hideTags;
                MelonCoroutines.Start(setTagsVisibility(!hideTags));
                updateTexts();
            }
        }

        private IEnumerator setTagsVisibility(bool visible)
        {
            yield return new WaitForSeconds(1);

            foreach (var player in Calls.Managers.GetPlayerManager().AllPlayers)
            {
                PlayerController controller = player.Controller;

                GameObject health = null;
                GameObject nameTag = null;

                if (player != Calls.Managers.GetPlayerManager().LocalPlayer)
                {
                    health = controller.transform.GetChild(5).gameObject;
                    nameTag = controller.transform.GetChild(9).gameObject;
                }
                else
                {
                    health = localHealthBar;
                }

                LayerMask toLayer = LayerMask.NameToLayer("Default");

                if (!visible)
                {
                    toLayer = dontShowOnLIVLayer;
                }

                if (health != null)
                {
                    health.transform.GetChild(0).GetChild(0).gameObject.layer = toLayer;
                    health.transform.GetChild(0).GetChild(1).GetChild(0).gameObject.layer = toLayer;
                    health.transform.GetChild(0).GetChild(1).GetChild(1).gameObject.layer = toLayer;
                    health.transform.GetChild(0).GetChild(1).GetChild(2).gameObject.layer = toLayer;
                    health.transform.GetChild(1).GetChild(0).gameObject.layer = toLayer;
                    health.transform.GetChild(1).GetChild(1).gameObject.layer = toLayer;
                }

                if (nameTag != null)
                {
                    for (int i = 0; i < nameTag.transform.childCount; i++)
                    {
                        nameTag.transform.GetChild(i).gameObject.layer = toLayer;
                    }
                }
            }
        }

        private void ToggleLoadInList(List<string> loadList, GameObject uiButton)
        {
            int fileIndex = filePage * UIfileOptions.Count + selectedFileOption;
            if (fileIndex >= fileNames.Count)
            {
                return;
            }

            string selectedFile = fileNames[fileIndex];

            if (loadList.Contains(selectedFile))
            {
                loadList.Remove(selectedFile);
                uiButton.GetComponent<Image>().color = new Color(0.194f, 0.358f, 0.215f, 1.0f);
            }
            else
            {
                loadList.Add(selectedFile);
                uiButton.GetComponent<Image>().color = new Color(0.338f, 0.603f, 0.338f, 1f);
            }

            SaveOtherSettings();
        }

        private void addCircleCurve()
        {
            Curve newCircle = AddCurve();
            newCircle.IsLoop = true;

            float controlDistance = 0.552284749831f;
            float scaleMultiplier = 0.25f;

            newCircle.AddPoint(new Vector3(1, 0, 0) * scaleMultiplier, new Vector3(1, 0, -controlDistance) * scaleMultiplier); // Quadrant 1
            newCircle.AddPoint(new Vector3(0, 0, 1) * scaleMultiplier, new Vector3(controlDistance, 0, 1) * scaleMultiplier); // Quadrant 2
            newCircle.AddPoint(new Vector3(-1, 0, 0) * scaleMultiplier, new Vector3(-1, 0, controlDistance) * scaleMultiplier); // Quadrant 3
            newCircle.AddPoint(new Vector3(0, 0, -1) * scaleMultiplier, new Vector3(-controlDistance, 0, -1) * scaleMultiplier); // Quadrant 4
            newCircle.AddPoint(new Vector3(1, 0, 0) * scaleMultiplier, new Vector3(1, 0, -controlDistance) * scaleMultiplier); // Loop

            currentCurve = newCircle;
            newCircle.UpdatePoints();
            newCircle.Render(true);

            newCircle.OffsetCurve(mainHandTransform.position);

            updateTexts();
        }

        private void Undo()
        {
            if (undoStates.Count - statesUndone > 1)
            {
                statesUndone++;
                isStateChanging = true;
                loadCurves(undoStates[undoStates.Count - 1 - statesUndone]);
                isStateChanging = false;
            }
        }

        private void Redo()
        {
            if (statesUndone > 0)
            {
                statesUndone--;
                isStateChanging = true;
                loadCurves(undoStates[undoStates.Count - 1 - statesUndone]);
                isStateChanging = false;
            }
        }

        private void loadCurves(CurveFile curveFile)
        {
            if (curveFile.IsStatic)
            {
                isCameraStatic = true;
                staticCameraPos = curveFile.StaticCameraPos;
                staticCameraRot = curveFile.StaticCameraRot;
                staticCameraFOV = curveFile.StaticFOV;
                staticCameraRoll = curveFile.StaticRoll;
                livCameraIndicator.SetActive(true);
                updateTexts();
                return;
            }

            foreach (Curve curve in Curves)
            {
                curve.Unrender();
            }

            Curves.Clear();
            Curves = curveFile.Curves.Select(curve => new Curve(curve.Id, curve.RotTargetID, curve.FocusTargetID, curve.Owner)
            {
                ControlPoints = curve.ControlPoints.Select(point => new ControlPoint(curve, point.WorldPos, point.Handle1, point.Handle2, point.Type)).ToList(),
                Markers = curve.Markers.Select(marker => new MarkerPoint(marker.CurveID, marker.CurvePos, marker.Type, marker.Value)).ToList(),
                IsHidden = curve.IsHidden,
                IsEnabled = curve.IsEnabled,
                IsLoop = curve.IsLoop,
                duration = curve.duration
            }).ToList();

            currentCurve = Curves.FirstOrDefault(curve => curve.Id == curveFile.SelectedCurveID);

            foreach (Curve curve in Curves)
            {
                foreach (MarkerPoint marker in curve.Markers)
                {
                    marker.Curve = curve;
                    marker.WorldPos = curve.CurvePosToWorldPos(marker.CurvePos, 20, out _);
                }

                curve.RotTarget = GetCurveByID(curve.RotTargetID);

                if (!curve.IsHidden)
                    curve.Render(curve == currentCurve);

                currentCurve.UpdatePoints();
            }

            updateTexts();
        }

        private void appendCurves(CurveFile curveFile)
        {
            if (curveFile.IsStatic)
            {
                isCameraStatic = true;
                staticCameraPos = curveFile.StaticCameraPos;
                staticCameraRot = curveFile.StaticCameraRot;
                staticCameraFOV = curveFile.StaticFOV;
                staticCameraRoll = curveFile.StaticRoll;
                updateTexts();
                return;
            }

            foreach (Curve newCurve in curveFile.Curves)
            {
                Curve existingCurve = GetCurveByID(newCurve.Id);
                if (existingCurve != null)
                {
                    string newID = GenerateUniqueId();

                    foreach (Curve curveCheck in curveFile.Curves)
                    {
                        if (curveCheck.RotTargetID == newCurve.Id)
                        {
                            newCurve.RotTargetID = newID;
                        }
                    }

                    if (newCurve.Id == curveFile.SelectedCurveID)
                    {
                        curveFile.SelectedCurveID = newID;
                    }

                    newCurve.SetID(newID);
                }
                
                // Append the new curve
                Curves.Add(newCurve);
            }

            currentCurve = GetCurveByID(curveFile.SelectedCurveID);

            foreach (Curve curve in Curves)
            {
                foreach (MarkerPoint marker in curve.Markers)
                {
                    marker.Curve = curve;
                    marker.WorldPos = curve.CurvePosToWorldPos(marker.CurvePos, 20, out _);
                }

                curve.RotTarget = GetCurveByID(curve.RotTargetID);

                if (!curve.IsHidden)
                    curve.Render(curve == currentCurve);

                currentCurve.UpdatePoints();
            }

            updateTexts();
        }

        private void overwriteCurves(CurveFile curveFile)
        {
            if (curveFile.IsStatic)
            {
                isCameraStatic = true;
                staticCameraPos = curveFile.StaticCameraPos;
                staticCameraRot = curveFile.StaticCameraRot;
                staticCameraFOV = curveFile.StaticFOV;
                staticCameraRoll = curveFile.StaticRoll;
                updateTexts();
                return;
            }

            foreach (Curve newCurve in curveFile.Curves)
            {
                Curve existingCurve = GetCurveByID(newCurve.Id);
                if (existingCurve != null)
                {
                    // Overwrite the existing curve
                    existingCurve.Unrender();
                    Curves.Remove(existingCurve);
                }
                // Append the new curve
                Curves.Add(newCurve);
            }

            currentCurve = GetCurveByID(curveFile.Curves[0].Id);

            // Render all curves
            foreach (Curve curve in Curves)
            {
                foreach (MarkerPoint marker in curve.Markers)
                {
                    marker.Curve = curve;
                    marker.WorldPos = curve.CurvePosToWorldPos(marker.CurvePos, 20, out _);
                }

                curve.RotTarget = GetCurveByID(curve.RotTargetID);

                if (!curve.IsHidden)
                    curve.Render(curve == currentCurve);

                currentCurve.UpdatePoints();
            }

            updateTexts();
        }

        private void RefreshFileList()
        {
            fileNames.Clear();
            string[] files = Directory.GetFiles(directoryPath, "*.json")
                                      .OrderByDescending(f => File.GetCreationTime(f))
                                      .ToArray();
            foreach (string file in files)
            {
                fileNames.Add(Path.GetFileName(file));
            }

            LoadOtherSettings();
            UpdateFileUI();
        }

        private void UpdateFileUI()
        {
            int startIndex = filePage * UIfileOptions.Count;
            int endIndex = Mathf.Min(startIndex + UIfileOptions.Count, fileNames.Count);

            for (int i = 0; i < UIfileOptions.Count; i++)
            {
                GameObject optionText = UIfileOptions[i].transform.GetChild(1).gameObject;
                GameObject optionButton = UIfileOptions[i].transform.GetChild(0).gameObject;

                if (startIndex + i < endIndex)
                {
                    string fileName = fileNames[startIndex + i];
                    optionText.GetComponent<TextMeshProUGUI>().text = fileName;
                    optionText.SetActive(true);

                    // Highlight the selected file
                    if (startIndex + i == filePage * UIfileOptions.Count + selectedFileOption)
                    {
                        optionButton.GetComponent<Image>().color = new Color(0.337f, 0.604f, 0.337f, 1.0f);
                    }
                    else
                    {
                        optionButton.GetComponent<Image>().color = new Color(0.094f, 0.094f, 0.094f, 1.0f);
                    }
                }
                else
                {
                    optionText.SetActive(false);
                }
            }

            // Update the toggle buttons based on the selected file
            if (selectedFileOption < fileNames.Count)
            {
                string selectedFile = fileNames[selectedFileOption];

                if (loadInGym.Contains(selectedFile))
                {
                    UILoadInGym.GetComponent<Image>().color = new Color(0.338f, 0.603f, 0.338f, 1f);
                }
                else
                {
                    UILoadInGym.GetComponent<Image>().color = new Color(0.194f, 0.358f, 0.215f, 1.0f);
                }

                if (loadInPark.Contains(selectedFile))
                {
                    UILoadInPark.GetComponent<Image>().color = new Color(0.338f, 0.603f, 0.338f, 1f);
                }
                else
                {
                    UILoadInPark.GetComponent<Image>().color = new Color(0.194f, 0.358f, 0.215f, 1.0f);
                }

                if (loadInRing.Contains(selectedFile))
                {
                    UILoadInRing.GetComponent<Image>().color = new Color(0.338f, 0.603f, 0.338f, 1f);
                }
                else
                {
                    UILoadInRing.GetComponent<Image>().color = new Color(0.194f, 0.358f, 0.215f, 1.0f);
                }

                if (loadInPit.Contains(selectedFile))
                {
                    UILoadInPit.GetComponent<Image>().color = new Color(0.338f, 0.603f, 0.338f, 1f);
                }
                else
                {
                    UILoadInPit.GetComponent<Image>().color = new Color(0.194f, 0.358f, 0.215f, 1.0f);
                }
            }
        }

        private void buttonUnclick()
        {
            slidingValue = null;
            modifyingCurveDuration = false;
            modifyingStaticFOV = false;
            modifyingStaticRoll = false;
        }

        public void disableAllMenus()
        {
            UISelectMenu.SetActive(false);
            UIEditMenu.SetActive(false);
            UIMarkersMenu.SetActive(false);
            UIStaticMenu.SetActive(false);
            UINetworkMenu.SetActive(false);
            UIFileMenu.SetActive(false);
            UISettingsMenu.SetActive(false);
        }

        public List<Curve> GetAvailableTargetCurves()
        {
            List<Curve> targetCurves = new List<Curve>();
            targetCurves.Add(null);
            targetCurves.AddRange(Curves);
            targetCurves.Remove(currentCurve);
            return targetCurves;
        }

        private void writeToFile(bool saveStatic)
        {
            int fileCount = Directory.GetFiles(directoryPath, "*.json").Length;
            string filePath = Path.Combine(directoryPath, $"{fileCount + 1}.json");

            CurveFile curveFile = new CurveFile
            {
                FileName = Path.GetFileName(filePath),
                Owner = Calls.Managers.GetPlayerManager().LocalPlayer.Data.GeneralData.PlayFabMasterId,
                Curves = saveStatic ? new List<Curve>() : curvesToSave,
                SelectedCurveID = saveStatic ? null : currentCurve?.Id // Store the selected curve ID
            };

            if (saveStatic)
            {
                curveFile.IsStatic = true;
                curveFile.StaticCameraPos = staticCameraPos;
                curveFile.StaticCameraRot = staticCameraRot;
                curveFile.StaticFOV = staticCameraFOV;
                curveFile.StaticRoll = staticCameraRoll;
            }

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(curveFile, Newtonsoft.Json.Formatting.Indented,
            new Newtonsoft.Json.JsonSerializerSettings
            {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            });

            File.WriteAllText(filePath, json);
        }

        private CurveFile readFromFile()
        {
            int fileIndex = filePage * UIfileOptions.Count + selectedFileOption;
            if (fileIndex >= fileNames.Count)
            {
                return new CurveFile();
            }

            string selectedFile = Path.Combine(directoryPath, fileNames[fileIndex]);
            string json = File.ReadAllText(selectedFile);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<CurveFile>(json);
        }

        public Curve GetCurveByID(string id)
        {
            return Curves.FirstOrDefault(curve => curve.Id == id);
        }

        public string GetPlayerNameByID(string id)
        {
            foreach (Il2CppRUMBLE.Players.Player player in Calls.Managers.GetPlayerManager().AllPlayers)
            {
                if (player.Data.GeneralData.PlayFabMasterId == id)
                {
                    return player.Data.GeneralData.PublicUsername;
                }
            }

            return id;
        }

        private void selectCurve(int index)
        {
            currentCurve = Curves[index];
            updateTexts();
        }

        private void selectCurve(Curve curve)
        {
            currentCurve = curve;
            updateTexts();
        }

        private void updateToolButtons()
        {
            foreach (GameObject buttonObject in UIToolButtons)
            {
                Image component = buttonObject.GetComponent<Image>();
                component.color = new Color(0.196f, 0.357f, 0.216f, 1f);
            }

            switch (selectedTool)
            {
                case Tool.Select:
                    UIToolButtons[0].GetComponent<Image>().color = new Color(0.337f, 0.604f, 0.337f, 1.0f);
                    break;
                case Tool.Pen:
                    UIToolButtons[1].GetComponent<Image>().color = new Color(0.337f, 0.604f, 0.337f, 1.0f);
                    UIToolButtons[2].GetComponent<Image>().color = new Color(0.337f, 0.604f, 0.337f, 1.0f);
                    break;
                case Tool.Anchor:
                    UIToolButtons[3].GetComponent<Image>().color = new Color(0.337f, 0.604f, 0.337f, 1.0f);
                    break;
                case Tool.SelectMarker:
                    UIToolButtons[4].GetComponent<Image>().color = new Color(0.337f, 0.604f, 0.337f, 1.0f);
                    UIToolButtons[12].GetComponent<Image>().color = new Color(0.337f, 0.604f, 0.337f, 1.0f);
                    break;
                case Tool.AddPauseMarker:
                    UIToolButtons[5].GetComponent<Image>().color = new Color(0.337f, 0.604f, 0.337f, 1.0f);
                    break;
                case Tool.AddRollMarker:
                    UIToolButtons[6].GetComponent<Image>().color = new Color(0.337f, 0.604f, 0.337f, 1.0f);
                    break;
                case Tool.DeleteMarker:
                    UIToolButtons[7].GetComponent<Image>().color = new Color(0.337f, 0.604f, 0.337f, 1.0f);
                    break;
                case Tool.AddFOVMarker:
                    UIToolButtons[8].GetComponent<Image>().color = new Color(0.337f, 0.604f, 0.337f, 1.0f);
                    break;
                case Tool.AddSpeedMarker:
                    UIToolButtons[9].GetComponent<Image>().color = new Color(0.337f, 0.604f, 0.337f, 1.0f);
                    break;
                case Tool.MoveStatic:
                    UIToolButtons[10].GetComponent<Image>().color = new Color(0.337f, 0.604f, 0.337f, 1.0f);
                    break;
                case Tool.GrabStatic:
                    UIToolButtons[11].GetComponent<Image>().color = new Color(0.337f, 0.604f, 0.337f, 1.0f);
                    break;
            }
        }

        private void updateTexts()
        {
            foreach (var text in UISelectedIDTexts)
            {
                if (currentCurve != null)
                {
                    text.GetComponent<TextMeshProUGUI>().text = currentCurve.Id;
                }
                else
                {
                    text.GetComponent<TextMeshProUGUI>().text = EmptyField;
                }
            }

            if (currentCurve != null)
            {
                if (currentCurve.RotTarget != null)
                {
                    UISelectedRotTarget.GetComponent<TextMeshProUGUI>().text = currentCurve.RotTarget.Id;
                }
                else
                {
                    UISelectedRotTarget.GetComponent<TextMeshProUGUI>().text = EmptyField;
                }

                UIOrderPosText.GetComponent<TextMeshProUGUI>().text = (Curves.IndexOf(currentCurve) + 1).ToString() + "/" + Curves.Count.ToString();

                UICurveDuration.GetComponent<TextMeshProUGUI>().text = currentCurve.duration.ToString() + " ms";
                UICurveDurationPauses.GetComponent<TextMeshProUGUI>().text = Math.Floor(currentCurve.GetDurationWithPauses()).ToString() + " ms";

                if (currentCurve.IsEnabled)
                    UIIsEnabled.GetComponent<TextMeshProUGUI>().text = "■";
                else
                    UIIsEnabled.GetComponent<TextMeshProUGUI>().text = "";

                if (currentCurve.IsHidden)
                    UIIsHidden.GetComponent<TextMeshProUGUI>().text = "■";
                else
                    UIIsHidden.GetComponent<TextMeshProUGUI>().text = "";

                if (currentCurve.IsLoop)
                    UIIsLoop.GetComponent<TextMeshProUGUI>().text = "■";
                else
                    UIIsLoop.GetComponent<TextMeshProUGUI>().text = "";

                if (currentCurve.Owner != null)
                {
                    UISelectedOwnerText.GetComponent<TextMeshProUGUI>().text = GetPlayerNameByID(currentCurve.Owner);
                }
            }
            else
            {
                UISelectedRotTarget.GetComponent<TextMeshProUGUI>().text = EmptyField;
                UICurveDuration.GetComponent<TextMeshProUGUI>().text = EmptyField;
                UICurveDurationPauses.GetComponent<TextMeshProUGUI>().text = EmptyField;
                UISelectedOwnerText.GetComponent<TextMeshProUGUI>().text = EmptyField;
                UIIsEnabled.GetComponent<TextMeshProUGUI>().text = "";
                UIIsHidden.GetComponent<TextMeshProUGUI>().text = "";
                UIIsLoop.GetComponent<TextMeshProUGUI>().text = "";
            }

            if (isCameraStatic) UIIsStatic.GetComponent<TextMeshProUGUI>().text = "■"; else UIIsStatic.GetComponent<TextMeshProUGUI>().text = "";
            UIStaticFOV.GetComponent<TextMeshProUGUI>().text = staticCameraFOV.ToString() + "°";
            UIStaticRoll.GetComponent<TextMeshProUGUI>().text = staticCameraRoll.ToString() + "°";

            if (curveSendRequest != null)
            {
                UIRequestIDText.GetComponent<TextMeshProUGUI>().text = curveSendRequest.Curves[0].Id;
                UIRequestOwnerText.GetComponent<TextMeshProUGUI>().text = GetPlayerNameByID(curveSendRequest.Owner);
            }
            else
            {
                UIRequestIDText.GetComponent<TextMeshProUGUI>().text = EmptyField;
                UIRequestOwnerText.GetComponent<TextMeshProUGUI>().text = EmptyField;
            }

            if (swapMainHand)
                UISwapHandsText.GetComponent<TextMeshProUGUI>().text = "■";
            else
                UISwapHandsText.GetComponent<TextMeshProUGUI>().text = "";

            if (snapPosition)
                UISnapRotText.GetComponent<TextMeshProUGUI>().text = "■";
            else
                UISnapRotText.GetComponent<TextMeshProUGUI>().text = "";

            if (snapRotation)
                UISnapRotText.GetComponent<TextMeshProUGUI>().text = "■";
            else
                UISnapRotText.GetComponent<TextMeshProUGUI>().text = "";

            if (cameraAudio)
                UICameraAudioText.GetComponent<TextMeshProUGUI>().text = "■";
            else
                UICameraAudioText.GetComponent<TextMeshProUGUI>().text = "";

            if (hideTags)
                UIHideTagsText.GetComponent<TextMeshProUGUI>().text = "■";
            else
                UIHideTagsText.GetComponent<TextMeshProUGUI>().text = "";
        }

        private Curve AddCurve()
        {
            Curve newCurve = new Curve();
            Curves.Add(newCurve);
            return newCurve;
        }

        private void SelectControl(Vector3 handPos)
        {
            float closestDistance = float.MaxValue;

            if (selectedTool == Tool.Pen || selectedTool == Tool.Anchor)
            {
                // Select nearest control point/handle

                ControlPoint closestPoint = null;
                Vector3 closestHandle = Vector3.zero;
                bool isHandle = false;
                int handleIndex = -1;

                foreach (var point in currentCurve.ControlPoints)
                {
                    float distanceToPoint = Vector3.Distance(handPos, point.WorldPos);
                    if (distanceToPoint < closestDistance && distanceToPoint < pointGripRadius)
                    {
                        closestDistance = distanceToPoint;
                        closestPoint = point;
                        isHandle = false;
                    }

                    float distanceToHandle1 = Vector3.Distance(handPos, point.Handle1);
                    if (distanceToHandle1 < closestDistance && distanceToHandle1 < pointGripRadius)
                    {
                        closestDistance = distanceToHandle1;
                        closestPoint = point;
                        closestHandle = point.Handle1;
                        isHandle = true;
                        handleIndex = 0;
                    }

                    float distanceToHandle2 = Vector3.Distance(handPos, point.Handle2);
                    if (distanceToHandle2 < closestDistance && distanceToHandle2 < pointGripRadius)
                    {
                        closestDistance = distanceToHandle2;
                        closestPoint = point;
                        closestHandle = point.Handle2;
                        isHandle = true;
                        handleIndex = 1;
                    }
                }

                if (closestPoint != null)
                {
                    // Highlight the selected control point or handle
                    if (isHandle)
                    {
                        if (handleIndex == 0 && closestPoint.GetHandle1RenderObject() != null)
                        {
                            closestPoint.GetHandle1RenderObject().GetComponent<Renderer>().material.color = Color.yellow;
                        }
                        else if (handleIndex == 1 && closestPoint.GetHandle2RenderObject() != null)
                        {
                            closestPoint.GetHandle2RenderObject().GetComponent<Renderer>().material.color = Color.yellow;
                        }
                    }
                    else if (closestPoint.GetPointRenderObject() != null)
                    {
                        closestPoint.GetPointRenderObject().GetComponent<Renderer>().material.color = Color.yellow;
                    }

                    selectedControl = new SelectedControl(closestPoint, isHandle, handleIndex);
                }
                else
                {
                    selectedControl = null;
                }
            }
            else if (addMarkerTools.Contains(selectedTool) || selectedTool == Tool.SelectMarker || selectedTool == Tool.DeleteMarker)
            {
                // Select nearest marker
                MarkerPoint closestMarker = null;

                foreach (var marker in currentCurve.Markers)
                {
                    if (marker.Type == toolToMarkerType(selectedTool) || selectedTool == Tool.SelectMarker || selectedTool == Tool.DeleteMarker)
                    {
                        float distanceToPoint = Vector3.Distance(handPos, marker.WorldPos);
                        if (distanceToPoint < closestDistance && distanceToPoint < pointGripRadius)
                        {
                            closestDistance = distanceToPoint;
                            closestMarker = marker;
                        }
                    }
                }

                if (closestMarker != null)
                {
                    SelectedMarker = closestMarker;
                    GameObject.Destroy(markerPreview);
                    markerPreview = null;
                }
                else
                {
                    if (!currentGripState)
                    SelectedMarker = null;
                }
            }
        }

        public string GenerateUniqueId()
        {
            string guid = Guid.NewGuid().ToString("N");
            return guid.Substring(0, 8).ToUpper();
        }

        private GameObject? LoadAssetBundle(string bundleName, string objectName)
        {
            using Stream stream = ((MelonBase)this).MelonAssembly.Assembly.GetManifestResourceStream(bundleName);
            byte[] array = new byte[stream.Length];
            stream.Read(array, 0, array.Length);

            // Manually create Il2CppStructArray<byte> from byte[]
            Il2CppStructArray<byte> il2CppArray = new Il2CppStructArray<byte>(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                il2CppArray[i] = array[i];
            }

            Il2CppAssetBundle val = Il2CppAssetBundleManager.LoadFromMemory(il2CppArray);
            return UnityEngine.Object.Instantiate<GameObject>(val.LoadAsset<GameObject>(objectName));
        }
    }

    [System.Serializable]
    public class Curve
    {
        [Newtonsoft.Json.JsonIgnore]
        public List<PointData> Points { get; set; } = new List<PointData>();

        public List<ControlPoint> ControlPoints { get; set; } = new List<ControlPoint>();
        public List<MarkerPoint> Markers { get; set; } = new List<MarkerPoint>();

        public string Id { get; private set; }
        public Curve RotTarget { get; set; }

        public string RotTargetID;

        public string Owner;

        public bool IsHidden = false;

        public bool IsEnabled = true;

        public bool IsLoop = false;

        public Curve FocusTarget { get; set; }

        public string FocusTargetID;
        public float duration { get; set; } = 5000;

        [Newtonsoft.Json.JsonIgnore]
        public GameObject? curveObject; // The game object that represents the curve
        [Newtonsoft.Json.JsonIgnore]
        private GameObject? lineObject; // Child of the curve; the actual line

        public override bool Equals(object obj)
        {
            if (obj is Curve other)
            {
                return Id == other.Id &&
                       RotTargetID == other.RotTargetID &&
                       FocusTargetID == other.FocusTargetID &&
                       Owner == other.Owner &&
                       IsHidden == other.IsHidden &&
                       IsEnabled == other.IsEnabled &&
                       IsLoop == other.IsLoop &&
                       duration == other.duration &&
                       ControlPoints.SequenceEqual(other.ControlPoints) &&
                       Markers.SequenceEqual(other.Markers);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + (Id?.GetHashCode() ?? 0);
            hash = hash * 31 + (RotTargetID?.GetHashCode() ?? 0);
            hash = hash * 31 + (FocusTargetID?.GetHashCode() ?? 0);
            hash = hash * 31 + (Owner?.GetHashCode() ?? 0);
            hash = hash * 31 + IsHidden.GetHashCode();
            hash = hash * 31 + IsEnabled.GetHashCode();
            hash = hash * 31 + IsLoop.GetHashCode();
            hash = hash * 31 + duration.GetHashCode();
            hash = hash * 31 + ControlPoints.Aggregate(0, (acc, point) => acc + point.GetHashCode());
            hash = hash * 31 + Markers.Aggregate(0, (acc, marker) => acc + marker.GetHashCode());
            return hash;
        }

        public Curve()
        {
            curveObject = new GameObject("Curve");
            curveObject.transform.parent = ScriptedLCMain.Instance.CurvesParent.transform;

            Id = ScriptedLCMain.Instance.GenerateUniqueId();
            Owner = Calls.Managers.GetPlayerManager().LocalPlayer.Data.GeneralData.PlayFabMasterId;
        }

        [Newtonsoft.Json.JsonConstructor]
        public Curve(string id, string rotTargetID, string focusTargetID, string owner)
        {
            curveObject = new GameObject("Curve");
            curveObject.transform.parent = ScriptedLCMain.Instance.CurvesParent.transform;

            Id = id;
            RotTargetID = rotTargetID;
            FocusTargetID = focusTargetID;
            Owner = owner;
        }

        public ControlPoint AddPoint(Vector3 position)
        {
            ControlPoint newPoint = new ControlPoint(this, position);
            ControlPoints.Add(newPoint);
            
            foreach (MarkerPoint marker in Markers)
            {
                marker.SetPosition(marker.CurvePos);
            }

            return newPoint;
        }

        public ControlPoint AddPoint(Vector3 position, Vector3 handle1)
        {
            ControlPoint newPoint = new ControlPoint(this, position, handle1);
            ControlPoints.Add(newPoint);

            foreach (MarkerPoint marker in Markers)
            {
                marker.SetPosition(marker.CurvePos);
            }

            return newPoint;
        }

        public ControlPoint AddPoint(Vector3 position, Vector3 handle1, Vector3 handle2)
        {
            ControlPoint newPoint = new ControlPoint(this, position, handle1, handle2);
            ControlPoints.Add(newPoint);

            foreach (MarkerPoint marker in Markers)
            {
                marker.SetPosition(marker.CurvePos);
            }

            return newPoint;
        }

        public MarkerPoint AddMarker(float curvePosition, MarkerPoint.MarkerType markerType)
        {
            MarkerPoint newMarker = new MarkerPoint(this, curvePosition, markerType);
            Markers.Add(newMarker);
            return newMarker;
        }

        public MarkerPoint AddMarker(Vector3 worldPos, MarkerPoint.MarkerType markerType)
        {
            MarkerPoint newMarker = new MarkerPoint(this, worldPos, markerType);
            Markers.Add(newMarker);
            return newMarker;
        }

        public void OffsetCurve(Vector3 offset)
        {
            foreach (ControlPoint point in ControlPoints)
            {
                point.SetPosition(ScriptedLCMain.Instance.Snap(point.WorldPos + offset));
            }

            foreach (MarkerPoint marker in Markers)
            {
                marker.SetPosition(ScriptedLCMain.Instance.Snap(marker.WorldPos + offset));
            }

            for (int i = 0; i < Points.Count; i++)
            {
                Points[i].WorldPos += offset;
            }

            Render(true);
        }

        public void ScaleCurve(Vector3 scale)
        {
            curveObject.transform.localScale = scale;
            List<Vector3> newControlPositions = new List<Vector3>();

            foreach (ControlPoint point in ControlPoints)
            {
                newControlPositions.Add(point.GetPointRenderObject().transform.position);

                if (point.Type == ControlPoint.ControlPointType.Symmetric)
                {
                    newControlPositions.Add(point.GetHandle1RenderObject().transform.position);
                }

                if (point.Type == ControlPoint.ControlPointType.Free)
                {
                    newControlPositions.Add(point.GetHandle1RenderObject().transform.position);
                    newControlPositions.Add(point.GetHandle1RenderObject().transform.position);
                }
            }

            curveObject.transform.localScale = Vector3.one;

            for (int i = 0; i < newControlPositions.Count; i++)
            {
                ControlPoint point = ControlPoints[i];
                point.SetPosition(newControlPositions[i]);
                
                if (point.Type == ControlPoint.ControlPointType.Symmetric)
                {
                    point.Handle1 = newControlPositions[i + 1];
                    i += 1;
                }

                if (point.Type == ControlPoint.ControlPointType.Free)
                {
                    point.Handle1 = newControlPositions[i + 1];
                    point.Handle2 = newControlPositions[i + 2];
                    i += 2;
                }
            }

            foreach (MarkerPoint marker in Markers)
            {
                marker.SetPosition(marker.CurvePos);
            }
        }

        public void RotateCurve(Vector3 rotation)
        {
            curveObject.transform.localRotation = Quaternion.Euler(rotation);
            List<Vector3> newControlPositions = new List<Vector3>();

            foreach (ControlPoint point in ControlPoints)
            {
                newControlPositions.Add(point.GetPointRenderObject().transform.position);

                if (point.Type == ControlPoint.ControlPointType.Symmetric)
                {
                    newControlPositions.Add(point.GetHandle1RenderObject().transform.position);
                }

                if (point.Type == ControlPoint.ControlPointType.Free)
                {
                    newControlPositions.Add(point.GetHandle1RenderObject().transform.position);
                    newControlPositions.Add(point.GetHandle1RenderObject().transform.position);
                }
            }

            curveObject.transform.localRotation = Quaternion.identity;

            for (int i = 0; i < newControlPositions.Count; i++)
            {
                ControlPoint point = ControlPoints[i];
                point.SetPosition(newControlPositions[i]);

                if (point.Type == ControlPoint.ControlPointType.Symmetric)
                {
                    point.Handle1 = newControlPositions[i + 1];
                    i += 1;
                }

                if (point.Type == ControlPoint.ControlPointType.Free)
                {
                    point.Handle1 = newControlPositions[i + 1];
                    point.Handle2 = newControlPositions[i + 2];
                    i += 2;
                }
            }

            foreach (MarkerPoint marker in Markers)
            {
                marker.SetPosition(marker.CurvePos);
            }
        }

        public void UpdatePoints()
        {
            Points = GetPointsAlongCurve(20, out _);
        }

        public float GetDurationWithPauses()
        {
            float durationWithPauses = 0f;
            foreach (MarkerPoint marker in Markers)
            {
                if (marker.Type == MarkerType.Pause && marker.Value > 0f)
                {
                    durationWithPauses += marker.Value;
                }
            }
            return durationWithPauses + duration;
        }

        public void SetID(string newID)
        {
            Id = newID;
            foreach (Curve curve in ScriptedLCMain.Instance.Curves)
            {
                if (curve.RotTarget != null && curve.RotTargetID == Id)
                {
                    curve.RotTargetID = newID;
                    curve.RotTarget.SetID(newID);
                }
            }
        }

        public void RemovePoint(ControlPoint point)
        {
            point.Unrender();
            ControlPoints.Remove(point);

            if (ControlPoints.Count < 2)
            {
                GameObject.Destroy(lineObject);
            }
        }

        public void RemoveMarker(MarkerPoint marker)
        {
            marker.Unrender();
            Markers.Remove(marker);
        }

        public void Render(bool selected)
        {
            // Render points
            foreach (ControlPoint point in ControlPoints)
            {
                point.Render(curveObject.transform, selected);
            }

            // Render markers
            foreach (MarkerPoint marker in Markers)
            {
                if (selected)
                    marker.Render();
                else
                    marker.Unrender();
            }

            // Render curve
            if (lineObject == null && ControlPoints.Count > 1)
            {
                lineObject = new GameObject("Line");
                MeshRenderer mr = lineObject.AddComponent<MeshRenderer>();
                mr.material = ScriptedLCMain.Instance.curveMaterial;
                lineObject.transform.parent = this.curveObject.transform;
                lineObject.layer = ScriptedLCMain.Instance.dontShowOnLIVLayer;

                if (selected)
                {
                    mr.material.color = Color.blue;
                }
                else
                {
                    mr.material.color = new Color(0f, 0f, 0.3f);
                }
            }

            if (ControlPoints.Count > 1)
            {
                int segmentCount = 12;
                List<Vector3> curvePoints = new List<Vector3>();

                for (int i = 0; i < ControlPoints.Count - 1; i++)
                {
                    ControlPoint point1 = ControlPoints[i];
                    ControlPoint point2 = ControlPoints[i + 1];

                    Vector3 p0 = point1.WorldPos;
                    Vector3 p1 = point1.Type != ControlPoint.ControlPointType.Corner ? point1.Handle2 : point1.WorldPos;
                    Vector3 p2 = point2.Type != ControlPoint.ControlPointType.Corner ? point2.Handle1 : point2.WorldPos;
                    Vector3 p3 = point2.WorldPos;

                    for (int j = 0; j <= segmentCount; j++)
                    {
                        float t = j / (float)segmentCount;
                        Vector3 pointOnCurve = GetPointOnCubicBezierCurve(p0, p1, p2, p3, t);
                        curvePoints.Add(pointOnCurve);
                    }
                }

                renderLine(lineObject, curvePoints);
            }
        }

        public float GetLengthFromPoints(List<PointData> curvePoints)
        {
            float totalLength = 0f;
            for (int i = 0; i < curvePoints.Count - 1; i++)
            {
                totalLength += Vector3.Distance(curvePoints[i].WorldPos, curvePoints[i + 1].WorldPos);
            }

            return totalLength;
        }

        public float WorldPosToCurvePos(Vector3 worldPos)
        {
            float closestDistance = float.MaxValue;
            float closestCurvePos = 0f;

            foreach (PointData point in Points)
            {
                if (Vector3.Distance(worldPos, point.WorldPos) < closestDistance)
                {
                    closestDistance = Vector3.Distance(worldPos, point.WorldPos);
                    closestCurvePos = point.CurvePos;
                }
            }

            return closestCurvePos;
        }

        public Vector3 CurvePosToWorldPos(float t, int segmentCount, out Vector3 tangent)
        {
            List<PointData> curvePoints = new List<PointData>();
            List<Vector3> tangentPoints = new List<Vector3>();

            if (Points.Count == 0)
            {
                // If there are no points, return zero vector
                tangent = Vector3.zero;
                return Vector3.zero;
            }

            if (Points.Count == 1)
            {
                // If there's only one point, return its position
                tangent = Vector3.zero;
                return Points[0].WorldPos;
            }

            curvePoints = GetPointsAlongCurve(segmentCount, out tangentPoints);
            float totalLength = GetLengthFromPoints(curvePoints);
            float targetLength = totalLength * t;
            float currentLength = 0f;

            for (int i = 0; i < curvePoints.Count; i++)
            {
                float segmentLength = Vector3.Distance(curvePoints[i].WorldPos, curvePoints[i + 1].WorldPos);
                if (currentLength + segmentLength >= targetLength)
                {
                    float segmentT = (targetLength - currentLength) / segmentLength;
                    tangent = Vector3.Lerp(tangentPoints[i], tangentPoints[i + 1], segmentT);
                    return Vector3.Lerp(curvePoints[i].WorldPos, curvePoints[i + 1].WorldPos, segmentT);
                }
                currentLength += segmentLength;
            }

            tangent = tangentPoints[tangentPoints.Count - 1];
            return curvePoints[curvePoints.Count - 1].WorldPos;
        }

        public List<PointData> GetPointsAlongCurve(int segmentCount, out List<Vector3> tangentPoints)
        {
            List<PointData> curvePoints = new List<PointData>();
            tangentPoints = new List<Vector3>();

            if (ControlPoints.Count == 0)
            {
                // If there are no control points, return empty lists
                return curvePoints;
            }

            if (ControlPoints.Count == 1)
            {
                // If there's only one control point, return its position
                curvePoints.Add(new PointData(ControlPoints[0].WorldPos, 0f));
                return curvePoints;
            }

            for (int i = 0; i < ControlPoints.Count - 1; i++)
            {
                ControlPoint point1 = ControlPoints[i];
                ControlPoint point2 = ControlPoints[i + 1];

                Vector3 p0 = point1.WorldPos;
                Vector3 p1 = point1.Type != ControlPoint.ControlPointType.Corner ? point1.Handle2 : point1.WorldPos;
                Vector3 p2 = point2.Type != ControlPoint.ControlPointType.Corner ? point2.Handle1 : point2.WorldPos;
                Vector3 p3 = point2.WorldPos;

                for (int j = 0; j <= segmentCount; j++)
                {
                    float segmentT = j / (float)segmentCount;
                    Vector3 pointOnCurve = GetPointOnCubicBezierCurve(p0, p1, p2, p3, segmentT);
                    Vector3 tangentOnCurve = GetTangentOnCubicBezierCurve(p0, p1, p2, p3, segmentT);
                    float curvePos = (i + segmentT) / (ControlPoints.Count - 1);
                    curvePoints.Add(new PointData(pointOnCurve, curvePos));
                    tangentPoints.Add(tangentOnCurve);
                }
            }

            curvePoints.Add(new PointData(ControlPoints[ControlPoints.Count - 1].WorldPos, 1.0f));
            return curvePoints;
        }

        public float GetPositionWithPauses(float input)
        {
            List<MarkerPoint> pauseMarkers = Markers.Where(marker => marker.Type == MarkerType.Pause).ToList();
            List<MarkerPoint> passedMarkers = new List<MarkerPoint>();

            float totalPauseDuration = 0f;
            foreach (var marker in pauseMarkers)
            {
                if (marker.CurvePos < input)
                {
                    totalPauseDuration += marker.Value;
                    passedMarkers.Add(marker);
                }
            }

            float adjustedInput = input;
            foreach (MarkerPoint marker in passedMarkers)
            {
                if (adjustedInput > marker.CurvePos)
                {
                    float markerDuration = marker.Value / duration;
                    if (adjustedInput < marker.CurvePos + markerDuration)
                    {
                        adjustedInput = marker.CurvePos;
                    }
                    else
                    {
                        adjustedInput -= markerDuration;
                    }
                }
            }

            return adjustedInput;
        }

        public float GetPositionWithSpeeds(float oldCurvePos)
        {
            List<MarkerPoint> markers = Markers.Where(marker => marker.Type == MarkerType.Speed).OrderBy(marker => marker.Value).ToList();

            // If there are no markers, return the original position
            if (markers.Count == 0) return oldCurvePos;

            // Handle cases where the position is before the first marker
            if (oldCurvePos <= markers[0].Value)
            {
                return oldCurvePos * (markers[0].CurvePos / markers[0].Value);
            }

            if (oldCurvePos >= markers[^1].Value)
            {
                return ((oldCurvePos - markers[^1].Value) * ((1 - markers[^1].CurvePos) / (1 - markers[^1].Value))) + markers[^1].CurvePos;
            }

            // Find the segment the position falls into
            for (int i = 0; i < markers.Count - 1; i++)
            {
                if (oldCurvePos >= markers[i].Value && oldCurvePos <= markers[i + 1].Value)
                {
                    float segmentStart = markers[i].Value;
                    float segmentEnd = markers[i + 1].Value;
                    float curveStart = markers[i].CurvePos;
                    float curveEnd = markers[i + 1].CurvePos;

                    return ((oldCurvePos - segmentStart) * ((curveEnd - curveStart) / (segmentEnd - segmentStart))) + curveStart;
                }
            }

            return oldCurvePos;
        }


        public float GetAdjustedPosition(float input)
        {
            return GetPositionWithPauses(GetPositionWithSpeeds(input));
        }

        public MarkerPoint GetNextMarker(float t, MarkerPoint.MarkerType type)
        {
            float closestDistance = float.MaxValue;
            MarkerPoint nextMarker = null;

            foreach (MarkerPoint marker in Markers)
            {
                float distance = marker.CurvePos - t;
                if (distance >= 0 && distance < closestDistance && marker.Type == type)
                {
                    closestDistance = distance;
                    nextMarker = marker;
                }
            }

            return nextMarker;
        }

        public MarkerPoint GetPrevMarker(float t, MarkerPoint.MarkerType type)
        {
            float closestDistance = float.MinValue;
            MarkerPoint prevMarker = null;

            foreach (MarkerPoint marker in Markers)
            {
                float distance = marker.CurvePos - t;
                if (distance <= 0 && distance > closestDistance && marker.Type == type)
                {
                    closestDistance = distance;
                    prevMarker = marker;
                }
            }

            return prevMarker;
        }

        Vector3 GetPointOnCubicBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * p0; // (1-t)^3 * p0
            p += 3 * uu * t * p1; // 3 * (1-t)^2 * t * p1
            p += 3 * u * tt * p2; // 3 * (1-t) * t^2 * p2
            p += ttt * p3; // t^3 * p3

            return p;
        }

        Vector3 GetTangentOnCubicBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;

            Vector3 tangent = 3 * uu * (p1 - p0);
            tangent += 6 * u * t * (p2 - p1);
            tangent += 3 * tt * (p3 - p2);

            return tangent.normalized;
        }
        
        public void Unrender()
        {
            // Unrender points
            foreach (ControlPoint point in ControlPoints)
            {
                point.Unrender();
            }

            foreach (MarkerPoint marker in Markers)
            {
                marker.Unrender();
            }

            // Unrender line
            GameObject.Destroy(lineObject);
        }

        private void renderLine(GameObject gameObject, List<Vector3> points)
        {
            if (points == null || points.Count < 2)
            {
                return;
            }

            if (gameObject.GetComponent<MeshFilter>() == null)
            {
                gameObject.AddComponent<MeshFilter>();
            }

            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            Mesh mesh = new Mesh();
            meshFilter.mesh = mesh;

            Vector3[] vertices = points.ToArray();
            int[] indices = new int[(points.Count - 1) * 2];

            for (int i = 0; i < points.Count - 1; i++)
            {
                indices[i * 2] = i;
                indices[i * 2 + 1] = i + 1;
            }

            mesh.vertices = vertices;
            mesh.SetIndices(indices, MeshTopology.Lines, 0);
            mesh.RecalculateBounds();
        }
    }

    [System.Serializable]
    public class ControlPoint
    {
        public enum ControlPointType
        {
            Corner,
            Symmetric,
            Free
        }

        public Curve Curve { get; set; }
        public string Id { get; private set; }
        public ControlPointType Type { get; private set; }

        public Vector3 WorldPos { get; set; }
        public float CurvePos { get; set; }
        private Vector3 handle1;
        public Vector3 Handle1
        {
            get => handle1;
            set
            {
                handle1 = value;

                if (Type == ControlPointType.Symmetric)
                {
                    handle2 = GetHandleMirrorPos(value);
                }
            }
        }
        private Vector3 handle2;
        public Vector3 Handle2
        {
            get => handle2;
            set
            {
                handle2 = value;

                if (Type == ControlPointType.Symmetric)
                {
                    handle1 = GetHandleMirrorPos(value);
                }
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        private GameObject? pointRenderObject;
        [Newtonsoft.Json.JsonIgnore]
        private GameObject? handle1RenderObject;
        [Newtonsoft.Json.JsonIgnore]
        private GameObject? handle2RenderObject;
        [Newtonsoft.Json.JsonIgnore]
        private GameObject? handle1ConnectorRenderObject;
        [Newtonsoft.Json.JsonIgnore]
        private GameObject? handle2ConnectorRenderObject;

        public override bool Equals(object obj)
        {
            if (obj is ControlPoint other)
            {
                return Id == other.Id &&
                       Type == other.Type &&
                       WorldPos == other.WorldPos &&
                       CurvePos == other.CurvePos &&
                       Handle1 == other.Handle1 &&
                       Handle2 == other.Handle2;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + (Id?.GetHashCode() ?? 0);
            hash = hash * 31 + Type.GetHashCode();
            hash = hash * 31 + WorldPos.GetHashCode();
            hash = hash * 31 + CurvePos.GetHashCode();
            hash = hash * 31 + Handle1.GetHashCode();
            hash = hash * 31 + Handle2.GetHashCode();
            return hash;
        }

        public ControlPoint(Curve curve, Vector3 position)
        {
            Id = ScriptedLCMain.Instance.GenerateUniqueId();
            Type = ControlPointType.Corner;
            Curve = curve;

            WorldPos = position;
            Handle1 = position;
            Handle2 = position;
        }

        public ControlPoint(Curve curve, Vector3 position, Vector3 handle1)
        {
            Id = ScriptedLCMain.Instance.GenerateUniqueId();
            Type = ControlPointType.Symmetric;
            Curve = curve;

            WorldPos = position;
            Handle1 = handle1;
            MirrorHandles();
        }

        [Newtonsoft.Json.JsonConstructor]
        public ControlPoint(Curve curve, Vector3 position, Vector3 handle1, Vector3 handle2, ControlPointType type)
        {
            Id = ScriptedLCMain.Instance.GenerateUniqueId();
            Type = type;
            Curve = curve;

            WorldPos = position;
            this.handle1 = handle1;
            this.handle2 = handle2;
        }

        public ControlPoint(Curve curve, Vector3 position, Vector3 handle1, Vector3 handle2)
        {
            Id = ScriptedLCMain.Instance.GenerateUniqueId();
            Type = ControlPointType.Free;
            Curve = curve;

            WorldPos = position;
            Handle1 = handle1;
            Handle2 = handle2;
        }

        public void SetPosition(Vector3 newPosition)
        {
            Vector3 delta = newPosition - WorldPos;
            WorldPos = newPosition;

            Handle1 += delta;
            if (Type == ControlPointType.Free)
            {
                Handle2 += delta;
            }
        }

        public void SetPosition(float newPosition)
        {
            SetPosition(Curve.CurvePosToWorldPos(newPosition, 20, out _));
        }

        public void SetType(ControlPointType newType)
        {
            Type = newType;

            if (newType == ControlPointType.Symmetric)
            {
                MirrorHandles();
            }

            if (newType == ControlPointType.Corner)
            {
                Unrender();
                Handle1 = WorldPos;
                Handle2 = WorldPos;
            }
        }

        private Vector3 GetHandleMirrorPos(Vector3 handle)
        {
            return WorldPos + (WorldPos - handle);
        }

        public void MirrorHandles()
        {
            Handle2 = GetHandleMirrorPos(Handle1);
        }

        public void Render(Transform parent, bool selected)
        {
            if (pointRenderObject == null)
            {
                pointRenderObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pointRenderObject.GetComponent<BoxCollider>().enabled = false;
                pointRenderObject.transform.localScale = (Vector3.one * 0.03f * -1f);
                pointRenderObject.transform.parent = parent;
                pointRenderObject.layer = ScriptedLCMain.Instance.dontShowOnLIVLayer;
            }

            if (pointRenderObject != null)
            {
                Renderer mr = pointRenderObject.GetComponent<Renderer>();
                mr.material = ScriptedLCMain.Instance.curveMaterial;

                if (selected)
                {
                    mr.material.color = Color.red;
                    pointRenderObject.transform.localScale = Vector3.one * 0.03f;
                }
                else
                {
                    mr.material.color = new Color(0.4f, 0f, 0f);
                    pointRenderObject.transform.localScale = Vector3.one * 0.03f * 0.5f;
                }
            }

            if (Type != ControlPointType.Corner)
            {
                if (handle1RenderObject == null)
                {
                    handle1RenderObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    handle1RenderObject.GetComponent<SphereCollider>().enabled = false;
                    handle1RenderObject.transform.localScale = (Vector3.one * 0.03f * -1f);
                    handle1RenderObject.transform.parent = parent;
                    handle1RenderObject.layer = ScriptedLCMain.Instance.dontShowOnLIVLayer;
                }

                if (handle2RenderObject == null)
                {
                    handle2RenderObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    handle2RenderObject.GetComponent<SphereCollider>().enabled = false;
                    handle2RenderObject.transform.localScale = (Vector3.one * 0.03f * -1f);
                    handle2RenderObject.transform.parent = parent;
                    handle2RenderObject.layer = ScriptedLCMain.Instance.dontShowOnLIVLayer;
                }

                if (handle1ConnectorRenderObject == null)
                {
                    handle1ConnectorRenderObject = new GameObject("Handle1Connector");
                    handle1ConnectorRenderObject.AddComponent<MeshRenderer>();
                    handle1ConnectorRenderObject.transform.parent = parent;
                    handle1ConnectorRenderObject.layer = ScriptedLCMain.Instance.dontShowOnLIVLayer;
                }

                if (handle2ConnectorRenderObject == null)
                {
                    handle2ConnectorRenderObject = new GameObject("Handle2Connector");
                    handle2ConnectorRenderObject.AddComponent<MeshRenderer>();
                    handle2ConnectorRenderObject.transform.parent = parent;
                    handle2ConnectorRenderObject.layer = ScriptedLCMain.Instance.dontShowOnLIVLayer;
                }

                handle1RenderObject.transform.position = Handle1;
                handle2RenderObject.transform.position = Handle2;

                ScriptedLCMain.Instance.renderLine(handle1ConnectorRenderObject, new List<Vector3> { WorldPos, Handle1 });
                ScriptedLCMain.Instance.renderLine(handle2ConnectorRenderObject, new List<Vector3> { WorldPos, Handle2 });
            }

            if (handle1RenderObject != null)
            {
                Renderer mr = handle1RenderObject.GetComponent<Renderer>();
                mr.material = ScriptedLCMain.Instance.curveMaterial;
                mr.material.color = Color.red;
                handle1RenderObject.SetActive(selected);
            }

            if (handle2RenderObject != null)
            {
                Renderer mr = handle2RenderObject.GetComponent<Renderer>();
                mr.material = ScriptedLCMain.Instance.curveMaterial;
                mr.material.color = Color.red;
                handle2RenderObject.SetActive(selected);
            }

            if (handle1ConnectorRenderObject != null)
            {
                MeshRenderer mr = handle1ConnectorRenderObject.GetComponent<MeshRenderer>();
                mr.material = ScriptedLCMain.Instance.curveMaterial;
                mr.material.color = Color.red;
                handle1ConnectorRenderObject.SetActive(selected);
            }

            if (handle2ConnectorRenderObject != null)
            {
                MeshRenderer mr = handle2ConnectorRenderObject.GetComponent<MeshRenderer>();
                mr.material = ScriptedLCMain.Instance.curveMaterial;
                mr.material.color = Color.red;
                handle2ConnectorRenderObject.SetActive(selected);
            }

            pointRenderObject.transform.position = WorldPos;
        }

        public GameObject? GetPointRenderObject()
        {
            return pointRenderObject;
        }

        public GameObject? GetHandle1RenderObject()
        {
            return handle1RenderObject;
        }

        public GameObject? GetHandle2RenderObject()
        {
            return handle2RenderObject;
        }

        public GameObject? GetHandle1ConnectorRenderObject()
        {
            return handle1ConnectorRenderObject;
        }

        public GameObject? GetHandle2ConnectorRenderObject()
        {
            return handle2ConnectorRenderObject;
        }

        public void Unrender()
        {
            if (pointRenderObject != null)
            {
                GameObject.Destroy(pointRenderObject);
            }
            if (handle1RenderObject != null)
            {
                GameObject.Destroy(handle1RenderObject);
            }
            if (handle2RenderObject != null)
            {
                GameObject.Destroy(handle2RenderObject);
            }
            if (handle1ConnectorRenderObject != null)
            {
                GameObject.Destroy(handle1ConnectorRenderObject);
            }
            if (handle2ConnectorRenderObject != null)
            {
                GameObject.Destroy(handle2ConnectorRenderObject);
            }
        }
    }

    [System.Serializable]
    public class MarkerPoint
    {
        public float Value {  get; set; }
        public string Id { get; private set; }
        public Curve Curve { get; set; }
        public string CurveID { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public Vector3 WorldPos { get; set; }
        public float CurvePos { get; set; }

        public MarkerType Type { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public GameObject? RenderObject { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public GameObject? SymbolRenderObject;
        [Newtonsoft.Json.JsonIgnore]
        public GameObject? DotRenderObject;
        [Newtonsoft.Json.JsonIgnore]
        public GameObject? TextRenderObject;

        public enum MarkerType
        {
            Pause,
            Roll,
            FOV,
            Speed
        }

        public override bool Equals(object obj)
        {
            if (obj is MarkerPoint other)
            {
                return Id == other.Id &&
                       CurveID == other.CurveID &&
                       WorldPos == other.WorldPos &&
                       CurvePos == other.CurvePos &&
                       Type == other.Type &&
                       Value == other.Value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + (Id?.GetHashCode() ?? 0);
            hash = hash * 31 + (CurveID?.GetHashCode() ?? 0);
            hash = hash * 31 + WorldPos.GetHashCode();
            hash = hash * 31 + CurvePos.GetHashCode();
            hash = hash * 31 + Type.GetHashCode();
            hash = hash * 31 + Value.GetHashCode();
            return hash;
        }

        [Newtonsoft.Json.JsonConstructor]
        public MarkerPoint(string curveID, float curvePos, MarkerType type, float value)
        {
            Value = value;
            Id = ScriptedLCMain.Instance.GenerateUniqueId();
            CurveID = curveID;

            CurvePos = curvePos;
            Type = type;
        }

        public MarkerPoint(Curve curve, Vector3 worldPos, MarkerType type)
        {
            Value = ScriptedLCMain.Instance.GetDefaultMarkerValue(type);
            Id = ScriptedLCMain.Instance.GenerateUniqueId();
            Curve = curve;
            CurveID = curve.Id;
            WorldPos = worldPos;
            CurvePos = curve.WorldPosToCurvePos(worldPos);
            Type = type;
        }

        public MarkerPoint(Curve curve, float curvePos, MarkerType type)
        {
            Value = ScriptedLCMain.Instance.GetDefaultMarkerValue(type);
            Id = ScriptedLCMain.Instance.GenerateUniqueId();
            Curve = curve;
            CurveID = curve.Id;
            CurvePos = curvePos;
            WorldPos = curve.CurvePosToWorldPos(curvePos, 20, out _);
            Type = type;
        }

        public void setValue(float value)
        {
            switch (Type)
            {
                case MarkerType.Pause:
                    Value = (int)value;
                    break;
                case MarkerType.Roll:
                    Value = (int)value;
                    break;
                case MarkerType.FOV:
                    Value = (int)value;
                    break;
                case MarkerType.Speed:
                    Value = value;
                    break;
            }
        }

        public void Render()
        {
            if (RenderObject == null)
            {
                RenderObject = new GameObject("Marker");

                DotRenderObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                DotRenderObject.GetComponent<SphereCollider>().enabled = false;
                DotRenderObject.transform.localScale = (Vector3.one * 0.035f * -1f);
                DotRenderObject.transform.parent = RenderObject.transform;
                DotRenderObject.layer = ScriptedLCMain.Instance.dontShowOnLIVLayer;
                MeshRenderer mr = DotRenderObject.GetComponent<MeshRenderer>();
                mr.material = ScriptedLCMain.Instance.curveMaterial;

                TextRenderObject = new GameObject("Text");
                TextMeshPro tmp = TextRenderObject.AddComponent<TextMeshPro>();
                tmp.alignment = TextAlignmentOptions.Center;
                TextRenderObject.transform.localScale = (Vector3.one * 0.01f);
                TextRenderObject.transform.parent = RenderObject.transform;
                TextRenderObject.transform.localPosition = (Vector3.up * 0.1f);

                switch (Type)
                {
                    case MarkerType.Pause:
                        SymbolRenderObject = GameObject.Instantiate(ScriptedLCMain.Instance.pauseMarkerBlueprint);
                        break;
                    case MarkerType.Roll:
                        SymbolRenderObject = GameObject.Instantiate(ScriptedLCMain.Instance.rollMarkerBlueprint);
                        break;
                    case MarkerType.FOV:
                        SymbolRenderObject = GameObject.Instantiate(ScriptedLCMain.Instance.fovMarkerBlueprint);
                        break;
                    case MarkerType.Speed:
                        SymbolRenderObject = GameObject.Instantiate(ScriptedLCMain.Instance.speedMarkerBlueprint);
                        break;
                }

                SymbolRenderObject.transform.localScale = (Vector3.one * 0.05f);
                SymbolRenderObject.transform.parent = RenderObject.transform;
                SymbolRenderObject.transform.localPosition = (Vector3.up * 0.05f);
                SymbolRenderObject.layer = ScriptedLCMain.Instance.dontShowOnLIVLayer;
            }

            DotRenderObject.GetComponent<MeshRenderer>().material.color = Color.white;

            if (this == ScriptedLCMain.Instance.SelectedMarker)
            {
                DotRenderObject.GetComponent<MeshRenderer>().material.color = Color.yellow;
            }

            if (this == ScriptedLCMain.Instance.ActiveRawMarker)
            {
                DotRenderObject.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.5f, 0f);
            }

            RenderObject.transform.position = WorldPos;
            SymbolRenderObject.transform.localRotation = Quaternion.LookRotation(SymbolRenderObject.transform.position - Camera.main.transform.position);
            TextRenderObject.transform.localRotation = Quaternion.LookRotation(TextRenderObject.transform.position - Camera.main.transform.position);
            TextMeshPro textRenderObjectTMP = TextRenderObject.GetComponent<TextMeshPro>();

            switch (Type)
            {
                case MarkerType.Pause:
                    if (Value >= 0)
                        textRenderObjectTMP.text = (Value / 1000) + " S";
                    else
                        textRenderObjectTMP.text = "∞";
                    break;
                case MarkerType.Roll:
                    textRenderObjectTMP.text = Value + "°";
                    break;
                case MarkerType.FOV:
                    textRenderObjectTMP.text = Value + "°";
                    break;
                case MarkerType.Speed:
                    textRenderObjectTMP.text = Math.Round(Value * 100) + "%";
                    break;
            }

            if (this == ScriptedLCMain.Instance.ActiveRawMarker || this == ScriptedLCMain.Instance.SelectedMarker)
            {
                TextRenderObject.SetActive(true);
            }
            else
            {
                TextRenderObject.SetActive(false);
            }
        }

        public void Unrender()
        {
            if (RenderObject != null)
            {
                GameObject.Destroy(RenderObject);
            }
        }

        public void SetPosition(Vector3 pos)
        {
            WorldPos = pos;
            CurvePos = Curve.WorldPosToCurvePos(WorldPos);
        }

        public void SetPosition(float pos)
        {
            CurvePos = pos;
            WorldPos = Curve.CurvePosToWorldPos(CurvePos, 20, out _);
        }
    }

    public class SelectedControl
    {
        public ControlPoint ControlPoint { get; set; }
        public bool IsHandle { get; set; }
        public int HandleIndex { get; set; } // 0 for Handle1, 1 for Handle2

        public SelectedControl(ControlPoint controlPoint, bool isHandle, int handleIndex)
        {
            ControlPoint = controlPoint;
            IsHandle = isHandle;
            HandleIndex = handleIndex;
        }
    }

    public class SlideValue
    {
        public float Value { get; set; }
        public float Min { get; set; }
        public float Max { get; set; }
        public float Step { get; set; }
        public float InitialHandHeight { get; set; }
        public SlideValue(float value, float min, float max, float step, float initialHandHeight)
        {
            Value = value;
            Min = min;
            Max = max;
            Step = step;
            InitialHandHeight = initialHandHeight;
        }

        public float Slide(Vector3 mainHandPos)
        {
            float offset = mainHandPos.y - InitialHandHeight;

            if (ScriptedLCMain.Instance.currentMainPrimaryState && !ScriptedLCMain.Instance.currentMainSecondaryState) offset *= 2f;
            if (!ScriptedLCMain.Instance.currentMainPrimaryState && ScriptedLCMain.Instance.currentMainSecondaryState) offset *= 0.5f;

            float newValue = Value + offset * Step;
            if (newValue < Min)
            {
                newValue = Min;
            }
            else if (newValue > Max)
            {
                newValue = Max;
            }

            return newValue;
        }
    }

    public class CurveFile
    {
        public string FileName { get; set; }
        public bool IsStatic { get; set; } = false;
        public float StaticRoll { get; set; } = 0f;
        public float StaticFOV { get; set; } = 60f;
        public Vector3 StaticCameraPos { get; set; } = Vector3.zero;
        public Quaternion StaticCameraRot { get; set; } = Quaternion.identity;
        public List<Curve> Curves { get; set; }
        public string Owner { get; set; }
        public string SelectedCurveID { get; set; }

        public CurveFile()
        {
            Curves = new List<Curve>();
        }

        public override bool Equals(object obj)
        {
            if (obj is CurveFile other)
            {
                return FileName == other.FileName &&
                       Owner == other.Owner &&
                       SelectedCurveID == other.SelectedCurveID &&
                       IsStatic == other.IsStatic &&
                       StaticRoll == other.StaticRoll &&
                       StaticFOV == other.StaticFOV &&
                       StaticCameraPos == other.StaticCameraPos &&
                       StaticCameraRot == other.StaticCameraRot &&
                       Curves.SequenceEqual(other.Curves);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + (FileName?.GetHashCode() ?? 0);
            hash = hash * 31 + (Owner?.GetHashCode() ?? 0);
            hash = hash * 31 + (SelectedCurveID?.GetHashCode() ?? 0);
            hash = hash * 31 + IsStatic.GetHashCode();
            hash = hash * 31 + StaticRoll.GetHashCode();
            hash = hash * 31 + StaticFOV.GetHashCode();
            hash = hash * 31 + StaticCameraPos.GetHashCode();
            hash = hash * 31 + StaticCameraRot.GetHashCode();
            hash = hash * 31 + Curves.Aggregate(0, (acc, curve) => acc + curve.GetHashCode());
            return hash;
        }
    }

    public class PointData
    {
        public Vector3 WorldPos { get; set; }
        public float CurvePos { get; set; }

        public PointData(Vector3 worldPos, float curvePos)
        {
            WorldPos = worldPos;
            CurvePos = curvePos;
        }
    }

    public static class hasComponent
    {
        public static bool HasComponent<T>(this GameObject flag) where T : Component
        {
            return flag.GetComponent<T>() != null;
        }
    }
}