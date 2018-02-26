using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class flood : MonoBehaviour
{
    [SerializeField] private Image dstImage;
    Mat srcMat, dstMat;

    void Start()
    {
        srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/kizuna.jpg", 1);
        Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGR2RGB);

        dstMat = new Mat(new Size(100, 100), 0);
        //dstMat = srcMat.clone();
        Imgproc.floodFill(srcMat, dstMat, new Point(10, 10), new Scalar(255, 255, 0, 255));

        Texture2D t2d = new Texture2D(dstMat.width(), dstMat.height());
        Utils.matToTexture2D(dstMat, t2d);
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        dstImage.sprite = sp;
        dstImage.preserveAspect = true;
    }
}
