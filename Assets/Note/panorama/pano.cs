using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class pano : MonoBehaviour
{
    [SerializeField] private Image m_showImage;
    Mat src1, src2, dstMat;
    List<Mat> images;

    void Start()
    {
        src1 = Imgcodecs.imread(Application.dataPath + "/Textures/p1.jpg", 1);
        src2 = Imgcodecs.imread(Application.dataPath + "/Textures/p2.jpg", 1);
        Imgproc.cvtColor(src1, src1, Imgproc.COLOR_BGR2RGB);
        Imgproc.cvtColor(src2, src2, Imgproc.COLOR_BGR2RGB);
        images.Add(src1);
        images.Add(src2);

        //全景图
        Mat pano = new Mat();
        //缺少stitcher.hpp

        Texture2D t2d = new Texture2D(dstMat.width(), dstMat.height());
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_showImage.sprite = sp;
        m_showImage.preserveAspect = true;
        Utils.matToTexture2D(dstMat, t2d);
    }
}
