using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;
    public Transform camHolder;

    float xRotation;
    float yRotation;

    private float tilt;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Myszka
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Obrót kamery i orientacji
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, tilt);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void DoTilt(float zTilt)
    {
        tilt = Mathf.Lerp(tilt, zTilt, Time.deltaTime * 10f);
    }
}