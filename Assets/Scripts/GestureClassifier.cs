using System.Collections.Generic;
using UnityEngine;
using Accord.IO;
using System.IO;
using Accord.MachineLearning.DecisionTrees;
using Accord.Statistics.Analysis;
using Accord.MachineLearning;
using Accord.Math.Distances;
using Accord.MachineLearning.VectorMachines;
using Accord.Statistics.Kernels;
using Accord.MachineLearning.VectorMachines.Learning;

public class GestureClassifier : MonoBehaviour
{
    [SerializeField]
    private DataService dataService;

    private RandomForest classifier;

    [SerializeField]
    private string modelName;

    private string modelPath;

    public bool ModelExists { get; set; }
    public bool TrainingFinished { get; set; }

    [SerializeField]
    private bool calculateAccuracy;

    [SerializeField]
    private List<int> testOutputs;
    private List<double[]> testInputs;

    private void Start()
    {
    #if UNITY_EDITOR
        modelPath = string.Format(@"Assets/StreamingAssets/Model/{0}", modelName);
    #else
            string directoryPath = string.Format("{0}/Model", Application.persistentDataPath);

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            string filePath = directoryPath + "/" + modelName;

            if(!File.Exists(filePath))
            {
                string pathToModel = Application.dataPath + "/StreamingAssets/Model/" + modelName;
	            File.Copy(pathToModel, filePath);
            }

            modelPath = filePath;
    #endif

        if (File.Exists(modelPath))
        {
            classifier = loadModel();
            ModelExists = true;
        }

        if(calculateAccuracy)
        {
            testInputs = new List<double[]>();
        }
    }

    public void trainClassifier()
    {
        List<FeatureVector> featureVectors = dataService.getAllFeatureVectors();

        double [][] inputs = new double[featureVectors.Count][];
        int[] outputs = new int[featureVectors.Count];

        createInputsAndOutputs(inputs, outputs, featureVectors);


        // Code For creating a MulticlassSupportVectorMachine
        // Create the multi-class learning algorithm for the machine
        /*
        var teacher = new MulticlassSupportVectorLearning<Gaussian>()
        {
            // Configure the learning algorithm to use SMO to train the
            //  underlying SVMs in each of the binary class subproblems.
            Learner = (param) => new SequentialMinimalOptimization<Gaussian>()
            {
                // Estimate a suitable guess for the Gaussian kernel's parameters.
                // This estimate can serve as a starting point for a grid search.
                UseKernelEstimation = true
            }
        };

        // Learn a machine
        var machine = teacher.Learn(inputs, outputs);


        // Create the multi-class learning algorithm for the machine
        var calibration = new MulticlassSupportVectorLearning<Gaussian>()
        {
            Model = machine, // We will start with an existing machine

            // Configure the learning algorithm to use Platt's calibration
            Learner = (param) => new ProbabilisticOutputCalibration<Gaussian>()
            {
                Model = param.Model // Start with an existing machine
            }
        };

        classifier = calibration.Learn(inputs, outputs);*/

        // Code for creating a KNN classifier
        /*
        int K = (int)(Mathf.Sqrt(inputs.GetLength(0)) / 2.0f);
        classifier = new KNearestNeighbors(k: K, distance: new Euclidean());
        classifier.Learn(inputs, outputs);*/


        // Code For creating a random forest classifier.

        // Create the forest learning algorithm

        var teacher = new RandomForestLearning()
        {
            NumberOfTrees = 50,
        };

        classifier = teacher.Learn(inputs, outputs);
        

        saveModel();
        TrainingFinished = true;
    }

    public string classifyGesture(double[] inputVector)
    {
        int gestureClassLabel = classifier.Decide(inputVector);

        if(calculateAccuracy)
        {
            testInputs.Add(inputVector);

            if (testInputs.Count == testOutputs.Count)
            {
                calculateConfusionMatrix();
            }
        }

        return dataService.classLabelToGesture(gestureClassLabel);
    }

    private void calculateConfusionMatrix()
    {
        GeneralConfusionMatrix cm = GeneralConfusionMatrix.Estimate(classifier, testInputs.ToArray(), testOutputs.ToArray());

        double error = cm.Error;
        double accuracy = cm.Accuracy;

        Debug.Log("Error - " + error);
        Debug.Log("Accuracy - " + accuracy);

        testInputs.Clear();
    }

    private void createInputsAndOutputs(double[][] inputs, int[] outputs, List<FeatureVector> featureVectors)
    {
        for (int i = 0; i < featureVectors.Count; i++)
        {
            inputs[i] = featureVectors[i].createInputVector();
            outputs[i] = featureVectors[i].GestureClassLabel;
        }
    }

    private void saveModel()
    {
        classifier.Save(modelPath);
    }

    protected RandomForest loadModel()
    {
        return Serializer.Load<RandomForest>(modelPath);
    }
}
