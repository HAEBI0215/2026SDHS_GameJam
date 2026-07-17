using UnityEditor;
using UnityEngine;

public static class PlayerAnimationCreator
{
    private const float FrameRate = 30f;
    private const float Duration = 0.4f;

    [MenuItem("Tools/Player Animation/Create Run Clip")]
    private static void CreateRunClip()
    {
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            Debug.LogError("Hierarchy에서 VRoot를 먼저 선택하세요.");
            return;
        }

        Transform root = selectedObject.transform;

        Transform head = root.Find("Head");
        Transform body = root.Find("Body");
        Transform arms = root.Find("Arms");
        Transform legs = root.Find("Legs");

        if (head == null || body == null || arms == null || legs == null)
        {
            Debug.LogError(
                "VRoot 아래에 Head, Body, Arms, Legs 오브젝트가 모두 있어야 합니다."
            );
            return;
        }

        string savePath = EditorUtility.SaveFilePanelInProject(
            "달리기 애니메이션 저장",
            "PlayerRun",
            "anim",
            "애니메이션 클립을 저장할 위치를 선택하세요."
        );

        if (string.IsNullOrEmpty(savePath))
        {
            return;
        }

        AnimationClip clip = new AnimationClip
        {
            name = "PlayerRun",
            frameRate = FrameRate
        };

        CreateBodyAnimation(clip, root, body);
        CreateHeadAnimation(clip, root, head);
        CreateArmsAnimation(clip, root, arms);
        CreateLegsAnimation(clip, root, legs);

        SetLoop(clip);

        AssetDatabase.CreateAsset(clip, savePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = clip;

        Debug.Log($"달리기 애니메이션 생성 완료: {savePath}");
    }

    private static void CreateBodyAnimation(
        AnimationClip clip,
        Transform root,
        Transform body)
    {
        string path = AnimationUtility.CalculateTransformPath(body, root);

        Vector3 position = body.localPosition;
        Vector3 scale = body.localScale;

        SetCurve(
            clip,
            path,
            "m_LocalPosition.y",
            position.y,
            position.y + 0.055f,
            position.y,
            position.y + 0.055f,
            position.y
        );

        SetCurve(
            clip,
            path,
            "localEulerAnglesRaw.z",
            -1.5f,
            1.5f,
            1.5f,
            -1.5f,
            -1.5f
        );

        SetCurve(
            clip,
            path,
            "m_LocalScale.x",
            scale.x * 1.025f,
            scale.x * 0.98f,
            scale.x * 1.025f,
            scale.x * 0.98f,
            scale.x * 1.025f
        );

        SetCurve(
            clip,
            path,
            "m_LocalScale.y",
            scale.y * 0.975f,
            scale.y * 1.025f,
            scale.y * 0.975f,
            scale.y * 1.025f,
            scale.y * 0.975f
        );
    }

    private static void CreateHeadAnimation(
        AnimationClip clip,
        Transform root,
        Transform head)
    {
        string path = AnimationUtility.CalculateTransformPath(head, root);

        Vector3 position = head.localPosition;
        Vector3 scale = head.localScale;

        SetCurve(
            clip,
            path,
            "m_LocalPosition.x",
            position.x - 0.012f,
            position.x + 0.015f,
            position.x + 0.012f,
            position.x - 0.015f,
            position.x - 0.012f
        );

        SetCurve(
            clip,
            path,
            "m_LocalPosition.y",
            position.y,
            position.y + 0.075f,
            position.y,
            position.y + 0.075f,
            position.y
        );

        SetCurve(
            clip,
            path,
            "localEulerAnglesRaw.z",
            -2f,
            2f,
            2f,
            -2f,
            -2f
        );

        SetCurve(
            clip,
            path,
            "m_LocalScale.x",
            scale.x * 1.01f,
            scale.x * 0.995f,
            scale.x * 1.01f,
            scale.x * 0.995f,
            scale.x * 1.01f
        );

        SetCurve(
            clip,
            path,
            "m_LocalScale.y",
            scale.y * 0.99f,
            scale.y * 1.01f,
            scale.y * 0.99f,
            scale.y * 1.01f,
            scale.y * 0.99f
        );
    }

