using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Assets.GradientGenerator;


public class GradientGenerator: EditorWindow
{
   private static EditorWindow currentGradientWindow;
   private static Texture2D icon;
   private static int prevTextureSizze = 256;
   public static Texture2D prevTexture;
   public static Vector2 gradientSize;
   public static Gradient gradientPicker;
   private bool moreOptionsBool;
   private static long timeToCreate;

   public static bool reverse;
   public static GradientGeneratorEnums.BlendType blendType;
   public static GradientGeneratorEnums.GradientDirection direction;
   public static string defaultSaveFolder;
   public static string fileName;
   [Range(0, 1)]
   public static float slide;
   [Range(1, 10000)]
   public static int size = 128;
   private static bool onePixel;
   private static bool alwaysRefresh;

   private static Texture2D background;
   private static string backgroundName;

   [MenuItem("Tools/Gradient Generator")]
   public static void Init() {
      prevTexture = null;
      currentGradientWindow = GetWindow(typeof(GradientGenerator));
      gradientPicker = new Gradient();
      gradientSize = new Vector2(200, 1);
      defaultSaveFolder = string.Empty;
      fileName = "Texture";
      currentGradientWindow.Show();
      currentGradientWindow.minSize = new Vector2(350, 660);

      slide = -1;
      timeToCreate = 0;
      icon = new Texture2D(1, 1);
      icon.LoadImage(File.ReadAllBytes(Path.Combine(Application.dataPath, "GradientGenerator", "Gradient Generator-logos_transparent.png")));
   }

