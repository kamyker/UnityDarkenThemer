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
using System.Threading.Tasks;

namespace KS.UnityDarken
{
	[CreateAssetMenu( fileName = "UnityDarkenSettings", menuName = "Darken/CreateSettings", order = 1 )]
	public class UnityDarkenSettings : ScriptableObject
	{
		static string loadedOnceKey => $"KS.UnityDarken.loadedOnce";
		static bool loadedOnce
		{
			get => SessionState.GetBool( loadedOnceKey, false );
			set => SessionState.SetBool( loadedOnceKey, value );
		}

		[Header("Additive colors based on brightness of light skin")]
		[SerializeField]
		ColorWithRange[] additiveColors = new ColorWithRange[] {
				new ColorWithRange()
				{
					BrightnessRangeStart = 0,
					BrightnessRangeEnd = 1f,
					ColorToAdd = new Gradient()
					{ colorKeys = new GradientColorKey[]
						{
							new GradientColorKey(Color.black, 0f),
							new GradientColorKey(Color.blue, 1f)
						}
					},
					Multiplier = -0.05f
				}
		};

		public bool RunOnEditorLoad = true;
		[SerializeField,HideInInspector]
		public bool RunInvertGui = false; //used from editor script
		public bool RunCreateTextures = false; //used from editor script
		[SerializeField] List<StyleSheet> styleSheets = new List<StyleSheet>();
		[SerializeField] List<StyleSheet> styleSheetsInverted = new List<StyleSheet>();

		[InitializeOnLoadMethod]
		static void OnProjectLoadedInEditor()
		{
			var soPath = AssetDatabase.FindAssets("t:"+ nameof( UnityDarkenSettings )).FirstOrDefault();
			var so = AssetDatabase.LoadAssetAtPath<UnityDarkenSettings>( AssetDatabase.GUIDToAssetPath( soPath ) );

			if ( so != null && so.RunOnEditorLoad && !loadedOnce )
			{
				EditorApplication.delayCall += async () =>
				{
					// give editor enough time to complete initial load
					await Task.Delay( 500 );
					loadedOnce = true;
					var t = GetOrCreateSettings();
					t.ClearLists();
					QuickFix( t );
				};
			}
		}

		[MenuItem( "Tools/Darken/Create Textures" )]
		static void CreateTextures()
		{
			var t = GetOrCreateSettings();
			t.RunCreateTextures = true;
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = t;
		}

		[MenuItem( "Tools/Darken/Quick Fix" )]
		static void QuickFix()
		{
			QuickFix( null );
		}
		static void QuickFix( UnityDarkenSettings t = null )
		{
			if ( t == null )
				t = GetOrCreateSettings();
			t.LoadStyleSheets( true );
			t.DarkenAll( true );
			t.RunInvertGui = true;
			EditorUtility.FocusProjectWindow();
			EditorUtility.SetDirty( t );
			Selection.activeObject = t;
		}

		[MenuItem( "Tools/Darken/Force Unity RefreshGlobalStyleCatalog" )]
		static void RefreshGlobalStyleCatalog()
		{
			var refresh = typeof( UnityEditor.Experimental.EditorResources )
				.GetField( "s_RefreshGlobalStyleCatalog", BindingFlags.NonPublic | BindingFlags.Static );
			refresh.SetValue( null, true );
		}

		static UnityDarkenSettings GetOrCreateSettings()
		{
			string[] guids = AssetDatabase.FindAssets("t:"+ nameof( UnityDarkenSettings ));

			if ( guids.Length == 0 )
			{
				var asset = CreateInstance<UnityDarkenSettings>();
				const string path = "Assets/UnityDarkenThemer/UnityDarkenThemerSettings.asset";
				Directory.CreateDirectory( System.IO.Path.GetDirectoryName( path ) );
				AssetDatabase.CreateAsset( asset, path );
				AssetDatabase.SaveAssets();
			}

			guids = AssetDatabase.FindAssets( "t:" + nameof( UnityDarkenSettings ) );
			return AssetDatabase.LoadAssetAtPath<UnityDarkenSettings>( AssetDatabase.GUIDToAssetPath( guids[0] ) );
		}

		public void ClearLists()
		{
			styleSheets.Clear();
			styleSheetsInverted.Clear();
		}

		public void Refresh()
		{
			RefreshGlobalStyleCatalog();
			InternalEditorUtility.RepaintAllViews();
			RefreshGlobalStyleCatalog();
			EditorUtility.RequestScriptReload();
			RefreshGlobalStyleCatalog();
		}

