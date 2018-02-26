using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class affineTest : MonoBehaviour
{
    Mat srcMat, dstMat, warpMat, rotMat;
    [SerializeField] private Image dstImage;

    void Start()
    {
        srcMat = Imgcodecs.imread(Application.dataPath + "/Resources/aragaki.jpg", 1);
        Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGR2RGB);

        dstMat = srcMat.clone();

        ///* 仿射变换
        Point srcPoint0 = new Point(0, 0);
        Point srcPoint1 = new Point(srcMat.width() - 1, 0);
        Point srcPoint2 = new Point(0, srcMat.height() - 1);
        MatOfPoint2f srcTri = new MatOfPoint2f(new Point[3] { srcPoint0, srcPoint1, srcPoint2 });
        Point dstPoint0 = new Point(0, srcMat.height() * 0.33d);
        Point dstPoint1 = new Point(srcMat.width() * 0.85d, srcMat.height() * 0.25d);
        Point dstPoint2 = new Point(srcMat.width() * 0.15d, srcMat.height() * 0.7d);
        MatOfPoint2f dstTri = new MatOfPoint2f(new Point[3] { dstPoint0, dstPoint1, dstPoint2 });
        warpMat = Imgproc.getAffineTransform(srcTri, dstTri);
        Imgproc.warpAffine(srcMat, dstMat, warpMat, new Size(dstMat.width(), dstMat.height()));
        //*/

        /* 旋转变换
        //拷贝整个画布
        dstMat.copyTo(srcMat);
        Point center = new Point(srcMat.width() / 2, srcMat.height() / 2);
        double angle = -50.0d;
        double scale = -0.6d;
        rotMat = Imgproc.getRotationMatrix2D(center, angle, scale);
        Imgproc.warpAffine(srcMat, dstMat, rotMat, new Size(dstMat.width(), dstMat.height()));
        */

        /*
        //透视变换
        Point srcPoint0 = new Point(0, 0);
        Point srcPoint1 = new Point(srcMat.width() - 1, 0);
        Point srcPoint2 = new Point(0, srcMat.height() - 1);
        Point srcPoint3 = new Point(srcMat.width() - 1, srcMat.height() - 1);
        MatOfPoint2f srcTri = new MatOfPoint2f(new Point[4] { srcPoint0, srcPoint1, srcPoint2, srcPoint3 });
        Point dstPoint0 = new Point(srcMat.width() * 0.05d, srcMat.height() * 0.33d);
        Point dstPoint1 = new Point(srcMat.width() * 0.9d, srcMat.height() * 0.25d);
        Point dstPoint2 = new Point(srcMat.width() * 0.2d, srcMat.height() * 0.7d);
        Point dstPoint3 = new Point(srcMat.width() * 0.8d, srcMat.height() * 0.9d);
        MatOfPoint2f dstTri = new MatOfPoint2f(new Point[4] { dstPoint0, dstPoint1, dstPoint2, dstPoint3 });
        warpMat = Imgproc.getPerspectiveTransform(srcTri, dstTri);
        Imgproc.warpPerspective(srcMat, dstMat, warpMat, new Size(dstMat.width(), dstMat.height()));
        */

        Texture2D t2d = new Texture2D(dstMat.width(), dstMat.height());
        Utils.matToTexture2D(dstMat, t2d);
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        dstImage.sprite = sp;
        dstImage.preserveAspect = true;
    }
}