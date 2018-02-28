using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class circles : MonoBehaviour
{
    [SerializeField] private Image m_showImage;
    Mat srcMat, grayMat;

    void Start()
    {
        srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/feature.jpg", 1);
        grayMat = new Mat();
        Imgproc.cvtColor(srcMat, grayMat, Imgproc.COLOR_RGBA2GRAY);

        //会把五边形识别成圆。模糊处理，提高精确度。
        Imgproc.GaussianBlur(grayMat, grayMat, new Size(7, 7), 2, 2);

        Mat circles = new Mat();
        //霍夫圆
        Imgproc.HoughCircles(grayMat, circles, Imgproc.CV_HOUGH_GRADIENT, 2, 10, 160, 50, 10, 40);
        //Debug.Log(circles);

        //圆心坐标
        Point pt = new Point();
        for (int i = 0; i < circles.cols(); i++)
        {
            double[] data = circles.get(0, i);
            pt.x = data[0];
            pt.y = data[1];
            double rho = data[2];
            //绘制圆心
            Imgproc.circle(srcMat, pt, 3, new Scalar(255, 255, 0), -1, 8, 0);
            //绘制圆轮廓
            Imgproc.circle(srcMat, pt, (int)rho, new Scalar(255, 0, 0, 255), 5);
        }

        //在Mat上写字
        Imgproc.putText(srcMat, "W:" + srcMat.width() + " H:" + srcMat.height(), new Point(5, srcMat.rows() - 10), Core.FONT_HERSHEY_SIMPLEX, 1.0, new Scalar(255, 255, 255, 255), 2, Imgproc.LINE_AA, false);

        Texture2D t2d = new Texture2D(srcMat.width(), srcMat.height());
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_showImage.sprite = sp;
        m_showImage.preserveAspect = true;
        Utils.matToTexture2D(srcMat, t2d);
    }

    //漫水填充？
    void OnFloodFill()
    {
        srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/cv.png", 1);
        Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGR2RGBA);

        Mat mask = new Mat();
        mask.create(srcMat.rows() + 2, srcMat.cols() + 2, CvType.CV_8UC1);
        for (int i = 0; i < 60; i++)
        {
            for (int j = 0; j < 60; j++)
            {
                //mask.at<uchar>(i, j) = 255;
            }
        }
        
        //OpenCVForUnity.Rect ccomp = new OpenCVForUnity.Rect(20, 20, 30, 30);
        //Point seedPoint = new Point(50, 300);
        //Imgproc.floodFill(srcMat, srcMat, seedPoint, new Scalar(255, 255, 0), ccomp, new Scalar(20, 30, 40, 0), new Scalar(2, 30, 40, 0), 0);

        Texture2D t2d = new Texture2D(srcMat.width(), srcMat.height());
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_showImage.sprite = sp;
        m_showImage.preserveAspect = true;
        Utils.matToTexture2D(srcMat, t2d);
    }
}
