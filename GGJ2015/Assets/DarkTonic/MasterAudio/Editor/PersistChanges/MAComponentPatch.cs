using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

public class MAComponentPatch {
    private Dictionary<string, object> values = null;

    public MAComponentPatch(Component component) {
        this.ComponentObject = component;
    }
    private bool isComponentObjectNull = true;
    private Component _componentObject = null;
    private Component ComponentObject {
        get {
            return _componentObject;
        }
        set {
            _componentObject = value;
            isComponentObjectNull = _componentObject == null;
        }
    }

    public string ComponentName {
        get {
            string[] parts = ComponentObject.GetType().ToString().Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            return parts[parts.Length - 1];
        }
    }

    public void StoreSettings() {
        if (ComponentObject != null) {
            values = new Dictionary<string, object>();

            List<PropertyInfo> properties = GetProperties();
            List<FieldInfo> fields = GetFields();

            foreach (PropertyInfo property in properties) {
                values.Add(property.Name, property.GetValue(ComponentObject, null));
            }
            foreach (FieldInfo field in fields) {
                values.Add(field.Name, field.GetValue(ComponentObject));
            }
        }
    }

    private List<FieldInfo> GetFields() {
        List<FieldInfo> fields = new List<FieldInfo>();

        foreach (FieldInfo fieldInfo in ComponentObject.GetType().GetFields()) {
            if (fieldInfo.IsPublic) {
                if (!Attribute.IsDefined(fieldInfo, typeof(HideInInspector))) {
                    fields.Add(fieldInfo);
                }
            }
        }

        return fields;
    }

    private List<PropertyInfo> GetProperties() {
        List<PropertyInfo> properties = new List<PropertyInfo>();

        foreach (PropertyInfo propertyInfo in ComponentObject.GetType().GetProperties()) {
            if (!Attribute.IsDefined(propertyInfo, typeof(HideInInspector))) {
                MethodInfo setMethod = propertyInfo.GetSetMethod();
                if (null != setMethod && setMethod.IsPublic) {
                    properties.Add(propertyInfo);
                }
            }
        }

        return properties;
    }

    //return component is changes have been made
    public Component RestoreSettings() {
        Component resultChangedComponent = null;

        if (!isComponentObjectNull) {
            ComponentObject = EditorUtility.InstanceIDToObject(ComponentObject.GetInstanceID()) as Component;
        } else {
            ComponentObject = null;
        }

        if (ComponentObject != null && values != null) {
            foreach (string name in values.Keys) {
                object newValue = values[name];

                PropertyInfo property = ComponentObject.GetType().GetProperty(name);

                if (null != property) {
                    object currentValue = property.GetValue(ComponentObject, null);

                    if (HasValueChanged(newValue, currentValue)) {
                        property.SetValue(ComponentObject, newValue, null);
                        resultChangedComponent = ComponentObject;
                    }
                } else {
                    FieldInfo field = ComponentObject.GetType().GetField(name);
                    object currentValue = field.GetValue(ComponentObject);

                    if (HasValueChanged(newValue, currentValue)) {
                        field.SetValue(ComponentObject, newValue);
                        resultChangedComponent = ComponentObject;
                    }
                }
            }
        }

        values = null;

        return resultChangedComponent;
    }

    private bool HasValueChanged(object newValue, object oldValue) {
        bool valuesChanged = true;

        if (null != newValue && null != oldValue) {
            IComparable valueToCompare = newValue as IComparable;

            if (null == valueToCompare) {
                try {
                    XmlSerializer serializer = new XmlSerializer(newValue.GetType());

                    using (MemoryStream streamNew = new MemoryStream()) {
                        serializer.Serialize(streamNew, newValue);

                        UTF8Encoding encoding = new UTF8Encoding();

                        string oldValueSerialized = encoding.GetString(streamNew.ToArray());

                        using (MemoryStream streamOld = new MemoryStream()) {
                            serializer.Serialize(streamOld, oldValue);

                            string newValueSerialized = encoding.GetString(streamOld.ToArray());

                            valuesChanged = !string.Equals(newValueSerialized, oldValueSerialized);
                        }
                    }
                }
                catch {
                    valuesChanged = true;
                }
            } else {
                valuesChanged = valueToCompare.CompareTo(oldValue) != 0;
            }
        } else if (null == oldValue && null == newValue) {
            valuesChanged = false;
        }

        return valuesChanged;
    }
}
