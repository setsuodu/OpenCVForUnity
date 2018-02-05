using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class blur : MonoBehaviour
{
    [SerializeField] private Image m_maskImage;
    [SerializeField] private Image m_backgroundImage;
    OpenCVForUnity.Rect roi; //感兴趣区域ROI
    Mat srcMat = new Mat();
    Mat logo; //掩码

    void Awake()
    {
    }

    void Start()
    {
        srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/kizuna.jpg", 1); //背景图
        logo = Imgcodecs.imread(Application.dataPath + "/Textures/mask.png", 1); //logo图
        Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGR2RGB); //转RGB
        Imgproc.cvtColor(logo, logo, Imgproc.COLOR_BGR2RGB); //转RGB

        // 两种方式定义ROI  
        Mat imgROI = new Mat(logo, new OpenCVForUnity.Rect(10, 10, 40, 40)); //裁剪，不能超出边际，unity会奔溃
        logo.copyTo(imgROI, logo); //目标(new, 源)

        Texture2D t2d = new Texture2D(imgROI.width(), imgROI.height());
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_maskImage.sprite = sp;
        m_maskImage.preserveAspect = true;
        m_maskImage.rectTransform.offsetMin = new Vector2(0, 0);
        m_maskImage.rectTransform.offsetMax = new Vector2(imgROI.width(), imgROI.height());
        Utils.matToTexture2D(imgROI, t2d);

        Texture2D t2d1 = new Texture2D(srcMat.width(), srcMat.height());
        Sprite sp1 = Sprite.Create(t2d1, new UnityEngine.Rect(0, 0, t2d1.width, t2d1.height), Vector2.zero);
        m_backgroundImage.sprite = sp1;
        m_backgroundImage.preserveAspect = true;
        m_backgroundImage.rectTransform.offsetMin = new Vector2(0, 0);
        m_backgroundImage.rectTransform.offsetMax = new Vector2(srcMat.width(), srcMat.height());
        Utils.matToTexture2D(srcMat, t2d1);
    }
}
