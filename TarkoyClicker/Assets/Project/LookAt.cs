using UnityEngine;

public class LookAt : MonoBehaviour
{
    [SerializeField] private Transform _objectToLook;

    private void Update()
    {
        transform.LookAt(_objectToLook);
    }
}
