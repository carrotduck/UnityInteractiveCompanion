using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class CompanionLocomotion : MonoBehaviour
{
    private Animator characterAnimator;
    private NavMeshAgent agent;

    void Awake()
    {
        characterAnimator = GetComponent<Animator>();
        agent = GetComponentInParent<NavMeshAgent>();

        characterAnimator.applyRootMotion = false;
    }
// 接收走路动画的脚步通知，暂时不播放声音。
private void OnFootstep(AnimationEvent animationEvent)
{
}

    void LateUpdate()
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

        // 将实际移动速度传给动画，让站立和走路平滑切换。
        characterAnimator.SetFloat(
            "Speed", speed, 0.1f, Time.deltaTime);

        // 当前伙伴始终在地面导航，不使用跳跃和下落。
        characterAnimator.SetBool("Grounded", true);
        characterAnimator.SetBool("Jump", false);
        characterAnimator.SetBool("FreeFall", false);
        characterAnimator.SetFloat("MotionSpeed", 1f);
    }
}