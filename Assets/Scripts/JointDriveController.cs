using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class JointDriveController : MonoBehaviour
{
    [Header("Joint Drive Settings")] [Space(10)]
    public float maxJointSpring;

    public float jointDampen;
    public float maxJointForceLimit;
    float facingDot;

    [HideInInspector] public Dictionary<Transform, BodyPart> bodyPartsDict = new Dictionary<Transform, BodyPart>();

    [HideInInspector] public List<BodyPart> bodyPartsList = new List<BodyPart>();

    /// <summary>
    /// Create BodyPart object and add it to dictionary.
    /// </summary>
    public void SetupBodyPart(Transform t)
    {
        BodyPart bp = new BodyPart
        {
            rb = t.GetComponent<Rigidbody>(),
            joint = t.GetComponent<ConfigurableJoint>(),
            startingPos = t.position,
            startingRot = t.rotation
        };
        bp.rb.maxAngularVelocity = 100;

        // Add & setup the ground contact script
        bp.groundContact = t.GetComponent<GroundContact>();
        if (!bp.groundContact)
        {
            bp.groundContact = t.gameObject.AddComponent<GroundContact>();
            bp.groundContact.agent = gameObject.GetComponent<Agent>();
        }
        else
        {
            bp.groundContact.agent = gameObject.GetComponent<Agent>();
        }

        // Add & setup the target contact script
        bp.targetContact = t.GetComponent<TargetContact>();
        if (!bp.targetContact)
        {
            bp.targetContact = t.gameObject.AddComponent<TargetContact>();
        }

        bp.thisJDController = this;
        bodyPartsDict.Add(t, bp);
        bodyPartsList.Add(bp);
    }

    public void GetCurrentJointForces()
    {
        foreach (var bodyPart in bodyPartsDict.Values)
        {
            if (bodyPart.joint)
            {
                bodyPart.currentJointForce = bodyPart.joint.currentForce;
                bodyPart.currentJointForceSqrMag = bodyPart.joint.currentForce.magnitude;
                bodyPart.currentJointTorque = bodyPart.joint.currentTorque;
                bodyPart.currentJointTorqueSqrMag = bodyPart.joint.currentTorque.magnitude;
                if (Application.isEditor)
                {
                    if (bodyPart.jointForceCurve.length > 1000)
                    {
                        bodyPart.jointForceCurve = new AnimationCurve();
                    }

                    if (bodyPart.jointTorqueCurve.length > 1000)
                    {
                        bodyPart.jointTorqueCurve = new AnimationCurve();
                    }

                    bodyPart.jointForceCurve.AddKey(Time.time, bodyPart.currentJointForceSqrMag);
                    bodyPart.jointTorqueCurve.AddKey(Time.time, bodyPart.currentJointTorqueSqrMag);
                }
            }
        }
    }
}
