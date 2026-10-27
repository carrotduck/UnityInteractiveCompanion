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
    // Starter Assets 的走路动画包含脚步事件；当前演示不播放脚步声。
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

        // 用实际速度混合站立和走路动作。
        characterAnimator.SetFloat(
            "Speed", speed, 0.1f, Time.deltaTime);

        // 伙伴沿地面导航，保持动画的着地状态。
        characterAnimator.SetBool("Grounded", true);
        characterAnimator.SetBool("Jump", false);
        characterAnimator.SetBool("FreeFall", false);
        characterAnimator.SetFloat("MotionSpeed", 1f);
    }
}
