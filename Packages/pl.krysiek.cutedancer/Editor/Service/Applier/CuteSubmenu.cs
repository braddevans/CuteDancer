using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using AvatarDescriptor = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
using ExpressionsMenu = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu;
using System.IO;

namespace VRF
{
    public class CuteSubmenu : AvatarApplierInterface
    {
        static string CUTE_MENU = Path.Combine("Assets", "CuteDancer", "Build", "CuteDancer-VRCMenu.asset"); // TODO read from build configuration
        static string DANCE_ICON = Path.Combine("Packages", "pl.krysiek.cutedancer", "Runtime", "Icons", "CuteDancer.png");

        AvatarDescriptor avatar;
        ExpressionsMenu expressionMenu;

        public void SetAvatar(AvatarDescriptor avatarDescriptor)
        {
            avatar = avatarDescriptor;
            expressionMenu = avatarDescriptor.expressionsMenu;
        }

        public void ClearForm()
        {
            avatar = null;
            expressionMenu = null;
        }

        public ApplyStatus GetStatus()
        {
            if (avatar == null)
            {
                return ApplyStatus.EMPTY;
            }
            if (expressionMenu == null)
            {
                return ApplyStatus.ADD;
            }
            ExpressionsMenu cuteMenu = AssetDatabase.LoadAssetAtPath<ExpressionsMenu>(CUTE_MENU);
            if (expressionMenu.controls.Exists(menuEntry => menuEntry.subMenu == cuteMenu)) {
                return ApplyStatus.REMOVE;
            }
            Texture2D cuteIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(DANCE_ICON);
            if (expressionMenu.controls.Exists(menuEntry => menuEntry.name == "CuteDancer") || expressionMenu.controls.Exists(menuEntry => menuEntry.icon == cuteIcon)) {
                return ApplyStatus.UPDATE;
            }
            if (expressionMenu.controls.ToArray().Length >= 8)
            {
                return ApplyStatus.BLOCKED;
            }
            return ApplyStatus.ADD;
        }

        public void HandleAdd()
        {
            if (!expressionMenu && !CreateExpressionMenu())
            {
                return;
            }

            DoBackup();

            ExpressionsMenu cuteMenu = AssetDatabase.LoadAssetAtPath(CUTE_MENU, typeof(ExpressionsMenu)) as ExpressionsMenu;

            var menuEntry = new ExpressionsMenu.Control
            {
                name = "CuteDancer",
                icon = AssetDatabase.LoadAssetAtPath(DANCE_ICON, typeof(Texture2D)) as Texture2D,
                type = ExpressionsMenu.Control.ControlType.SubMenu,
                subMenu = cuteMenu
            };

            Debug.Log("Adding expression menu control to menu [name=" + expressionMenu.name + "]");
            expressionMenu.controls.Add(menuEntry);
            EditorUtility.SetDirty(expressionMenu);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public void HandleRemove()
        {
            DoBackup();

            Texture2D cuteIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(DANCE_ICON);
            int ix = expressionMenu.controls.FindIndex(menuEntry => menuEntry.icon == cuteIcon);

            Debug.Log("Removing expression menu control from menu [name=" + expressionMenu.name + "]");
            expressionMenu.controls.RemoveAt(ix);
            EditorUtility.SetDirty(expressionMenu);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        void DoBackup()
        {
            CuteBackup.CreateBackup(AssetDatabase.GetAssetPath(expressionMenu));
        }

        bool CreateExpressionMenu()
        {
            string path = $"Assets/{avatar.name}-ExpressionMenu.asset";
            bool ok = EditorUtility.DisplayDialog("CuteScript", $"It seems your avatar does not have expression menu. Empty one will be created and assigned to your avatar.\n\nNew asset will be saved under path:\n{path}", "Create it!", "Cancel");
            if (!ok)
            {
                EditorUtility.DisplayDialog("CuteScript", "Operation aborted. Expresion Menu is NOT added!", "OK");
                return false;
            }

            ExpressionsMenu emptyMenu = ScriptableObject.CreateInstance(typeof(ExpressionsMenu)) as ExpressionsMenu;
            emptyMenu.controls = new List<ExpressionsMenu.Control>();

            AssetDatabase.CreateAsset(emptyMenu, path);
            expressionMenu = AssetDatabase.LoadAssetAtPath<ExpressionsMenu>(path);

            avatar.expressionsMenu = expressionMenu;
            avatar.customExpressions = true;
            EditorUtility.SetDirty(expressionMenu);
            EditorUtility.SetDirty(avatar);

            return true;
        }
    }
}