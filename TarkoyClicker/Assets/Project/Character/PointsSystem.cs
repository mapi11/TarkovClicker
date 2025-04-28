using TMPro;
using UnityEngine;

public class PointsSystem : MonoBehaviour
{
    public int Points { get; private set; }

    public TextMeshProUGUI _txtPoints;


    private void Awake()
    {
        _txtPoints = GameObject.Find("txtPoints").GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        _txtPoints.text = "Points: " + Points.ToString();
    }

    public void AddPoints(int amount)
    {
        Points += amount;

        _txtPoints.text = "Points: " + Points.ToString();
    }
}
