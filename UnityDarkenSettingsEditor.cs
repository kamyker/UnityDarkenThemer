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


namespace KS.UnityDarken
{

	[CustomEditor( typeof( UnityDarkenSettings ) )]
	public class UnityDarkenSettingsEditor : Editor
	{
		UnityDarkenSettings t;
		SerializedProperty styleSheets;
		SerializedProperty styleSheetsInverted;
		SerializedProperty additiveColors;
		SerializedProperty runInvertGui;
		SerializedProperty runOnEditorLoad;
		SerializedProperty runCreateTextures;
		void OnEnable()
		{
			t = (UnityDarkenSettings)target;
			styleSheets = serializedObject.FindProperty( "styleSheets" );
			styleSheetsInverted = serializedObject.FindProperty( "styleSheetsInverted" );
			additiveColors = serializedObject.FindProperty( "additiveColors" );
			runInvertGui = serializedObject.FindProperty( "RunInvertGui" );
			runOnEditorLoad = serializedObject.FindProperty( "RunOnEditorLoad" );
			runCreateTextures = serializedObject.FindProperty( "RunCreateTextures" );
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField( runOnEditorLoad );
			EditorGUILayout.PropertyField( additiveColors );

			EditorGUILayout.Space();


			if ( GUILayout.Button( "Create All Textures" ) )
				t.CreateAllTextures();

			if ( GUILayout.Button( "Load Styles From Memory" ) )
				t.LoadStyleSheets();

			if ( GUILayout.Button( "Set GUI.skin backgrounds to local" ) )
				t.SetGUIStylesImageToLocal();

			if ( GUILayout.Button( "Set style sheets images to local" ) )
				t.SetStyleSheetsImageToLocal();

			if ( GUILayout.Button( "Invert All GUI.skin styles" ) )
				t.DarkenAllGUIStyles();

			if ( runInvertGui.boolValue )
			{
				runInvertGui.boolValue = false;
				t.SetGUIStylesImageToLocal( true );
				t.DarkenAllGUIStyles( true );
				serializedObject.ApplyModifiedProperties();
				t.Refresh();
			}

			if ( runCreateTextures.boolValue )
			{
				runCreateTextures.boolValue = false;
				t.CreateAllTextures();
				t.Refresh();
			}

			if ( GUILayout.Button( "Darken All" ) )
				t.DarkenAll();

			if ( GUILayout.Button( "Uncolor All" ) )
				t.UncolorAll();

			EditorGUILayout.Space();

			EditorGUILayout.Space();


			EditorGUILayout.LabelField( "Light Sheets:" );
			for ( int i = 0; i < styleSheets.arraySize; i++ )
			{
				using ( SerializedProperty serializedSheet = styleSheets.GetArrayElementAtIndex( i ) )
				{
					StyleSheet sheet = (StyleSheet)serializedSheet.objectReferenceValue;
					if ( sheet == null )
						continue;
					//var sheetCont = new StyleSheetController(sheet);
					//var serialized = new SerializedObject(MyListRef.objectReferenceValue);
					EditorGUILayout.BeginHorizontal();

					//if (!sheetCont.IsInverted)
					//{
					EditorGUILayout.LabelField( "- " + sheet.name + " " + sheet.GetHashCode() );
					if ( GUILayout.Button( "Darken" ) )
					{
						t.Darken( sheet, false );
					}
				//}

				EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUILayout.Space();

			EditorGUILayout.LabelField( "Inverted Sheets:" );
			for ( int i = 0; i < styleSheetsInverted.arraySize; i++ )
			{
				using ( SerializedProperty serializedSheet = styleSheetsInverted.GetArrayElementAtIndex( i ) )
				{
					StyleSheet sheet = (StyleSheet)serializedSheet.objectReferenceValue;
					EditorGUILayout.BeginHorizontal();
					if ( sheet != null )
					{
						EditorGUILayout.LabelField( "- " + sheet.name + " " + sheet.GetHashCode() ?? "" );
						if ( GUILayout.Button( "Uncolor" ) )
						{
							t.Uncolor( sheet );
						}
					}
				EditorGUILayout.EndHorizontal();
				}
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}