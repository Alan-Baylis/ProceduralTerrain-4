using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCameraControl : MonoBehaviour
{
    [SerializeField] private float m_xSpeed = 10f;
    [SerializeField] private float m_ySpeed = 10f;
    [SerializeField] private int m_xDistFromScreenBorder = 50;
    [SerializeField] private int m_yDistFromScreenBorder = 10;
    [SerializeField] private float m_zoomSpeed = 10f;
    /*[SerializeField] private float m_distMin = 1;
    [SerializeField] private float m_distMax = 5;*/

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        MoveCamera();
    }

    private void FixedUpdate()
    {
        ZoomCamera();
    }

    void MoveCamera()
    {
        float _xSpeed = m_xSpeed * Time.deltaTime;
        float _ySpeed = m_ySpeed * Time.deltaTime;
        Vector3 _mousePos = Input.mousePosition;

        SCameraModel.CameraMovement(_xSpeed, _ySpeed, transform, _mousePos,
            m_xDistFromScreenBorder, m_yDistFromScreenBorder);
    }

    void ZoomCamera()
    {
        float _zoomSpeed = m_zoomSpeed * Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime;
        Vector3 _mousePos = Input.mousePosition;

        SCameraModel.CameraZoom(_zoomSpeed, transform, _mousePos/*, m_distMin, m_distMax*/);
    }
}
