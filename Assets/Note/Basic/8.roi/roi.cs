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
        Point p0 = new Point(0, 0);
        Point p1 = new Point(0, 256);
        Point p2 = new Point(256, 0);
        MatOfPoint pts1 = new MatOfPoint(new Point[3] { p0, p1, p2 });
        Point p3 = new Point(256, 0);
        Point p4 = new Point(512, 0);
        Point p5 = new Point(512, 256);
        MatOfPoint pts2 = new MatOfPoint(new Point[3] { p3, p4, p5 });
        List<MatOfPoint> contour = new List<MatOfPoint>() { pts1, pts2 };

        //轮廓提取
        Mat roiMat = new Mat();
        for (int i = 0; i < contour.Count; i++)
        {
            Imgproc.drawContours(mask, contour, i, new Scalar(255), -1); //全部放到mask上
        }

        //------------------------------------------------//

        // Offset points by left top corner of the respective rectangles
        OpenCVForUnity.Rect r1 = Imgproc.boundingRect(pts1);
        OpenCVForUnity.Rect r2 = Imgproc.boundingRect(pts2);
        //Debug.Log(r1); //{0, 0, 257x257}
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
        //Debug.Log(r2.height + "," + r2.width);

        // Get mask by filling triangle
        //Mat _mask = Mat.zeros(r2.height, r2.width, CvType.CV_32FC3);
        //Imgproc.fillConvexPoly(_mask, t2RectInt, new Scalar(1.0, 1.0, 1.0), 16, 0);

        MatOfPoint PointArray = new MatOfPoint();
        Mat src = Mat.zeros(480, 640, CvType.CV_8UC3);
        PointArray.fromList(new List<Point>()
        {
            new Point(50,10),
            new Point(300,12),
            new Point(350,250),
            new Point(9,250),
        });
        Debug.Log(PointArray);
        Imgproc.fillConvexPoly(src, PointArray, new Scalar(255, 0, 0), 4, 0);

        // Apply warpImage to small rectangular patches

        //------------------------------------------------//

        //srcMat.copyTo(roiMat, mask);

        Texture2D dst_t2d = new Texture2D(src.width(), src.height());
        Utils.matToTexture2D(src, dst_t2d);
        Sprite sp = Sprite.Create(dst_t2d, new UnityEngine.Rect(0, 0, dst_t2d.width, dst_t2d.height), Vector2.zero);
        m_roiImage.sprite = sp;
        m_roiImage.preserveAspect = true;

    }
}
