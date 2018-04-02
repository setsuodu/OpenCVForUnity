 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class watershed : MonoBehaviour
{
    [SerializeField] private Image m_dstImage;
    Mat srcMat, dstMat, markers;

    void Start()
    {
        srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/palace.jpg");
        Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGR2RGB);

        //dstMat = srcMat.clone();
        //dstMat = removeBackground(srcMat);
        //dstMat = MyThresholdHsv(srcMat);
        //dstMat = myGrabCut(srcMat, new Point(50d, 0d), new Point(300d, 250d));
        //dstMat = MyFindLargestRectangle(srcMat);
        //dstMat = MyWatershed(srcMat);
        //dstMat = MyCanny(srcMat, 100);
        dstMat = MyFloodFill(srcMat);

        Texture2D t2d = new Texture2D(dstMat.width(), dstMat.height());
        Utils.matToTexture2D(dstMat, t2d);
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_dstImage.sprite = sp;
        m_dstImage.preserveAspect = true;
    }

    // threshold根据反差去掉深色单色背景
    public static Mat removeBackground(Mat nat)
    {
        Mat m = new Mat();

        Imgproc.cvtColor(nat, m, Imgproc.COLOR_BGR2GRAY);
        double threshold = Imgproc.threshold(m, m, 0, 255, Imgproc.THRESH_OTSU);
        Mat pre = new Mat(nat.size(), CvType.CV_8UC3, new Scalar(0, 0, 0));
        Mat fin = new Mat(nat.size(), CvType.CV_8UC3, new Scalar(0, 0, 0));
        for (int i = 0; i < m.rows(); i++)
        {
            for (int j = 0; j < m.cols(); j++)
            {
                double[] ds = m.get(i, j);
                double[] data = { ds[0] / 255, ds[0] / 255, ds[0] / 255 };
                pre.put(i, j, data);
            }
        }
        for (int i = 0; i < pre.rows(); i++)
        {
            for (int j = 0; j < pre.cols(); j++)
            {
                double[] pre_ds = pre.get(i, j);
                double[] nat_ds = nat.get(i, j);
                double[] data = { pre_ds[0] * nat_ds[0], pre_ds[1] * nat_ds[1], pre_ds[2] * nat_ds[2] };
                fin.put(i, j, data);
            }
        }
        return fin;
    }

    // threshold根据亮度去除背景
    private static Mat MyThresholdHsv(Mat frame)
    {
        Mat hsvImg = new Mat();
        List<Mat> hsvPlanes = new List<Mat>();
        Mat thresholdImg = new Mat();

        // threshold the image with the average hue value  
        hsvImg.create(frame.size(), CvType.CV_8U);
        Imgproc.cvtColor(frame, hsvImg, Imgproc.COLOR_BGR2HSV);
        Core.split(hsvImg, hsvPlanes); //3个通道

        // get the average hue value of the image  
        Scalar average = Core.mean(hsvPlanes[0]);
        double threshValue = average.val[0];
        Imgproc.threshold(hsvPlanes[0], thresholdImg, threshValue, 179.0, Imgproc.THRESH_BINARY_INV);

        Imgproc.blur(thresholdImg, thresholdImg, new Size(15, 15));

        // dilate to fill gaps, erode to smooth edges  
        Imgproc.dilate(thresholdImg, thresholdImg, new Mat(), new Point(-1, -1), 1);
        Imgproc.erode(thresholdImg, thresholdImg, new Mat(), new Point(-1, -1), 3);

        Imgproc.threshold(thresholdImg, thresholdImg, threshValue, 179.0, Imgproc.THRESH_BINARY);

        // create the new image  
        Mat foreground = new Mat(frame.size(), CvType.CV_8UC3, new Scalar(0, 0, 0));
        thresholdImg.convertTo(thresholdImg, CvType.CV_8U);
        frame.copyTo(foreground, thresholdImg);
        return foreground;
    }

    //grabCut分割技术
    public static Mat myGrabCut(Mat src, Point tl, Point br)
    {
        Mat mask = new Mat();
        Mat image = src;
        mask.create(image.size(), CvType.CV_8UC1);
        mask.setTo(new Scalar(0));

        Mat bgdModel = new Mat();// Mat.eye(1, 13 * 5, CvType.CV_64FC1);  
        Mat fgdModel = new Mat();// Mat.eye(1, 13 * 5, CvType.CV_64FC1);  

        Mat source = new Mat(1, 1, CvType.CV_8U, new Scalar(3));
        OpenCVForUnity.Rect rectangle = new OpenCVForUnity.Rect(tl, br);
        Imgproc.grabCut(image, mask, rectangle, bgdModel, fgdModel, 3, Imgproc.GC_INIT_WITH_RECT);
        Core.compare(mask, source, mask, Core.CMP_EQ);
        Mat foreground = new Mat(image.size(), CvType.CV_8UC1, new Scalar(0, 0, 0));
        image.copyTo(foreground, mask);

        return foreground;
    }

    //findContours分割技术
    private static Mat MyFindLargestRectangle(Mat original_image)
    {
        Mat imgSource = original_image;
        Imgproc.cvtColor(imgSource, imgSource, Imgproc.COLOR_BGR2GRAY);
        Imgproc.Canny(imgSource, imgSource, 50, 50);
        Imgproc.GaussianBlur(imgSource, imgSource, new Size(5, 5), 5);
        List<MatOfPoint> contours = new List<MatOfPoint>();
        Imgproc.findContours(imgSource, contours, new Mat(), Imgproc.RETR_LIST, Imgproc.CHAIN_APPROX_SIMPLE);
        double maxArea = 0;
        int maxAreaIdx = -1;
        MatOfPoint largest_contour = contours[0];
        MatOfPoint2f approxCurve = new MatOfPoint2f();
        for (int idx = 0; idx < contours.Count; idx++)
        {
            MatOfPoint temp_contour = contours[idx];
            double contourarea = Imgproc.contourArea(temp_contour);
            if (contourarea - maxArea > 1)
            {
                maxArea = contourarea;
                largest_contour = temp_contour;
                maxAreaIdx = idx;
                MatOfPoint2f new_mat = new MatOfPoint2f(temp_contour.toArray());
                int contourSize = (int)temp_contour.total();
                Imgproc.approxPolyDP(new_mat, approxCurve, contourSize * 0.05, true);
            }
        }

        Imgproc.drawContours(imgSource, contours, -1, new Scalar(255, 0, 0), 1);
        Imgproc.fillConvexPoly(imgSource, largest_contour, new Scalar(255, 255, 255));
        Imgproc.drawContours(imgSource, contours, maxAreaIdx, new Scalar(0, 0, 255), 3);

        return imgSource;
    }

    //watershed分水岭分割技术
    public static Mat MyWatershed(Mat img)
    {
        Mat threeChannel = new Mat();

        Imgproc.cvtColor(img, threeChannel, Imgproc.COLOR_BGR2GRAY);
        //Imgproc.threshold(threeChannel, threeChannel, 200, 255, Imgproc.THRESH_BINARY);  
        Imgproc.threshold(threeChannel, threeChannel, 0, 255, Imgproc.THRESH_OTSU);

        Mat fg = new Mat(img.size(), CvType.CV_8U);
        Imgproc.erode(threeChannel, fg, new Mat());

        Mat bg = new Mat(img.size(), CvType.CV_8U);
        Imgproc.dilate(threeChannel, bg, new Mat());
        Imgproc.threshold(bg, bg, 1, 128, Imgproc.THRESH_BINARY_INV);

        Mat markers = new Mat(img.size(), CvType.CV_8U, new Scalar(0));
        Core.add(fg, bg, markers);

        Mat result = new Mat();
        markers.convertTo(result, CvType.CV_32SC1);
        Imgproc.watershed(img, result);
        result.convertTo(result, CvType.CV_8U);

        return result;
    }

    //Canny分割技术
    public static Mat MyCanny(Mat img, int threshold)
    {
        Imgproc.cvtColor(img, img, Imgproc.COLOR_BGR2GRAY);
        Imgproc.Canny(img, img, threshold, threshold * 3, 3, true);
        return img;
    }

    //漫水填充
    public static Mat MyFloodFill(Mat img)
    {
        OpenCVForUnity.Rect ccomp = new OpenCVForUnity.Rect();
        Mat mask = new Mat();
        Imgproc.floodFill(img, mask, new Point(50, 20), new Scalar(0, 0, 0), ccomp, new Scalar(10, 10, 10), new Scalar(10, 10, 10), 0);

        return img;
    }

    //黑色转透明
    public static Mat BlackToAlpha(Mat img)
    {

        return img;
    }
}
