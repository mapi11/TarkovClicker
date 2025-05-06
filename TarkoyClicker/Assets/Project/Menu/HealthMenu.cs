using UnityEngine;

public class HealthMenu : MonoBehaviour
{
    [SerializeField] private GameObject imgFracture;

    TimingClick timingClick;

    private void Awake()
    {
        timingClick = FindAnyObjectByType<TimingClick>();
    }

    private void Update()
    {
        if (timingClick.IsArmBroken == true)
        {
            imgFracture.SetActive(true);
        }
        else
        {
            imgFracture.SetActive(false);
        }
    }
}