		public void LoadStyleSheets( bool skipRefresh = false )
		{
			var allSheets = Resources.FindObjectsOfTypeAll<StyleSheet>();

			styleSheets.Clear();

			foreach ( var sheet in allSheets )
			{
				//}
				//for ( int i = 0; i < allSheets.Count; i++ )
				//{
				bool isUnitySheet = (bool)(typeof( StyleSheet ).GetField( "isUnityStyleSheet",
					BindingFlags.NonPublic |
					BindingFlags.Instance |
					BindingFlags.IgnoreCase ).GetValue( sheet ));

				if ( !styleSheetsInverted.Contains( sheet ) )
					styleSheets.Add( sheet );
			}

			EditorUtility.SetDirty( this );

			if ( !skipRefresh )
				Refresh();
		}

		public void DarkenAll( bool skipRefresh = false )
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
			}
			EditorUtility.SetDirty( this );
			if ( !skipRefresh )
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

			Directory.CreateDirectory( Path.Combine( "Assets", "Editor Default Resources", "Icons" ) );
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

		public void DarkenAllGUIStyles( bool skipRefresh = false )
		{
			foreach ( var style in GUI.skin.customStyles )
			{
				style.normal.textColor = Utils.InvertColorWithAddition( style.normal.textColor, additiveColors );
				style.onNormal.textColor = Utils.InvertColorWithAddition( style.onNormal.textColor, additiveColors );
				style.onActive.textColor = Utils.InvertColorWithAddition( style.onActive.textColor, additiveColors );
				style.onFocused.textColor = Utils.InvertColorWithAddition( style.onFocused.textColor, additiveColors );
				style.onHover.textColor = Utils.InvertColorWithAddition( style.onHover.textColor, additiveColors );
				style.hover.textColor = Utils.InvertColorWithAddition( style.hover.textColor, additiveColors );
				style.focused.textColor = Utils.InvertColorWithAddition( style.focused.textColor, additiveColors );
				style.active.textColor = Utils.InvertColorWithAddition( style.active.textColor, additiveColors );
			}
			if ( !skipRefresh )
				Refresh();
		}

