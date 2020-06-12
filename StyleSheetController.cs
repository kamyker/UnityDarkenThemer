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
	public class StyleSheetController
	{
		public StyleSheet Sheet;

		public StyleSheetController(StyleSheet style )
		{
			Sheet = style;
		}

		//public void InvertColors(  )
		//{
		//	var serialized = new SerializedObject(Sheet);
		//	serialized.Update();
		//	var colors = serialized.FindProperty("colors");
		//	if ( colors != null )
		//	{
		//		for ( int i = 0; i < colors.arraySize; i++ )
		//		{
		//			var property = colors.GetArrayElementAtIndex(i);
		//			//Debug.Log( $"name: {property.name} color: {property.colorValue}" );
		//			property.colorValue = Utils.InvertColor( property.colorValue );
		//		}
		//		serialized.ApplyModifiedProperties();
		//	}
		//}

		public void InvertColorsWithAddition( ColorWithRange[] palette)
		{
			var serialized = new SerializedObject(Sheet);
			serialized.Update();
			var colors = serialized.FindProperty("colors");
			if ( colors != null )
			{
				for ( int i = 0; i < colors.arraySize; i++ )
				{
					var property = colors.GetArrayElementAtIndex(i);
					//Debug.Log( $"name: {property.name} color: {property.colorValue}" );
					property.colorValue = Utils.InvertColorWithAddition( property.colorValue, palette );
				}
				serialized.ApplyModifiedProperties();
			}
		}

		public void ReverseInvertColorsWithAddition( ColorWithRange[] palette )
		{
			var serialized = new SerializedObject(Sheet);
			serialized.Update();
			var colors = serialized.FindProperty("colors");
			if ( colors != null )
			{
				for ( int i = 0; i < colors.arraySize; i++ )
				{
					var property = colors.GetArrayElementAtIndex(i);
					//Debug.Log( $"name: {property.name} color: {property.colorValue}" );
					property.colorValue = Utils.ReverseInvertColorWithAddition( property.colorValue, palette );
				}
				serialized.ApplyModifiedProperties();
			}
		}
	}
}