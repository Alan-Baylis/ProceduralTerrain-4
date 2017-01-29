using UnityEngine;

public class TestAnimator : MonoBehaviour
{
    private Animator m_character;
    private SPatrol m_patrol;

	// Use this for initialization
	void Start () {
        m_character = GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();
        m_patrol = GameObject.FindGameObjectWithTag("Player").GetComponent<SPatrol>();
    }
	
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("trigger"))
        {
            m_character.SetBool("WakeUp", true);
            m_patrol.StartPatrol = true;
        }
    }
}