		//this doesn't work or "SetGUIStylesImageToLocal" already does it
		public void SetStyleSheetsImageToLocal( bool skipRefresh = false )
		{
			foreach ( var style in styleSheetsInverted )
			{
				var flags = BindingFlags.Instance | BindingFlags.NonPublic;
				var imgs = typeof( StyleSheet )
					.GetField( "scalableImages", flags )
					.GetValue(style);
				foreach ( object scalableImg in (Array)imgs )
				{
					var normal = scalableImg.GetType().GetField( "normalImage", flags ).GetValue( scalableImg ) as Texture2D;
					var hRes = scalableImg.GetType().GetField( "highResolutionImage", flags ).GetValue( scalableImg ) as Texture2D;
					if ( normal != null )
						ChangePointer( normal );
					if ( hRes != null )
						ChangePointer( hRes );
				}
			}

			if ( !skipRefresh )
				Refresh();

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

		public void SetGUIStylesImageToLocal( bool skipRefresh = false )
		{
			var assembly = typeof(EditorStyles).Assembly;
			var a = assembly.GetType( "UnityEditor.DockArea" ).GetNestedType("Styles", BindingFlags.Static | BindingFlags.NonPublic);
			foreach ( var item in a.GetFields( BindingFlags.Static | BindingFlags.NonPublic ) )
			{
				Debug.Log( $"variable: {item.Name}" );
			}
			var b = a.GetField( "dockTitleBarStyle", BindingFlags.Static | BindingFlags.Public);

			var dockTitleBarStyleInfo = b.GetValue(null) as GUIStyle;
			//Debug.Log( $"dockTitleBarStyleInfo.normal.background.name: {dockTitleBarStyleInfo.normal.background.name}" );
			//dockTitleBarStyleInfo.fixedHeight = 5;
			dockTitleBarStyleInfo.fontStyle = FontStyle.Normal;

			var path = "Assets/Editor Default Resources/Icons";
			Directory.CreateDirectory( path );
			var icons = AssetDatabase.FindAssets(" t:texture2D", new[] { path });
			//Debug.Log( icons.Length );
			foreach ( var icon in icons )
			{
				var name = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(icon));
				var texture = (Texture2D)EditorGUIUtility.Load(name);
				//if ( name == "dockarea back@2x" )
				//	;
				if ( texture != null )
					ChangePointer( texture );
			}

			var skin = GUI.skin;

			foreach ( var style in GUI.skin.customStyles )
			{
				//if ( style.name == "dragtab" )
				//	;

				//style.fontStyle = FontStyle.Bold;

				ChangeBackgroundsPointers( style.normal );
				ChangeBackgroundsPointers( style.onNormal );
				ChangeBackgroundsPointers( style.onActive );
				ChangeBackgroundsPointers( style.onFocused );
				ChangeBackgroundsPointers( style.onHover );
				ChangeBackgroundsPointers( style.hover );
				ChangeBackgroundsPointers( style.active );
				ChangeBackgroundsPointers( style.focused );
			}

			GUI.skin = skin;


			if ( !skipRefresh )
				Refresh();

			GUIStyleState ChangeBackgroundsPointers( GUIStyleState state )
			{
				//state.background. = Utils.InvertColor( state.textColor );
				if ( state.background != null )
					state.background = ChangePointer( state.background );

				for ( int i = 0; i < state.scaledBackgrounds.Length; i++ )
				{
					if ( state.scaledBackgrounds[i] != null )
						state.scaledBackgrounds[i] = ChangePointer( state.scaledBackgrounds[i] );
				}

				return state;
			}

			Texture2D ChangePointer( Texture2D texture )
			{
				try
				{
					if ( !File.Exists( Application.dataPath + "/Editor Default Resources/Icons/" + texture.name + ".png" ))
					{
						Debug.Log( $"Skipping texture: {texture.name}\n" +
							$"Exception: {texture.name}" );
						return null;
					}
					var textureInverted = (Texture2D)EditorGUIUtility.LoadRequired("Icons/" + texture.name + ".png");
					texture.UpdateExternalTexture( textureInverted.GetNativeTexturePtr() );
					return textureInverted;
				}
				catch ( Exception e )
				{
					Debug.Log( $"Skipping texture: {texture.name}\n" +
						$"Exception: {e.ToString()}" );
					return texture;
				}
			}
			//void ChangePointer( Texture2D texture )
			//{
			//	try
			//	{
			//		texture = (Texture2D)EditorGUIUtility.LoadRequired( "Icons/" + texture.name + ".png" );
			//		//var texture2 = (Texture2D)EditorGUIUtility.LoadRequired("Icons/" + texture.name + ".png");
			//		//texture.UpdateExternalTexture( texture2.GetNativeTexturePtr() );
			//	}
			//	catch ( Exception e )
			//	{
			//		Debug.Log( $"Skipping texture: {texture.name}\n" +
			//			$"Exception: {e.ToString()}" );
			//	}
			//}
		}

		public void Darken( StyleSheet style, bool skipRefresh )
		{
			//AssetDatabase.GetAssetPath( style )
			//if( style.

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
			controller.InvertColorsWithAddition( additiveColors );
			styleSheetsInverted.Add( style );
			styleSheets.Remove( style );
			EditorUtility.SetDirty( this );

			if ( !skipRefresh )
				Refresh();
		}

		public StyleSheet FindDarkSheet( StyleSheet style )
		{
			StyleSheet darkStyle = null;
			if ( style.name.Contains( "light" ) || style.name.Contains( "Light" ) )
			{
				var darkName = style.name.Replace("Light", "Dark").Replace("light", "dark");
				darkStyle = styleSheets.FirstOrDefault( s => s.name == darkName );
			}
			return darkStyle;
		}

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
					new StyleSheetController( style ).ReverseInvertColorsWithAddition( additiveColors );
					styleSheets.Add( style );
				}
				styleSheetsInverted.Remove( style );
			}

			Refresh();
		}

		public void Uncolor( StyleSheet sheet )
		{
			var controller = new StyleSheetController(sheet);
			controller.ReverseInvertColorsWithAddition( additiveColors );
			styleSheets.Add( sheet );
			styleSheetsInverted.Remove( sheet );

			EditorUtility.SetDirty( this );
			Refresh();
		}

		void SwapSheets( StyleSheet a, StyleSheet b )
		{
			StyleSheet temp = CreateInstance<StyleSheet>();
			EditorUtility.CopySerialized( a, temp );
			EditorUtility.CopySerialized( b, a );
			EditorUtility.CopySerialized( temp, b );
		}

	}
}