using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Leap.Unity;
using Leap;

public class FreeMode : MonoBehaviour
{
    [SerializeField]
    private GestureClassifier classifier;

    [SerializeField]
    private GameObject leapController;

    private LeapServiceProvider leapControllerProvider;

    [SerializeField]
    private Text gestureSignText;

    public string GestureSign { get { return gestureSignText.text; } set {gestureSignText.text = value; } }

    public readonly static string coroutineName = "freeMode";

    private FeatureVectorPreprocessor featureVectorPreprocessor;

    private bool gestureVerified = false;

    public void startFreeMode()
    {
        leapControllerProvider = leapController.GetComponent<LeapServiceProvider>();
        featureVectorPreprocessor = new FeatureVectorPreprocessor();
        StartCoroutine(coroutineName);
    }

    private IEnumerator freeMode()
    {
        while (true)
        {
            Frame frame = leapControllerProvider.GetLeapController().Frame();

            if (classifier.ModelExists && frame.Hands.Count > 0)
            {
                if (!gestureVerified)
                {
                    FeatureVector featureVector = featureVectorPreprocessor.createFeatureVector(frame);
                    GestureSign = classifier.classifyGesture(featureVector.createInputVector());
                    gestureVerified = true;
                }
            }
            else
                gestureVerified = false;

            yield return null;
        }
    }

    public void stopFreeMode()
    {
        StopCoroutine(coroutineName);
    }
}
