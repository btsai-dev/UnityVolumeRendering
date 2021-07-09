using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BetterGrabber : XRGrabInteractable
{
    private Vector3 interactorPos = Vector3.zero;
    private Quaternion interactorRot = Quaternion.identity;

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        base.OnSelectEntering(args);
        
        if (args.interactor is XRRayInteractor) 
        {
            interactorPos = args.interactor.attachTransform.localPosition;
            interactorRot = args.interactor.attachTransform.localRotation;

            bool hasAttach = attachTransform != null;
            args.interactor.attachTransform.position = hasAttach ? attachTransform.position : transform.position;
            args.interactor.attachTransform.rotation = hasAttach ? attachTransform.rotation : transform.rotation;
        }
    }
    
    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);
        
        if(args.interactor is XRRayInteractor)
        {
            interactorPos = Vector3.zero;
            interactorRot = Quaternion.identity;
        }
    } 
}
