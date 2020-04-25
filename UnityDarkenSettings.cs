using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using System.Reflection;

namespace KS.UnityDarken
{
	[CreateAssetMenu( fileName = "UnityDarkenSettings", menuName = "Darken/CreateSettings", order = 1 )]
	public class UnityDarkenSettings : ScriptableObject
	{
		[MenuItem( "Tools/Darken/Quick Fix" )]
		public static void QuickFix()
		{
			string[] guids1 = AssetDatabase.FindAssets("t:UnityDarkenSettings", null);

			var darken = AssetDatabase.LoadAssetAtPath<UnityDarkenSettings>( AssetDatabase.GUIDToAssetPath( guids1[0] ) );
			Selection.activeObject = darken;
			darken.RunInvertGui = true;
			darken.Refresh();

		}

		public Color[] colorsPalette = new Color[] { Color.white, Color.grey, Color.black };
		public List<StyleSheet> styleSheets = new List<StyleSheet>();
		public List<StyleSheet> styleSheetsInverted = new List<StyleSheet>();
		[SerializeField,HideInInspector]
		public bool RunInvertGui = false; //used from editor script

		private void Refresh()
		{
			EditorUtility.RequestScriptReload();
			InternalEditorUtility.SwitchSkinAndRepaintAllViews();
		}

		public void Load()
		{
			var allSheets = Resources.FindObjectsOfTypeAll<StyleSheet>().ToList();
			styleSheets.Clear();

			for ( int i = 0; i < allSheets.Count; i++ )
			{
				bool isUnitySheet = (bool)(typeof( StyleSheet ).GetField( "isUnityStyleSheet",
					BindingFlags.NonPublic |
					BindingFlags.Instance |
					BindingFlags.IgnoreCase ).GetValue( allSheets[i] ));

				if ( !styleSheetsInverted.Contains( allSheets[i] ) )
						styleSheets.Add( allSheets[i] );
			}

			Refresh();
		}

		public void DarkenAll()
		{
			#region Old when there was PreviewWindow style, can be used to skip styles
			//int crashProtection = 10000;

			////#if UNITY_2019_3_OR_NEWER
			////		int leftCount = 1;
			////#elif UNITY_2019_2_OR_NEWER
			//int leftCount = 0;
			////#endif

			//while ( styleSheets.Count > leftCount && crashProtection > 0 )
			//{
			//	crashProtection--;
			//	while ( styleSheets.Count > leftCount && crashProtection > 0 )//only preview window style should not be changed
			//	{
			//		crashProtection--;
			//		var style = styleSheets.First(s => s.name != "PreviewWindow");

			//		var darkStyle = FindDarkSheet(style);
			//		if ( darkStyle != null )
			//		{
			//			SwapSheets( style, darkStyle );
			//			styleSheetsInverted.Add( darkStyle );
			//			styleSheets.Remove( darkStyle );
			//		}
			//		else
			//			new StyleSheetController( ref style ).InvertColors();
			//		styleSheetsInverted.Add( style );
			//		styleSheets.Remove( style );
			//	}

			//	Refresh();
			//}
			#endregion

			int crashProtection = 10000;
			while ( styleSheets.Count > 0 && crashProtection > 0 )
			{
				crashProtection--;
				var style = styleSheets[0];

				#region Was used to swap styles to dark versions instead of inverting colors
				//var darkStyle = FindDarkSheet(style);
				//if ( darkStyle != null )
				//{
				//	SwapSheets( style, darkStyle );
				//	styleSheetsInverted.Add( darkStyle );
				//	styleSheets.Remove( darkStyle );
				//}
				//else
				#endregion

				Darken( style, true );
				//new StyleSheetController( ref style ).InvertColors();
				//styleSheetsInverted.Add( style );
				//styleSheets.Remove( style );
			}
			Refresh();
		}

