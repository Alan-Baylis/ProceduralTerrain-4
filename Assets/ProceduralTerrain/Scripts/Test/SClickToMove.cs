using UnityEngine;
using UnityEngine.AI;

public class SClickToMove : MonoBehaviour
{
    private NavMeshAgent m_agent;
    
    // Use this for initialization
    private void Start()
    {
        m_agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit _hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hit, 100))
            {
                m_agent.destination = _hit.point;
            }
        }
    }
}
