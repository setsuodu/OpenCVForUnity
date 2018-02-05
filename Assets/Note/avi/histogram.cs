using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class histogram : MonoBehaviour
{
    [SerializeField] private Image m_showImage;
    private Mat srcImage;
    private List<Mat> images = new List<Mat>();
    private int size = 256;
    private int scale = 1;

    void Start()
    {
        string path = Application.dataPath + "/Textures/0.jpg";
        srcImage = Imgcodecs.imread(path, 0);
        Core.split(srcImage, images);

        Texture2D t2d = new Texture2D(srcImage.width(), srcImage.height());
        Utils.matToTexture2D(srcImage, t2d);
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_showImage.sprite = sp;

        //MatND类型用于存放直方图数据
        MatOfInt channels = new MatOfInt(0);
        Mat dstHist = new Mat();
        MatOfInt histSize = new MatOfInt(25);
        MatOfFloat ranges = new MatOfFloat(0, 256);
        //calcHist(srcImage, 1, channels, Mat(), dstHist, dims, size, ranges);

        //Debug.Log(images.Count); //通道数=1，灰度通道
        Imgproc.calcHist(images, channels, new Mat(), dstHist, histSize, ranges); //计算直方图

        Debug.Log(dstHist.width() + "," + dstHist.height());
        Texture2D ha = new Texture2D(dstHist.width(), dstHist.height());
        Utils.matToTexture2D(dstHist, ha);
        Sprite haha = Sprite.Create(ha, new UnityEngine.Rect(0, 0, ha.width, ha.height), Vector2.zero);
        m_showImage.sprite = haha;

        Mat dstImage = new Mat(size * scale, size, CvType.CV_8U, new Scalar(0));

        //获取最大值和最小值
        double minValue = 0;
        double maxValue = 0;
        //minMaxLoc(dstHist, &minValue, &maxValue, 0, 0);
        Core.minMaxLoc(dstHist); //坐标体系
        Debug.Log("最大值是：" + maxValue + ",最小值是：" + minValue);
        Debug.Log(dstHist.width() + "," + dstHist.height());

        for (int i = 0; i < 256; i++)
        {

        }
    }
}