		public void CreateAllTextures()
		{
			var textures = Resources.FindObjectsOfTypeAll<Texture2D>().ToList();

			foreach ( var style in GUI.skin.customStyles )
			{
				AddTexturesToList( style.normal );
				AddTexturesToList( style.onNormal );
				AddTexturesToList( style.onActive );
				AddTexturesToList( style.onFocused );
				AddTexturesToList( style.onHover );
				AddTexturesToList( style.hover );
				AddTexturesToList( style.active );
			}

			Directory.CreateDirectory( Path.Combine( Application.dataPath, "Editor Default Resources", "Icons" ) );
			for ( int i = 0; i < textures.Count; i++ )//textures.Count
			{

				var text = textures[i];

				if ( //text.name.Contains( "dockarea" ) ||    //preview window background
					text.name.Contains( "gameviewbackground" ) ||
					text.name.Contains( "ColorPicker-Hue" ) ||
					text.name.Contains( "OL box" ) ) //text or script background in preview
					continue;


				if ( File.Exists( Application.dataPath + "/Editor Default Resources/Icons/" + text.name + ".png" ) )
					continue;

				var path = AssetDatabase.GetAssetPath(text) + "/" + text.name;

				if (
						path.Contains( "Resources/unity_builtin_extra" ) ||
						path.Contains( "Library/unity editor resources" ) ||
						path == "" )
				{
					Debug.Log( $"copying: {text.name}" );
					RenderTexture tmp = RenderTexture.GetTemporary(
					text.width,
					text.height,
					0,
					RenderTextureFormat.Default,
					RenderTextureReadWrite.Linear);

					Graphics.Blit( text, tmp );
					RenderTexture previous = RenderTexture.active;
					RenderTexture.active = tmp;
					Texture2D myTexture2D = new Texture2D(text.width, text.height, TextureFormat.RGBA32, false);
					myTexture2D.ReadPixels( new Rect( 0, 0, tmp.width, tmp.height ), 0, 0 );
					myTexture2D.Apply();
					var pixels = myTexture2D.GetPixels();
					for ( int j = 0; j < pixels.Length; j++ )
						pixels[j] = Utils.InvertColor( pixels[j] );
					myTexture2D.SetPixels( pixels );
					myTexture2D.Apply();
					RenderTexture.active = previous;
					RenderTexture.ReleaseTemporary( tmp );
					byte[] bytes = myTexture2D.EncodeToPNG();

					File.WriteAllBytes( Path.Combine( Application.dataPath, "Editor Default Resources/Icons", text.name + ".png" ), bytes );
				}
			}
			AssetDatabase.Refresh();
			Refresh();

			void AddTexturesToList( GUIStyleState state )
			{
				if ( state.background != null && !textures.Any( t => t.name == state.background.name ) )
					textures.Add( state.background );

				for ( int i = 0; i < state.scaledBackgrounds.Length; i++ )
				{
					if ( state.scaledBackgrounds[i] != null && !textures.Any( t => t.name == state.scaledBackgrounds[i].name ) )
						textures.Add( state.scaledBackgrounds[i] );
				}
			}
		}

		public void DarkenAllGUIStyles()
		{
			foreach ( var style in GUI.skin.customStyles )
			{
				style.normal.textColor = Utils.InvertColor( style.normal.textColor );
				style.onNormal.textColor = Utils.InvertColor( style.onNormal.textColor );
				style.onActive.textColor = Utils.InvertColor( style.onActive.textColor );
				style.onFocused.textColor = Utils.InvertColor( style.onFocused.textColor );
				style.onHover.textColor = Utils.InvertColor( style.onHover.textColor );
				style.hover.textColor = Utils.InvertColor( style.hover.textColor );
				style.focused.textColor = Utils.InvertColor( style.focused.textColor );
				style.active.textColor = Utils.InvertColor( style.active.textColor );
			}

			Refresh();
		}

		public void SetGUIStylesImageToLocal()
		{
			var icons = AssetDatabase.FindAssets(" t:texture2D", new[] { "Assets/Editor Default Resources/Icons" });
			Debug.Log( icons.Length );
			foreach ( var icon in icons )
			{
				var name = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(icon));
				var texture = (Texture2D)EditorGUIUtility.Load(name);
				if ( texture != null )
					ChangePointer( texture );
			}

			//var skin = GUI.skin;

