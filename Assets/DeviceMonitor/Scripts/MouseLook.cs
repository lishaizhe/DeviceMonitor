using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseLook : MonoBehaviour
{
    public enum RotationAxes
    {
        MouseXandY = 0,//水平，垂直
        MouseX = 1,//水平
        MouseY = 2//垂直
    }
    public RotationAxes axes = RotationAxes.MouseXandY;
    public float sensitivityHor = 9.0f;
    public float sensitivityVert = 9.0f;

    public float minimumVert = -45.0f;
    public float maximumVert = 45.0f;

    private float _rotationX = 0;
    private MainView m_mainView;

    private void Start()
    {
        // 避免物理仿真影响
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (rigidbody != null) rigidbody.freezeRotation = true;
    }

    public void SetMainViewHandler(MainView mainView)
    {
        m_mainView = mainView;
    }


    void Update()
    {
        if (!m_mainView.IsMouseEnable())
            return;
        //滚轮放大缩小
        Vector2 scrollDelta = Input.mouseScrollDelta;
        if (!scrollDelta.Equals(Vector2.zero))
        {
            var pos = transform.position;
            if (pos.z + scrollDelta.y > m_mainView.C_MaxZ || pos.z + scrollDelta.y < m_mainView.C_MinZ)
                return;
            transform.position += new Vector3(0, 0, scrollDelta.y);
        }

        if (Input.GetMouseButton(1))
        {
            if (axes == RotationAxes.MouseX)
            {
                //
                transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityHor, 0);
            }
            else if (axes == RotationAxes.MouseY)
            {
                _rotationX -= Input.GetAxis("Mouse Y") * sensitivityVert;
                _rotationX = Mathf.Clamp(_rotationX, minimumVert, maximumVert);

                float rotationY = transform.localEulerAngles.y;

                transform.localEulerAngles = new Vector3(_rotationX, rotationY, 0);
            }
            else
            {
                _rotationX -= Input.GetAxis("Mouse Y") * sensitivityVert;
                _rotationX = Mathf.Clamp(_rotationX, minimumVert, maximumVert);

                float detal = Input.GetAxis("Mouse X") * sensitivityHor;
                float rotationY = transform.localEulerAngles.y + detal;

                transform.localEulerAngles = new Vector3(_rotationX, rotationY, 0);
            }
        }
    }

}