    private static void CreateArmsAnimation(
        AnimationClip clip,
        Transform root,
        Transform arms)
    {
        string path = AnimationUtility.CalculateTransformPath(arms, root);

        Vector3 position = arms.localPosition;

        SetCurve(
            clip,
            path,
            "m_LocalPosition.x",
            position.x - 0.045f,
            position.x + 0.035f,
            position.x + 0.045f,
            position.x - 0.035f,
            position.x - 0.045f
        );

        SetCurve(
            clip,
            path,
            "m_LocalPosition.y",
            position.y + 0.01f,
            position.y + 0.045f,
            position.y + 0.01f,
            position.y + 0.045f,
            position.y + 0.01f
        );

        SetCurve(
            clip,
            path,
            "localEulerAnglesRaw.z",
            -4f,
            3f,
            4f,
            -3f,
            -4f
        );
    }

    private static void CreateLegsAnimation(
        AnimationClip clip,
        Transform root,
        Transform legs)
    {
        string path = AnimationUtility.CalculateTransformPath(legs, root);

        Vector3 position = legs.localPosition;
        Vector3 scale = legs.localScale;

        SetCurve(
            clip,
            path,
            "m_LocalPosition.x",
            position.x + 0.04f,
            position.x,
            position.x - 0.04f,
            position.x,
            position.x + 0.04f
        );

        SetCurve(
            clip,
            path,
            "m_LocalPosition.y",
            position.y,
            position.y - 0.02f,
            position.y,
            position.y - 0.02f,
            position.y
        );

        SetCurve(
            clip,
            path,
            "localEulerAnglesRaw.z",
            3f,
            0f,
            -3f,
            0f,
            3f
        );

        SetCurve(
            clip,
            path,
            "m_LocalScale.x",
            scale.x * 1.035f,
            scale.x,
            scale.x * 1.035f,
            scale.x,
            scale.x * 1.035f
        );

        SetCurve(
            clip,
            path,
            "m_LocalScale.y",
            scale.y * 0.965f,
            scale.y,
            scale.y * 0.965f,
            scale.y,
            scale.y * 0.965f
        );
    }

    private static void SetCurve(
        AnimationClip clip,
        string path,
        string propertyName,
        float value0,
        float value1,
        float value2,
        float value3,
        float value4)
    {
        float quarter = Duration / 4f;

        AnimationCurve curve = new AnimationCurve(
            new Keyframe(0f, value0),
            new Keyframe(quarter, value1),
            new Keyframe(quarter * 2f, value2),
            new Keyframe(quarter * 3f, value3),
            new Keyframe(Duration, value4)
        );

        for (int i = 0; i < curve.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(
                curve,
                i,
                AnimationUtility.TangentMode.Auto
            );

            AnimationUtility.SetKeyRightTangentMode(
                curve,
                i,
                AnimationUtility.TangentMode.Auto
            );
        }

        EditorCurveBinding binding = EditorCurveBinding.FloatCurve(
            path,
            typeof(Transform),
            propertyName
        );

        AnimationUtility.SetEditorCurve(clip, binding, curve);
    }

    private static void SetLoop(AnimationClip clip)
    {
        SerializedObject serializedClip = new SerializedObject(clip);

        SerializedProperty settings =
            serializedClip.FindProperty("m_AnimationClipSettings");

        if (settings != null)
        {
            SerializedProperty loopTime = settings.FindPropertyRelative("m_LoopTime");

            if (loopTime != null)
            {
                loopTime.boolValue = true;
            }
        }

        serializedClip.ApplyModifiedProperties();
    }
}