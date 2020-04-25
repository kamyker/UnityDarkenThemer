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
		SerializedObject GetTarget;
		SerializedProperty ThisList;
		SerializedProperty styleSheetsInverted;
		SerializedProperty colors;
		SerializedProperty runInvertGui;

		void OnEnable()
		{
			t = (UnityDarkenSettings)target;
			//t.Load();
			GetTarget = new SerializedObject( t );
			ThisList = GetTarget.FindProperty( "styleSheets" );
			styleSheetsInverted = GetTarget.FindProperty( "styleSheetsInverted" );
			colors = GetTarget.FindProperty( "colorsPalette" );
			runInvertGui = GetTarget.FindProperty( "RunInvertGui" );

		}

		public override void OnInspectorGUI()
		{
			GetTarget.Update();

			EditorGUILayout.PropertyField( colors );

			EditorGUILayout.Space();


			if ( GUILayout.Button( "Create All Textures" ) )
				t.CreateAllTextures();

			if ( GUILayout.Button( "Load Styles From Memory" ) )
				t.Load();

			if ( GUILayout.Button( "Set GUI.skin backgrounds to local" ) )
				t.SetGUIStylesImageToLocal();

			if ( GUILayout.Button( "Invert All GUI.skin styles" ) )
				t.DarkenAllGUIStyles();

			if ( runInvertGui.boolValue )
			{
				runInvertGui.boolValue = false;
				t.Load();
				t.DarkenAll();
				t.SetGUIStylesImageToLocal();
				t.DarkenAllGUIStyles();
			}

			if ( GUILayout.Button( "Darken All" ) )
				t.DarkenAll();

			if ( GUILayout.Button( "Uncolor All" ) )
				t.UncolorAll();

			EditorGUILayout.Space();

			EditorGUILayout.Space();


			EditorGUILayout.LabelField( "Light Sheets:" );
			for ( int i = 0; i < ThisList.arraySize; i++ )
			{
				SerializedProperty MyListRef = ThisList.GetArrayElementAtIndex(i);
				StyleSheet sheet = (StyleSheet)MyListRef.objectReferenceValue;
				//var sheetCont = new StyleSheetController(sheet);
				//var serialized = new SerializedObject(MyListRef.objectReferenceValue);
				EditorGUILayout.BeginHorizontal();

				//if (!sheetCont.IsInverted)
				//{
				EditorGUILayout.LabelField( "- " + sheet.name );
				if ( GUILayout.Button( "Darken" ) )
				{
					t.Darken( sheet, false );
				}
				//}

				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.Space();

			EditorGUILayout.LabelField( "Inverted Sheets:" );
			for ( int i = 0; i < styleSheetsInverted.arraySize; i++ )
			{
				SerializedProperty MyListRef = styleSheetsInverted.GetArrayElementAtIndex(i);
				StyleSheet sheet = (StyleSheet)MyListRef.objectReferenceValue;
				EditorGUILayout.BeginHorizontal();
				if ( sheet != null )
				{
					EditorGUILayout.LabelField( "- " + sheet.name ?? "" );
					if ( GUILayout.Button( "Uncolor" ) )
					{
						t.Uncolor( sheet );
					}
				}
				EditorGUILayout.EndHorizontal();
			}

			GetTarget.ApplyModifiedProperties();
		}
	}
}