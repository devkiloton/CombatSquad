using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public TMP_Text overheatedMessage;
    public static UIController Instance;

    private void Awake()
    {
        Instance = this;
    }

}