   public void OnGUI() {
      GUIStyle style = GUI.skin.GetStyle("Label");
      style.richText = true;
      GUIStyle myStyle = new GUIStyle(GUI.skin.label);
      myStyle.margin = new RectOffset(11, 22, 33, 44);

      if(GUI.Button(new Rect(position.width - 28, 8, 24, 24), "?")) {
         HelpMenu helpMenu = (HelpMenu)GetWindow(typeof(HelpMenu), false, "Gradient Generator Help");
         helpMenu.Show();
         helpMenu.Init(icon);
      }

      GUI.DrawTexture(new Rect((position.width - 256) / 2, 10, 256, 20), icon);
      EditorGUILayout.Space(50);
      gradientPicker = EditorGUILayout.GradientField("Gradient", gradientPicker);
      EditorGUILayout.BeginHorizontal();
      direction = (GradientGeneratorEnums.GradientDirection)EditorGUILayout.EnumPopup("Gradient Direction", direction);
      float originalValue = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = 50;
      reverse = EditorGUILayout.Toggle("Reverse", reverse, GUILayout.Width(120));
      EditorGUIUtility.labelWidth = originalValue;
      EditorGUILayout.EndHorizontal();

      if(direction == GradientGeneratorEnums.GradientDirection.Radial) {
         gradientSize.y = prevTextureSizze;
         gradientSize.x = prevTextureSizze;
         if(slide == -1)
            slide = 0.5f;
         slide = EditorGUILayout.Slider("Scale", slide, 0f, 1f);
      } else if(direction == GradientGeneratorEnums.GradientDirection.Horizontal) {
         gradientSize.y = 1;
         gradientSize.x = prevTextureSizze;
      } else if(direction == GradientGeneratorEnums.GradientDirection.Vertical) {
         gradientSize.x = 1;
         gradientSize.y = prevTextureSizze;
      }
      DrawBackgroundPicker();
      DrawDivider();
      GUILayout.BeginArea(new Rect((Screen.width / 2) - 40, (direction == GradientGeneratorEnums.GradientDirection.Radial) ? 180 : 160, 80, 100));
      EditorGUILayout.LabelField("<size=18><b>Preview</b></size>", style);
      GUILayout.EndArea();
      if(GUILayout.Button("Refresh")) {
         UpdateGradient(out prevTexture, prevTextureSizze, background);
      }
      var textureRect = EditorGUILayout.GetControlRect();
      textureRect.x = (position.width - prevTextureSizze) / 2;
      textureRect.y += 10;
      textureRect.width = prevTextureSizze;
      textureRect.height = prevTextureSizze;

      GUI.DrawTexture(textureRect, background?? Helpers.GetDefaultBackground(prevTextureSizze));
      if(prevTexture != null) {
         GUI.DrawTexture(textureRect, prevTexture);
      }
      GUILayout.Space(textureRect.height);
      EditorGUILayout.BeginHorizontal();
      if(GUILayout.Button("128")) {
         size = 128;
      }
      if(GUILayout.Button("256")) {
         size = 256;
      }
      if(GUILayout.Button("512")) {
         size = 512;
      }
      if(GUILayout.Button("1024")) {
         size = 1024;
      }
      if(GUILayout.Button("2048")) {
         size = 2048;
      }
      EditorGUILayout.EndHorizontal();
      size = EditorGUILayout.IntField("Size", size);
      if(size == 0)
         size = 1;
      defaultSaveFolder = EditorGUILayout.TextField("Save directory", defaultSaveFolder);
      fileName = EditorGUILayout.TextField("File name", fileName);
      EditorGUILayout.Space(10);
      EditorGUILayout.LabelField("<b>Output: " + "Assets/" + defaultSaveFolder
                                 + (defaultSaveFolder == string.Empty ? "" : "/")
                                 + fileName + ".png</b>", style);
      if(GUILayout.Button("Create", GUILayout.Height(30))) {
         CreateGradient();
      }

      EditorGUILayout.Space(10);
      moreOptionsBool = EditorGUILayout.Foldout(moreOptionsBool, "More Options");
      if(moreOptionsBool) {
         currentGradientWindow.minSize = new Vector2(350, 710);

         GUILayout.BeginArea(new Rect(20, (direction == GradientGeneratorEnums.GradientDirection.Radial) ? 680 : 665, 350, 100));
         if(direction == GradientGeneratorEnums.GradientDirection.Horizontal) {
            onePixel = EditorGUILayout.Toggle("Crunch to 1px height", onePixel, GUILayout.Width(120));
         }
         if(direction == GradientGeneratorEnums.GradientDirection.Vertical) {
            onePixel = EditorGUILayout.Toggle("Crunch to 1px width", onePixel, GUILayout.Width(120));
         }
         if(direction == GradientGeneratorEnums.GradientDirection.Radial) {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Toggle("Crunch to 1px width", false, GUILayout.Width(120));
            EditorGUI.EndDisabledGroup();
         }

         alwaysRefresh = EditorGUILayout.Toggle("Always refresh", alwaysRefresh, GUILayout.Width(120));

         if(timeToCreate != 0) {
            GUILayout.Label($"Last image create time: { timeToCreate } milliseconds");
         }
         GUILayout.EndArea();
      } else {
         if(currentGradientWindow.minSize != new Vector2(350, 660))
            currentGradientWindow.minSize = new Vector2(350, 660);
      }

      if(alwaysRefresh)
         UpdateGradient(out prevTexture, prevTextureSizze, background);
   }

   private void DrawBackgroundPicker() {
      EditorGUILayout.Space(10);
      blendType = (GradientGeneratorEnums.BlendType)EditorGUILayout.EnumPopup("Blend Type", blendType);
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("Background: " + (string.IsNullOrEmpty(backgroundName) ? "None" : backgroundName));
      if(GUILayout.Button("Select File...")) {
         var imagePath = EditorUtility.OpenFilePanel("Select Image", Application.dataPath, "png,jpg,jpeg,tiff,bmp");
         if(Helpers.IsValidExtension(imagePath)) {
            background = new Texture2D(prevTextureSizze, prevTextureSizze);
            try {
               var data = File.ReadAllBytes(Path.GetFullPath(imagePath));
               background.LoadImage(data);
               backgroundName = Path.GetFileName(imagePath);
               background = Helpers.ScaleTexture(background, prevTextureSizze, prevTextureSizze);
               Debug.Log("Succesfully imported background: " + imagePath);
            } catch(Exception) {
               Debug.LogError("File not an image!");
            }
         } else {
            Debug.LogError("File not an image!");
         }
      }
      if(GUILayout.Button("Clear")) {
         background = null;
         backgroundName = string.Empty;
      }
      EditorGUILayout.EndHorizontal();
   }

