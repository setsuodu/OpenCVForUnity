using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class affineTest : MonoBehaviour
{
    Mat srcMat, dstMat;

    void Start()
    {
        srcMat = Imgcodecs.imread(Application.dataPath + "/Resources/cat.jpg", 1);
        Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGR2RGBA);

        MatOfPoint2f srcP2f = new MatOfPoint2f();
        srcP2f.push_back(srcMat);

        dstMat = new Mat();
        MatOfPoint2f dstP2f = new MatOfPoint2f();
        dstP2f.push_back(dstMat);

        applyAffineTransform(srcMat, dstMat, srcP2f, new MatOfPoint2f());

        Texture2D t2d = new Texture2D(dstMat.cols(), dstMat.rows());
        Utils.matToTexture2D(dstMat, t2d);
    }

    void applyAffineTransform(Mat warpImage, Mat src, MatOfPoint2f srcTri, MatOfPoint2f dstTri)
    {
        // Given a pair of triangles, find the affine transform.
        Mat warpMat = Imgproc.getAffineTransform(srcTri, dstTri);

        // Apply the Affine Transform just found to the src image
        Imgproc.warpAffine(src, warpImage, warpMat, warpImage.size(), Imgproc.INTER_LINEAR, Core.BORDER_REFLECT_101, new Scalar(255, 0, 0, 255));
    }
}
