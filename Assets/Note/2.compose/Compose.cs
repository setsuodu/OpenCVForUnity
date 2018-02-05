using OpenCVForUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//拼接
public class Compose : MonoBehaviour
{
    public Texture2D t2d;
    public RawImage output;

    void Start()
    {
        t2d = MatMerg();
        output.texture = t2d;
    }

    Texture2D MatMerg()
    {
        Mat srcMat = Imgcodecs.imread(Application.dataPath + "/Textures/head.png", Imgcodecs.CV_LOAD_IMAGE_UNCHANGED);
        Mat dstMat = Imgcodecs.imread(Application.dataPath + "/Textures/background.png", Imgcodecs.CV_LOAD_IMAGE_UNCHANGED); //无法读取图片时，会导致奔溃

        Imgproc.cvtColor(srcMat, srcMat, Imgproc.COLOR_BGRA2RGBA); //透明
        Imgproc.cvtColor(dstMat, dstMat, Imgproc.COLOR_BGR2RGB);

        Mat bgmat_roi = new Mat(dstMat, new OpenCVForUnity.Rect(797, 269, srcMat.cols(), srcMat.rows())); //不能超出边际，unity会奔溃
        cvAdd4cMat_q(bgmat_roi, srcMat, 1.0);

        Texture2D texture = new Texture2D(dstMat.cols(), dstMat.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(dstMat, texture);

        return texture;
    }

    private bool cvAdd4cMat_q(Mat dst, Mat scr, double scale)
    {
        if (dst.channels() != 3 || scr.channels() != 4)
        {
            return true;
        }
        if (scale < 0.01) return false;

        List<Mat> scr_channels = new List<Mat>();
        List<Mat> dstt_channels = new List<Mat>();
        Core.split(scr, scr_channels);
        Core.split(dst, dstt_channels);
        //CV_Assert(scr_channels.size() == 4 && dstt_channels.size() == 3);  

        if (scale < 1)
        {
            scr_channels[3] *= scale;
            scale = 1;
        }
        for (int i = 0; i < 3; i++)
        {
            dstt_channels[i] = dstt_channels[i].mul(new Mat(scr_channels[3].size(), CvType.CV_8UC1, new Scalar(255.0 / scale)) - scr_channels[3], scale / 255.0);
            dstt_channels[i] += scr_channels[i].mul(scr_channels[3], scale / 255.0);
        }
        //Debug.Log(dstt_channels[0]);

        Core.merge(dstt_channels, dst);
        return true;
    }
}
