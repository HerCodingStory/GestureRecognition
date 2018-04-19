using Accord.Statistics;
using Leap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class FeatureVectorPreprocessor
{
    /*
     * This function creates a vector containing the distance between the palm of the hand 
     * and tip of each finger. It also constructs the distances between adjacent fingers,
     * how many fingers are extended, the pinch strength and the grab strength of the hand,
     * and the sphere radius of the hand.
     */
    public FeatureVector createFeatureVector(Frame frame)
    {
        List<double> featureVectorList = new List<double>();

        calculatePalmToFingerDistances(featureVectorList, frame);

        calculateAdjacentFingerDistances(featureVectorList, frame);

        calculateHandToFingerDistances(featureVectorList, frame);

        //double [] centered = Tools.Center(featureVectorList.ToArray<double>());
        //List<double> standardizedVectorList = Tools.Standardize(centered).ToList();

        getMiscellaneousFeatures(featureVectorList, frame);

        FeatureVector featureVector = constructFeatureVector(featureVectorList);

        featureVector.NumExtendedFingers = getNumExtendedFingers(frame);

        return featureVector;
    }

    private FeatureVector constructFeatureVector(List<double> standardizedVectorList)
    {
        FeatureVector featureVector = new FeatureVector();

        Type type = featureVector.GetType();
        PropertyInfo[] properties = type.GetProperties();

        int propertyIndex = 0;

        /*
         * The minus 4 is so we skip the last 4 properties
         * in the feature vector class. Those get assigned 
         * when the user assigns a name to the gesture. 
         */

        for (int i = 0; i < properties.Length - 4; i++)
        {
            PropertyInfo property = properties[i];
            property.SetValue(featureVector, standardizedVectorList[propertyIndex], null);
            propertyIndex++;
        }

        return featureVector;
    }

    private Matrix createHandTransform(Hand hand)
    {
        Vector handXBasis = hand.PalmNormal.Cross(hand.Direction).Normalized;
        Vector handYBasis = -hand.PalmNormal;
        Vector handZBasis = -hand.Direction;
        Vector handOrigin = hand.PalmPosition;
        Matrix handTransform = new Matrix(handXBasis, handYBasis, handZBasis, handOrigin);
        return handTransform.RigidInverse();
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
            Matrix handTransform = createHandTransform(hand);

            foreach (Finger finger in hand.Fingers)
            {
                Vector transformedTipPosition = handTransform.TransformPoint(finger.TipPosition);
                Vector transformedPalmPosition = handTransform.TransformPoint(hand.PalmPosition);

                featureVectorList.Add(transformedTipPosition.DistanceTo(transformedPalmPosition) / hand.SphereRadius);
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
            Matrix handTransform = createHandTransform(hand);

            for (int i = hand.Fingers.Count - 1; i > 0; i--)
            {
                Vector currentFinger = hand.Fingers[i].TipPosition - hand.PalmPosition;
                Vector previousFinger = hand.Fingers[i - 1].TipPosition - hand.PalmPosition;

                Vector transformedCurrTipPosition = handTransform.TransformPoint(currentFinger);
                Vector transformedPrevTipPosition = handTransform.TransformPoint(previousFinger);

                featureVectorList.Add(transformedCurrTipPosition.DistanceTo(transformedPrevTipPosition) / hand.SphereRadius);
            }
        }
    }

    /*
     * featureVectorList[9] = ThumbToHandNormal Distance
     * featureVectorList[10] = IndexToHandNormal Distance
     * featureVectorList[11] = MiddleToHandNormal Distance
     * featureVectorList[12] = RingToHandNormal Distance
     * featureVectorList[13] = PinkyToHandNormal Distance
     */
    private void calculateHandToFingerDistances(List<double> featureVectorList, Frame frame)
    {
        foreach (Hand hand in frame.Hands)
        {
            foreach (Finger finger in hand.Fingers)
            {
                double angle = getFingerAngle(hand, finger);

                double curFingerMag = finger.Direction.Magnitude;
                double handNormMag = hand.Direction.Magnitude;

                double distance = Math.Pow(curFingerMag, 2) + Math.Pow(handNormMag, 2) +
                                  - 2 * curFingerMag * handNormMag * angle;

                featureVectorList.Add(distance / hand.SphereRadius);
            }
        }
    }

    private double getFingerAngle(Hand hand, Finger finger)
    {
        Vector handZDirection = -hand.Basis.zBasis;
        Vector fingerTip = -finger.Bone(Bone.BoneType.TYPE_DISTAL).Basis.zBasis;

        double rawangle = handZDirection.AngleTo(fingerTip) * 180 / Math.PI;

        Vector crossBones = handZDirection.Cross(fingerTip);
        Vector boneXBasis = finger.Bone(Bone.BoneType.TYPE_METACARPAL).Basis.xBasis;

        if (hand.IsLeft)
            boneXBasis = -boneXBasis;

        int sign = (crossBones.Dot(boneXBasis) >= 0) ? 1 : -1;

        return sign * rawangle;
    }

    /*
     * featureVectorList[14] = RadiusSphere
     * featureVectorList[15] = PinchStrength
     * featureVectorList[16] = GrabStrength
     */
    private void getMiscellaneousFeatures(List<double> featureVectorList, Frame frame)
    {
        foreach (Hand hand in frame.Hands)
        {
            featureVectorList.Add(hand.SphereRadius);
            featureVectorList.Add(hand.PinchStrength);
            featureVectorList.Add(hand.GrabStrength);
        }
    }

    /*
    * featureVectorList[17] = NumExtendedFingers
    */
    private int getNumExtendedFingers(Frame frame)
    {
        return frame.Fingers.Extended().Count;
    }
}
