using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarInputConverter : MonoBehaviour
{
    public Transform MainAvatarTransform;
    public Transform AvatarHead;
    public Transform AvatarBody;
    public Transform OVRHead;
    public Vector3 HeadPositionOffset;
    public Vector3 BodyPositionOffset;

    // Update is called once per frame
    void Update()
    {
        //Head and Body synch
        MainAvatarTransform.position = Vector3.Lerp(MainAvatarTransform.position, OVRHead.position + HeadPositionOffset, 0.5f);        
        AvatarHead.rotation = Quaternion.Lerp(AvatarHead.rotation, OVRHead.rotation, 0.5f);
        AvatarBody.rotation = Quaternion.Lerp(AvatarBody.rotation, Quaternion.Euler(new Vector3(0, AvatarHead.rotation.eulerAngles.y, 0)), 0.05f);
    }
}
