using Leap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Leap.Unity;

public class FeatureVectorPreprocessor
{
    /*
     * This function creates a vector containing the distance between the palm of the hand 
     * and tip of each finger. It also constructs the distances between adjacent fingers.
     * All the distances are normalized based on the max and min distances from the fingertip
     * and the max and min distances between adjacent fingers.
     */
    public FeatureVector createFeatureVector(Frame frame)
    {
        List<double> palmToFingerFeatureVector = new List<double>();
        calculatePalmToFingerDistances(palmToFingerFeatureVector, frame);
        //List<double> palmToFingerNormalizedFeatureVector = normalizeFeatureVector(palmToFingerFeatureVector);

        List<double> adjacentFingerFeatureVector = new List<double>();
        calculateAdjacentFingerDistances(adjacentFingerFeatureVector, frame);
        //List<double> adjacentFingerNormalizedFeatureVector = normalizeFeatureVector(adjacentFingerFeatureVector);

        List<double> completeFeatureVector = new List<double>();

        foreach(double feature in palmToFingerFeatureVector)
        {
            completeFeatureVector.Add(feature);
        }

        foreach (double feature in adjacentFingerFeatureVector)
        {
            completeFeatureVector.Add(feature);
        }

        calculateFingerBending(completeFeatureVector, frame);

        return constructFeatureVectorFromProperties(completeFeatureVector);
    }

    private FeatureVector constructFeatureVectorFromProperties(List<double> standardizedVectorList)
    {
        FeatureVector featureVector = new FeatureVector();

        Type type = featureVector.GetType();
        PropertyInfo[] properties = type.GetProperties();

        /*
         * The minus 3 is so we skip the last 3 properties
         * in the feature vector class. Those get assigned 
         * when the user assigns a name to the gesture. 
         */

        for (int propertyIndex = 0; propertyIndex < properties.Length - 3; propertyIndex++)
        {
            PropertyInfo property = properties[propertyIndex];
            property.SetValue(featureVector, standardizedVectorList[propertyIndex], null);
        }

        return featureVector;
    }

    /*
     * featureVectorList[0] = PalmToThumb Distance
     * featureVectorList[1] = PalmToIndex Distance
     * featureVectorList[2] = PalmToMiddle Distance
     * featureVectorList[3] = PalmToRing Distance
     * featureVectorList[4] = PalmToPinky Distance
     */
    private void calculatePalmToFingerDistances(List<double> featureVectorList, Frame frame)
    {
        foreach (Hand hand in frame.Hands)
        {
            Vector palmPosition = new Vector(hand.PalmPosition.x, hand.PalmPosition.y, 0);

            foreach (Finger finger in hand.Fingers)
            {
                Vector tipPosition = new Vector(finger.TipPosition.x, finger.TipPosition.y, 0);
                featureVectorList.Add(tipPosition.DistanceTo(palmPosition));
            }
        }
    }

    /* 
     * featureVectorList[5] = PinkyToRing Distance
     * featureVectorList[6] = RingToMiddle Distance
     * featureVectorList[7] = MiddleToIndex Distance
     * featureVectorList[8] = IndexToThumb Distance
     */

    private void calculateAdjacentFingerDistances(List<double> featureVectorList, Frame frame)
    {
        foreach (Hand hand in frame.Hands)
        {
            for (int i = hand.Fingers.Count - 1; i > 0; i--)
            {
                Vector currentFinger = hand.Fingers[i].TipPosition;
                currentFinger.z = 0;

                Vector previousFinger = hand.Fingers[i - 1].TipPosition;
                previousFinger.z = 0;

                featureVectorList.Add(currentFinger.DistanceTo(previousFinger));
            }
        }
    }

    private List<double> normalizeFeatureVector(List<double> featureVector)
    {
        List<double> normalizedFeatureVector = new List<double>();

        double minDistance = featureVector.Min();
        double maxDistance = featureVector.Max() - minDistance;

        foreach (float distance in featureVector)
        {
            normalizedFeatureVector.Add(normalizeDistance(distance, minDistance, maxDistance));
        }

        return normalizedFeatureVector;
    }

    private double normalizeDistance(double distance, double minDistance, double maxDistance)
    {
        return Math.Round((distance - minDistance) / maxDistance, 5);
    }

    private void calculateFingerBending(List<double> featureVectorList, Frame frame)
    {
        int boneIndex = 1;

        foreach (Hand hand in frame.Hands)
        {
            foreach (Finger finger in hand.Fingers)
            {
                Vector3 boneDir1 = finger.bones[boneIndex].Direction.ToVector3();
                Vector3 boneDir2 = finger.bones[boneIndex + 1].Direction.ToVector3();

                featureVectorList.Add(Vector3.Angle(boneDir1, boneDir2));
            }
        }
    }
}
