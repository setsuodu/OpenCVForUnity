 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class transpancy : MonoBehaviour
{
    [SerializeField]
    private Image m_dstImage;
    [SerializeField, Range(0, 255)]
    private int thread = 255;

    [ContextMenu("Reset")]
    void Start()
    {
        Mat srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/sample.jpg");
        Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGR2RGBA);
        //Debug.Log(srcMat.channels()); //4

        //提取通道
        List<Mat> channels = new List<Mat>();
        Core.split(srcMat, channels);

        byte[] byteArrayR = new byte[channels[0].width() * channels[0].height()];
        byte[] byteArrayG = new byte[channels[0].width() * channels[0].height()];
        byte[] byteArrayB = new byte[channels[0].width() * channels[0].height()];
        byte[] byteArrayA = new byte[channels[0].width() * channels[0].height()];
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
                int rValue = byteArrayR[x + width * y];
                int gValue = byteArrayG[x + width * y];
                int bValue = byteArrayB[x + width * y];

                if (bValue >= thread && gValue >= thread && rValue >= thread)
                {
                    byteArrayA[x + width * y] = (byte)(0);
                }
                else
                {
                    byteArrayA[x + width * y] = (byte)(255);
                }
            }
        }

        //拷贝回Mat
        Utils.copyToMat(byteArrayA, channels[3]);
        //合并通道
        Core.merge(channels, srcMat);

        Texture2D t2d = new Texture2D(srcMat.width(), srcMat.height(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(srcMat, t2d);
        Sprite sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_dstImage.sprite = sp;
        m_dstImage.preserveAspect = true;
    }
}