   public static void DrawDivider() {
      EditorGUILayout.Space(10);
      var rect = EditorGUILayout.BeginHorizontal();
      Handles.color = Color.gray;
      Handles.DrawLine(new Vector2(rect.x - 15, rect.y), new Vector2(rect.width + 15, rect.y));
      EditorGUILayout.EndHorizontal();
      EditorGUILayout.Space(30);
   }

   public void UpdateGradient(out Texture2D texture, int size, Texture2D bg) {
      Color tempCol;
      if(direction == GradientGeneratorEnums.GradientDirection.Radial) {
         texture = new Texture2D(size, size);
         for(int i = 0; i < size; i++) {
            for(int j = 0; j < size; j++) {
               Vector2 CurrPos = new Vector2((float)i / (float)size, (float)j / (float)size);
               Vector2 MiddlePos = new Vector2(0.5f, 0.5f);
               float t = (CurrPos - MiddlePos).sqrMagnitude * Mathf.Sqrt(2);
               float time = t + (slide - 0.5f) * 2;
               if(reverse) {
                  tempCol = gradientPicker.Evaluate(Mathf.Abs(1 - time));
               } else {
                  tempCol = gradientPicker.Evaluate(time);
               }

               if(background != null) {
                  Color bgPixel = bg.GetPixelBilinear((float)i / (float)size, (float)j / (float)size);
                  tempCol = Helpers.GetFinalColor(blendType, tempCol, bgPixel);
                  tempCol.a = bgPixel.a;

               } else {
                  tempCol = Helpers.GetFinalColor(blendType, tempCol, Color.white);
               }

               if(background != null) {
                  if(background.GetPixel(i, j).a >= 0)
                     texture.SetPixel(i, j, tempCol);
               } else {
                  texture.SetPixel(i, j, tempCol);
               }
            }
         }
      } else {
         texture = new Texture2D(size, size);
         for(int i = 0; i < size; i++) {
            for(int j = 0; j < size; j++) {
               tempCol = reverse
                        ? gradientPicker.Evaluate(1 - ((float)i / size))
                        : gradientPicker.Evaluate((float)i / size);
               if(background != null) {
                  Color bgPixel = bg.GetPixelBilinear((float)i / (float)size, (float)j / (float)size);
                  tempCol = Helpers.GetFinalColor(blendType, tempCol, bgPixel);
                  tempCol.a = bgPixel.a;

               } else {
                  tempCol = Helpers.GetFinalColor(blendType, tempCol, Color.white);
               }
               if(direction == GradientGeneratorEnums.GradientDirection.Horizontal) {
                  if(background != null) {
                     if(background.GetPixel(i, j).a > 0)
                        texture.SetPixel(i, j, tempCol);
                     else
                        texture.SetPixel(i, j, Color.clear);
                  } else {
                     texture.SetPixel(i, j, tempCol);
                  }
               } else {
                  if(background != null) {
                     if(background.GetPixel(j, i).a > 0)
                        texture.SetPixel(j, i, tempCol);
                     else
                        texture.SetPixel(j, i, Color.clear);
                  } else {
                     texture.SetPixel(j, i, tempCol);
                  }
               }
            }
         }
      }
      texture.filterMode = FilterMode.Point;
      texture.Apply();
   }

   public void CreateGradient() {
      Texture2D bg = (background != null) ? Helpers.ScaleTexture(background, 256, 256) : new Texture2D(size, size);
      UpdateGradient(out var finalTexture, 256, bg);
      Texture2D final = Helpers.ScaleTexture(finalTexture, size, size);
      System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
      st.Start();
      try {
         File.WriteAllBytes(Application.dataPath + (defaultSaveFolder == string.Empty ? "" : "/") + defaultSaveFolder + "/" + fileName + ".png", final.EncodeToPNG());
         timeToCreate = st.ElapsedMilliseconds;
      } catch(Exception e) {
         Debug.LogError(e.Message);
      }
      AssetDatabase.Refresh();
   }
}