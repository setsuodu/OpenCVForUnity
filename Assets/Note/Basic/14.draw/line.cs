using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

//http://blog.csdn.net/zangle260/article/details/52962967
public class line : MonoBehaviour
{
    [SerializeField] private Image m_dstImage;
    Mat p1Mat, p2Mat, dstMat;

    void Start()
    {
        dstMat = new Mat();
        p1Mat = Imgcodecs.imread(Application.dataPath + "/Textures/1.jpg", 1);
        p2Mat = Imgcodecs.imread(Application.dataPath + "/Textures/3.jpg", 1);
        Imgproc.cvtColor(p1Mat, p1Mat, Imgproc.COLOR_BGR2RGB);
        Imgproc.cvtColor(p2Mat, p2Mat, Imgproc.COLOR_BGR2RGB);
        Imgproc.resize(p2Mat, p2Mat, new Size(p1Mat.width(), p1Mat.height()));
        Debug.Log(p2Mat);

        Point p1 = new Point(50, 125);
        Point p2 = new Point(p1Mat.size().width - 50, 45);
        int thickness = 2;
        Imgproc.line(p1Mat, p1, p2, new Scalar(255, 0, 0), thickness);
        //Imgproc.circle(srcMat, pt, 3, new Scalar(255, 255, 0), -1, 8, 0); //绘制圆心
        //Imgproc.circle(srcMat, pt, (int)rho, new Scalar(255, 0, 0, 255), 5); //绘制圆轮廓

        Texture2D t2d = new Texture2D(p1Mat.width(), p1Mat.height());
        Utils.matToTexture2D(p1Mat, t2d);
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_dstImage.sprite = sp;
    }
}
