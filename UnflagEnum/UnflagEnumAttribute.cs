using System;
	
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;

[assembly: OdinVisualDesignerAttributeItem("VFX", typeof(VFXPropertySelectorAttribute))]
#endif

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class UnflagEnumAttribute : Attribute { }