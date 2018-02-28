using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class roi : MonoBehaviour
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

        Mat mask = Mat.zeros(srcMat.size(), CvType.CV_8UC1);
        Point p0 = new Point(0, 0);
        Point p1 = new Point(0, 256);
        Point p2 = new Point(256, 0);
        MatOfPoint pts1 = new MatOfPoint(new Point[3] { p0, p1, p2 });
        MatOfPoint2f srcTri = new MatOfPoint2f(new Point[3] { p0, p1, p2 });
        Point p3 = new Point(256, 0);
        Point p4 = new Point(512, 0);
        Point p5 = new Point(512, 256);
        Point p6 = new Point(256, 64);
        MatOfPoint pts2 = new MatOfPoint(new Point[3] { p3, p4, p5 });
        MatOfPoint2f dstTri = new MatOfPoint2f(new Point[3] { p0, p1, p6 });
        List<MatOfPoint> contour = new List<MatOfPoint>() { pts1 };
        for (int i = 0; i < contour.Count; i++)
        {
            //轮廓提取
            Imgproc.drawContours(mask, contour, i, new Scalar(255), -1); //全部放到mask上
        }
        srcMat.copyTo(mask, mask);
        Mat warpMat = Imgproc.getAffineTransform(srcTri, dstTri);
        Mat warpImage = Mat.zeros(mask.size(), mask.type());
        Imgproc.warpAffine(mask, warpImage, warpMat, warpImage.size());

        //------------------------------------------------//
        /*
        // Offset points by left top corner of the respective rectangles
        OpenCVForUnity.Rect r1 = Imgproc.boundingRect(pts1);
        OpenCVForUnity.Rect r2 = Imgproc.boundingRect(pts2);
        MatOfPoint2f t1Rect = new MatOfPoint2f();
        MatOfPoint2f t2Rect = new MatOfPoint2f();
        MatOfPoint t2RectInt = new MatOfPoint();

        for (int i = 0; i < 3; i++)
        {
            t1Rect.push_back(new Mat((int)pts1.toList()[i].x - r1.x, (int)pts1.toList()[i].y - r1.y, 0));
            t2Rect.push_back(new Mat((int)pts2.toList()[i].x - r2.x, (int)pts2.toList()[i].y - r2.y, 0));
            t2RectInt.push_back(new Mat((int)pts2.toList()[i].x - r2.x, (int)pts2.toList()[i].y - r2.y, 0)); // for fillConvexPoly
        }
        Debug.Log(t2RectInt);
        
        MatOfPoint PointArray = new MatOfPoint();
        dstMat = Mat.zeros(srcMat.size(), CvType.CV_8UC3);
        PointArray.fromList(new List<Point>()
        {
            new Point(50,10),
            new Point(300,12),
            new Point(350,250),
            new Point(9,250),
        });
        Debug.Log(PointArray);
        Imgproc.fillConvexPoly(dstMat, PointArray, new Scalar(255, 0, 0), 4, 0);
        */
        //------------------------------------------------//

        Texture2D dst_t2d = new Texture2D(warpImage.width(), warpImage.height());
        Utils.matToTexture2D(warpImage, dst_t2d);
        Sprite sp = Sprite.Create(dst_t2d, new UnityEngine.Rect(0, 0, dst_t2d.width, dst_t2d.height), Vector2.zero);
        m_roiImage.sprite = sp;
        m_roiImage.preserveAspect = true;
        m_roiImage.rectTransform.offsetMin = new Vector2(0, 0);
        m_roiImage.rectTransform.offsetMax = new Vector2(srcMat.width(), srcMat.height());
        m_roiImage.rectTransform.anchoredPosition = Vector2.zero;
    }
}
