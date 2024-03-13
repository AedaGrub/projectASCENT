using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    #region HUD CAMERA
    [Header("HUD CAMERA")]
    [SerializeField] private GameObject HUDParent;
    [SerializeField] private Camera mainCam;
    private Vector3 camLastPos;
    private Vector3 HUDDirection;
    private float cameraTravel;
    private Vector3 velocity;
    private Vector3 targetPos;
    public float smoothTime;
    #endregion

    #region HUD ROOM
    [Header("HUD ROOM")]
    [SerializeField] private Image playerProgressNode;
    #endregion

    #region HUD COOLDOWN
    [Header("HUD COOLDOWN")]
    [SerializeField] private Image cooldownNode;
    #endregion

    #region HUD HEALTH
    [Header("HUD HEALTH")]
    [SerializeField] private GameObject healthFillScale;
    [SerializeField] private Image healthFill;
    [SerializeField] private Image healthMaxMarker;
    #endregion

    #region HUD SHIELD
    [Header("HUD SHIELD")]
    [SerializeField] private GameObject shieldFillScale;
    [SerializeField] private Image shieldFill;
    #endregion

    #region BLACK SCREEN
    [Header("BLACK SCREEN")]
    [SerializeField] private Animator blackScreen;
    const string blackStart = "boonsGreyIn";
    const string blackExit = "boonsGreyOut";
    #endregion

    #region POST PRO
    [Header("POST PRO VOLUME")]
    [SerializeField] private Volume vignetteVol;
    private float vignetteWeight;
    #endregion

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        UpdateReferences();
        UpdateHealth();
    }

    public void UpdateReferences()
    {
        mainCam = Camera.main;
        vignetteVol = GameObject.FindWithTag("VignetteVol").GetComponent<Volume>();
        UpdateVignette(vignetteWeight);
    }

    public void BlackScreenStart()
    {
        blackScreen.Play(blackStart);
    }

    public void BlackScreenExit()
    {
        blackScreen.Play(blackExit);
    }

    void Update()
    {
        #region CAMERA
        //HUD FOLLOW CAM LOOSELY
        if (mainCam == null)
        {
            UpdateReferences();
        }

        if (cameraTravel > 1 && !levelLoader.instance.isTransitioning)
        {
            targetPos = new Vector3(HUDDirection.x, HUDDirection.y, HUDParent.transform.position.z);
        }
        else
        {
            targetPos = new Vector3(0, 0, 0);
        }
        HUDParent.transform.localPosition = Vector3.SmoothDamp(HUDParent.transform.localPosition, targetPos, ref velocity, smoothTime);

        //TRACK CAMERA VALUES
        TrackCamera();
        #endregion

        #region COOLDOWN UI
        if (!gameManager.instance.canDash)
        {
            cooldownNode.fillAmount = 1f;
        }
        else
        {
            cooldownNode.fillAmount = gameManager.instance.currentChargeValue / 1;
        }
        #endregion

        #region SHIELD UI
        if (gameManager.instance.currentShieldRate <= 0)
        {
            shieldFill.fillAmount = 0f;
        }
        else
        {
            shieldFill.fillAmount = gameManager.instance.currentShieldValue / 1;
        }
        #endregion
    }

    private void TrackCamera()
    {
        cameraTravel = Vector3.Distance(mainCam.transform.position, camLastPos) * 50;
        HUDDirection = (mainCam.transform.position - camLastPos).normalized * -cameraTravel;
        camLastPos = mainCam.transform.position;
    }

    public void UpdateHealth()
    {
        float x = gameManager.instance.maxHealth;
        float y = gameManager.instance.CurrentHealth;

        LeanTween.scaleX(healthFillScale, y, 0.5f).setEaseOutExpo();
        LeanTween.scaleX(shieldFillScale, y, 0.5f).setEaseOutExpo();

      
        //REPOSITION MAX HEALTH MARKER
        float z = ((x * 30) - 15);
        healthMaxMarker.rectTransform.localPosition = new Vector3(z, 0, 0);

        if (y != x)
        {
            healthMaxMarker.gameObject.SetActive(true);
        }
        else
        {
            healthMaxMarker.gameObject.SetActive(false);
        }

        //UPDATE VIGNETTE
        float v = vignetteWeight;
        if (y == 1)
        {
            healthFill.color = new Color(1f, 0.25f, 0.25f);
            LeanTween.value(v, 1f, 0.5f).setOnUpdate(UpdateVignette).setEaseOutExpo();
            audioManager.instance.LowHealth(true);
        }
        else
        {
            healthFill.color = Color.white;
            LeanTween.value(v, 0f, 0.5f).setOnUpdate(UpdateVignette).setEaseOutExpo();
            audioManager.instance.LowHealth(false);
        }
    }

    private void UpdateVignette(float value)
    {
        vignetteWeight = value;
        vignetteVol.weight = vignetteWeight;
    }

    public void UpdateRoomUI()
    {
        float p = playerProgressNode.rectTransform.localPosition.x;
        float target = (-75 + (50 * gameManager.instance.currentRoom));
        LeanTween.value(p, target, 0.5f).setOnUpdate(UpdateRoomUIValue).setEaseOutExpo();
    }

    private void UpdateRoomUIValue(float value)
    {
        playerProgressNode.rectTransform.localPosition = new Vector2(value, playerProgressNode.rectTransform.localPosition.y);
    }
}
