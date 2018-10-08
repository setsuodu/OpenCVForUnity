# OpencvForUnity Step-by-step.

## Required

- [OpenCVForUnity](https://assetstore.unity.com/packages/tools/integration/opencv-for-unity-21088);
- [Dlib FaceLandmark Detector](https://assetstore.unity.com/packages/tools/integration/dlib-facelandmark-detector-64314)

## Introduce

这是一个在unity中使用OpenCV的入门参考，使用商业插件OpencvForUnity。为确保项目正常运行，请购买并导入最新版OpencvForUnity。项目通过一系列独立的例子，由浅入深一步步掌握opencv图片处理的方法。

该插件的官方example里面有非常丰富的实例，但是基础API讲解并没有，不太适合从unity转到opencv开发的新人。这个仓库主要通过收集网上的c++/python opencv文章/源码，API翻译，作为为unity所用的基础入门案例补充。详细的介绍可以阅读我的博客[相关文章](https://blog.csdn.net/mseol/article/category/7372367)。

|order|theme|code|blog|
|---|---|---|---|
|1|图片裁切|√|√|
|2|图片拼接|√|√|
|3|灰度化、二值化|√|√|
|4|边缘检测（梯度）、滤波|√|√|
|5|模糊、锐化|√|√|
|6|反色|√|√|
|7|色度图|√|√|
|8|ROI|√|√|
|9|腐蚀、膨胀|√|√|
|10|直方图|√|√|
|n|orb特征提取|√|√|
|n|全景拼接|√|√|
|n|分类器|√|√|
|n|检测器|×|×|
|n|训练器|×|×|
|n|追踪器|×|×|

## Shortage

The plugin didn't provide all original functions, I list some:

1. Lut(src,lut,dst); //未实现
2. SURF/SIFT //未实现
3. Stitcher //未实现

## References

- [官方网站](https://enoxsoftware.com/);
- [官方论坛](https://forum.unity.com/threads/released-opencv-for-unity.277080/);
