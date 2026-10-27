using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(NavMeshAgent))]
public class CompanionApproach : MonoBehaviour
{
    public Transform userTarget;
    public TMP_Text greetingText;
    public Animator characterAnimator;
    public LayerMask obstructionLayers;
    public Transform meetingSpot;
    public Button acceptButton;
    public Button declineButton;
    public Button waitButton;

    public float moveSpeed = 2f;
    public float stoppingDistance = 1.5f;
    public float turnSpeed = 180f;
    public float noticeDistance = 3f;
    public float personalDistance = 1f;
    public float retreatSpeed = 3f;
    public float greetingHoldDuration = 5.1f;
    public float guideWaitDistance = 3.5f;
    public float guideResumeDistance = 2.5f;

    private enum Activity { Following, TurningToUser, Choosing, Guiding, Arrived }
    private Activity activity = Activity.Following;
    private NavMeshAgent agent;
    private NavMeshPath route;
    private bool hasGreeted;
    private bool interactionPaused;
    private bool invitationOffered;
    private bool waitingForUser;
    private bool turningBackToRoute;
    private bool reconsidering;
    private float greetingEndTime;
    private Vector3 guideDestination;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        route = new NavMeshPath();
        agent.updateRotation = false;
        agent.autoBraking = true;
        SetChoicesVisible(false);
        ClearGreeting();

