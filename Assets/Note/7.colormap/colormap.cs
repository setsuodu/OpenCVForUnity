using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class colormap : MonoBehaviour
{
    [SerializeField] private List<Image> m_imageList;
    Mat srcMat;

    void Start()
    {
        srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/sample.jpg", 0);

        //基础色度图
        for (int i = 0; i < 13; i++)
        {
            Mat dstMat = new Mat();

            //Imgproc.applyColorMap(srcMat, dstMat, Imgproc.COLORMAP_JET);
            Imgproc.applyColorMap(srcMat, dstMat, i);

            Texture2D t2d = new Texture2D(srcMat.width(), srcMat.height());
            Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
            m_imageList[i].sprite = sp;
            m_imageList[i].preserveAspect = true;
            Utils.matToTexture2D(dstMat, t2d);
        }
    }

    //自定义色度图
    void LutColorMap()
    {

    }
}

/* 13种固定colormap，自定义要使用LUT
 * public const int COLORMAP_AUTUMN = 0;
 * public const int COLORMAP_BONE = 1;
 * public const int COLORMAP_JET = 2;
 * public const int COLORMAP_WINTER = 3;
 * public const int COLORMAP_RAINBOW = 4;
 * public const int COLORMAP_OCEAN = 5;
 * public const int COLORMAP_SUMMER = 6;
 * public const int COLORMAP_SPRING = 7;
 * public const int COLORMAP_COOL = 8;
 * public const int COLORMAP_HSV = 9;
 * public const int COLORMAP_PINK = 10;
 * public const int COLORMAP_HOT = 11;
 * public const int COLORMAP_PARULA = 12;
 */
