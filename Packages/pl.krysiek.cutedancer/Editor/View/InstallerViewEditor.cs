#if VRC_SDK_VRCSDK3
using System;
using System.IO;
using UnityEngine;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using AvatarDescriptor = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
using VRC.SDK3.Avatars.ScriptableObjects;
using UnityEditor.Animations;
using System.Linq;


namespace VRF
{
    public class InstallerViewEditor : VisualElement
    {
        private static Logger log = new Logger("InstallerViewEditor");

        public enum Buttons
        {
            AvatarApplyBtn,
            AvatarRemoveBtn,
            AvatarUpdateBtn
        }

        private readonly AvatarApplyService avatarApplyService = new AvatarApplyService();

        private readonly InstallerViewData viewData;

        private readonly BuildInfoEditor buildInfoEditor;

        public InstallerViewEditor()
        {
            viewData = ScriptableObject.CreateInstance<InstallerViewData>();

            CuteResources.LoadView("InstallerView").CloneTree(this);
            this.Bind(new SerializedObject(viewData));

            // assign types - that cannot be done in UXML (at least in Unity 2019)
            this.Q<ObjectField>("Build").objectType = typeof(BuildInfoData);
            this.Q<ObjectField>("Avatar").objectType = typeof(AvatarDescriptor);
            this.Q<ObjectField>("AvatarGameObject").objectType = typeof(GameObject);
            this.Q<ObjectField>("AvatarExpressionParameters").objectType = typeof(VRCExpressionParameters);
            this.Q<ObjectField>("AvatarExpressionsMenu").objectType = typeof(VRCExpressionsMenu);
            this.Q<ObjectField>("AvatarActionController").objectType = typeof(AnimatorController);
            this.Q<ObjectField>("AvatarFxController").objectType = typeof(AnimatorController);

            this.Q<ObjectField>("Build").RegisterValueChangedCallback(HandleBuildSelect);
            this.Q<ObjectField>("Avatar").RegisterValueChangedCallback(HandleAvatarSelect);

            RegisterButtonClick(Buttons.AvatarApplyBtn, e => { avatarApplyService.AddToAvatar(); Validate(); });
            RegisterButtonClick(Buttons.AvatarRemoveBtn, e => { avatarApplyService.RemoveFromAvatar(); Validate(); });
            RegisterButtonClick(Buttons.AvatarUpdateBtn, e => log.LogError("not implemented"));

            buildInfoEditor = this.Q<BuildInfoEditor>("BuildInfo");

            this.RegisterCallback<ChangeEvent<UnityEngine.Object>>((changeEvent) => Validate());

            Validate();
        }

        public void Validate()
        {
            // TODO do complex validation for the avatar
            if (avatarApplyService.Validate())
            {
                ShowButton(Buttons.AvatarApplyBtn, false, viewData.avatar);
                ShowButton(Buttons.AvatarRemoveBtn, true, viewData.avatar);
            }
            else
            {
                ShowButton(Buttons.AvatarApplyBtn, true, viewData.avatar);
                ShowButton(Buttons.AvatarRemoveBtn, false, viewData.avatar);
            }
            ShowButton(Buttons.AvatarUpdateBtn, false);
        }

        private void RegisterButtonClick(Buttons btn, Action<EventBase> action)
        {
            this.Q<Button>(btn.ToString()).clickable = new Clickable(action);
        }

        private void ShowButton(Buttons btn, bool show, bool enabled = true)
        {
            Button button = this.Q<Button>(btn.ToString());
            button.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            button.SetEnabled(enabled);
        }

        private void HandleBuildSelect(ChangeEvent<UnityEngine.Object> evt)
        {
            BuildInfoData buildInfo = (BuildInfoData)evt.newValue;
            avatarApplyService.BuildInfo = buildInfo;
            buildInfoEditor.BuildInfoData = buildInfo;
        }

        private void HandleAvatarSelect(ChangeEvent<UnityEngine.Object> evt)
        {
            AvatarDescriptor avatar = (AvatarDescriptor)evt.newValue;
            avatarApplyService.Avatar = avatar;
            if (avatar)
            {

                this.Q<ObjectField>("AvatarGameObject").value = avatar.gameObject;
                this.Q<ObjectField>("AvatarExpressionParameters").value = avatar.expressionParameters;
                this.Q<ObjectField>("AvatarExpressionsMenu").value = avatar.expressionsMenu;
                this.Q<ObjectField>("AvatarActionController").value = Array.Find(avatar.baseAnimationLayers, layer => layer.type == AvatarDescriptor.AnimLayerType.Action).animatorController as AnimatorController;
                this.Q<ObjectField>("AvatarFxController").value = Array.Find(avatar.baseAnimationLayers, layer => layer.type == AvatarDescriptor.AnimLayerType.FX).animatorController as AnimatorController;
            }
            else
            {
                this.Q<ObjectField>("AvatarGameObject").value = null;
                this.Q<ObjectField>("AvatarExpressionParameters").value = null;
                this.Q<ObjectField>("AvatarExpressionsMenu").value = null;
                this.Q<ObjectField>("AvatarActionController").value = null;
                this.Q<ObjectField>("AvatarFxController").value = null;
            }
        }

    }
}

#endif