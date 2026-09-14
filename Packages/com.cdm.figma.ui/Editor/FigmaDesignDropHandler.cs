using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Cdm.Figma.UI.Editor
{
    [InitializeOnLoad]
    public static class FigmaDesignDropHandler
    {
        static FigmaDesignDropHandler()
        {
            DragAndDrop.AddDropHandlerV2((DragAndDrop.HierarchyDropHandlerV2)OnHierarchyDropHandler);
            DragAndDrop.AddDropHandlerV2((DragAndDrop.SceneDropHandler)OnSceneDropHandler);
        }

        [OnOpenAsset(0)]
        public static bool OnOpenFigmaDesign(int instanceID, int line)
        {
            var go = EditorUtility.EntityIdToObject(EntityId.FromULong((ulong)instanceID)) as GameObject;
            if (go != null)
            {
                var figmaDesign = go.GetComponent<FigmaDesign>();
                if (figmaDesign != null)
                {
                    FigmaDesignPreviewStage.Show(figmaDesign);
                    return true;
                }

                var figmaPage = go.GetComponent<FigmaPage>();
                if (figmaPage != null)
                {
                    FigmaDesignPreviewStage.Show(figmaPage);
                    return true;
                }
            }

            return false;
        }

        private static DragAndDropVisualMode OnHierarchyDropHandler(
            EntityId dropTargetEntityId,
            HierarchyDropFlags dropMode,
            Transform parentForDraggedObjects,
            bool perform)
        {
            if (DragAndDrop.objectReferences == null || DragAndDrop.objectReferences.Length == 0)
                return DragAndDropVisualMode.None;

            var go = DragAndDrop.objectReferences[0] as GameObject;
            if (go != null)
            {
                var figmaDesign = go.GetComponent<FigmaDesign>();
                if (figmaDesign != null)
                {
                    if (perform)
                    {
                        var parentObject = EditorUtility.EntityIdToObject(dropTargetEntityId) as GameObject;
                        InstantiateFigmaDesign(figmaDesign, parentObject);
                    }

                    return DragAndDropVisualMode.Copy;
                }
            }

            return DragAndDropVisualMode.None;
        }

        private static DragAndDropVisualMode OnSceneDropHandler(
            Object dropUpon,
            Vector3 worldPosition,
            Vector2 viewportPosition,
            Transform parentForDraggedObjects,
            bool perform)
        {
            if (DragAndDrop.objectReferences == null || DragAndDrop.objectReferences.Length == 0)
                return DragAndDropVisualMode.None;

            var go = DragAndDrop.objectReferences[0] as GameObject;
            if (go != null)
            {
                var figmaDesign = go.GetComponent<FigmaDesign>();
                if (figmaDesign != null)
                {
                    if (perform)
                    {
                        var figmaDesignInstance = InstantiateFigmaDesign(figmaDesign, dropUpon as GameObject);
                        if (figmaDesignInstance != null)
                        {
                            figmaDesignInstance.transform.position = worldPosition;
                        }
                    }

                    return DragAndDropVisualMode.Copy;
                }
            }

            return DragAndDropVisualMode.None;
        }

        private static GameObject InstantiateFigmaDesign(FigmaDesign figmaDesign, GameObject parentObject)
        {
            var parent = parentObject != null ? parentObject.transform : null;
            var document = FigmaDocument.InstantiatePrefab(figmaDesign.document, parent);
            return document.gameObject;
        }
    }
}