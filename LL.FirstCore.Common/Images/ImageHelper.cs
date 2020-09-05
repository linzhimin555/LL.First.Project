using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LL.FirstCore.Common.Images
{
    /// <summary>
    /// 图片处理帮助类
    /// 官方地址:https://github.com/SixLabors/ImageSharp
    /// </summary>
    public class ImageHelper
    {
        /// <summary>
        /// 图片合并转换到GIF
        /// 注意:gif图片间隔:100 => 1 second
        /// </summary>
        /// <param name="sourceImages">待合成的图片信息(有序的)</param>
        /// <param name="targetPath">gif保存路径</param>
        /// <returns></returns>
        public static void RegularImageToGif(List<(string path, int duration)> sourceImages, string targetPath)
        {
            int width = 1000, height = 1000;
            using (var gif = new Image<Rgba32>(width, height))
            {
                for (int i = 0; i < sourceImages.Count; i++)
                {
                    using (var image = Image.Load(sourceImages[i].path))
                    {
                        //重置图片到指定输出大小
                        image.Mutate(ctx => ctx.Resize(width, height));
                        //设置图片间隔
                        image.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay = sourceImages[i].duration;
                        //添加图片到gif中
                        gif.Frames.InsertFrame(i, image.Frames.RootFrame);
                    }
                }

                using (var fileStream = new FileStream(targetPath, FileMode.Create))
                {
                    gif.SaveAsGif(fileStream);
                }
            }
        }
    }
}
