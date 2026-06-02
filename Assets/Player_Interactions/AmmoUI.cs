using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    public static AmmoUI Instance;

    public TMP_Text AmmoText;

    void Awake()
    {
        Instance = this;
        HideAmmo();
    }

    public void ShowAmmo(int currentAmmo, int reserveAmmo)
    {
        if (AmmoText == null) return;

        AmmoText.gameObject.SetActive(true);
        AmmoText.text = $"Ammo: {currentAmmo} / {reserveAmmo}";
    }

    public void HideAmmo()
    {
        if (AmmoText == null) return;

        AmmoText.gameObject.SetActive(false);
    }
}
