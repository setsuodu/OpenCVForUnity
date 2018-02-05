using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class CropTexture2D : MonoBehaviour
{
    public Mat mat;
    public Texture2D source;
    public Texture2D[] result = new Texture2D[4];
    public RawImage[] output = new RawImage[4];

    void Start()
    {
        int width = source.width;
        int height = source.height;
        //Debug.Log(width + "*" + height); // 256*256, 打印查看一下图片尺寸

        //rows 是行数，对应height
        //cols 是列数，对应width
        mat = new Mat(height, width, CvType.CV_8UC3); // Mat对象尺寸要和Texture2d一样
        Utils.texture2DToMat(source, mat);

        for (int i = 0; i < 2; i++)
        {
            for (int t = 0; t < 2; t++)
            {
                int id = i * 2 + t;
                Debug.Log("[顺序]" + (id)); //0->1->2->3

                // 按output数组中的顺序，裁切并赋予新图片
                OpenCVForUnity.Rect rectCrop = new OpenCVForUnity.Rect(width / 2 * t, height / 2 * i, width / 2, height / 2); // 调试小心，超出取值范围，unity会奔溃
                Mat croppedImage = new Mat(mat, rectCrop);

                result[id] = new Texture2D(croppedImage.width(), croppedImage.height()); // Texture2d尺寸也要和Mat一样
                Utils.matToTexture2D(croppedImage, result[id]);
                output[id].texture = result[id];
            }
        }
    }
}
