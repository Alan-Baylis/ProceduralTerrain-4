using UnityEngine;

public class SPatrol : MonoBehaviour
{
    [SerializeField] private Transform[] points;
    [SerializeField] private float m_remainingDistance = 0.5f;

    private int m_destPoint = 0;
    private UnityEngine.AI.NavMeshAgent m_agent;
    private bool m_startPatrol;

    public bool StartPatrol
    {
        get
        {
            return m_startPatrol;
        }

        set
        {
            m_startPatrol = value;
        }
    }

    void Start()
    {
        m_startPatrol = false;
        m_agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        // Disabling auto-braking allows for continuous movement
        // between points (ie, the agent doesn't slow down as it
        // approaches a destination point).
        m_agent.autoBraking = false;
    }


    void GotoNextPoint()
    {
        // Returns if no points have been set up
        if (points.Length == 0)
            return;

        // Set the agent to go to the currently selected destination.
        m_agent.destination = points[m_destPoint].position;

        // Choose the next point in the array as the destination,
        // cycling to the start if necessary.
        m_destPoint = (m_destPoint + 1) % points.Length;
    }


    void Update()
    {
        // Choose the next destination point when the agent gets
        // close to the current one.
        if (m_startPatrol)
        {
            if (m_agent.remainingDistance < m_remainingDistance)
            {
                GotoNextPoint();
            }
        }
    }
}