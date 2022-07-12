using System;
using UnityEngine;
using System.IO;

namespace Assets.GradientGenerator
{
   public static class Helpers
   {
      public enum GradientDirection { Horizontal = 0, Vertical = 1, Radial = 2, Angle = 3 }
      public enum BlendType { Opacity, Screen, Multiply, Overlay }

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

      public static Texture2D ComputeGradientUnderAngle(float angle, int size, Texture2D oldTex) {
         oldTex = ScaleTexture(oldTex, (int)(size * 1.1f), (int)(size * 1.1f));
         var texture = new Texture2D(size, size);
         angle = Mathf.Deg2Rad * angle;

         float angleSine = Mathf.Sin(angle);
         float angleCos = Mathf.Cos(angle);
         float fSize = (float)size;
         float x0 = ((fSize / 2.0f) - (angleCos * (fSize / 2.0f)) - (angleSine * (fSize / 2.0f)));
         float y0 = ((fSize / 2.0f) - (angleCos * (fSize / 2.0f)) + (angleSine * (fSize / 2.0f)));

         for (int y = 0; y < size; y++)
         {
            for (int x = 0; x < size; x++)
            {
               float xFinal = (angleCos * x) + (angleSine * y) + x0;
               float yFinal = (-angleSine * x) + (angleCos * y) + y0;

               texture.SetPixel(x, y, oldTex.GetPixel((int)Mathf.Clamp(xFinal,0,fSize), (int)Mathf.Clamp(yFinal, 0, fSize)));
            }
         }
         texture.Apply();
         return texture;
      }

      public static Texture2D ScaleTexture(Texture2D textureIn, int width, int height) {
         textureIn.wrapMode = TextureWrapMode.Clamp;
         Texture2D temp = new Texture2D(width, height);
         Color[] transparent = new Color[width * height];
         for(int i = 0; i < transparent.Length; i++) {
            transparent[i] = Color.clear;
         }
         temp.SetPixels(0, 0, width, height, transparent);
         temp.wrapMode = TextureWrapMode.Clamp;
         int widthNeeded = 0;
         int heightNeeded = 0;
         int finalWidth = width;
         int finalHeight = height;
         float stepX = 1;
         float stepY = 1;

         float aspectIn = (float)textureIn.width / (float)textureIn.height;
         float aspectOut = (float)width / (float)height;
         
         if(aspectIn != aspectOut) {
            if(aspectIn < 1) {
               finalWidth = (int)((float)width * aspectIn);
               widthNeeded = (int)(width - finalWidth) / 2;
               stepX = (float)textureIn.width / (float)width;
               stepY = (float)textureIn.height / (float)height;
            } else {
               finalHeight = (int)((float)height * aspectIn);
               heightNeeded = (int)(height - finalHeight) / 2;
               stepY = (float)textureIn.height / (float)height;
            }
         }

         for(int i = widthNeeded; i < finalWidth + widthNeeded; i++) {
            for(int j = heightNeeded; j < finalHeight + heightNeeded; j++) {
               float u = ((float)i * stepX) / (float)textureIn.width;
               float v = ((float)j * stepY) / (float)textureIn.height;
               temp.SetPixel(i, j, textureIn.GetPixelBilinear(u, v));
            }
         }

         temp.Apply();
         return temp;
      }

      public static Color GetFinalColor(BlendType blendType, Color colorIn, Color bgColor) {
         float alpha = colorIn.a;
         colorIn = new Color(colorIn.r, colorIn.g, colorIn.b, 1);
         Color finalColor;

         switch(blendType) {
            case BlendType.Screen:
               finalColor = new Color(
                     1 - (1 - colorIn.r) * (1 - bgColor.r),
                     1 - (1 - colorIn.g) * (1 - bgColor.g),
                     1 - (1 - colorIn.b) * (1 - bgColor.b),
                     1 - (1 - colorIn.a) * (1 - bgColor.a)
                  );
               break;
            case BlendType.Multiply:
               finalColor = colorIn * bgColor;
               break;
            case BlendType.Overlay:
               finalColor = new Color(
                     (float)((Convert.ToInt32(colorIn.r > 0.5)) * (1 - (1 - 2 * (colorIn.r - 0.5)) * (1 - bgColor.r))) + (float)(Convert.ToInt32(colorIn.r <= 0.5) * ((2 * colorIn.r) * bgColor.r)),
                     (float)((Convert.ToInt32(colorIn.g > 0.5)) * (1 - (1 - 2 * (colorIn.g - 0.5)) * (1 - bgColor.g))) + (float)(Convert.ToInt32(colorIn.g <= 0.5) * ((2 * colorIn.g) * bgColor.g)),
                     (float)((Convert.ToInt32(colorIn.b > 0.5)) * (1 - (1 - 2 * (colorIn.b - 0.5)) * (1 - bgColor.b))) + (float)(Convert.ToInt32(colorIn.b <= 0.5) * ((2 * colorIn.b) * bgColor.b)),
                     (float)((Convert.ToInt32(colorIn.a > 0.5)) * (1 - (1 - 2 * (colorIn.a - 0.5)) * (1 - bgColor.a))) + (float)(Convert.ToInt32(colorIn.a <= 0.5) * ((2 * colorIn.a) * bgColor.a))
                  );
               break;
            default:
               finalColor = colorIn;
               break;
         }

         return Color.Lerp(bgColor, finalColor, alpha);
      }

      public static bool IsValidExtension(string imagePath) {
         return Path.GetExtension(imagePath) == ".png"
            || Path.GetExtension(imagePath) == ".jpg"
            || Path.GetExtension(imagePath) == ".jpeg"
            || Path.GetExtension(imagePath) == ".tiff"
            || Path.GetExtension(imagePath) == ".bmp";
      }

      public static bool ExpandedMenu(GradientDirection dir) {
         return (dir == GradientDirection.Radial || dir == GradientDirection.Angle);
      }
   }
}
