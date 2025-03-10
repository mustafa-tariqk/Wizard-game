using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Interactables : MonoBehaviour
{
    public bool InInteractableRange;
    public bool Buyable, Removeable, CauldronBuyable;
    public float timer, Maxtimer;
    public InteractableVariables InterVaris;
    public WeaponSwapControl iwsc;
    public PlayerInflicts playerInflicts;
    public MovementController movementController;
    public TextMeshProUGUI txt;
    public Slider slider;
    private bool CanBuy;
    private bool HasBought;

    private PlayerController controls;

    private void Awake()
    {
        // Initialize the input system
        controls = new PlayerController();

        // Subscribe to the Interact action
        controls.PlayerControls.Interact.started += ctx => StartInteraction();
        controls.PlayerControls.Interact.canceled += ctx => StopInteraction();
    }

    private void Start()
    {
        timer = Maxtimer;
        slider.maxValue = Maxtimer;
        slider.value = Maxtimer;
        slider.gameObject.SetActive(false);
        txt.transform.parent.gameObject.SetActive(false);
        playerInflicts = GetComponentInParent<PlayerInflicts>();
        movementController = GetComponentInParent<MovementController>();
    }

    private void Update()
    {
        if (InInteractableRange)
        {
            if (InterVaris.BuyableObject != null)
            {
                txt.text = "Buy " + InterVaris.BuyableObject.name + " for " + InterVaris.Cost;
                txt.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                txt.text = "Buy barrier for " + InterVaris.Cost;
                txt.transform.parent.gameObject.SetActive(true);
            }
        }
        else
        {
            txt.transform.parent.gameObject.SetActive(false);
        }

        if (InInteractableRange && slider.gameObject.activeSelf)
        {
            timer -= Time.deltaTime;
            slider.value = timer;

            if (timer <= 0)
            {
                if (Removeable)
                {
                    iwsc.points -= InterVaris.Cost;
                    Destroy(InterVaris.gameObject);
                    ResetInteractState();
                }
                else if (Buyable)
                {
                    CanBuy = true;
                    for (int e = 0; e < iwsc.EquippedStaffs.Length; e++)
                    {
                        if (iwsc.EquippedStaffs[e] == InterVaris.BuyableObject)
                        {
                            CanBuy = false;
                        }
                    }

                    if (CanBuy)
                    {
                        iwsc.points -= InterVaris.Cost;

                        for (int i = 0; i < iwsc.EquippedStaffs.Length; i++)
                        {
                            if (iwsc.EquippedStaffs[i] == null)
                            {
                                iwsc.EquippedStaffs[i] = InterVaris.BuyableObject;
                                iwsc.EquippedStaffs[i].gameObject.GetComponent<SpellController>().movementController = iwsc.movementController;
                                HasBought = true;
                                break;
                            }
                        }

                        if (!HasBought)
                        {
                            iwsc.EquippedStaffs[iwsc.tempstaff] = InterVaris.BuyableObject;
                            iwsc.CurrentEquippedStaff = InterVaris.BuyableObject;
                            Destroy(iwsc.tempobject);
                            iwsc.tempobject = Instantiate(iwsc.CurrentEquippedStaff, iwsc.StaffSpawnPoint);
                            iwsc.tempobject.GetComponent<SpellController>().movementController = iwsc.movementController;
                            iwsc.tempobject.GetComponent<SpellController>().enabled = true;
                            HasBought = true;
                        }

                        ResetInteractState();
                    }
                }
                else if (CauldronBuyable)
                {

                    if (InterVaris.HealthUp)
                    {
                        playerInflicts.PlayerMaxHealth += 50;
                    }
                    else if (InterVaris.SpeedUp)
                    {
                        movementController.walkSpeed = 6f;
                        movementController.sprintSpeed = 9f;
                    }
                    else if (InterVaris.StaminaUp)
                    {
                        //increase player sprint time when it is made a variable again
                    }
                    else if (InterVaris.ManaUp)
                    {
                        //i dont even know where to do this right now man
                    }
                }
            }
        }
    }

    private void StartInteraction()
    {
        if (InInteractableRange && InterVaris.Cost <= iwsc.points)
        {
            slider.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("Too Broke");
        }
    }

    private void StopInteraction()
    {
        ResetInteractState();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Removeable"))
        {
            InInteractableRange = true;
            InterVaris = other.GetComponent<InteractableVariables>();
            Removeable = true;
        }

        if (other.CompareTag("Buyable"))
        {
            InInteractableRange = true;
            InterVaris = other.GetComponent<InteractableVariables>();
            Buyable = true;
        }
        if (other.CompareTag("CauldronBuyable"))
        {
            InInteractableRange = true;
            InterVaris = other.GetComponent<InteractableVariables>();
            CauldronBuyable = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Removeable") || other.CompareTag("Buyable"))
        {
            ResetInteractState();
        }
    }

    private void ResetInteractState()
    {
        InInteractableRange = false;
        timer = Maxtimer;
        InterVaris = null;
        Removeable = false;
        Buyable = false;
        CanBuy = false;
        CauldronBuyable = false;
        HasBought = false;
        slider.value = Maxtimer;
        slider.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        controls.PlayerControls.Enable();
    }

    private void OnDisable()
    {
        controls.PlayerControls.Disable();
    }
}