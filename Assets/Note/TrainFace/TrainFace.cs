using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;
using OpenCVForUnityExample;

public class TrainFace : MonoBehaviour
{
    [SerializeField] private Image m_srcImage;
    string facerec_0_bmp_filepath;
    string facerec_1_bmp_filepath;
    string facerec_sample_bmp_filepath;

    void Start()
    {
        facerec_0_bmp_filepath = Utils.getFilePath("facerec/facerec_0.bmp");
        facerec_1_bmp_filepath = Utils.getFilePath("facerec/facerec_1.bmp");
        facerec_sample_bmp_filepath = Utils.getFilePath("facerec/facerec_sample.bmp");

        Run();
    }

    private void Run()
    {
        List<Mat> images = new List<Mat>();
        List<int> labelsList = new List<int>();
        MatOfInt labels = new MatOfInt();

        images.Add(Imgcodecs.imread(facerec_0_bmp_filepath, 0));
        images.Add(Imgcodecs.imread(facerec_1_bmp_filepath, 0));

        labelsList.Add(0); //积极
        labelsList.Add(1); //消极
        labels.fromList(labelsList);

        Mat testSampleMat = Imgcodecs.imread(facerec_sample_bmp_filepath, 0);
        //Mat testSampleMat = Imgcodecs.imread(facerec_0_bmp_filepath, 0);

        int testSampleLabel = 0;
        int[] predictedLabel = new int[1];
        double[] predictedConfidence = new double[1];

        BasicFaceRecognizer faceRecognizer = EigenFaceRecognizer.create();
        //faceRecognizer.train(images, labels); //会清空之前模型
        faceRecognizer.update(images, labels); //不会清空load的模型
        faceRecognizer.predict(testSampleMat, predictedLabel, predictedConfidence);
        //faceRecognizer.save(Application.dataPath + "/Cascades/train_face.txt"); //生成一个txt

        Debug.Log("Predicted class: " + predictedLabel[0] + " / " + "Actual class: " + testSampleLabel);
        Debug.Log("Confidence: " + predictedConfidence[0]);

        Mat predictedMat = images[predictedLabel[0]];
        Mat baseMat = new Mat(testSampleMat.rows(), predictedMat.cols() + testSampleMat.cols(), CvType.CV_8UC1);
        predictedMat.copyTo(baseMat.submat(new OpenCVForUnity.Rect(0, 0, predictedMat.cols(), predictedMat.rows())));
        testSampleMat.copyTo(baseMat.submat(new OpenCVForUnity.Rect(predictedMat.cols(), 0, testSampleMat.cols(), testSampleMat.rows())));

        Imgproc.putText(baseMat, "Predicted", new Point(10, 15), Core.FONT_HERSHEY_SIMPLEX, 0.4, new Scalar(255), 1, Imgproc.LINE_AA, false);
        Imgproc.putText(baseMat, "Confidence:", new Point(5, 25), Core.FONT_HERSHEY_SIMPLEX, 0.2, new Scalar(255), 1, Imgproc.LINE_AA, false);
        Imgproc.putText(baseMat, "   " + predictedConfidence[0], new Point(5, 33), Core.FONT_HERSHEY_SIMPLEX, 0.2, new Scalar(255), 1, Imgproc.LINE_AA, false);
        Imgproc.putText(baseMat, "TestSample", new Point(predictedMat.cols() + 10, 15), Core.FONT_HERSHEY_SIMPLEX, 0.4, new Scalar(255), 1, Imgproc.LINE_AA, false);

        Texture2D t2d = new Texture2D(baseMat.width(), baseMat.height());
        Utils.matToTexture2D(baseMat, t2d);
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_srcImage.sprite = sp;
        m_srcImage.preserveAspect = true;
    }
}
