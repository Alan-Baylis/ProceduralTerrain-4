using UnityEngine;

public static class SCameraModel
{
    public static void CameraMovement(float xSpeed, float ySpeed, Transform transform, Vector3 mousePos,
        int xDistFromScreenBorder, int yDistFromScreenBorder)
    {
        if (mousePos.x < xDistFromScreenBorder || mousePos.x > Screen.width - xDistFromScreenBorder)
        {
            if(mousePos.x < xDistFromScreenBorder)
            {
                transform.Translate(-xSpeed, 0, 0, Space.World);
            }
            if(mousePos.x > Screen.width - xDistFromScreenBorder)
            {
                transform.Translate(xSpeed, 0, 0, Space.World);
            }
        }

        if (mousePos.y < yDistFromScreenBorder || mousePos.y > Screen.height - yDistFromScreenBorder)
        {
            if (mousePos.y < yDistFromScreenBorder)
            {
                transform.Translate(0, 0, -ySpeed, Space.World);
            }
            if (mousePos.y > Screen.height - yDistFromScreenBorder)
            {
                transform.Translate(0, 0, ySpeed, Space.World);
            }
        } 
    }

    public static void CameraZoom(float speedZoom, Transform transform, Vector3 mousePos/*, float distMin, float distMax*/)
    {
        RaycastHit _hit;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(mousePos), out _hit, 100))
        {
            Vector3 _mouseOrigin = Camera.main.ScreenPointToRay(mousePos).origin;
            Vector3 _mouseToWorld = _hit.point - _mouseOrigin;
            //float _dist = Vector3.Distance(_hit.point, _mouseOrigin);

            //if (_dist <= distMax && _dist >= distMin)
            //{
                transform.Translate(_mouseToWorld * speedZoom);
            //}
        }
    }
}