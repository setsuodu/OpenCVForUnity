using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class avi : MonoBehaviour
{
    [SerializeField] private Image m_showImage;
    private Texture2D t2d;
    Mat rgbMat = new Mat();
    Color32[] colors;
    VideoCapture capture;

    void Start()
    {
        capture = new VideoCapture();
        capture.open(Utils.getFilePath("dance.avi"));
        if (capture.isOpened())
        {
            Debug.Log("opened");
        }

        capture.grab(); //抓取
        capture.retrieve(rgbMat, 0); //检索

        colors = new Color32[rgbMat.width() * rgbMat.height()];
        t2d = new Texture2D(rgbMat.width(), rgbMat.height());
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_showImage.sprite = sp;
        m_showImage.preserveAspect = true;
    }

    void Update()
    {
        if(capture.grab())
        {
            capture.retrieve(rgbMat, 0);
            Imgproc.cvtColor(rgbMat, rgbMat, Imgproc.COLOR_BGR2RGB); //BGR转RGB
            Utils.matToTexture2D(rgbMat, t2d, colors);
        }
    }
}
