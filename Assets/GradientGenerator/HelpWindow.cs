using UnityEditor;
using UnityEngine;

public class HelpMenu: EditorWindow
{
   private Texture2D icon;
   public void Init(Texture2D _icon) {
      icon = _icon;
      var helpMenu = GetWindow(typeof(HelpMenu));
      helpMenu.minSize = new Vector2(400, 600);
   }
   private void OnGUI() {
      GUIStyle style = GUI.skin.GetStyle("Label");
      style.richText = true;
      style.wordWrap = true;

      GUI.DrawTexture(new Rect((position.width - 256) / 2, 10, 256, 20), icon);
      EditorGUILayout.Space(40);

      GradientGenerator.DrawDivider();

      GUILayout.BeginArea(new Rect((Screen.width / 2) - 25, 60, 50, 130));
      EditorGUILayout.LabelField("<size=16><b>Help</b></size>", style);
      GUILayout.EndArea();

      EditorGUILayout.LabelField("This is a simple gradient generator to be used inside Unity directly, " +
         "allowing you to quickly create and export gradients for UI.", style);
      EditorGUILayout.LabelField("Using unity's gradients, pick a direction, " +
         "set a size and export to whichever folder you desire.", style);
      EditorGUILayout.LabelField("-Crunch to 1px option lets you set the horizontal " +
         "or vertical size of the gradient to 1px as to save a lot of space since they tile. " +
         "<b>Note:</b> This is only supported for horizontal and vertical gradients", style);
      EditorGUILayout.LabelField("-Always refresh option refreshes the window on any change made " +
         "to the gradient, image, scale or blend mode", style);

      EditorGUILayout.Space(30);
      EditorGUILayout.LabelField("Made by Adnan Mujkic (https://thebosniangamedev.com)");
   }
}
