# Unity渲染效果 不断更新

这个项目将集成一些相关的渲染效果，最终成为一个完整的场景

目前已完成：

动态雪地

雪地覆盖

## 动态雪地



<img src="./README.assets/image-20230731163813301.png" alt="image-20230731163813301" style="zoom:50%;" />

采用两张深度图分别记录物体位置与地面位置，计算出运动轨迹，根据轨迹进行凹陷。同时进行边缘检测，高斯模糊，实现雪地边缘的略微突起与内部平滑凹陷。

这个效果位于Assets/SnowGround下：

- CameraRenderOnce.cs: 手动使相机渲染一次，记录地面的初始深度
- ComputeDepthDiffer.cs: 调用ComputeShader计算深度差，并进行边缘检测、高斯模糊等处理（这个脚本的命名没有很准确）
- DifferDepth.compute: 实现上述效果的Compute Shader
- InteractiveSnowGround.shader: 雪地主材质shader
- SnowGroundManager.cs: 为shader进行参数赋值，如相机参数等

待改进的问题：边缘过渡还是不够平滑，例如人物两腿间距小，导致有锐利的突起，看上去更像碎冰。



## 雪地覆盖

这个效果相对而言比较容易，直接用顶点法线与向上的法向量相乘即可得到雪覆盖的效果，加上遮罩贴图来提升质感

在做这个效果中学到的主要是从blender中烘焙贴图，这里场景中使用的均为提前烘焙的静态贴图，因此不对光照做出反应

<img src="./README.assets/image-20230803134434173.png" alt="image-20230803134434173" style="zoom:75%;" />

### 场景破坏

