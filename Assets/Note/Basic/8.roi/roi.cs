using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class roi : MonoBehaviour
{
    [SerializeField] private RawImage m_srcImage; //注意不能用Image，会破坏Rect
    [SerializeField] private Image m_roiImage;
    Mat srcMat;
    OpenCVForUnity.Point cv_point;
    OpenCVForUnity.Size cv_size;
    OpenCVForUnity.Rect cv_rect; //感兴趣区域ROI

    void Start()
    {
        srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/lena.jpg", 1); //512
        Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGR2RGB);
        Imgproc.resize(srcMat, srcMat, new Size(512, 512));

        Texture2D t2d = new Texture2D(srcMat.width(), srcMat.height());
        Utils.matToTexture2D(srcMat, t2d);
        m_srcImage.texture = t2d;
        m_srcImage.rectTransform.offsetMin = new Vector2(0, 0);
        m_srcImage.rectTransform.offsetMax = new Vector2(srcMat.width(), srcMat.height());
        m_srcImage.rectTransform.anchoredPosition = Vector2.zero;

        /*
        //切下一块roi，贴到rawImage
        cv_point = new Point(50, 50); //左上角为坐标原点
        cv_size = new Size(200, 200); //(长,宽)
        cv_rect = new OpenCVForUnity.Rect(cv_point, cv_size);
        Mat roiMat = new Mat(srcMat, cv_rect); //300,300
        Debug.Log(roiMat);
        Imgproc.cvtColor(roiMat, roiMat, Imgproc.COLOR_RGB2GRAY); //roi区域变灰
        */

        Mat mask = Mat.zeros(srcMat.size(), CvType.CV_8UC1);
        Point p0 = new Point(30, 45);
        Point p1 = new Point(500, 35);
        Point p2 = new Point(430, 240);
        Point p3 = new Point(50, 250);
        Point p4 = new Point(100, 250);
        Point p5 = new Point(75, 100);
        MatOfPoint pts = new MatOfPoint(new Point[3] { p0, p1, p2 });
        MatOfPoint pts2 = new MatOfPoint(new Point[3] { p3, p4, p5 });
        List<MatOfPoint> contour = new List<MatOfPoint>() { pts, pts2 };

        //轮廓提取
        Mat roiMat = new Mat();
        for (int i = 0; i < contour.Count; i++)
        {
            Imgproc.drawContours(mask, contour, i, new Scalar(255), -1); //全部放到mask上
        }
        srcMat.copyTo(roiMat, mask);

        Texture2D dst_t2d = new Texture2D(roiMat.width(), roiMat.height());
        Utils.matToTexture2D(roiMat, dst_t2d);
        Sprite sp = Sprite.Create(dst_t2d, new UnityEngine.Rect(0, 0, dst_t2d.width, dst_t2d.height), Vector2.zero);
        m_roiImage.sprite = sp;
        m_roiImage.preserveAspect = true;
    }
}
