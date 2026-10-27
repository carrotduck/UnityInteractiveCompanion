using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class CompanionLocomotion : MonoBehaviour
{
    private Animator characterAnimator;
    private NavMeshAgent agent;

    private void Awake()
    {
        characterAnimator = GetComponent<Animator>();
        agent = GetComponentInParent<NavMeshAgent>();

        characterAnimator.applyRootMotion = false;
    }
    // Starter Assets walking clips contain footstep events; this demo has no footstep audio.
    private void OnFootstep(AnimationEvent animationEvent)
    {
    }

    private void LateUpdate()
    {
        float speed = 0f;

        if (agent != null &&
            agent.enabled &&
            agent.isOnNavMesh &&
            !agent.isStopped)
        {
            Vector3 velocity = agent.velocity;
            velocity.y = 0f;
            speed = velocity.magnitude;
        }

        // Blend idle and walking animations using actual movement speed.
        characterAnimator.SetFloat(
            "Speed", speed, 0.1f, Time.deltaTime);

        // Keep the animation grounded while the companion navigates along the floor.
        characterAnimator.SetBool("Grounded", true);
        characterAnimator.SetBool("Jump", false);
        characterAnimator.SetBool("FreeFall", false);
        characterAnimator.SetFloat("MotionSpeed", 1f);
    }
}
