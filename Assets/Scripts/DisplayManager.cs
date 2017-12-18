using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class DisplayManager : MonoBehaviour {

    [SerializeField] private List<Camera> _cams = new List<Camera>();
    [SerializeField] private VideoClip[] _vClips;
    [SerializeField] private int _frameVal;
    [SerializeField] private float _interval;
    [SerializeField] private string[] ContentName;

    public VideoPlayer _vp;

    public int FrameVal
    {
        get
        {
            return _frameVal;
        }

        set
        {
            _frameVal = value;
        }
    }

    void Start()
    {
        Screen.SetResolution(1920, 1080, true);
        float frustumHeight = 2.0f * 2.0f* Mathf.Tan(_cams[0].fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frustumWidth = frustumHeight * _cams[0].aspect;
        frustumHeight = frustumWidth / _cams[0].aspect;
        
        _cams[0].transform.position = new Vector3(-frustumWidth * 0.5f, frustumHeight * 0.5f, 0);
        _cams[1].transform.position = new Vector3(frustumWidth * 0.5f, frustumHeight * 0.5f, 0);
        _cams[2].transform.position = new Vector3(frustumWidth * 0.5f, -frustumHeight * 0.5f, 0);
        _cams[3].transform.position = new Vector3(-frustumWidth * 0.5f, -frustumHeight * 0.5f, 0);
        
        Display.onDisplaysUpdated += OnDisplaysUpdated;
        mapCameraToDisplay();
        
        _vp = GameObject.Find("moviePanel").GetComponent<VideoPlayer>();
        _vp.playOnAwake = false;
        _vp.isLooping = true;

        _vp.clip = _vClips[0];
    }
 
    void mapCameraToDisplay()
    {
    //Loop over Connected Displays
        for (int i = 0; i < Display.displays.Length; i++)
        {
            Debug.Log(Display.displays.Length);
            _cams[i].targetDisplay = i; //Set the Display in which to render the camera to
            Display.displays[i].Activate(); //Enable the display
        }
    }

    public void ChangeVideo(int index){
        _vp.clip = _vClips[index];
        _vp.Play();
    }

    void OnDisplaysUpdated()
    {
        Debug.Log("New Display Connected. Show Display Option Menu....");
    }
    

	void Update () {
    }
}
