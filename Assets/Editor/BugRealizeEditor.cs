using UnityEditor;


[CustomEditor(typeof(BugRealize))]
public class BugRealizeEditor : Editor
{
    private SerializedProperty type;
    private SerializedProperty distance;
    private SerializedProperty time;
    private SerializedProperty playerLayer;
    private SerializedProperty rayCount;
    private SerializedProperty AnimTime;
    private SerializedProperty ColorCount;
    private SerializedProperty ColorSChange;
    private SerializedProperty EndPosition;
    private SerializedProperty RayScale;

    private void OnEnable()
    {
        type = serializedObject.FindProperty("type");
        distance = serializedObject.FindProperty("distance");
        time = serializedObject.FindProperty("time");
        playerLayer = serializedObject.FindProperty("playerLayer");
        rayCount = serializedObject.FindProperty("rayCount");
        AnimTime = serializedObject.FindProperty("AnimTime");
        ColorCount = serializedObject.FindProperty("ColorCount");
        ColorSChange = serializedObject.FindProperty("ColorSChange");
        EndPosition = serializedObject.FindProperty("EndPosition");
        RayScale = serializedObject.FindProperty("RayScale");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.PropertyField(type);
        
        if ((BugType)type.enumValueIndex == BugType.Stack)
        {
            EditorGUILayout.PropertyField(distance);
        }

        if ((BugType)type.enumValueIndex == BugType.ArgumentOut)
        {
            EditorGUILayout.PropertyField(ColorCount);
            EditorGUILayout.PropertyField(ColorSChange);
            EditorGUILayout.PropertyField(EndPosition);
        }

        EditorGUILayout.PropertyField(time);
        EditorGUILayout.PropertyField(playerLayer);
        EditorGUILayout.PropertyField(rayCount);
        EditorGUILayout.PropertyField(AnimTime);
        EditorGUILayout.PropertyField(RayScale);
        
        serializedObject.ApplyModifiedProperties();
    }
}
