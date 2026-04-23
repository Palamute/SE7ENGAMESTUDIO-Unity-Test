using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BallController : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public GameObject kickButtonUI;
    public CameraController cameraController;
    public GameObject confettiPrefab;
    public Transform[] goals;

    [Header("Settings")]
    public float detectionRadius = 3f;
    public float kickForce = 15f;
    public float cameraReturnDelay = 2f;

    private Rigidbody rb;
    private bool isKicked = false;
    private bool hasScored = false;

    public static List<BallController> allBalls = new List<BallController>();
    private static BallController currentNearBall = null;

    void Awake()
    {
        allBalls.Add(this);
        if (kickButtonUI != null) kickButtonUI.SetActive(false);
    }

    void OnDestroy()
    {
        allBalls.Remove(this);
        if (currentNearBall == this)
        {
            currentNearBall = null;
            foreach (var ball in allBalls)
            {
                if (ball != null && ball.kickButtonUI != null)
                {
                    ball.kickButtonUI.SetActive(false);
                    break;
                }
            }
        }
    }

    void Start() => rb = GetComponent<Rigidbody>();

    void Update()
    {
        if (isKicked || hasScored) return;
        if (playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        bool isNear = dist <= detectionRadius;

        if (isNear && currentNearBall == null)
        {
            currentNearBall = this;
            if (kickButtonUI != null) kickButtonUI.SetActive(true);
        }
        else if (!isNear && currentNearBall == this)
        {
            currentNearBall = null;
            if (kickButtonUI != null) kickButtonUI.SetActive(false);
        }
    }

    public void OnKickButton()
    {
        if (currentNearBall != this) return;
        Kick(GetNearestGoal(playerTransform.position));
    }

    public void OnAutoKickButton()
    {
        BallController farthest = null;
        float maxDist = -1f;

        for (int i = allBalls.Count - 1; i >= 0; i--)
        {
            var ball = allBalls[i];
            if (ball == null) { allBalls.RemoveAt(i); continue; }
            if (ball.isKicked || ball.hasScored) continue;
            float d = Vector3.Distance(ball.transform.position, playerTransform.position);
            if (d > maxDist) { maxDist = d; farthest = ball; }
        }

        if (farthest != null)
            farthest.Kick(farthest.GetNearestGoal(farthest.transform.position));
    }

    void Kick(Transform goal)
    {
        if (goal == null || isKicked) return;
        isKicked = true;

        if (currentNearBall == this)
        {
            currentNearBall = null;
            if (kickButtonUI != null) kickButtonUI.SetActive(false);
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce((goal.position - transform.position).normalized * kickForce, ForceMode.Impulse);
        cameraController?.SwitchToBall(transform);
    }

    Transform GetNearestGoal(Vector3 from)
    {
        Transform nearest = goals[0];
        float minDist = float.MaxValue;
        foreach (var g in goals)
        {
            float d = Vector3.Distance(from, g.position);
            if (d < minDist) { minDist = d; nearest = g; }
        }
        return nearest;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Goal") || hasScored) return;
        hasScored = true;
        StartCoroutine(GoalRoutine());
    }

    IEnumerator GoalRoutine()
    {
        if (confettiPrefab != null)
        {
            var confetti = Instantiate(confettiPrefab, transform.position, Quaternion.identity);
            var ps = confetti.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            Destroy(confetti, ps != null ? ps.main.duration + 1f : 3f);
        }

        GetComponentInChildren<MeshRenderer>().enabled = false;
        rb.isKinematic = true;

        yield return new WaitForSeconds(cameraReturnDelay);
        cameraController?.SwitchToPlayer();
        Destroy(gameObject);
    }
}