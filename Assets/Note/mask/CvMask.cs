using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

/// <summary>
/// 掩膜mask
/// </summary>
public class CvMask : MonoBehaviour
{
    [SerializeField] private Image m_showImage;
    Mat srcMat, mask;
    OpenCVForUnity.Rect cv_rect;

    //一维直方图
    void Start()
    {
        srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/sample.jpg", 1);
        Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGR2RGB);

        cv_rect = new OpenCVForUnity.Rect(100, 50, 100, 200);
        mask = Mat.zeros(srcMat.size(), CvType.CV_8UC1); //eye/ones/zeros
        //mask = new Mat(srcMat, cv_rect); //这里改变了srcMat
        mask.setTo(new Scalar(255), mask);

        //Mat img1 = Imgcodecs.imread(Application.dataPath + "/Textures/lena.jpg", 1);
        //Imgproc.cvtColor(img1, img1, Imgproc.COLOR_BGR2RGB);
        Mat img1 = new Mat();
        srcMat.copyTo(img1, mask); //原始图srcMat拷贝到目的图img1上
        Debug.Log(img1);

        Texture2D t2d = new Texture2D(img1.width(), img1.height());
        Utils.matToTexture2D(img1, t2d);
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_showImage.sprite = sp;
        m_showImage.preserveAspect = true;
    }
}
