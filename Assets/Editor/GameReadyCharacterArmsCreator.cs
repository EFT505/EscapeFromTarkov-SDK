﻿using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;
using Diz.Skinning;
using EFT.Visual;
using UnityEditorInternal;
using System.Reflection;

public class GameReadyCharacterArmsCreatorEditor : EditorWindow
{
    private int numItemsToCreate = 1;
    private GameObject[] mainGameObjects;
    private GameObject[] skinGameObjects;
    private Preset[] skinPresets;

    [MenuItem("Custom/Game Ready Object Creator/First Person Character Arms Creator")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(GameReadyCharacterArmsCreatorEditor));
    }

    private void OnGUI()
    {
        GUILayout.Label("Enter the number of items you want to create:");
        numItemsToCreate = EditorGUILayout.IntField("Number of Items", numItemsToCreate);

        if (mainGameObjects == null || mainGameObjects.Length != numItemsToCreate)
        {
            mainGameObjects = new GameObject[numItemsToCreate];
            skinGameObjects = new GameObject[numItemsToCreate];
            skinPresets = new Preset[numItemsToCreate];
        }

        for (int i = 0; i < numItemsToCreate; i++)
        {
            EditorGUILayout.Space();
            GUILayout.Label("Item " + (i + 1));

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            mainGameObjects[i] = (GameObject)EditorGUILayout.ObjectField("Gameobject_MAIN", mainGameObjects[i], typeof(GameObject), true);
            skinGameObjects[i] = (GameObject)EditorGUILayout.ObjectField("Gameobject_SKIN", skinGameObjects[i], typeof(GameObject), true);

            EditorGUILayout.LabelField("Select a Skin Preset:");
            skinPresets[i] = EditorGUILayout.ObjectField("Skin Preset", skinPresets[i], typeof(Preset), false) as Preset;

            EditorGUILayout.EndVertical();
        }

        EditorGUI.BeginDisabledGroup(!AllGameObjectsSet());
        if (GUILayout.Button("Create Game Ready Character Arms"))
        {
            CreateGameReadyCharacterArms();
        }
        EditorGUI.EndDisabledGroup();
    }

    private bool AllGameObjectsSet()
    {
        for (int i = 0; i < numItemsToCreate; i++)
        {
            if (mainGameObjects[i] == null || skinGameObjects[i] == null)
            {
                return false;
            }
        }
        return true;
    }

    private void CreateGameReadyCharacterArms()
    {
        for (int i = 0; i < numItemsToCreate; i++)
        {
            GameObject mainGameObject = mainGameObjects[i];
            GameObject skinGameObject = skinGameObjects[i];
            Preset skinPreset = skinPresets[i];

            // Your existing CreateGameReadyCharacterArms() logic here using the current item's objects and preset

            // Example: Unpack the mainGameObject prefab if it is a prefab
            if (PrefabUtility.GetPrefabInstanceStatus(mainGameObject) == PrefabInstanceStatus.Connected)
            {
                PrefabUtility.UnpackPrefabInstance(mainGameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }

            // Add "Lodded Skin" script to the main_gameobject
            LoddedSkin loddedSkinComponent = mainGameObject.AddComponent<LoddedSkin>();

            // Add "Skin" script to skin_gameobject
            Skin skinComponent = skinGameObject.AddComponent<Skin>();

            // Apply the selected preset to the Skin component if a valid preset is selected
            if (skinPreset != null)
            {
                skinPreset.ApplyTo(skinComponent);
            }

            // Set the SkinnedMeshRenderer for the Skin component using reflection
            SkinnedMeshRenderer skinnedMeshRenderer = skinGameObject.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                FieldInfo rendererField = typeof(Skin).GetField("_skinnedMeshRenderer", BindingFlags.NonPublic | BindingFlags.Instance);
                rendererField.SetValue(skinComponent, skinnedMeshRenderer);
            }
            else
            {
                Debug.LogError("SkinnedMeshRenderer not found on the Skin GameObject.");
            }

            // Set the LOD levels to 1 and assign the skinGameObject to Element 0 of the _lods array using reflection
            FieldInfo lodsField = typeof(LoddedSkin).GetField("_lods", BindingFlags.NonPublic | BindingFlags.Instance);
            AbstractSkin[] lodsArray = new AbstractSkin[1];
            lodsArray[0] = skinGameObject.GetComponent<AbstractSkin>();
            lodsField.SetValue(loddedSkinComponent, lodsArray);

            // Add "Hot Object" script to the skin gameobject
            HotObject hotObjectComponent = skinGameObject.AddComponent<HotObject>();
            skinGameObject.AddComponent<RainCondensator>();

            // Set the temperature values directly (no need for reflection)
            hotObjectComponent.Temperature = new Vector3(0.7f, 1f, 4f);

            // Move the skin_gameobject to be a child of our main_gameobject
            skinGameObject.transform.SetParent(mainGameObject.transform);

            // Reset the references to the gameobjects
            mainGameObjects[i] = null;
            skinGameObjects[i] = null;
        }

        // Repaint the window to update the GUI
        Repaint();

        // Notify the user that the process is complete
        EditorUtility.DisplayDialog("GameReady Character Arms Created", "The GameReady Character Arms have been created successfully!", "OK");
    }
}
