using UnityEngine;

public class Landing : StateMachineBehaviour
{
    private PlayerControllerPlaya playerController;
    private bool playerSearched = false;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        FindPlayerIfNeeded();
        
        if (playerController != null)
        {
            playerController.StartLanding();
        }
        else
        {
            Debug.LogWarning("PlayerControllerPlaya no encontrado en objeto con tag 'Player'");
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playerController != null)
        {
            playerController.EndLanding();
        }
    }

    private void FindPlayerIfNeeded()
    {
        if (!playerSearched)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerController = player.GetComponent<PlayerControllerPlaya>();
            }
            playerSearched = true;
        }
    }

    
}