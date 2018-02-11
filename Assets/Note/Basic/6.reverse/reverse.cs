using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class reverse : MonoBehaviour
{
    [SerializeField] private Image m_showImage;
    Mat srcMat;
    List<Mat> channels;
    byte[] byteArrayR, byteArrayG, byteArrayB;

    void Start()
    {
        srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/sample.jpg");
        Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGR2RGB);
        //Debug.Log(srcMat.channels()); //3

        //提取通道
        channels = new List<Mat>();
        Core.split(srcMat, channels);

        //byteArray = new byte[srcMat.width() * srcMat.height()];
        byteArrayR = new byte[channels[0].width() * channels[0].height()];
        byteArrayG = new byte[channels[0].width() * channels[0].height()];
        byteArrayB = new byte[channels[0].width() * channels[0].height()];
        Utils.copyFromMat<byte>(channels[0], byteArrayR);
        Utils.copyFromMat<byte>(channels[1], byteArrayG);
        Utils.copyFromMat<byte>(channels[2], byteArrayB);

        //遍历像素
        int width = srcMat.width();
        int height = srcMat.height();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                //反色操作
                int rValue = 255 - byteArrayR[x + width * y];
                byteArrayR[x + width * y] = (byte)rValue;
                //Debug.Log(rValue); //r通道值

                int gValue = 255 - byteArrayG[x + width * y];
                byteArrayG[x + width * y] = (byte)gValue;

                int bValue = 255 - byteArrayB[x + width * y];
                byteArrayB[x + width * y] = (byte)bValue;
            }
        }

        //拷贝回Mat
        Utils.copyToMat(byteArrayR, channels[0]);
        Utils.copyToMat(byteArrayG, channels[1]);
        Utils.copyToMat(byteArrayB, channels[2]);
        //合并通道
        Core.merge(channels, srcMat);

        //ugui显示
        Texture2D t2d = new Texture2D(srcMat.width(), srcMat.height());
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_showImage.sprite = sp;
        Utils.matToTexture2D(srcMat, t2d);
    }
}
