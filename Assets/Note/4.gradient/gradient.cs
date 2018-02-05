using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class gradient : MonoBehaviour
{
    [SerializeField] private Texture2D t2d;
    [SerializeField] private RawImage sobelRawImage;
    [SerializeField] private RawImage laplaceRawImage;
    [SerializeField] private AspectRatioFitter sobelAspectRatioFitter;
    [SerializeField] private AspectRatioFitter laplaceAspectRatioFitter;
    Mat grayMat = new Mat();
    Mat sobel = new Mat();
    Mat grad_x = new Mat();
    Mat grad_y = new Mat();
    Mat abs_grad_x = new Mat();
    Mat abs_grad_y = new Mat();

    Mat dst = new Mat();
    Mat abs_dst = new Mat();

    void Awake()
    {
        sobelAspectRatioFitter = sobelRawImage.GetComponent<AspectRatioFitter>();
        laplaceAspectRatioFitter = laplaceRawImage.GetComponent<AspectRatioFitter>();
    }

    void Start()
    {
        sobelRawImage.texture = SobelGradient(t2d);
        sobelAspectRatioFitter.aspectRatio = (float)t2d.width / (float)t2d.height;

        laplaceRawImage.texture = LaplaceGradient(t2d);
        laplaceAspectRatioFitter.aspectRatio = (float)t2d.width / (float)t2d.height;
    }

    //索贝尔滤波
    public Texture2D SobelGradient(Texture2D t2d)
    {
        // Texture2D转为Mat
        Mat src = new Mat(t2d.height, t2d.width, CvType.CV_8UC4); //注意，函数是先高度，再宽度
        Utils.texture2DToMat(t2d, src);

        // 原图置灰
        Imgproc.cvtColor(src, grayMat, Imgproc.COLOR_BGR2GRAY);

        // 计算水平方向梯度
        Imgproc.Sobel(grayMat, grad_x, CvType.CV_16S, 1, 0, 3, 1, 0);
        // 计算垂直方向梯度
        Imgproc.Sobel(grayMat, grad_y, CvType.CV_16S, 0, 1, 3, 1, 0);
        // 计算两个方向上的梯度的绝对值
        Core.convertScaleAbs(grad_x, abs_grad_x);
        Core.convertScaleAbs(grad_y, abs_grad_y);
        // 计算结果梯度
        Core.addWeighted(abs_grad_x, 0.5, abs_grad_y, 0.5, 1, sobel);

        // Mat转Texture2D
        Texture2D processedImage = new Texture2D(sobel.cols(), sobel.rows());
        Utils.matToTexture2D(sobel, processedImage);

        return processedImage;
    }

    //拉普拉斯滤波
    public Texture2D LaplaceGradient(Texture2D t2d)
    {
        // Texture2D转为Mat
        Mat src = new Mat(t2d.height, t2d.width, CvType.CV_8UC4); //注意，函数是先高度，再宽度
        Utils.texture2DToMat(t2d, src);

        // 原图置灰
        Imgproc.cvtColor(src, grayMat, Imgproc.COLOR_BGR2GRAY);

        /*
        // 计算水平方向梯度
        Imgproc.Sobel(grayMat, grad_x, CvType.CV_16S, 1, 0, 3, 1, 0);
        // 计算垂直方向梯度
        Imgproc.Sobel(grayMat, grad_y, CvType.CV_16S, 0, 1, 3, 1, 0);
        // 计算两个方向上的梯度的绝对值
        Core.convertScaleAbs(grad_x, abs_grad_x);
        Core.convertScaleAbs(grad_y, abs_grad_y);
        // 计算结果梯度
        Core.addWeighted(abs_grad_x, 0.5, abs_grad_y, 0.5, 1, sobel);
        */

        int scale = 1;
        int delta = 0;
        int ddepth = CvType.CV_16S;
        int kernel_size = 3;
        Imgproc.Laplacian(grayMat, dst, ddepth, kernel_size, scale, delta, Core.BORDER_DEFAULT);
        Core.convertScaleAbs(dst, abs_dst);

        // Mat转Texture2D
        Texture2D processedImage = new Texture2D(abs_dst.cols(), abs_dst.rows());
        Utils.matToTexture2D(abs_dst, processedImage);

        return processedImage;
    }
}
