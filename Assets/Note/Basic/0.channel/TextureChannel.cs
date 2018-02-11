using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

//从通道读取，合成
public class TextureChannel : MonoBehaviour
{
    Mat srcImage;
    //定义一个Mat向量容器保存拆分后的数据
    List<Mat> channels = new List<Mat>();
    public Texture2D t2d;
    public RawImage undeal, output;

    Mat imageRed, imageGreen, imageBlue;
    Mat mergeImage;
    
    void Start()
    {
        string path = Application.dataPath + "/Textures/sample.jpg";
        srcImage = Imgcodecs.imread(path, 1);

        t2d = new Texture2D(srcImage.width(), srcImage.height());
        Utils.matToTexture2D(srcImage, t2d);
        undeal.texture = t2d;

        //判断文件加载是否正确
        //Debug.Log(File.Exists(path));

        //split/merge 对颜色通道进行处理
        //split 通道的拆分。用于将一幅多通道的图像的各个通道分离。
        Core.split(srcImage, channels);
        Debug.Log(channels.Count); //jpg/png都只拆出3个通道(YUV?)

        //提取红色通道的数据
        imageRed = new Mat();
        imageRed = channels[0];
        //提取绿色通道的数据
        imageGreen = new Mat();
        imageGreen = channels[1];
        //提取蓝色通道的数据
        imageBlue = new Mat();
        imageBlue = channels[2];

        channels[0] = channels[2];
        channels[2] = imageRed;
        
        //对拆分的通道数据合并
        //merge 与split 相反。可以将多个单通道图像合成一幅多通道图像。
        mergeImage = new Mat();
        Core.merge(channels, mergeImage);

        t2d = new Texture2D(mergeImage.width(), mergeImage.height());
        output.texture = t2d;
        Utils.matToTexture2D(mergeImage, t2d);
    }
}