			foreach ( var style in GUI.skin.customStyles )
			{
				ChangeBackgroundsPointers( style.normal );
				ChangeBackgroundsPointers( style.onNormal );
				ChangeBackgroundsPointers( style.onActive );
				ChangeBackgroundsPointers( style.onFocused );
				ChangeBackgroundsPointers( style.onHover );
				ChangeBackgroundsPointers( style.hover );
				ChangeBackgroundsPointers( style.active );
				ChangeBackgroundsPointers( style.focused );
			}

			//GUI.skin = skin;

			Refresh();

			GUIStyleState ChangeBackgroundsPointers( GUIStyleState state )
			{
				if ( state.background != null )
					ChangePointer( state.background );

				for ( int i = 0; i < state.scaledBackgrounds.Length; i++ )
					if ( state.scaledBackgrounds[i] != null )
						ChangePointer( state.scaledBackgrounds[i] );

				return state;
			}

			void ChangePointer( Texture2D texture )
			{
				try
				{
					var texture2 = (Texture2D)EditorGUIUtility.LoadRequired("Icons/" + texture.name + ".png");
					texture.UpdateExternalTexture( texture2.GetNativeTexturePtr() );
				}
				catch ( Exception e )
				{
					Debug.Log( $"Skipping texture: {texture.name}\n" +
						$"Exception: {e.ToString()}" );
				}
			}
		}

		public void Darken( StyleSheet style, bool skipRefresh )
		{
			var controller = new StyleSheetController(style);
			//var darkStyle = FindDarkSheet(style);
			//if ( darkStyle != null )
			//{
			//	StyleSheet temp = CreateInstance<StyleSheet>();
			//	EditorUtility.CopySerialized( style, temp );
			//	EditorUtility.CopySerialized( darkStyle, style );
			//	EditorUtility.CopySerialized( temp, darkStyle );
			//}
			//else
			controller.InvertColors();
			styleSheetsInverted.Add( style );
			styleSheets.Remove( style );

			if ( !skipRefresh )
				Refresh();
		}

		//public StyleSheet FindDarkSheet( StyleSheet style )
		//{
		//	StyleSheet darkStyle = null;
		//	if ( style.name.Contains( "light" ) || style.name.Contains( "Light" ) )
		//	{
		//		var darkName = style.name.Replace("Light", "Dark").Replace("light", "dark");
		//		darkStyle = styleSheets.FirstOrDefault( s => s.name == darkName );
		//	}
		//	return darkStyle;
		//}

		//public StyleSheet FindLightSheet( StyleSheet style )
		//{
		//	StyleSheet darkStyle = null;
		//	if ( style.name.Contains( "dark" ) || style.name.Contains( "Dark" ) )
		//	{
		//		var darkName = style.name.Replace("Dark", "Light").Replace("dark", "light");
		//		darkStyle = styleSheetsInverted.FirstOrDefault( s => s.name == darkName );
		//	}
		//	return darkStyle;
		//}

		public void UncolorAll()
		{
			int crashSafer = 10000;
			while ( styleSheetsInverted.Count > 0 && crashSafer > 0 )
			{
				crashSafer--;
				var style = styleSheetsInverted[0];
				//var lightStyle = FindLightSheet(style);
				//if ( lightStyle != null )
				//{
				//	SwapSheets( style, lightStyle );
				//	styleSheetsInverted.Remove( lightStyle );
				//	styleSheets.Add( lightStyle );
				//}
				//else
				if ( style != null )
				{
					new StyleSheetController( style ).InvertColors();
					styleSheets.Add( style );
				}
				styleSheetsInverted.Remove( style );
			}

			Refresh();
		}

		public void Uncolor( StyleSheet sheet )
		{
			var controller = new StyleSheetController(sheet);
			controller.InvertColors();
			styleSheets.Add( sheet );
			styleSheetsInverted.Remove( sheet );

			Refresh();
		}

		//void SwapSheets( StyleSheet a, StyleSheet b )
		//{
		//	StyleSheet temp = CreateInstance<StyleSheet>();
		//	EditorUtility.CopySerialized( a, temp );
		//	EditorUtility.CopySerialized( b, a );
		//	EditorUtility.CopySerialized( temp, b );
		//}

	}

	public static class Utils
	{
		public static Color InvertColor( Color color )
		//tried also hsv inversion but this looks better
		{
			return new Color( 1 - color.r, 1 - color.g, 1 - color.b, color.a );
		}
	}
}