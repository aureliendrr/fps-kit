using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public PlayerSettings settings;
    public InputSystem input;

    [SerializeField] private GameObject playerObj;
    private PlayerView playerView;
    private PlayerMovements playerMovements;
    private PlayerWeapon playerWeapon;

    private void Awake() {

        if(settings == null){
            Debug.LogError("Player Manager need a PlayerSettings");
        }

        if(instance != null) {
            Destroy(this);
        }
        else {
            instance = this;
        }
        DontDestroyOnLoad(this);

        InitializePlayer(playerObj);
    }

    private void Start() {
        input = InputSystem.Instance;
    }

    public void InitializePlayer(GameObject player)
    {
        playerView = player.GetComponent<PlayerView>();
        playerMovements = player.GetComponent<PlayerMovements>();
        playerWeapon = player.GetComponent<PlayerWeapon>();
    }

    private void Update()
    {
        HandlePlayerView();
        if(playerMovements != null){
            HandlePlayerMovements();
        }
        if(playerWeapon != null){
            HandlePlayerWeapons();
        }
    }

    private void HandlePlayerView()
    {
        //
    }

    private void HandlePlayerMovements()
    {
        if(settings.canCrouch && input.Action(KeybindingAction.Crouch)){
            playerMovements.RequestCrouch();
        }

        if(settings.canProne && input.Action(KeybindingAction.Prone)){
            playerMovements.RequestProning();
        }

        if(settings.canJump && input.Action(KeybindingAction.Jump)){
            playerMovements.RequestJump();
        }

        if(settings.canSlide && input.Action(KeybindingAction.Slide)){
            playerMovements.RequestSlide();
        }
    }

    private void HandlePlayerWeapons()
    {
        if(input.Action(KeybindingAction.Shoot)){
            playerWeapon.RequestShoot();
        }
    }
}