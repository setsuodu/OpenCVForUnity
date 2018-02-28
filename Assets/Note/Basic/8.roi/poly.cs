using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class poly : MonoBehaviour
{
    [SerializeField] private Image m_srcImage; //注意不能用Image，会破坏Rect
    [SerializeField] private Image m_roiImage;
    Mat srcMat, dstMat;

    void Start()
    {
        srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/lena.jpg", 1); //512
        Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGR2RGB);
        Imgproc.resize(srcMat, srcMat, new Size(512, 512));

        Texture2D src_t2d = new Texture2D(srcMat.width(), srcMat.height());
        Utils.matToTexture2D(srcMat, src_t2d);
        Sprite src_sp = Sprite.Create(src_t2d, new UnityEngine.Rect(0, 0, src_t2d.width, src_t2d.height), Vector2.zero);
        m_srcImage.sprite = src_sp;
        m_srcImage.rectTransform.offsetMin = new Vector2(0, 0);
        m_srcImage.rectTransform.offsetMax = new Vector2(srcMat.width(), srcMat.height());
        m_srcImage.rectTransform.anchoredPosition = Vector2.zero;

        //------------------------------------------------//

        //轮廓提取
        Mat mask = Mat.zeros(srcMat.size(), CvType.CV_8UC3);
        Point p0 = new Point(0, 0);
        Point p1 = new Point(0, 256);
        Point p2 = new Point(256, 0);
        MatOfPoint pts1 = new MatOfPoint(new Point[3] { p0, p1, p2 });
        Point p3 = new Point(256, 0);
        Point p4 = new Point(512, 0);
        Point p5 = new Point(512, 256);
        MatOfPoint pts2 = new MatOfPoint(new Point[3] { p3, p4, p5 });
        List<MatOfPoint> contour = new List<MatOfPoint>() { pts1, pts2 };
        for (int i = 0; i < contour.Count; i++)
        {
            Imgproc.drawContours(mask, contour, i, new Scalar(255), -1); //全部放到mask上
        }

        //------------------------------------------------//
        
        MatOfPoint PointArray = new MatOfPoint();
        dstMat = Mat.zeros(srcMat.size(), CvType.CV_8UC3);
        PointArray.fromList(new List<Point>()
        {
            new Point(50,10),
            new Point(300,12),
            new Point(350,250),
            new Point(9,250),
        });
        Imgproc.fillConvexPoly(dstMat, PointArray, new Scalar(255, 0, 0), 4, 0);

        //------------------------------------------------//

        OpenCVForUnity.Rect r1 = Imgproc.boundingRect(pts1);


        Texture2D dst_t2d = new Texture2D(dstMat.width(), dstMat.height());
        Utils.matToTexture2D(dstMat, dst_t2d);
        Sprite dst_sp = Sprite.Create(dst_t2d, new UnityEngine.Rect(0, 0, dst_t2d.width, dst_t2d.height), Vector2.zero);
        m_roiImage.sprite = dst_sp;
        m_roiImage.preserveAspect = true;
        m_roiImage.rectTransform.offsetMin = new Vector2(0, 0);
        m_roiImage.rectTransform.offsetMax = new Vector2(dstMat.width(), dstMat.height());
        m_roiImage.rectTransform.anchoredPosition = Vector2.zero;
    }
}
