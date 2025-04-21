using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AmeixaController : MonoBehaviour
{

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterAmeixa();
        }
        else
        {
            Debug.LogWarning("GameManager instance not found!");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CollectAmeixa();
            }
            DestroyAmeixa();
        }
    }

    private void DestroyAmeixa()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DestroyAmeixa();
        }
        Destroy(gameObject);
    }
}
