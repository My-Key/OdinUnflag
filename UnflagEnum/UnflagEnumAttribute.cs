using System;
	
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;

[assembly: OdinVisualDesignerAttributeItem("Custom", typeof(UnflagEnumAttribute))]
#endif

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class UnflagEnumAttribute : Attribute { }