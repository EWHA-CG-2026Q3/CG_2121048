using UnityEngine;


public class MoonlitViewModes : MonoBehaviour
{
    public Camera detailCamera;
    public Camera wideCamera;
    public Camera walkCamera;
    public Camera gardenCamera;
    public MonoBehaviour sceneWalker;

    void Start() { SetMode(4); }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) SetMode(1);
        if (Input.GetKeyDown(KeyCode.F2)) SetMode(2);
        if (Input.GetKeyDown(KeyCode.F3)) SetMode(3);
        if (Input.GetKeyDown(KeyCode.F4)) SetMode(4);
    }
    void SetMode(int mode)
    {
        SetCamera(detailCamera, mode == 1);
        SetCamera(wideCamera, mode == 3);
        SetCamera(walkCamera, mode == 2);
        SetCamera(gardenCamera, mode == 4);
        if (sceneWalker != null) sceneWalker.enabled = (mode == 2);
        if (mode != 2)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    void SetCamera(Camera camera, bool active)
    {
        if (camera == null) return;
        camera.enabled = active;
        var audio = camera.GetComponent<AudioListener>();
        if (audio != null) audio.enabled = active;
    }
}
