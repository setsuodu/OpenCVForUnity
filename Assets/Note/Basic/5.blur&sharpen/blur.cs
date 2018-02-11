using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class blur : MonoBehaviour
{
    [SerializeField] private Image m_srcImage;
    [SerializeField] private Image m_blurImage;
    [SerializeField] private Toggle m_origToggle;
    [SerializeField] private Toggle m_saltToggle;
    [SerializeField] private Toggle m_simpleBlurToggle;
    [SerializeField] private Toggle m_gaussianToggle;
    [SerializeField] private Toggle m_medianToggle;
    [SerializeField] private Toggle m_bilateralToggle;
    [SerializeField] private Toggle m_simpleSharpenToggle;
    [SerializeField] private Toggle m_sobelToggle;
    Mat srcMat;
    byte[] byteArray;

    void Awake()
    {
        m_origToggle.onValueChanged.AddListener((bool value) => OnOrig(value));
        m_saltToggle.onValueChanged.AddListener((bool value) => OnSalt(value));
        m_simpleBlurToggle.onValueChanged.AddListener((bool value) => OnSimpleBlur(value));
        m_gaussianToggle.onValueChanged.AddListener((bool value) => OnGaussianBlur(value));
        m_medianToggle.onValueChanged.AddListener((bool value) => OnMedianBlur(value));
        m_bilateralToggle.onValueChanged.AddListener((bool value) => OnBilateralFilter(value));
        m_simpleSharpenToggle.onValueChanged.AddListener((bool value) => OnSimpleSharpen(value));
        m_sobelToggle.onValueChanged.AddListener((bool value) => OnSobelSharpen(value));
    }

    void Start()
    {
        srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/lena.jpg", 1);
        Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGR2RGB); //转RGB

        OnOrig(true);
    }

    void OnOrig(bool value)
    {
        m_blurImage.enabled = false;
        if (!value) return;

        Texture2D src_t2d = new Texture2D(srcMat.width(), srcMat.height());
        Sprite src_sp = Sprite.Create(src_t2d, new UnityEngine.Rect(0, 0, src_t2d.width, src_t2d.height), Vector2.zero);
        m_srcImage.sprite = src_sp;
        m_srcImage.preserveAspect = true;
        Utils.matToTexture2D(srcMat, src_t2d);
    }

    /// <summary>
    /// 椒盐噪声
    /// </summary>
    /// <param name="image">目标Mat</param>
    /// <param name="n">噪点数量</param>
    void OnSalt(bool value)
    {
        m_blurImage.enabled = true;
        if (!value) return;

        //这里仅写了单通道例子
        Mat dstMat = new Mat();
        Imgproc.cvtColor(srcMat, dstMat, Imgproc.COLOR_RGB2GRAY); //转灰度
        int number = 500;
        byteArray = new byte[dstMat.width() * dstMat.height()];
        Utils.copyFromMat<byte>(dstMat, byteArray); //单通道

        for (int k = 0; k < number; k++)
        {
            int i = UnityEngine.Random.Range(0, dstMat.cols());
            int j = UnityEngine.Random.Range(0, dstMat.rows());
            byteArray[i + dstMat.width() * j] = 255;
        }
        Utils.copyToMat<byte>(byteArray, dstMat);

        Texture2D t2d = new Texture2D(dstMat.width(), dstMat.height());
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_blurImage.sprite = sp;
        m_blurImage.preserveAspect = true;
        Utils.matToTexture2D(dstMat, t2d);
    }

    #region Blur

    //简单模糊
    void OnSimpleBlur(bool value)
    {
        m_blurImage.enabled = true;
        if (!value) return;

        Mat dstMat = new Mat();
        dstMat = srcMat.clone();
        Size kSize = new Size(10d, 10d);
        Imgproc.blur(dstMat, dstMat, kSize);

        Texture2D t2d = new Texture2D(dstMat.width(), dstMat.height());
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_blurImage.sprite = sp;
        m_blurImage.preserveAspect = true;
        Utils.matToTexture2D(dstMat, t2d);
    }

    //高斯模糊
    void OnGaussianBlur(bool value)
    {
        m_blurImage.enabled = true;
        if (!value) return;

        Mat dstMat = new Mat();
        dstMat = srcMat.clone();
        //高斯半径（σ）对曲线形状的影响
        //σ越小，曲线越高越陡峭，模糊程度越小。
        //σ越大，曲线越低越平缓，模糊程度越大。
        Size kSize = new Size(7d, 7d);
        double sigmaX = 2d;
        double sigmaY = 2d;
        Imgproc.GaussianBlur(dstMat, dstMat, kSize, sigmaX, sigmaY);

        Texture2D t2d = new Texture2D(dstMat.width(), dstMat.height());
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_blurImage.sprite = sp;
        m_blurImage.preserveAspect = true;
        Utils.matToTexture2D(dstMat, t2d);
    }

    //中值模糊
    void OnMedianBlur(bool value)
    {
        m_blurImage.enabled = true;
        if (!value) return;

        Mat dstMat = new Mat();
        dstMat = srcMat.clone();
        int kSize = 7;
        //用像素点邻域灰度值的中值来代替该像素点的灰度。
        Imgproc.medianBlur(dstMat, dstMat, kSize);

        Texture2D t2d = new Texture2D(dstMat.width(), dstMat.height());
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_blurImage.sprite = sp;
        m_blurImage.preserveAspect = true;
        Utils.matToTexture2D(dstMat, t2d);
    }

    //双边滤波
    void OnBilateralFilter(bool value)
    {
        m_blurImage.enabled = true;
        if (!value) return;

        Mat dstMat = new Mat();
        dstMat = srcMat.clone();
        int d = 25;
        double sigmaColor = 50d;
        double sigmaSpace = 35d;
        Imgproc.bilateralFilter(srcMat, dstMat, d, sigmaColor, sigmaSpace);

        Texture2D t2d = new Texture2D(dstMat.width(), dstMat.height());
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_blurImage.sprite = sp;
        m_blurImage.preserveAspect = true;
        Utils.matToTexture2D(dstMat, t2d);
    }

    #endregion

    #region Sharpen

    //简单锐化
    void OnSimpleSharpen(bool value)
    {
        m_blurImage.enabled = true;
        if (!value) return;

        Mat dstMat = new Mat();
        Mat kernel = new Mat(3, 3, CvType.CV_32F, new Scalar(-1));
        kernel.put(0, 0, 0, -1, 0, -1, 5, -1, 0, -1, 0);
        //对图像srcMat和自定义核kernel做卷积，输出到dstMat
        Imgproc.filter2D(srcMat, dstMat, srcMat.depth(), kernel);

        Texture2D t2d = new Texture2D(dstMat.width(), dstMat.height());
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_blurImage.sprite = sp;
        m_blurImage.preserveAspect = true;
        Utils.matToTexture2D(dstMat, t2d);
    }

    //索贝尔算子锐化
    void OnSobelSharpen(bool value)
    {
        m_blurImage.enabled = true;
        if (!value) return;

        Mat dstMat = new Mat();
        Mat dst_x = new Mat();
        Mat abs_dst_x = new Mat();
        Mat dst_y = new Mat();
        Mat abs_dst_y = new Mat();

        //水平方向梯度
        Imgproc.Sobel(srcMat, dst_x, srcMat.depth(), 1, 0, 3, 1, 0);
        //垂直方向梯度
        Imgproc.Sobel(srcMat, dst_y, srcMat.depth(), 0, 1, 3, 1, 0);

        Core.convertScaleAbs(dst_x, abs_dst_x); //先缩放元素再取绝对值
        Core.convertScaleAbs(dst_y, abs_dst_y);
        Core.addWeighted(abs_dst_x, 0.5d, abs_dst_y, 0.5d, 0, dstMat); //x,y方向梯度，平均叠加融合
        Core.addWeighted(dstMat, 0.5d, srcMat, 1d, 0, dstMat); //把边缘叠加到原图

        Texture2D t2d = new Texture2D(dstMat.width(), dstMat.height());
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_blurImage.sprite = sp;
        m_blurImage.preserveAspect = true;
        Utils.matToTexture2D(dstMat, t2d);
    }

    #endregion
}
