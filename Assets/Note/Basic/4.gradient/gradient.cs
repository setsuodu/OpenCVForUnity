using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class gradient : MonoBehaviour
{
    [SerializeField] private Image sobelImage, laplaceImage, cannyImage;
    Mat grayMat, dstMat;

    void Awake()
    {
        grayMat = new Mat(); //不支持在外部new
        dstMat = new Mat();
    }

    void Start()
    {
        grayMat = Imgcodecs.imread(Application.dataPath + "/Textures/kizuna.jpg", 0); //读取为灰度

        sobelImage.sprite = SobelGradient();
        sobelImage.preserveAspect = true;

        laplaceImage.sprite = LaplaceGradient();
        laplaceImage.preserveAspect = true;

        cannyImage.sprite = CannyGradient();
        cannyImage.preserveAspect = true;
    }

    //索贝尔滤波
    public Sprite SobelGradient()
    {
        Mat grad_x = new Mat();
        Mat grad_y = new Mat();
        Mat abs_grad_x = new Mat();
        Mat abs_grad_y = new Mat();

        // 计算水平方向梯度
        Imgproc.Sobel(grayMat, grad_x, CvType.CV_16S, 1, 0, 3, 1, 0);
        // 计算垂直方向梯度
        Imgproc.Sobel(grayMat, grad_y, CvType.CV_16S, 0, 1, 3, 1, 0);
        // 计算两个方向上的梯度的绝对值
        Core.convertScaleAbs(grad_x, abs_grad_x);
        Core.convertScaleAbs(grad_y, abs_grad_y);
        // 计算结果梯度
        Core.addWeighted(abs_grad_x, 0.5, abs_grad_y, 0.5, 1, dstMat);

        // Mat转Texture2D
        Texture2D t2d = new Texture2D(dstMat.cols(), dstMat.rows());
        Utils.matToTexture2D(dstMat, t2d);
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        return sp;
    }

    //拉普拉斯滤波
    public Sprite LaplaceGradient()
    {
        Mat dst = new Mat();
        int scale = 1;
        int delta = 0;
        int ddepth = CvType.CV_16S;
        int kernel_size = 3;

        Imgproc.Laplacian(grayMat, dst, ddepth, kernel_size, scale, delta, Core.BORDER_DEFAULT);
        Core.convertScaleAbs(dst, dstMat);

        // Mat转Texture2D
        Texture2D t2d = new Texture2D(dstMat.cols(), dstMat.rows());
        Utils.matToTexture2D(dstMat, t2d);
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        return sp;
    }

    //Canny滤波
    public Sprite CannyGradient()
    {
        Mat edge = new Mat();
        double threshold1 = 0;
        double threshold2 = 100;

        Imgproc.blur(grayMat, edge, new Size(3, 3));
        Imgproc.Canny(edge, edge, threshold1, threshold2);
        Core.convertScaleAbs(edge, dstMat);

        // Mat转Texture2D
        Texture2D t2d = new Texture2D(dstMat.cols(), dstMat.rows());
        Utils.matToTexture2D(dstMat, t2d);
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        return sp;
    }
}
