using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEditor.Rendering.MaterialVariants
{
    public class HierarchyUI
    {
        public static class Styles
        {
            public const string materialVariantHierarchyText = "Material Variant Hierarchy";

            static public readonly GUIContent currentLabel = EditorGUIUtility.TrTextContent("Current", "The currently selected material.");
            static public readonly GUIContent parentLabel = EditorGUIUtility.TrTextContent("Parent", "A parent can be either a material or a shader.");
            static public readonly GUIContent rootLabel = EditorGUIUtility.TrTextContent("Root", "The root of the hierarchy.");
            static public readonly GUIContent emptyLabel = EditorGUIUtility.TrTextContent(" ", "");
        }

        Material m_Material;
        MaterialVariant m_MatVariant;

        string m_ParentGUID = "";
        Object m_Parent; // This can be Material, Shader or MaterialVariant
        Object m_ParentTarget; // This is the target object Material or Shader

        public HierarchyUI(Object materialEditorTarget)
        {
            m_Material = materialEditorTarget as Material;
            m_MatVariant = MaterialVariant.GetMaterialVariantFromObject(materialEditorTarget);
        }

        public void OnGUI()
        {
            if (m_MatVariant.parentGUID != m_ParentGUID)
            {
                m_ParentGUID = m_MatVariant.parentGUID;

                m_Parent = m_MatVariant.GetParent();
                m_ParentTarget = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GetAssetPath(m_Parent));
            }

            GUILayout.BeginVertical();

            // Draw ourselves in the hierarchy
            using (new EditorGUI.DisabledScope(true))
            {
                Material currentMaterial = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GetAssetPath(m_MatVariant));
                DrawLineageMember(Styles.currentLabel, currentMaterial);
            }

            // Display the rest of the hierarchy
            Object selectedParentTarget = null;
            if (m_Parent == null)
            {
                selectedParentTarget = DrawLineageMember(Styles.parentLabel, null);
            }
            else
            {
                bool isFirstAncestor = true;
                Object nextParent = m_Parent;
                GUIContent variantLabel = Styles.parentLabel, shaderLabel = Styles.parentLabel;
                while (nextParent)
                {
                    using (new EditorGUI.DisabledScope(!isFirstAncestor))
                    {
                        Object parentTargetForCurrentAncestor = null;

                        if (nextParent is MaterialVariant nextMatVariant)
                        {
                            parentTargetForCurrentAncestor = DrawLineageMember(variantLabel, nextMatVariant.material);
                            nextParent = nextMatVariant.GetParent();
                        }
                        else if (nextParent is Material nextMaterial)
                        {
                            parentTargetForCurrentAncestor = DrawLineageMember(Styles.parentLabel, nextParent);
                            nextParent = nextMaterial.shader;
                        }
                        else if (nextParent is Shader)
                        {
                            parentTargetForCurrentAncestor = DrawLineageMember(shaderLabel, nextParent);
                            nextParent = null;
                        }

                        if (isFirstAncestor)
                        {
                            isFirstAncestor = false;
                            selectedParentTarget = parentTargetForCurrentAncestor;
                            variantLabel = Styles.emptyLabel;
                            shaderLabel = Styles.rootLabel;
                        }
                    }
                }
            }

            GUILayout.EndVertical();

            // Reparenting: when the user selects a new parent, we change the rootGUID property - on the next OnGUI the hierarchy will be regenerated
            if (selectedParentTarget != m_ParentTarget)
            {
                // Validate selectedParentTarget: to avoid a loop, it must not be the current material or have the current material as one of its ancestors
                bool valid = (selectedParentTarget is Material || selectedParentTarget is Shader);
                if (valid)
                {
                    Object nextParent = MaterialVariant.GetMaterialVariantFromObject(selectedParentTarget);
                    while (nextParent != null)
                    {
                        if (nextParent == m_MatVariant)
                        {
                            valid = false;
                            break;
                        }

                        MaterialVariant nextMatVariant = nextParent as MaterialVariant;
                        nextParent = nextMatVariant ? nextMatVariant.GetParent() : null;
                    }
                }

                if (valid)
                {
                    m_MatVariant.SetParent(selectedParentTarget);
                    Shader newShader = null;
                    if (selectedParentTarget is Material material)
                        newShader = material.shader;
                    else if (selectedParentTarget is Shader shader)
                        newShader = shader;
                    if (newShader && newShader != m_Material.shader)
                        m_Material.shader = newShader;
                }
            }
        }

        Object DrawLineageMember(GUIContent label, Object asset)
        {
            // We could use this to start a Horizontal and add inline icons and toggles to show overridden/locked
            return EditorGUILayout.ObjectField(label, asset, typeof(Object), false);
        }
    }
}
