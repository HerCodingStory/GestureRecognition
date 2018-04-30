/*************************************************************************\
*                           USENS CONFIDENTIAL                            *
* _______________________________________________________________________ *
*                                                                         *
* [2015] - [2017] USENS Incorporated                                      *
* All Rights Reserved.                                                    *
*                                                                         *
* NOTICE:  All information contained herein is, and remains               *
* the property of uSens Incorporated and its suppliers,                   *
* if any.  The intellectual and technical concepts contained              *
* herein are proprietary to uSens Incorporated                            *
* and its suppliers and may be covered by U.S. and Foreign Patents,       *
* patents in process, and are protected by trade secret or copyright law. *
* Dissemination of this information or reproduction of this material      *
* is strictly forbidden unless prior written permission is obtained       *
* from uSens Incorporated.                                                *
*                                                                         *
\*************************************************************************/

using UnityEngine;
using System.Collections;
using Fingo;

/// <summary>
/// The gesture manager. Supported gestures can trigger corresponding events.
/// </summary>
public class GestureManager : MonoBehaviour 
{
    /// <summary>
    /// The hand type of the tracked hand.
    /// </summary>
    public HandType handType;
    private Hand hand;

	private GestureName currentFrameGestureName;
	private GestureName lastFrameGestureName;

    /// <summary>
    /// Broadcast the gesture event if a gesture type maintains longer than the threshold.
    /// </summary>
    public float gestureMaintainingThreshold = 0.4f;
    private float gestureMaintainingCounter;
  
    private bool isGestureMaintaining;

    private Vector3 currentFrameHandPos;
    private Vector3 lastFrameHandPos;
    /// <summary>
    /// The gesture event.
    /// </summary>
    public delegate void GestureEventHandler(HandType handType, GestureName gestureType);
    public static event GestureEventHandler GestureEvent;


    void Update()
    {
        hand = FingoMain.Instance.GetHand(handType);

        UpdateGesture();
        BroadcastIfGestureMaintains();
    }

    void UpdateGesture()
    {
        currentFrameGestureName = hand.IsDetected() ? hand.GetGestureName() : GestureName.None;

        if (currentFrameGestureName == lastFrameGestureName)
        {
            gestureMaintainingCounter += Time.deltaTime;
        }
        else if (currentFrameGestureName != lastFrameGestureName)
        {
            gestureMaintainingCounter = 0f;
            isGestureMaintaining = false;
        }
        lastFrameGestureName = currentFrameGestureName;
    }

    void BroadcastIfGestureMaintains()
    {
        if (gestureMaintainingCounter > gestureMaintainingThreshold && !isGestureMaintaining)
        {
            isGestureMaintaining = true;

            if (GestureEvent != null)
            {
                GestureEvent(handType, currentFrameGestureName);
            }
        }
    }
}
