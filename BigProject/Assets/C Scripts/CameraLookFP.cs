using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLookFP : MonoBehaviour
{

    public Transform playerReference;
    public LayerMask wallMask;
    public float mouseSensitivity;
    public float cameraTilt;
    public float maxCameraTilt;
    float xRotate = 0f;


    private bool iswallLeft;
    private bool iswallRight;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        CheckWall();
        
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotate = xRotate-mouseY;
        xRotate = Mathf.Clamp(xRotate,-90f,90f);
        transform.localRotation = Quaternion.Euler(xRotate,0f,0f);
        playerReference.Rotate(Vector3.up * mouseX);

        transform.localRotation = Quaternion.Euler(xRotate, 0f, cameraTilt);

        if(Mathf.Abs(cameraTilt) < maxCameraTilt && iswallRight)
            {
                cameraTilt += Time.deltaTime * maxCameraTilt * 2;
            }

            if(Mathf.Abs(cameraTilt) < maxCameraTilt && iswallLeft)
            {
                cameraTilt -= Time.deltaTime * maxCameraTilt * 2;
            } 

            if( cameraTilt > 0 && !iswallRight && !iswallLeft)
            {
                cameraTilt -= Time.deltaTime * maxCameraTilt * 2;
            }

            if( cameraTilt < 0 && !iswallRight && !iswallLeft)
            {
                cameraTilt += Time.deltaTime * maxCameraTilt * 2;
            } 
    }

    void CheckWall()
    {
        iswallRight = Physics.Raycast(transform.position, playerReference.right, 1f, wallMask);
        iswallLeft = Physics.Raycast(transform.position, -playerReference.right, 1f, wallMask);   
    }
}
