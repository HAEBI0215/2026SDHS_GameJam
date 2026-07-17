using UnityEditor;
using UnityEngine;

public static class PlayerAnimationCreator
{
    private const float FrameRate = 30f;
    private const float CarryDuration = 0.5f;

    // 음수일수록 팔이 더 아래로 내려감
    private const float CarryYOffset = -0.06f;

    [MenuItem("Tools/Player Animation/Create Carry Clip")]
    private static void CreateCarryClip()
    {
        GameObject selectedObject = Selection.activeGameObject;

        if(selectedObject == null)
        {
            Debug.LogError("Hierarchy에서 VRoot를 선택하세요.");
            return;
        }

        Transform root = selectedObject.transform;
        Transform arms = root.Find("Arms");

        if(arms == null)
        {
            Debug.LogError(
                "선택한 오브젝트 아래에서 Arms를 찾지 못했습니다. " +
                "VRoot 아래에 Arms 오브젝트가 있는지 확인하세요."
            );

            return;
        }

        string savePath = EditorUtility.SaveFilePanelInProject(
            "Carry 애니메이션 저장",
            "PlayerCarry",
            "anim",
            "PlayerCarry 애니메이션을 저장할 위치를 선택하세요."
        );

        if(string.IsNullOrEmpty(savePath))
            return;

        AnimationClip existingClip =
            AssetDatabase.LoadAssetAtPath<AnimationClip>(savePath);

        if(existingClip != null)
        {
            bool replace = EditorUtility.DisplayDialog(
                "애니메이션 덮어쓰기",
                "같은 경로에 애니메이션이 이미 있습니다.\n덮어쓰시겠습니까?",
                "덮어쓰기",
                "취소"
            );

            if(!replace)
                return;

            AssetDatabase.DeleteAsset(savePath);
        }

        AnimationClip clip = new AnimationClip
        {
            name = "PlayerCarry",
            frameRate = FrameRate
        };

        CreateCarryArmsAnimation(
            clip,
            root,
            arms
        );

        SetLoop(clip);

        AssetDatabase.CreateAsset(
            clip,
            savePath
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = clip;

        Debug.Log(
            $"Carry 애니메이션 생성 완료: {savePath}"
        );
    }

    private static void CreateCarryArmsAnimation(
        AnimationClip clip,
        Transform root,
        Transform arms)
    {
        string path =
            AnimationUtility.CalculateTransformPath(
                arms,
                root
            );

        float originalScaleY =
            arms.localScale.y;

        float flippedScaleY =
            -Mathf.Abs(originalScaleY);

        float originalPositionY =
            arms.localPosition.y;

        float carryPositionY =
            originalPositionY + CarryYOffset;

        // 팔 상하 반전
        SetConstantCurve(
            clip,
            path,
            "m_LocalScale.y",
            flippedScaleY
        );

        // 상하 반전으로 뜬 팔을 아래로 보정
        SetConstantCurve(
            clip,
            path,
            "m_LocalPosition.y",
            carryPositionY
        );
    }

    private static void SetConstantCurve(
        AnimationClip clip,
        string path,
        string propertyName,
        float value)
    {
        AnimationCurve curve = new AnimationCurve(
            new Keyframe(0f, value),
            new Keyframe(CarryDuration, value)
        );

        EditorCurveBinding binding =
            EditorCurveBinding.FloatCurve(
                path,
                typeof(Transform),
                propertyName
            );

        AnimationUtility.SetEditorCurve(
            clip,
            binding,
            curve
        );
    }

    private static void SetLoop(
        AnimationClip clip)
    {
        AnimationClipSettings settings =
            AnimationUtility.GetAnimationClipSettings(
                clip
            );

        settings.loopTime = true;

        AnimationUtility.SetAnimationClipSettings(
            clip,
            settings
        );
    }
}