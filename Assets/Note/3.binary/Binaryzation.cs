using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class Binaryzation : MonoBehaviour
{
    [SerializeField] private RawImage targetImage;
    [SerializeField] private Button grayButton;
    [SerializeField] private Button binaryButton;
    [SerializeField] private double minThresh = 200d; //0(黑)->255(白)
    [SerializeField] private double maxThresh = 250d;
    private AspectRatioFitter aspectRatioFitter;
    private Mat srcMat, dstMat;

    void Awake()
    {
        aspectRatioFitter = targetImage.GetComponent<AspectRatioFitter>();
        grayButton.onClick.AddListener(OnGray);
        binaryButton.interactable = false;
        binaryButton.onClick.AddListener(OnBinary);
    }

    void OnDestroy()
    {
        grayButton.onClick.RemoveListener(OnGray);
        binaryButton.onClick.RemoveListener(OnBinary);
    }

    void Start()
    {
        srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/kizuna.jpg", 1);

        List<Mat> channels = new List<Mat>();
        Core.split(srcMat, channels);
        Mat imageRed = new Mat();
        imageRed = channels[0];
        Mat imageGreen = new Mat();
        imageGreen = channels[1];
        Mat imageBlue = new Mat();
        imageBlue = channels[2];
        channels[0] = channels[2];
        channels[2] = imageRed;
        Mat mergeImage = new Mat();
        Core.merge(channels, srcMat);

        aspectRatioFitter.aspectRatio = (float)srcMat.width() / (float)srcMat.height();

        Texture2D t2d = new Texture2D(srcMat.width(), srcMat.height());
        Utils.matToTexture2D(srcMat, t2d);
        targetImage.texture = t2d;
    }

    void OnGray()
    {
        binaryButton.interactable = true;

        //方法1.读取时就转为灰度
        //srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/kizuna.jpg", 0); //flag=0，将读入的彩色图像直接以灰度图像读入
        //方法2.将现有Mat转为灰度
        Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGR2GRAY); // 转为灰度图像

        Texture2D t2d = new Texture2D(srcMat.width(), srcMat.height());
        Utils.matToTexture2D(srcMat, t2d);
        targetImage.texture = t2d;
    }

    void OnBinary()
    {
        //克隆一个副本
        dstMat = srcMat.clone();
        //二值化处理
        Imgproc.threshold(srcMat, dstMat, minThresh, maxThresh, Imgproc.THRESH_BINARY_INV); //CV_THRESH_BINARY

        Texture2D t2d = new Texture2D(dstMat.width(), dstMat.height());
        Utils.matToTexture2D(dstMat, t2d);
        targetImage.texture = t2d;
    }
}
