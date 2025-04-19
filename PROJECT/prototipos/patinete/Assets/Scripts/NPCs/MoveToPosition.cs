using UnityEngine;
using UnityEngine.AI;

public class MoveToPosition : MonoBehaviour
{
    Transform target;
    NavMeshAgent agent;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GetPlayer();
    }
    void Update()
    {
        agent.SetDestination(target.position);
    }


        private void GetPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogWarning("No hay objetivo asignado para la cámara.");
        }
    }
}
