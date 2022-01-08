using System;
using UnityEngine;
using System.IO;

namespace Assets.GradientGenerator
{
   public static class Helpers
   {
      public static Texture2D GetDefaultBackground(int textureSize) {
         int step = 16;
         bool darkPixel = false;
         Texture2D temp = new Texture2D(textureSize, textureSize);
         Color[] colorsDark = new Color[step * step];
         Color[] colorsWhite = new Color[step * step];

         for(int k = 0; k < step * step; k++) {
            colorsDark[k] = new Color(0.3f, 0.3f, 0.3f, 1);
            colorsWhite[k] = new Color(0.9f, 0.9f, 0.9f, 1);
         }

         for(int i = 0; i < textureSize; i += step) {
            for(int j = 0; j < textureSize; j += step) {
               if(darkPixel)
                  temp.SetPixels(i, j, step, step, colorsDark);
               else
                  temp.SetPixels(i, j, step, step, colorsWhite);
               darkPixel = !darkPixel;
            }
            darkPixel = !darkPixel;
         }

         temp.filterMode = FilterMode.Point;
         temp.Apply();

         return temp;
      }

      public static Texture2D ScaleTexture(Texture2D textureIn, int width, int height) {
         textureIn.wrapMode = TextureWrapMode.Clamp;
         Texture2D temp = new Texture2D(width, height);
         temp.wrapMode = TextureWrapMode.Clamp;

         for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
               temp.SetPixel(i, j, textureIn.GetPixelBilinear((float)i / (float)height, (float)j / (float)width));
            }
         }

         temp.Apply();
         return temp;
      }

      public static Color GetFinalColor(GradientGeneratorEnums.BlendType blendType, Color colorIn, Color bgColor) {
         switch(blendType) {
            case GradientGeneratorEnums.BlendType.Opacity:
               return colorIn;
            case GradientGeneratorEnums.BlendType.Screen:
               return new Color(
                     1 - (1 - colorIn.r) * (1 - bgColor.r),
                     1 - (1 - colorIn.g) * (1 - bgColor.g),
                     1 - (1 - colorIn.b) * (1 - bgColor.b),
                     1 - (1 - colorIn.a) * (1 - bgColor.a)
                  );
            case GradientGeneratorEnums.BlendType.Multiply:
               return colorIn * bgColor;
            case GradientGeneratorEnums.BlendType.Overlay:
               return new Color(
                     (float)((Convert.ToInt32(colorIn.r > 0.5)) * (1 - (1 - 2 * (colorIn.r - 0.5)) * (1 - bgColor.r))) + (float)(Convert.ToInt32(colorIn.r <= 0.5) * ((2 * colorIn.r) * bgColor.r)),
                     (float)((Convert.ToInt32(colorIn.g > 0.5)) * (1 - (1 - 2 * (colorIn.g - 0.5)) * (1 - bgColor.g))) + (float)(Convert.ToInt32(colorIn.g <= 0.5) * ((2 * colorIn.g) * bgColor.g)),
                     (float)((Convert.ToInt32(colorIn.b > 0.5)) * (1 - (1 - 2 * (colorIn.b - 0.5)) * (1 - bgColor.b))) + (float)(Convert.ToInt32(colorIn.b <= 0.5) * ((2 * colorIn.b) * bgColor.b)),
                     (float)((Convert.ToInt32(colorIn.a > 0.5)) * (1 - (1 - 2 * (colorIn.a - 0.5)) * (1 - bgColor.a))) + (float)(Convert.ToInt32(colorIn.a <= 0.5) * ((2 * colorIn.a) * bgColor.a))
                  );
            default:
               return colorIn;
         }
      }

      public static bool IsValidExtension(string imagePath) {
         return Path.GetExtension(imagePath) == ".png"
            || Path.GetExtension(imagePath) == ".jpg"
            || Path.GetExtension(imagePath) == ".jpeg"
            || Path.GetExtension(imagePath) == ".tiff"
            || Path.GetExtension(imagePath) == ".bmp";
      }
   }
}
