using UnityEngine;

public class AmeixaController : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.RegisterAmeixa();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.CollectAmeixa();
            Destroy(gameObject);
        }
    }

}