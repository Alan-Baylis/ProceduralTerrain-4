using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SAnimator : MonoBehaviour {
    private NavMeshAgent m_agent;
    private Transform m_finalTarget;
    [SerializeField] private float m_deltaTime = 1.5f;
    [SerializeField] private float m_deltaTimeWalkToRun = 0.005f;
    [SerializeField] private float m_deltaBlend = 0.005f;
    [SerializeField] private float m_maxSpeed = 1.0f;
    /*[SerializeField] private float m_deltaBlend = 0.005f;
    [SerializeField] private float m_deltaTime = 0.005f;
    private Animator m_animator;*/

    // Use this for initialization
    void Start () {
        m_agent = GetComponent<NavMeshAgent>();
        m_finalTarget = GameObject.FindGameObjectWithTag("targetFinal").transform;

    }

    // Update is called once per frame
    //void Update () {
		/*if(ActiveFunction)
        {

        }*/

    private IEnumerator IncrementSpeed()
    {
        //float _cpt = 0f;
        while (m_agent.speed < m_maxSpeed)
        {
            //_cpt += m_deltaBlend;
            m_agent.speed += m_deltaBlend;
            yield return new WaitForSeconds(m_deltaTimeWalkToRun);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("target"))
        {
            //yield return new WaitForSeconds(m_deltaTime);
            m_agent.destination = m_finalTarget.position;
            StartCoroutine(IncrementSpeed());
            
        }
}
}

