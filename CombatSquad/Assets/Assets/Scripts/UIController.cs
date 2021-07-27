using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    public TMP_Text overheatedMessage;
    public Slider WeaponTempSlider;

    public GameObject DeathScreen;
    public TMP_Text DeathText;

    public Slider SliderHealth;

    private void Awake()
    {
        Instance = this;
    }

}
