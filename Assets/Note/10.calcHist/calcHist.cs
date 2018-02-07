using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class calcHist : MonoBehaviour
{
    [SerializeField] private Image m_showImage;
    Mat grayMat, outputHist;
    List<Mat> images;

    void Start()
    {
        grayMat = Imgcodecs.imread(Application.dataPath + "/Textures/feature.jpg", 0);
        outputHist = new Mat();
        images = new List<Mat>();
        Core.split(grayMat, images);

        //定义变量计算直方图
        Mat mask = new Mat(); //不空即可
        MatOfInt channels = new MatOfInt(new int[] { 1 });
        MatOfInt histSize = new MatOfInt(new int[] { 256 }); ;
        MatOfFloat ranges = new MatOfFloat(new float[] { 0, 255 });
        bool accum = false; //是否累积
        Imgproc.calcHist(images, channels, mask, outputHist, histSize, ranges, accum);
        Debug.Log(outputHist);

        //画出直方图
        int scale = 1;
        Mat histPic = new Mat(256, 256 * scale, CvType.CV_8U, new Scalar(255)); //单通道255，白色
        //找到最大值和最小值
        Core.MinMaxLocResult res = Core.minMaxLoc(grayMat); //注意不要搞错Mat，是对grayMat计算
        double minValue = res.minVal;
        double maxValue = res.maxVal;
        Debug.Log("min:" + minValue + ", max:" + maxValue);

        //纵坐标缩放比例
        double rate = (256 / maxValue) * 1d;
        for (int i = 0; i < 256; i++)
        {
            //得到每个i和箱子的值
            //float value = outputHist.at<float>(i);
            byte[] byteArray = new byte[grayMat.width() * grayMat.height()];
            Utils.copyFromMat<byte>(grayMat, byteArray);
            float value = (float)byteArray[i];
            //Debug.Log(value);
            //画直线
            Imgproc.line(histPic, new Point(i * scale, 256), new Point(i * scale, 256 - value * rate), new Scalar(0)); //单通道0，黑色
        }

        Texture2D t2d = new Texture2D(histPic.width(), histPic.height());
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_showImage.sprite = sp;
        m_showImage.preserveAspect = true;
        Utils.matToTexture2D(histPic, t2d);
    }
}
