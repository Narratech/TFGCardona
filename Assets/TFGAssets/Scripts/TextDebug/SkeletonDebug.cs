using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// Esta clase se dedica a debugear por pantalla el estado de los huesos de las manos para Unity.
/// Crear un 3D Object-> 3D Text en escena y acoplarle este script
/// Arrastrar a los huecos del esqueleto los skeletons de la mano OVRLeftHand y OVRRightHand (Oculus Integration)
/// Arrastrar a los huecos de TextMeshPro los objetos de texto 3D generados en escena.
/// </summary>
public class SkeletonDebug : MonoBehaviour
{
    // Esqueletos de ambas manos
    [SerializeField]
    private OVRSkeleton RHskeleton;
    [SerializeField]
    private OVRSkeleton LHskeleton;

    // Texto 3D donde se muestra la info
    [SerializeField]
    private TextMeshPro RHDebugText;
    [SerializeField]
    private TextMeshPro LHDebugText;

    // Huesos de las manos
    private List<OVRBone> RHfingerBones;
    private List<OVRBone> LHfingerBones;

    // Start is called before the first frame update
    void Start()
    {
        RHfingerBones = new List<OVRBone>(RHskeleton.Bones);
        LHfingerBones = new List<OVRBone>(LHskeleton.Bones);
    }

    // Update is called once per frame
    void Update()
    {
        string RHtext = "Bone Debugger\n------------------------\nRIGHT HAND\n";
        string LHtext = "Bone Debugger\n------------------------\nLEFT HAND\n";
        //string rhheader = "rh";
        //int index = 0;
        //string boneHeader = "";
        
        //RH Information
        foreach (OVRBone rhbone in RHfingerBones)
        {
            //boneHeader = rhheader + index.ToString();
            RHtext = RHtext + rhbone.Id + " P: ";
            RHtext = RHtext + RHskeleton.transform.InverseTransformPoint(rhbone.Transform.position) + " - R: " + rhbone.Transform.rotation;
            RHtext = RHtext + "\n";
            //index++;
        }


        //LH Information
        //string lhheader = "lh";
        //index = 0;
        //boneHeader = "";
        foreach (OVRBone lhbone in LHfingerBones)
        {
            //boneHeader = lhheader + index.ToString();
            LHtext = LHtext + lhbone.Id + " P: ";
            LHtext = LHtext + LHskeleton.transform.InverseTransformPoint(lhbone.Transform.position) + " - R: " + lhbone.Transform.rotation;
            LHtext = LHtext + "\n";
            //index++;
        }

        RHDebugText.text = RHtext;
        LHDebugText.text = LHtext;
    }
}
