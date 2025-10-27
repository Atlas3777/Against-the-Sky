// using UnityEditor;
// using UnityEngine;

// [System.Serializable]
// public class MinMax
// {
//     public int Min = 1;
//     public int Max = 99;
// }

// namespace Assets.Game.Script.Editor
// {
//   [CustomPropertyDrawer(typeof(MinMax))]
//   public class MinMaxSlider : PropertyDrawer
//   {
//     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//     {
//         SerializedProperty minProp = property.FindPropertyRelative("Min");
//         SerializedProperty maxProp = property.FindPropertyRelative("Max");
//         position = EditorGUI.PrefixLabel(position, label);

//         var minLimit = 1f;
//         var maxLimit = 99f;
//         var minVal = minProp.floatValue;
//         var maxVal = maxProp.floatValue;
//         EditorGUI.MinMaxSlider(position, ref minVal, ref maxVal, minLimit, maxLimit);

//         minProp.intValue = Mathf.RoundToInt(minVal);
//         maxProp.intValue = Mathf.RoundToInt(maxVal);
//         minProp.intValue = Mathf.Clamp(minProp.intValue, (int)minLimit, maxProp.intValue);
//         maxProp.intValue = Mathf.Clamp(maxProp.intValue, minProp.intValue, (int)maxLimit);
//     }
//   }
// }


using UnityEditor;
using UnityEngine;

[System.Serializable]
public class MinMax
{
    public int Min = 1;
    public int Max = 99;
}

namespace Assets.Game.Script.Editor
{
    [CustomPropertyDrawer(typeof(MinMax))]
    public class MinMaxSlider : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty minProp = property.FindPropertyRelative("Min");
            SerializedProperty maxProp = property.FindPropertyRelative("Max");

            position = EditorGUI.PrefixLabel(position, label);

            float minLimit = 1f;
            float maxLimit = 99f;

            // Разделим позицию на три части: поле min | слайдер | поле max
            float fieldWidth = 40f;
            Rect minFieldRect = new Rect(position.x, position.y, fieldWidth, position.height);
            Rect sliderRect = new Rect(position.x + fieldWidth + 5, position.y, position.width - 2*fieldWidth - 10, position.height);
            Rect maxFieldRect = new Rect(position.x + position.width - fieldWidth, position.y, fieldWidth, position.height);

            // Числовые поля
            minProp.intValue = EditorGUI.IntField(minFieldRect, minProp.intValue);
            maxProp.intValue = EditorGUI.IntField(maxFieldRect, maxProp.intValue);

            // Слайдер
            float minVal = minProp.intValue;
            float maxVal = maxProp.intValue;
            EditorGUI.MinMaxSlider(sliderRect, ref minVal, ref maxVal, minLimit, maxLimit);

            // Конвертация обратно в int и ограничения
            minProp.intValue = Mathf.Clamp(Mathf.RoundToInt(minVal), (int)minLimit, maxProp.intValue);
            maxProp.intValue = Mathf.Clamp(Mathf.RoundToInt(maxVal), minProp.intValue, (int)maxLimit);
        }
    }
}
