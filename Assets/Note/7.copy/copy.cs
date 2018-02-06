using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class copy : MonoBehaviour
{
    [SerializeField] private Image m_maskImage;
    [SerializeField] private Image m_backgroundImage;
    OpenCVForUnity.Rect roi; //感兴趣区域ROI
    Mat srcMat, dstMat;

    void Start()
    {
        dstMat = Imgcodecs.imread(Application.dataPath + "/Textures/kizuna.jpg", 1); //背景图
        srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/mask.png", 1); //logo图
        Imgproc.cvtColor(dstMat, dstMat, Imgproc.COLOR_BGR2RGB); //转RGB
        Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGR2RGB);

        int _x = 50;
        int _y = 50;
        //Mat mask = new Mat(srcMat, new OpenCVForUnity.Rect(_x, _y, 50, 50)); //dst是src裁切下来的一部分
        Mat mask = new Mat(_x, _y, 1); //只有大小
        //Mat mask = srcMat.clone();

        Mat container = new Mat(200, 300, 1); //宽，长
        srcMat.copyTo(container); //源.copyTo(容器);容器尺寸跟随源
        Debug.Log(container.width() + "," + container.height());

        Texture2D t2d = new Texture2D(container.width(), container.height());
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_maskImage.sprite = sp;
        m_maskImage.preserveAspect = true;
        //m_maskImage.rectTransform.offsetMin = new Vector2(0, 0);
        //m_maskImage.rectTransform.offsetMax = new Vector2(mask.width(), mask.height());
        //m_maskImage.rectTransform.anchoredPosition = new Vector2(_x, _y);
        Utils.matToTexture2D(container, t2d);

        /*
        int _x1 = 0;
        int _y1 = 0;
        Texture2D t2d1 = new Texture2D(srcMat.width(), srcMat.height());
        Sprite sp1 = Sprite.Create(t2d1, new UnityEngine.Rect(_x1, _y1, t2d1.width, t2d1.height), Vector2.zero);
        m_backgroundImage.sprite = sp1;
        m_backgroundImage.preserveAspect = true;
        m_backgroundImage.rectTransform.offsetMin = new Vector2(0, 0);
        m_backgroundImage.rectTransform.offsetMax = new Vector2(srcMat.width(), srcMat.height());
        m_backgroundImage.rectTransform.anchoredPosition = new Vector2(_x1, _y1);
        Utils.matToTexture2D(srcMat, t2d1);
        */
    }
}
