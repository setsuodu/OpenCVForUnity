using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class erode : MonoBehaviour
{
    [SerializeField] private Image m_erodeImage;
    [SerializeField] private Image m_dilateImage;
    Mat srcMat, dstMat;

    void Start()
    {
        dstMat = Imgcodecs.imread(Application.dataPath + "/Textures/kizuna.jpg", 1); //背景图
        srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/mask.png", 1); //logo图
        Imgproc.cvtColor(dstMat, dstMat, Imgproc.COLOR_BGR2RGB); //转RGB
        Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGR2RGB);

        int _x = 50;
        int _y = 50;
        Mat mask = new Mat(_x, _y, 1); //只有大小

        Mat container = new Mat(200, 300, 1); //宽，长
        srcMat.copyTo(container); //源.copyTo(容器);容器尺寸跟随源
        Debug.Log(container.width() + "," + container.height());

        Texture2D t2d = new Texture2D(container.width(), container.height());
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_erodeImage.sprite = sp;
        m_erodeImage.preserveAspect = true;
        Utils.matToTexture2D(container, t2d);
    }
}