        // 自动连接点击事件，不需要手动配置按钮的 On Click。
        if (acceptButton != null)
            acceptButton.onClick.AddListener(AcceptInvitation);
        if (declineButton != null)
            declineButton.onClick.AddListener(DeclineInvitation);
        if (waitButton != null)
            waitButton.onClick.AddListener(RequestReconsideration);
    }

    void OnDestroy()
    {
        if (acceptButton != null)
            acceptButton.onClick.RemoveListener(AcceptInvitation);
        if (declineButton != null)
            declineButton.onClick.RemoveListener(DeclineInvitation);
        if (waitButton != null)
            waitButton.onClick.RemoveListener(RequestReconsideration);
    }

    bool CanNavigate()
    {
        return agent != null && agent.enabled && agent.isOnNavMesh;
    }

    void SetChoicesVisible(bool visible)
    {
        // 选择按钮与“等一下”按钮互斥；暂停期间全部隐藏。
        bool choosing = visible && !interactionPaused && activity == Activity.Choosing;
        bool canCall = visible && !interactionPaused && invitationOffered &&
            activity != Activity.Choosing && activity != Activity.TurningToUser;
        if (acceptButton != null)
        {
            acceptButton.gameObject.SetActive(choosing);
            acceptButton.interactable = true;
            TMP_Text label = acceptButton.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                label.text = reconsidering ? "Go together" : "Yes, let's go";
        }
        if (declineButton != null)
        {
            declineButton.gameObject.SetActive(choosing);
            declineButton.interactable = true;
            TMP_Text label = declineButton.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                label.text = reconsidering ? "Stay with me" : "Not now";
        }
        if (waitButton != null)
        {
            waitButton.gameObject.SetActive(canCall);
            waitButton.interactable = true;
            TMP_Text label = waitButton.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                label.text = "Wait a moment";
        }
    }

    void ClearGreeting()
    {
        if (greetingText != null)
            greetingText.text = "";
    }

    void ShowMessage(string message, bool persistent = false)
    {
        CancelInvoke(nameof(ClearGreeting));
        if (greetingText == null)
            return;
        greetingText.text = message;
        if (!persistent)
            Invoke(nameof(ClearGreeting), 3f);
    }

    void StopMoving()
    {
        if (!CanNavigate())
            return;
        agent.isStopped = true;
        agent.ResetPath();
    }

    Vector3 FlatDirection(Vector3 position)
    {
        Vector3 direction = position - transform.position;
        direction.y = 0f;
        return direction;
    }

    void FaceDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f)
            return;
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, Quaternion.LookRotation(direction),
            turnSpeed * Time.deltaTime);
    }

    bool IsBlocked()
    {
        return Physics.Linecast(
            transform.position + Vector3.down * 0.25f,
            userTarget.position + Vector3.down * 0.25f,
            obstructionLayers, QueryTriggerInteraction.Ignore);
    }

    bool FindGround(Vector3 position, out NavMeshHit hit)
    {
        position.y = transform.position.y - agent.baseOffset;
        return NavMesh.SamplePosition(position, out hit, 0.75f, agent.areaMask);
    }

    void OfferInvitation()
    {
        StopMoving();
        invitationOffered = true;
        activity = Activity.Choosing;
        ShowMessage("Would you like to come over here with me?", true);
        SetChoicesVisible(true);
    }
    public void AcceptInvitation()
    {
        if (!invitationOffered || activity != Activity.Choosing ||
            interactionPaused || !CanNavigate())
            return;
        StopMoving();

        NavMeshHit hit;
        if (meetingSpot == null || !FindGround(meetingSpot.position, out hit) ||
            !agent.CalculatePath(hit.position, route) ||
            route.status != NavMeshPathStatus.PathComplete)
        {
            activity = Activity.Following;
            waitingForUser = false;
            SetChoicesVisible(true);
            ShowMessage("I can't reach that spot. Let's stay here.");
            return;
        }

        turningBackToRoute = false;
        guideDestination = hit.position;
        activity = Activity.Guiding;
        waitingForUser = false;
        SetChoicesVisible(true);
        ShowMessage("Let's go! Follow me.");
    }

    public void DeclineInvitation()
    {
        if (!invitationOffered || activity != Activity.Choosing || interactionPaused)
            return;
        activity = Activity.Following;
        waitingForUser = false;
        StopMoving();
        SetChoicesVisible(true);
        ShowMessage("Of course. I'll follow you instead.");
    }

    public void RequestReconsideration()
    {
        if (!invitationOffered || interactionPaused || !CanNavigate() ||
            userTarget == null || activity == Activity.Choosing ||
            activity == Activity.TurningToUser)
            return;

        // 叫住只会中断当前安排；下一步选择才决定去哪里。
        StopMoving();
        waitingForUser = false;
        reconsidering = true;
        activity = Activity.TurningToUser;
        SetChoicesVisible(true);
        ShowMessage("Yes?", true);
    }

    void UpdateGuiding(Vector3 userDirection)
    {
        // 停下和继续使用不同距离，避免在边界附近反复停走。
        float waitDistance = Mathf.Max(0.2f, guideWaitDistance);
        float resumeDistance = Mathf.Clamp(
            guideResumeDistance, 0.1f, waitDistance - 0.1f);
        float userDistance = userDirection.magnitude;

        if (!waitingForUser && userDistance > waitDistance)
        {
            waitingForUser = true;
            turningBackToRoute = true;
            ShowMessage("I'll wait for you.", true);
        }

        if (waitingForUser)
        {
            StopMoving();
            FaceDirection(userDirection);
            if (userDistance > resumeDistance)
                return;

            waitingForUser = false;
            ShowMessage("Ready? Let's continue.");
        }
        if (FlatDirection(guideDestination).magnitude <= 0.35f)
        {
            StopMoving();
            activity = Activity.Arrived;
            SetChoicesVisible(true);
            FaceDirection(userDirection);
            ShowMessage("Here we are. I'll stay here.");
            return;
        }

        // 拒绝不完整路线，避免目的地不可达时仍然假装带路。
        if (!agent.CalculatePath(guideDestination, route) ||
            route.status != NavMeshPathStatus.PathComplete)
        {
            StopMoving();
            activity = Activity.Following;
            waitingForUser = false;
            SetChoicesVisible(true);
            ShowMessage("The route is blocked. Let's stay nearby.");
            return;
        }

        // 等到用户后，先原地转回路线方向，再恢复行走。
        if (turningBackToRoute)
        {
            StopMoving();
            Vector3[] corners = route.corners;
            Vector3 nextCorner = corners.Length > 1
                ? corners[1] : guideDestination;
            Vector3 routeDirection = FlatDirection(nextCorner);
            FaceDirection(routeDirection);
            if (routeDirection.sqrMagnitude > 0.001f &&
                Vector3.Angle(transform.forward, routeDirection) > 5f)
                return;
            turningBackToRoute = false;
        }

        agent.speed = moveSpeed;
        agent.stoppingDistance = 0.2f;
        agent.isStopped = false;
        agent.SetPath(route);
        FaceDirection(agent.steeringTarget - transform.position);
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.qKey.wasPressedThisFrame)
            {
                interactionPaused = true;
                SetChoicesVisible(false);
                ShowMessage("Okay, I'll wait here.");
            }
            else if (keyboard.eKey.wasPressedThisFrame)
            {
                interactionPaused = false;
                SetChoicesVisible(invitationOffered);
                if (activity == Activity.Choosing)
                {
                    SetChoicesVisible(true);
                    ShowMessage(reconsidering
                        ? "Would you like to change our plans?"
                        : "Would you like to come over here with me?", true);
                }
                else if (activity == Activity.Guiding && waitingForUser)
                {
                    ShowMessage("I'll wait for you.", true);
                }
                else
                {
                    ShowMessage("Okay, let's continue.");
                }
            }
        }

        if (!CanNavigate())
            return;
        if (interactionPaused || userTarget == null)
        {
            StopMoving();
            return;
        }

        Vector3 direction = FlatDirection(userTarget.position);
        float distance = direction.magnitude;

        // 先完成回看，再重新显示问题与选项。
        if (activity == Activity.TurningToUser)
        {
            StopMoving();
            FaceDirection(direction);
            if (direction.sqrMagnitude < 0.001f ||
                Vector3.Angle(transform.forward, direction) <= 5f)
            {
                activity = Activity.Choosing;
                ShowMessage("Would you like to change our plans?", true);
                SetChoicesVisible(true);
            }
            return;
        }

        if (activity == Activity.Guiding)
        {
            UpdateGuiding(direction);
            return;
        }
        if (activity == Activity.Arrived)
        {
            StopMoving();
            FaceDirection(direction);
            return;
        }
        if (activity == Activity.Choosing)
        {
            StopMoving();
            FaceDirection(direction);
            return;
        }

        if (distance > noticeDistance)
        {
            StopMoving();
            return;
        }

        // 首次发现：检查遮挡、转向、播放问候。
        if (!hasGreeted)
        {
            StopMoving();
            if (IsBlocked())
                return;
            FaceDirection(direction);
            if (direction.sqrMagnitude > 0.001f &&
                Vector3.Angle(transform.forward, direction) > 5f)
                return;

            ShowMessage("Welcome back!");
            if (characterAnimator != null)
                characterAnimator.SetTrigger("Greet");
            hasGreeted = true;
            greetingEndTime = Time.time + greetingHoldDuration;
            return;
        }

        if (Time.time < greetingEndTime)
        {
            StopMoving();
            FaceDirection(direction);
            return;
        }

        // 舒适距离过近时，保留原来的退让行为。
        if (distance < personalDistance)
        {
            FaceDirection(direction);
            Vector3 away = distance > 0.001f
                ? -direction.normalized : -transform.forward;
            Vector3 groundPosition = transform.position - Vector3.up * agent.baseOffset;
            Vector3 retreatTarget = groundPosition + away * (personalDistance - distance);
            NavMeshHit retreatHit;
            NavMeshHit blockedHit;
            if (NavMesh.SamplePosition(retreatTarget, out retreatHit, 0.5f, agent.areaMask) &&
                !agent.Raycast(retreatHit.position, out blockedHit))
            {
                agent.speed = retreatSpeed;
                agent.stoppingDistance = 0f;
                agent.isStopped = false;
                agent.SetDestination(retreatHit.position);
            }
            else StopMoving();
            return;
        }

        // 走近、面对用户后邀请一次；隔墙时不提出邀请。
        if (!invitationOffered && meetingSpot != null &&
            acceptButton != null && declineButton != null &&
            distance <= stoppingDistance + 0.15f && !IsBlocked())
        {
            StopMoving();
            FaceDirection(direction);
            if (direction.sqrMagnitude < 0.001f ||
                Vector3.Angle(transform.forward, direction) <= 5f)
                OfferInvitation();
            return;
        }

        NavMeshHit targetHit;
        if (!FindGround(userTarget.position, out targetHit))
        {
            StopMoving();
            FaceDirection(direction);
            return;
        }

        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.isStopped = false;
        agent.SetDestination(targetHit.position);
        if (agent.desiredVelocity.sqrMagnitude > 0.01f)
            FaceDirection(agent.desiredVelocity);
        else FaceDirection(direction);
    }
}
