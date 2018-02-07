using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class erode : MonoBehaviour
{
    [SerializeField] private Button m_dilateButton;
    [SerializeField] private Button m_erodeButton;
    Mat srcMat, dstMat;

    void Awake()
    {
        m_dilateButton.onClick.AddListener(OnDilate);
        m_erodeButton.onClick.AddListener(OnErode);
    }

    void Start()
    {
        srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/sample.jpg", 1); //背景图
        Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGR2RGB);
    }

    /// <summary>
    /// 膨胀
    /// </summary>
    void OnDilate()
    {
        dstMat = new Mat();
        int ksize = 7;
        Mat kernel = new Mat(ksize, ksize, CvType.CV_32F);
        Imgproc.dilate(srcMat, dstMat, kernel);

        Texture2D t2d = new Texture2D(dstMat.width(), dstMat.height());
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_dilateButton.image.sprite = sp;
        m_dilateButton.image.preserveAspect = true;
        Utils.matToTexture2D(dstMat, t2d);
    }

    /// <summary>
    /// 腐蚀
    /// </summary>
    void OnErode()
    {
        dstMat = new Mat();
        int ksize = 7;
        Mat kernel = new Mat(ksize, ksize, CvType.CV_32F);
        Imgproc.erode(srcMat, dstMat, kernel);

        Texture2D t2d = new Texture2D(dstMat.width(), dstMat.height());
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_erodeButton.image.sprite = sp;
        m_erodeButton.image.preserveAspect = true;
        Utils.matToTexture2D(dstMat, t2d);
    }
}
