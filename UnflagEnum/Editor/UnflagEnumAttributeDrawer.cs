using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

[DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
public class UnflagEnumAttributeDrawer<T> : OdinAttributeDrawer<UnflagEnumAttribute, T> where T : Enum
{
	private GUIContent m_buttonContent = new GUIContent();
	private SdfIconType m_sdfIcon;
	
	public override bool CanDrawTypeFilter(Type type)
	{
		return type.IsEnum && EnumTypeUtilities<T>.IsFlagEnum;
	}

	protected override void Initialize() => UpdateButtonContent();

	private void UpdateButtonContent()
	{
		m_buttonContent.text = !EditorGUI.showMixedValue
			? UnflagEnumSelector<T>.GetValueStringReflection(ValueEntry.SmartValue, out m_sdfIcon)
			: "-";
	}

	protected override void DrawPropertyLayout(GUIContent label)
	{
		var rect = EditorGUILayout.GetControlRect(label != null);

		if (label == null)
			rect = EditorGUI.IndentedRect(rect);
		else
			rect = EditorGUI.PrefixLabel(rect, label);

		var clicked = EditorGUI.DropdownButton(rect, new GUIContent(), FocusType.Keyboard);
		
		if (Event.current.type == EventType.Repaint)
		{
			if (m_sdfIcon != SdfIconType.None)
				SdfIcons.DrawIcon(rect.TakeFromLeft(rect.height * 1.2f).Padding(2f, 0.0f, 3f, 3f), m_sdfIcon);
		}

		var labelRect = rect;
		labelRect.width -= 16;
		labelRect.x += 2;
		GUI.Label(labelRect,m_buttonContent);
		
		if (!clicked)
			return;

		var selector = new UnflagEnumSelector<T>();
		
		selector.SetSelection(ValueEntry.SmartValue);
		selector.ShowInPopup(rect.position + Vector2.up * rect.height);

		selector.SelectionChanged += x =>
		{
			ValueEntry.Property.Tree.DelayAction(() =>
			{
				ValueEntry.SmartValue = x.FirstOrDefault();

				UpdateButtonContent();
			});
		};
	}

	public class UnflagEnumSelector<TEnum> : EnumSelector<TEnum> where TEnum : Enum
	{
		public bool DrawOnlySingleOptions { get; set; } = true;

		private TEnum m_currentValue;

		protected override void BuildSelectionTree(OdinMenuTree tree)
		{
			base.BuildSelectionTree(tree);

			if (DrawOnlySingleOptions)
				tree.MenuItems.RemoveAll(x => !IsSingleBit(((EnumTypeUtilities<TEnum>.EnumMember)x.Value).Value));
			else
				tree.MenuItems.RemoveAll(x =>
					((EnumTypeUtilities<TEnum>.EnumMember)x.Value).Value.Equals(GetZeroValue()));

			tree.Selection.SupportsMultiSelect = false;
		}

		public override void SetSelection(TEnum selected)
		{
			m_currentValue = selected;
			base.SetSelection(selected);
		}

		public override IEnumerable<TEnum> GetCurrentSelection()
		{
			var selection =  base.GetCurrentSelection();
			var currentEnum = selection.First();

			var currentEnumInt = Convert.ToInt64(currentEnum);
			var previousEnumInt = Convert.ToInt64(m_currentValue);
			var newEnumSelected = (TEnum) Enum.ToObject(typeof (TEnum), (currentEnumInt ^ previousEnumInt));

			m_currentValue = newEnumSelected;
			SetCurrentValue(Convert.ToUInt64(m_currentValue));
			
			return new []{m_currentValue};
		}

		private bool IsSingleBit(TEnum enumVal)
		{
			var value = Convert.ToInt64(enumVal);
			return value != 0 && ((value & (value - 1)) == 0);
		}

		private static TEnum GetZeroValue() => (TEnum) Convert.ChangeType(0, Enum.GetUnderlyingType(typeof (TEnum)));
		
		private static MethodInfo m_getValueString;
		
		public static string GetValueStringReflection(TEnum valueEntrySmartValue, out SdfIconType sdfIcon)
		{
			if (m_getValueString == null)
				m_getValueString =
					typeof(EnumSelector<TEnum>).GetMethod("GetValueString",
						BindingFlags.Static | BindingFlags.NonPublic);

			sdfIcon = SdfIconType.None;
			var args = new[] { (object)valueEntrySmartValue, sdfIcon };
			var result = (string)m_getValueString.Invoke(null, args);
			sdfIcon = (SdfIconType)args[1];
			return result;
		}
		
		private static FieldInfo m_currentValueReflection;
		
		public void SetCurrentValue(ulong value)
		{
			if (m_currentValueReflection == null)
				m_currentValueReflection =
					typeof(EnumSelector<TEnum>).GetField("curentValue",
						BindingFlags.Instance | BindingFlags.NonPublic);

			m_currentValueReflection.SetValue(this, value);
		}
	}
}