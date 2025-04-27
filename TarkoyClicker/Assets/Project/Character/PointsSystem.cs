using TMPro;
using UnityEngine;

public class PointsSystem : MonoBehaviour
{
    public float _PointsCount = 0;

    public TextMeshProUGUI _txtPoints;


    private void Awake()
    {
        _txtPoints = GameObject.Find("txtPoints").GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        _txtPoints.text = "Points: " + _PointsCount.ToString();
    }
}
