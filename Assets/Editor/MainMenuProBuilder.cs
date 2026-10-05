using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Construit la scene "MainMenu_Pro" avec les assets Layer Lab.
/// N'edite aucune scene existante : cree uniquement Assets/Scenes/MainMenu_Pro.unity.
/// </summary>
public static class MainMenuProBuilder
{
    private const string LL = "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/";
    private const string ScenePath = "Assets/Scenes/MainMenu_Pro.unity";

    private static readonly Color Navy = new Color32(0x0E, 0x2A, 0x5C, 255);
    private static readonly Color DarkNavy = new Color32(0x08, 0x18, 0x3A, 255);
    private static readonly Color Cyan = new Color32(0x12, 0xA4, 0xFF, 255);

    private static TMP_FontAsset font;
    private static Sprite sBtnYellow, sBtnGreen, sBtnBlue, sBtnGray, sBtnWhite175, sFrame, sFrameGrad, sFlushLeft;
    private static Sprite[] vehicleIcons;
    private static Sprite[] levelImages;

    [MenuItem("Race Enemy/Build MainMenu Pro")]
    public static void Build()
    {
        LoadAssets();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // --- Camera ---
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = DarkNavy;
        cam.orthographic = true;
        camGo.AddComponent<AudioListener>();
        camGo.AddComponent<UniversalAdditionalCameraData>();

        // --- EventSystem ---
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        // --- Canvas ---
        var canvasGo = new GameObject("Canvas", typeof(RectTransform));
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 100f;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();
        RectTransform root = canvasGo.GetComponent<RectTransform>();

        // --- Managers (nouvelles instances dans cette scene) ---
        var mgrGo = new GameObject("MainMenuManager");
        var mgr = mgrGo.AddComponent<MainMenuManager>();
        var selGo = new GameObject("MenuVehicleSelection");
        var sel = selGo.AddComponent<MenuVehicleSelection>();

        // --- Fond commun ---
        RectTransform bg = NewImage("Background", root, null, Navy);
        Stretch(bg);
        RectTransform topBar = NewImage("Bande Haut", bg, null, DarkNavy);
        Anchor(topBar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 0), new Vector2(0, 90));
        RectTransform topLine = NewImage("Ligne Haut", bg, null, Cyan);
        Anchor(topLine, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -90), new Vector2(0, 10));
        RectTransform botBar = NewImage("Bande Bas", bg, null, DarkNavy);
        Anchor(botBar, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(0, 90));
        RectTransform botLine = NewImage("Ligne Bas", bg, null, Cyan);
        Anchor(botLine, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 90), new Vector2(0, 10));

        // =============== PANEL PRINCIPAL ===============
        RectTransform home = NewPanel("Panel Principal", root);

        RectTransform title = NewText("Titre", home, "RACE ENEMY", 190, Color.white);
        Anchor(title, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -150), new Vector2(1500, 260));
        title.gameObject.AddComponent<MenuPulse>();
        var pulseT = title.GetComponent<MenuPulse>();
        SetFloat(pulseT, "scaleAmount", 0.02f);
        SetFloat(pulseT, "floatAmount", 8f);

        RectTransform tag = NewImage("Etiquette Alpha", home, sBtnYellow, Color.white);
        Anchor(tag, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(600, -330), new Vector2(240, 80));
        tag.GetComponent<Image>().type = Image.Type.Sliced;
        tag.localRotation = Quaternion.Euler(0, 0, 6f);
        RectTransform tagTxt = NewText("Texte", tag, "ALPHA", 48, Color.white);
        Stretch(tagTxt);

        Button playBtn = NewButton("Bouton Jouer", home, sBtnGreen, "JOUER", 100, new Vector2(0, -30), new Vector2(640, 220));
        playBtn.gameObject.AddComponent<MenuPulse>();
        SetFloat(playBtn.GetComponent<MenuPulse>(), "scaleAmount", 0.035f);

        Button vehMenuBtn = NewButton("Bouton Vehicules", home, sBtnBlue, "VÉHICULES", 50, new Vector2(-370, -270), new Vector2(350, 140));
        Button optBtn = NewButton("Bouton Options", home, sBtnBlue, "OPTIONS", 50, new Vector2(0, -270), new Vector2(350, 140));
        Button credBtn = NewButton("Bouton Credits", home, sBtnBlue, "CRÉDITS", 50, new Vector2(370, -270), new Vector2(350, 140));

        RectTransform ver = NewText("Version", home, "v" + Application.version + " • Alpha", 38, new Color(1, 1, 1, 0.7f));
        Anchor(ver, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0.5f), new Vector2(0, 45), new Vector2(900, 60));

        // =============== PANEL NIVEAUX ===============
        RectTransform levels = NewPanel("Panel Niveaux", root);

        RectTransform lvlTitle = NewText("Titre Niveaux", levels, "CHOISIR LE NIVEAU", 72, Color.white);
        Anchor(lvlTitle, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -8), new Vector2(1200, 100));

        Button backLvl = NewButton("Bouton Retour", levels, sFlushLeft, "RETOUR", 44, Vector2.zero, new Vector2(300, 110));
        Anchor(backLvl.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, -105), new Vector2(300, 100));

        Button playLv1 = null;
        for (int i = 0; i < 3; i++)
        {
            bool unlocked = i == 0;
            RectTransform card = NewImage("Carte Niveau " + (i + 1), levels, sFrame, unlocked ? new Color32(0x1B, 0x4A, 0x9A, 255) : new Color32(0x12, 0x2B, 0x59, 255));
            card.GetComponent<Image>().type = Image.Type.Sliced;
            Anchor(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2((i - 1) * 590, 70), new Vector2(560, 480));

            RectTransform border = NewImage("Cadre Image", card, null, unlocked ? new Color32(0x12, 0xA4, 0xFF, 255) : new Color32(0x0A, 0x1D, 0x42, 255));
            Anchor(border, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -18), new Vector2(516, 297));
            border.GetComponent<Image>().raycastTarget = false;
            RectTransform pic = NewImage("Image Niveau", border, unlocked ? levelImages[0] : null, unlocked ? Color.white : new Color32(0x08, 0x18, 0x3A, 255));
            Stretch(pic);
            pic.offsetMin = new Vector2(8, 8);
            pic.offsetMax = new Vector2(-8, -8);
            pic.GetComponent<Image>().raycastTarget = false;

            RectTransform lbl = NewText("Etiquette", card, "NIVEAU " + (i + 1), 60, new Color(1, 1, 1, unlocked ? 1f : 0.5f));
            Anchor(lbl, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -322), new Vector2(500, 70));

            if (unlocked)
            {
                playLv1 = NewButton("Bouton Jouer Niveau", card, sBtnGreen, "JOUER", 50, Vector2.zero, new Vector2(300, 76));
                Anchor(playLv1.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 12), new Vector2(300, 76));

                // Etoiles obtenues sur ce niveau (en bas a gauche de l'image)
                var starOn = AssetDatabase.LoadAssetAtPath<Sprite>(LL + "Sprites/Components/IconMisc/Icon_ImageIcon_StarGrade_l_On.png");
                var starOff = AssetDatabase.LoadAssetAtPath<Sprite>(LL + "Sprites/Components/IconMisc/Icon_ImageIcon_StarGrade_l_Off.png");
                var starImages = new Image[3];
                for (int k = 0; k < 3; k++)
                {
                    RectTransform star = NewImage("Etoile " + (k + 1), pic, starOff, Color.white);
                    Anchor(star, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(14 + k * 64, 10), new Vector2(60, 60));
                    star.GetComponent<Image>().preserveAspect = true;
                    star.GetComponent<Image>().raycastTarget = false;
                    starImages[k] = star.GetComponent<Image>();
                }
                var starsDisplay = pic.gameObject.AddComponent<LevelStarsDisplay>();
                var starsSo = new SerializedObject(starsDisplay);
                starsSo.FindProperty("sceneName").stringValue = "LV1";
                var starsArr = starsSo.FindProperty("stars");
                starsArr.arraySize = 3;
                for (int k = 0; k < 3; k++) starsArr.GetArrayElementAtIndex(k).objectReferenceValue = starImages[k];
                starsSo.FindProperty("starOn").objectReferenceValue = starOn;
                starsSo.FindProperty("starOff").objectReferenceValue = starOff;
                starsSo.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                RectTransform q = NewText("Point Interrogation", pic, "?", 220, new Color(1, 1, 1, 0.25f));
                Stretch(q);
                RectTransform soon = NewText("Bientot", card, "BIENTÔT", 56, new Color(1, 1, 1, 0.55f));
                Anchor(soon, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 20), new Vector2(420, 70));
            }
        }

        // --- Zone statistiques / missions (sous les cartes) ---
        RectTransform statTile = NewImage("Tuile Score Zombies", levels, sFrame, new Color32(0x08, 0x18, 0x3A, 255));
        statTile.GetComponent<Image>().type = Image.Type.Sliced;
        Anchor(statTile, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-320, 135), new Vector2(600, 130));
        statTile.GetComponent<Image>().raycastTarget = false;
        RectTransform statLbl = NewText("Titre", statTile, "ZOMBIES ÉLIMINÉS (RECORD)", 34, new Color32(0x7F, 0xD0, 0xFF, 255));
        Anchor(statLbl, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -12), new Vector2(580, 46));
        RectTransform statVal = NewText("Valeur", statTile, "0", 70, Color.white);
        Anchor(statVal, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 8), new Vector2(580, 80));
        var stats = statTile.gameObject.AddComponent<MenuStatsDisplay>();
        SetObjectRef(stats, "zombieRecordText", statVal.GetComponent<TextMeshProUGUI>());

        RectTransform missionTile = NewImage("Tuile Missions", levels, sFrame, new Color32(0x08, 0x18, 0x3A, 255));
        missionTile.GetComponent<Image>().type = Image.Type.Sliced;
        Anchor(missionTile, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(320, 135), new Vector2(600, 130));
        missionTile.GetComponent<Image>().raycastTarget = false;
        RectTransform missLbl = NewText("Titre", missionTile, "MISSIONS", 34, new Color32(0x7F, 0xD0, 0xFF, 255));
        Anchor(missLbl, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -12), new Vector2(580, 46));
        RectTransform missVal = NewText("Valeur", missionTile, "BIENTÔT", 56, new Color(1, 1, 1, 0.5f));
        Anchor(missVal, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 12), new Vector2(580, 70));

        // =============== PANEL VEHICULES ===============
        RectTransform vehicles = NewPanel("Panel Vehicules", root);
        RectTransform vehTitle = NewText("Titre Vehicules", vehicles, "CHOISIR LE VÉHICULE", 72, Color.white);
        Anchor(vehTitle, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -8), new Vector2(1200, 100));
        Button backVeh = NewButton("Bouton Retour", vehicles, sFlushLeft, "RETOUR", 44, Vector2.zero, new Vector2(300, 100));
        Anchor(backVeh.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, -105), new Vector2(300, 100));

        // 3 cartes sur la premiere ligne, le reste sur la seconde (centrees)
        int vehCount = vehicleIcons.Length;
        Button[] vehBtns = new Button[vehCount];
        var cards = new VehicleCard[vehCount];

        Sprite sCoin = Spr("Sprites/Components/Icon_ItemIcons/128/Icon_Golds.png");
        Sprite sBarBg = Spr("Sprites/Components/Slider/Slider_Basic02_Bg.png");
        Sprite[] statIcons =
        {
            Spr("Sprites/Components/Icon_ItemIcons/128/Icon_Heart.png"),
            Spr("Sprites/Components/Icon_ItemIcons/128/Icon_Boots.png"),
            Spr("Sprites/Components/Icon_ItemIcons/128/Icon_Bolt.png"),
        };
        Sprite[] statFillSprites =
        {
            Spr("Sprites/Components/Slider/Slider_Basic04_Fill_Green.png"),
            Spr("Sprites/Components/Slider/Slider_Basic01_Fill_Blue.png"),
            Spr("Sprites/Components/Slider/Slider_Basic04_Fill_Red.png"),
        };
        var statTextColor = new Color32(0x14, 0x2A, 0x5A, 255);

        for (int i = 0; i < vehCount; i++)
        {
            int perRow = 3;
            int row = i / perRow;
            int inRow = Mathf.Min(perRow, vehCount - row * perRow);
            float x = (i % perRow - (inRow - 1) * 0.5f) * 490f;
            float y = vehCount > perRow ? (row == 0 ? 175f : -210f) : 0f;
            var b = NewButton("Vehicule " + (i + 1), vehicles, sFrame, null, 0, Vector2.zero, new Vector2(450, 360));
            var cardRt = b.GetComponent<RectTransform>();
            Anchor(cardRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(450, 360));
            RectTransform icon = NewImage("Icone", cardRt, vehicleIcons[i], Color.white);
            Anchor(icon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 90), new Vector2(340, 165));
            icon.GetComponent<Image>().raycastTarget = false;
            vehBtns[i] = b;

            // Encart arrondi qui regroupe les 3 lignes de stats
            RectTransform statPanel = NewImage("Encart Stats", cardRt, sFrame, new Color32(0xD6, 0xE4, 0xFF, 255));
            statPanel.GetComponent<Image>().type = Image.Type.Sliced;
            statPanel.GetComponent<Image>().raycastTarget = false;
            Anchor(statPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -92), new Vector2(420, 148));

            // Stats : icone + barre coloree + chiffre
            var fills = new RectTransform[3];
            var values = new TextMeshProUGUI[3];
            for (int k = 0; k < 3; k++)
            {
                float ry = -52f - k * 41f;
                RectTransform ic = NewImage("Stat Icone " + (k + 1), cardRt, statIcons[k], Color.white);
                Anchor(ic, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-178, ry), new Vector2(42, 42));
                ic.GetComponent<Image>().raycastTarget = false;

                RectTransform statBg = NewImage("Stat Fond " + (k + 1), cardRt, sBarBg, Color.white);
                statBg.GetComponent<Image>().type = Image.Type.Sliced;
                statBg.GetComponent<Image>().raycastTarget = false;
                Anchor(statBg, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0.5f), new Vector2(-148, ry), new Vector2(190, 26));

                RectTransform statFill = NewImage("Stat Barre", statBg, statFillSprites[k], Color.white);
                statFill.GetComponent<Image>().type = Image.Type.Sliced;
                statFill.GetComponent<Image>().raycastTarget = false;
                Anchor(statFill, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(3, 0), new Vector2(184, 20));
                fills[k] = statFill;

                RectTransform val = NewText("Stat Valeur " + (k + 1), cardRt, "0", 30, statTextColor);
                Anchor(val, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(158, ry), new Vector2(110, 40));
                values[k] = val.GetComponent<TextMeshProUGUI>();
            }

            // Verrou : voile sombre arrondi + pieces et prix
            RectTransform lockRt = NewImage("Verrou", cardRt, sFrame, new Color(0, 0, 0, 0.6f));
            lockRt.GetComponent<Image>().type = Image.Type.Sliced;
            lockRt.GetComponent<Image>().raycastTarget = false;
            Stretch(lockRt);
            RectTransform coinRt = NewImage("Icone Piece", lockRt, sCoin, Color.white);
            Anchor(coinRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-95, 90), new Vector2(100, 100));
            coinRt.GetComponent<Image>().raycastTarget = false;
            RectTransform priceRt = NewText("Prix", lockRt, "", 64, new Color32(0xFF, 0xC8, 0x1E, 255));
            Anchor(priceRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(55, 90), new Vector2(240, 90));
            priceRt.GetComponent<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.NoWrap;
            priceRt.GetComponent<TextMeshProUGUI>().enableAutoSizing = true;
            priceRt.GetComponent<TextMeshProUGUI>().fontSizeMin = 28;
            priceRt.GetComponent<TextMeshProUGUI>().fontSizeMax = 64;
            lockRt.SetAsLastSibling();

            var card = b.gameObject.AddComponent<VehicleCard>();
            var cardSo = new SerializedObject(card);
            cardSo.FindProperty("vehicleIndex").intValue = i;
            cardSo.FindProperty("barWidth").floatValue = 184f;
            cardSo.FindProperty("selection").objectReferenceValue = sel;
            cardSo.FindProperty("icon").objectReferenceValue = icon.GetComponent<Image>();
            cardSo.FindProperty("lockOverlay").objectReferenceValue = lockRt.gameObject;
            cardSo.FindProperty("priceLabel").objectReferenceValue = priceRt.GetComponent<TextMeshProUGUI>();
            var fillArr = cardSo.FindProperty("statFills");
            var valArr = cardSo.FindProperty("statValues");
            fillArr.arraySize = 3;
            valArr.arraySize = 3;
            for (int k = 0; k < 3; k++)
            {
                fillArr.GetArrayElementAtIndex(k).objectReferenceValue = fills[k];
                valArr.GetArrayElementAtIndex(k).objectReferenceValue = values[k];
            }
            cardSo.ApplyModifiedPropertiesWithoutUndo();
            cards[i] = card;
        }

        // Pieces du joueur (panel Vehicules en haut a droite, accueil en haut a gauche)
        MakeCoinsTile(vehicles, new Vector2(1, 1), new Vector2(-30, -95));
        MakeCoinsTile(home, new Vector2(0, 1), new Vector2(40, -135));

        // =============== PANEL OPTIONS ===============
        RectTransform options = NewPopup("Panel Options", root, "OPTIONS", out RectTransform optBody, out Button optClose);
        RectTransform volLbl = NewText("Texte Volume", optBody, "VOLUME", 52, Color.white);
        Anchor(volLbl, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 145), new Vector2(600, 70));
        Slider volSlider = NewSlider("Slider Volume", optBody, new Vector2(0, 75), new Vector2(700, 50));
        volSlider.gameObject.AddComponent<MenuVolumeSlider>();

        RectTransform langLbl = NewText("Texte Langue", optBody, "LANGUE", 52, Color.white);
        Anchor(langLbl, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -15), new Vector2(600, 70));
        Button frBtn = NewButton("Bouton Francais", optBody, sBtnBlue, "FRANÇAIS", 44, new Vector2(-190, -115), new Vector2(340, 110));
        Button enBtn = NewButton("Bouton English", optBody, sBtnBlue, "ENGLISH", 44, new Vector2(190, -115), new Vector2(340, 110));
        SetString(frBtn.gameObject.AddComponent<LanguageButton>(), "languageCode", "fr");
        SetString(enBtn.gameObject.AddComponent<LanguageButton>(), "languageCode", "en");

        // =============== PANEL CREDITS ===============
        RectTransform credits = NewPopup("Panel Credits", root, "CRÉDITS", out RectTransform credBody, out Button credClose);
        RectTransform credTxt = NewText("Texte Credits", credBody, "RACE ENEMY\n\nMerci d'avoir joué !", 52, Color.white);
        Stretch(credTxt);

        // =============== PANEL CHARGEMENT ===============
        RectTransform loading = NewPanel("Panel Chargement", root);
        loading.GetComponent<Image>().color = new Color32(0x08, 0x18, 0x3A, 255);
        RectTransform loadTitle = NewText("Titre", loading, "RACE ENEMY", 150, Color.white);
        Anchor(loadTitle, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 150), new Vector2(1400, 220));
        Slider loadSlider = NewSlider("Slider", loading, new Vector2(0, -90), new Vector2(1000, 60));
        loadSlider.interactable = false;
        RectTransform loadTxt = NewText("Texte Progression", loading, "Chargement... 0%", 56, Color.white);
        Anchor(loadTxt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -190), new Vector2(1000, 80));

        // --- Aide des controles : bouton "?" en haut a droite de l'accueil ---
        var helpPanel = ControlsUIBuilder.CreateHelpPanel(root);
        ControlsUIBuilder.CreateHelpButton(home, helpPanel, new Vector2(1, 1), new Vector2(-50, -135), 110);

        // --- Connexions des managers ---
        var mgrSo = new SerializedObject(mgr);
        mgrSo.FindProperty("currentActivePanel").objectReferenceValue = home.gameObject;
        mgrSo.FindProperty("loadingPanel").objectReferenceValue = loading.gameObject;
        mgrSo.FindProperty("progressBar").objectReferenceValue = loadSlider;
        mgrSo.FindProperty("progressText").objectReferenceValue = loadTxt.GetComponent<TextMeshProUGUI>();
        mgrSo.ApplyModifiedPropertiesWithoutUndo();

        var selSo = new SerializedObject(sel);
        var arr = selSo.FindProperty("vehicleButtons");
        arr.arraySize = vehBtns.Length;
        for (int i = 0; i < vehBtns.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = vehBtns[i];
        selSo.ApplyModifiedPropertiesWithoutUndo();

        UnityEventTools.AddObjectPersistentListener(playBtn.onClick, mgr.SwitchPanel, levels.gameObject);
        UnityEventTools.AddObjectPersistentListener(vehMenuBtn.onClick, mgr.SwitchPanel, vehicles.gameObject);
        UnityEventTools.AddObjectPersistentListener(backVeh.onClick, mgr.SwitchPanel, home.gameObject);
        UnityEventTools.AddObjectPersistentListener(optBtn.onClick, mgr.SwitchPanel, options.gameObject);
        UnityEventTools.AddObjectPersistentListener(credBtn.onClick, mgr.SwitchPanel, credits.gameObject);
        UnityEventTools.AddObjectPersistentListener(backLvl.onClick, mgr.SwitchPanel, home.gameObject);
        UnityEventTools.AddObjectPersistentListener(optClose.onClick, mgr.SwitchPanel, home.gameObject);
        UnityEventTools.AddObjectPersistentListener(credClose.onClick, mgr.SwitchPanel, home.gameObject);
        UnityEventTools.AddStringPersistentListener(playLv1.onClick, mgr.LoadScene, "LV1");
        for (int i = 0; i < vehBtns.Length; i++)
            UnityEventTools.AddVoidPersistentListener(vehBtns[i].onClick, cards[i].Click);

        // Seul le panel principal est visible au depart.
        levels.gameObject.SetActive(false);
        vehicles.gameObject.SetActive(false);
        options.gameObject.SetActive(false);
        credits.gameObject.SetActive(false);
        loading.gameObject.SetActive(false);

        EditorSceneManager.SaveScene(scene, ScenePath);
        SceneLocalizer.Apply();
        Debug.Log("[MainMenuPro] Scene creee : " + ScenePath);
    }

    // ------------------------------------------------------------------ helpers

    private static void LoadAssets()
    {
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(LL + "Fonts/LilitaOne-Regular Outline_Extended ASCII_120 SDF.asset");
        sBtnYellow = Spr("Sprites/Components/Button/Button01_225_Yellow.png");
        sBtnGreen = Spr("Sprites/Components/Button/Button01_225_Green.png");
        sBtnBlue = Spr("Sprites/Components/Button/Button01_225_Blue.png");
        sBtnGray = Spr("Sprites/Components/Button/Button01_225_Gray.png");
        sBtnWhite175 = Spr("Sprites/Components/Button/Button01_175_White.png");
        sFlushLeft = Spr("Sprites/Components/Button/Button_FlushLeft_Gray.png");
        sFrame = Spr("Sprites/Components/Frame/BasicFrame_Round20.png");
        sFrameGrad = Spr("Sprites/Components/Frame/BasicFrame_Round12_Gradient.png");
        levelImages = new[] { AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Capture d'écran Niveau 01.png") };
        vehicleIcons = new[]
        {
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Capture d'écran 2026-07-27 192645.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Capture d'écran 2026-07-27 192702.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Capture d'écran 2026-07-27 192732.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>(VehicleAdder.IconFolder + "/Icone_Buggy.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>(VehicleAdder.IconFolder + "/Icone_Ute.png"),
        };
    }

    private static Sprite Spr(string rel)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(LL + rel);
        if (s == null) Debug.LogWarning("[MainMenuPro] Sprite introuvable : " + rel);
        return s;
    }

    private static RectTransform NewImage(string name, RectTransform parent, Sprite sprite, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        return go.GetComponent<RectTransform>();
    }

    private static void MakeCoinsTile(RectTransform parent, Vector2 corner, Vector2 pos)
    {
        RectTransform tile = NewImage("Tuile Pieces", parent, sFrame, new Color32(0x08, 0x18, 0x3A, 255));
        tile.GetComponent<Image>().type = Image.Type.Sliced;
        tile.GetComponent<Image>().raycastTarget = false;
        Anchor(tile, corner, corner, corner, pos, new Vector2(270, 90));
        RectTransform coin = NewImage("Icone Piece", tile, Spr("Sprites/Components/Icon_ItemIcons/128/Icon_Golds.png"), Color.white);
        Anchor(coin, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(12, 0), new Vector2(72, 72));
        coin.GetComponent<Image>().raycastTarget = false;
        RectTransform txt = NewText("Valeur", tile, "0", 56, new Color32(0xFF, 0xC8, 0x1E, 255));
        Stretch(txt);
        txt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        txt.offsetMin = new Vector2(96, 0);
        txt.offsetMax = new Vector2(-14, 0);
        var display = tile.gameObject.AddComponent<CoinsDisplay>();
        SetObjectRef(display, "label", txt.GetComponent<TextMeshProUGUI>());
    }

    private static RectTransform NewPanel(string name, RectTransform parent)
    {
        RectTransform rt = NewImage(name, parent, null, new Color(0, 0, 0, 0));
        Stretch(rt);
        rt.GetComponent<Image>().raycastTarget = false;
        return rt;
    }

    private static RectTransform NewText(string name, RectTransform parent, string text, float size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        return go.GetComponent<RectTransform>();
    }

    private static Button NewButton(string name, RectTransform parent, Sprite sprite, string label, float fontSize, Vector2 pos, Vector2 size)
    {
        RectTransform rt = NewImage(name, parent, sprite, Color.white);
        Anchor(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
        var img = rt.GetComponent<Image>();
        img.type = Image.Type.Sliced;
        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        btn.colors = colors;
        if (!string.IsNullOrEmpty(label))
        {
            RectTransform txt = NewText("Texte", rt, label, fontSize, Color.white);
            Stretch(txt);
            txt.offsetMin = new Vector2(10, 14);
            txt.offsetMax = new Vector2(-10, 0);
        }
        return btn;
    }

    private static RectTransform NewPopup(string name, RectTransform parent, string title, out RectTransform body, out Button close)
    {
        RectTransform panel = NewImage(name, parent, null, new Color(0, 0, 0, 0.65f));
        Stretch(panel);

        RectTransform win = NewImage("Fenetre", panel, sFrame, new Color32(0x14, 0x36, 0x78, 255));
        win.GetComponent<Image>().type = Image.Type.Sliced;
        Anchor(win, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000, 700));

        RectTransform ttl = NewText("Titre", win, title, 80, Color.white);
        Anchor(ttl, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(900, 110));

        body = NewImage("Contenu", win, null, new Color(0, 0, 0, 0));
        Anchor(body, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(900, 380));
        body.GetComponent<Image>().raycastTarget = false;

        close = NewButton("Bouton Retour", win, sBtnYellow, "RETOUR", 56, Vector2.zero, new Vector2(380, 130));
        Anchor(close.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 35), new Vector2(380, 130));
        return panel;
    }

    private static Slider NewSlider(string name, RectTransform parent, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        Anchor(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);

        RectTransform back = NewImage("Fond", rt, sFrame, new Color32(0x08, 0x18, 0x3A, 255));
        back.GetComponent<Image>().type = Image.Type.Sliced;
        Stretch(back);

        var fillArea = new GameObject("Zone Remplissage", typeof(RectTransform));
        fillArea.transform.SetParent(rt, false);
        RectTransform fa = fillArea.GetComponent<RectTransform>();
        Stretch(fa);
        fa.offsetMin = new Vector2(8, 8);
        fa.offsetMax = new Vector2(-8, -8);

        RectTransform fill = NewImage("Remplissage", fa, sFrame, new Color32(0xFF, 0xC8, 0x1E, 255));
        fill.GetComponent<Image>().type = Image.Type.Sliced;
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = new Vector2(0, 1);
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;

        var slider = go.GetComponent<Slider>();
        slider.fillRect = fill;
        slider.targetGraphic = back.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;
        slider.transition = Selectable.Transition.None;
        return slider;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void SetObjectRef(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetString(Object target, string field, string value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).stringValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetFloat(Object target, string field, float value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).floatValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
