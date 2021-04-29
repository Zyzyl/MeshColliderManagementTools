﻿using System;
using System.Collections.Generic;
using System.Linq;
using BaseX;
using FrooxEngine;
using FrooxEngine.UIX;
using FrooxEngine.Undo;

namespace MeshColliderManagementTools
{
    public enum ReplacementColliderComponent
    {
        BoxCollider,
        SphereCollider,
        ConvexHullCollider
    }
    public enum SetupBoundsType
    {
        None,
        SetupFromLocalBounds,
        SetupFromGlobalBounds
    }
    public enum UseTagMode
    {
        IgnoreTag,
        IncludeOnlyWithTag,
        ExcludeAllWithTag
    }

    // Wizard which allows batch or individual deletion or replacement of MeshColliders.
    [Category("Wizards")]
    public class MeshColliderManagementWizard : Component
    {
        public readonly Sync<bool> IgnoreInactive;
        public readonly Sync<bool> IgnoreDisabled;
        public readonly Sync<bool> IgnoreNonPersistent;
        public readonly Sync<bool> IgnoreUserHierarchies;
        public readonly Sync<bool> PreserveColliderSettings;
        public readonly Sync<bool> SetIgnoreRaycasts;
        public readonly Sync<bool> SetCharacterCollider;
        public readonly Sync<float> HighlightDuration;
        public readonly Sync<color> HighlightColor;
        public readonly Sync<ColliderType> setColliderType;
        public readonly Sync<SetupBoundsType> setupBoundsType;
        public readonly Sync<ReplacementColliderComponent> replacementColliderComponent;
        public readonly Sync<UseTagMode> useTagMode;
        public readonly SyncRef<Slot> ProcessRoot;
        public readonly SyncRef<TextField> tag;
        public readonly SyncRef<Text> resultsText;
        private int _count;
        private color _buttonColor;
        private LocaleString _text;
        private Slot _scrollAreaRoot;
        private UIBuilder _listBuilder;

        protected override void OnAwake()
        {
            base.OnAwake();
            IgnoreInactive.Value = true;
            IgnoreDisabled.Value = true;
            IgnoreNonPersistent.Value = true;
            IgnoreUserHierarchies.Value = true;
            setupBoundsType.Value = SetupBoundsType.SetupFromLocalBounds;
            PreserveColliderSettings.Value = true;
            HighlightDuration.Value = 1f;
            HighlightColor.Value = new color(1f, 1f, 1f);
        }

        protected override void OnAttach()
        {
            base.OnAttach();
            // Create the UI for the wizard.
            this.Slot.Name = "MeshCollider Management Wizard";
            base.Slot.Tag = "Developer";
            NeosCanvasPanel neosCanvasPanel = base.Slot.AttachComponent<NeosCanvasPanel>();
            neosCanvasPanel.Panel.AddCloseButton();
            neosCanvasPanel.Panel.AddParentButton();
            neosCanvasPanel.Panel.Title = "MeshCollider Management Wizard";
            neosCanvasPanel.CanvasSize = new float2(800f, 850f);
            UIBuilder uIBuilder = new UIBuilder(neosCanvasPanel.Canvas);
            List<RectTransform> rectList = uIBuilder.SplitHorizontally(0.5f, 0.5f);
            // Build left hand side UI - options and buttons.
            UIBuilder uIBuilder2 = new UIBuilder(rectList[0].Slot);
            Slot _layoutRoot = uIBuilder2.VerticalLayout(4f, 0f, new Alignment()).Slot;
            uIBuilder2.FitContent(SizeFit.Disabled, SizeFit.MinSize);
            uIBuilder2.Style.Height = 24f;
            UIBuilder uIBuilder3 = uIBuilder2;
            // Slot reference to which changes will be applied.
            _text = "Process root slot:";
            uIBuilder3.Text(in _text);
            uIBuilder3.Next("Root");
            uIBuilder3.Current.AttachComponent<RefEditor>().Setup(ProcessRoot);
            uIBuilder3.Spacer(24f);
            // Basic filtering settings for which MeshColliders are accepted for changes or listing.
            _text = "Ignore inactive:";
            uIBuilder3.HorizontalElementWithLabel(in _text, 0.9f, () => uIBuilder3.BooleanMemberEditor(IgnoreInactive));
            _text = "Ignore disabled:";
            uIBuilder3.HorizontalElementWithLabel(in _text, 0.9f, () => uIBuilder3.BooleanMemberEditor(IgnoreDisabled));
            _text = "Ignore non-persistent:";
            uIBuilder3.HorizontalElementWithLabel(in _text, 0.9f, () => uIBuilder3.BooleanMemberEditor(IgnoreNonPersistent));
            _text = "Ignore user hierarchies:";
            uIBuilder3.HorizontalElementWithLabel(in _text, 0.9f, () => uIBuilder3.BooleanMemberEditor(IgnoreUserHierarchies));
            _text = "Tag:";
            tag.Target = uIBuilder3.HorizontalElementWithLabel(in _text, 0.2f, () => uIBuilder3.TextField());
            _text = "Tag handling mode:";
            uIBuilder3.Text(in _text);
            uIBuilder3.EnumMemberEditor(useTagMode);
            uIBuilder3.Spacer(24f);
            // Settings for highlighing individual colliders.
            _text = "Highlight duration:";
            uIBuilder3.HorizontalElementWithLabel(in _text, 0.8f, () => uIBuilder3.PrimitiveMemberEditor(HighlightDuration));
            _text = "Highlight color:";
            uIBuilder3.Text(in _text);
            uIBuilder3.ColorMemberEditor(HighlightColor);
            uIBuilder3.NestOut();
            uIBuilder3.Spacer(24f);
            // Controls for specific replacement collider settings.
            _text = "Replacement collider component:";
            uIBuilder3.Text(in _text);
            uIBuilder3.EnumMemberEditor(replacementColliderComponent);
            _text = "Replacement setup action:";
            uIBuilder3.Text(in _text);
            uIBuilder3.EnumMemberEditor(setupBoundsType);
            uIBuilder3.Spacer(24f);
            _text = "Preserve existing collider settings:";
            uIBuilder3.HorizontalElementWithLabel(in _text, 0.9f, () => uIBuilder3.BooleanMemberEditor(PreserveColliderSettings));
            _text = "Set collision Type:";
            uIBuilder3.Text(in _text);
            Slot _hideTextSlot = _layoutRoot.GetAllChildren().Last();
            uIBuilder3.EnumMemberEditor(setColliderType);
            Slot _hideEnumSlot = _layoutRoot.GetAllChildren().Last().Parent.Parent;
            _text = "Set CharacterCollider:";
            Slot _hideBoolSlot1 = uIBuilder3.HorizontalElementWithLabel(in _text, 0.9f, () => uIBuilder3.BooleanMemberEditor(SetCharacterCollider)).Slot.Parent;
            _text = "Set IgnoreRaycasts:";
            Slot _hideBoolSlot2 = uIBuilder3.HorizontalElementWithLabel(in _text, 0.9f, () => uIBuilder3.BooleanMemberEditor(SetIgnoreRaycasts)).Slot.Parent;
            uIBuilder3.Spacer(24f);
            // Hide some options if preserving existing settings.
            var _valCopy = _layoutRoot.AttachComponent<ValueCopy<bool>>();
            var _boolValDriver = _layoutRoot.AttachComponent<BooleanValueDriver<bool>>();
            var _valMultiDriver = _layoutRoot.AttachComponent<ValueMultiDriver<bool>>();
            _valCopy.Source.Target = PreserveColliderSettings;
            _valCopy.Target.Target = _boolValDriver.State;
            _boolValDriver.TrueValue.Value = false;
            _boolValDriver.FalseValue.Value = true;
            _boolValDriver.TargetField.Target = _valMultiDriver.Value;
            for (int i = 0; i < 4; i++)
            {
                _valMultiDriver.Drives.Add();
            }
            _valMultiDriver.Drives[0].Target = _hideTextSlot.ActiveSelf_Field;
            _valMultiDriver.Drives[1].Target = _hideEnumSlot.ActiveSelf_Field;
            _valMultiDriver.Drives[2].Target = _hideBoolSlot1.ActiveSelf_Field;
            _valMultiDriver.Drives[3].Target = _hideBoolSlot2.ActiveSelf_Field;
            // Buttons for batch actions.
            _text = "List matching MeshColliders";
            uIBuilder3.Button(in _text, PopulateList);
            _text = "Replace all matching MeshColliders";
            uIBuilder3.Button(in _text, ReplaceAll);
            _text = "Remove all matching MeshColliders";
            uIBuilder3.Button(in _text, RemoveAll);
            uIBuilder3.Spacer(24f);
            _text = "------";
            resultsText.Target = uIBuilder3.Text(in _text);
            // Build right hand side UI - list of found MeshColliders.
            UIBuilder uIBuilder4 = new UIBuilder(rectList[1].Slot);
            uIBuilder4.ScrollArea();
            uIBuilder4.VerticalLayout(10f, 4f);
            _scrollAreaRoot = uIBuilder4.FitContent(SizeFit.Disabled, SizeFit.MinSize).Slot;
            // Prepare UIBuilder for addding elements to MeshCollider list.
            _listBuilder = uIBuilder4;
            _listBuilder.Style.MinHeight = 40f;
        }

        protected override void OnStart()
        {
            base.OnStart();
            base.Slot.GetComponentInChildrenOrParents<Canvas>()?.MarkDeveloper();
        }

        private void CreateScrollListElement(MeshCollider mc)
        {
            Slot _elementRoot = _listBuilder.Next("Element");
            var _refField = _elementRoot.AttachComponent<ReferenceField<MeshCollider>>();
            _refField.Reference.Target = mc;
            UIBuilder _listBuilder2 = new UIBuilder(_elementRoot);
            _listBuilder2.NestInto(_elementRoot);
            _listBuilder2.VerticalLayout(4f, 4f);
            _listBuilder2.HorizontalLayout(10f);
            _text = "Jump To";
            _buttonColor = new color(1f, 1f, 1f);
            _listBuilder2.ButtonRef<Slot>(in _text, in _buttonColor, JumpTo, mc.Slot);
            _text = "Highlight";
            _listBuilder2.ButtonRef<MeshCollider>(in _text, in _buttonColor, Remove, mc);
            _text = "Replace";
            _listBuilder2.ButtonRef<Slot>(in _text, in _buttonColor, Highlight, mc.Slot);
            _text = "Remove";
            _listBuilder2.ButtonRef<MeshCollider>(in _text, in _buttonColor, Replace, mc);
            _listBuilder2.NestOut();
            _listBuilder2.NestOut();
            _listBuilder2.Current.AttachComponent<RefEditor>().Setup(_refField.Reference);
        }

        private void ForeachMeshCollider(Action<MeshCollider> process)
        {
            if (ProcessRoot.Target != null)
            {
                UniLog.Log(tag.Target.TargetString);
                foreach (MeshCollider componentsInChild in ProcessRoot.Target.GetComponentsInChildren<MeshCollider>(delegate (MeshCollider mc)
                {
                    // Check whether collider should be filtered out.
                    return ((!IgnoreInactive.Value || mc.Slot.IsActive)
                    && (!IgnoreDisabled || mc.Enabled)
                    && (!IgnoreNonPersistent || mc.IsPersistent)
                    && (!IgnoreUserHierarchies || mc.Slot.ActiveUser == null)
                    && ((useTagMode == UseTagMode.IgnoreTag)
                    || (useTagMode == UseTagMode.IncludeOnlyWithTag && mc.Slot.Tag == tag.Target.TargetString)
                    || (useTagMode == UseTagMode.ExcludeAllWithTag && mc.Slot.Tag != tag.Target.TargetString)));
                }))
                {
                    process(componentsInChild);
                }
            }
            else ShowResults("No target root slot set.");
        }

        private void Highlight(IButton button, ButtonEventData eventData, Slot s)
        {
            HighlightHelper.FlashHighlight(s, null, HighlightColor, HighlightDuration);
        }

        private void JumpTo(IButton button, ButtonEventData eventData, Slot s)
        {
            LocalUserRoot.JumpToPoint(s.GlobalPosition);
        }

        private void PopulateList()
        {
            _scrollAreaRoot.DestroyChildren();
            ForeachMeshCollider(delegate (MeshCollider mc)
            {
                CreateScrollListElement(mc);
            });
        }

        private void PopulateList(IButton button, ButtonEventData eventData)
        {
            _count = 0;
            _scrollAreaRoot.DestroyChildren();
            ForeachMeshCollider(delegate (MeshCollider mc)
            {
                CreateScrollListElement(mc);
                _count++;
            });
            ShowResults($"{_count} matching MeshColliders listed.");
        }

        private void Remove(IButton button, ButtonEventData eventData, MeshCollider mc)
        {
            mc.UndoableDestroy();
            PopulateList();
            ShowResults($"MeshCollider removed.");
        }

        private void Replace(IButton button, ButtonEventData eventData, MeshCollider mc)
        {
            World.BeginUndoBatch("Replace MeshCollider");
            switch (replacementColliderComponent.Value)
            {
                case ReplacementColliderComponent.BoxCollider:
                    var bc = mc.Slot.AttachComponent<BoxCollider>();
                    bc.CreateSpawnUndoPoint();
                    SetupNewCollider(bc, mc);
                    break;
                case ReplacementColliderComponent.SphereCollider:
                    var sc = mc.Slot.AttachComponent<SphereCollider>();
                    sc.CreateSpawnUndoPoint();
                    SetupNewCollider(sc, mc);
                    break;
                case ReplacementColliderComponent.ConvexHullCollider:
                    mc.Slot.AttachComponent<ConvexHullCollider>().CreateSpawnUndoPoint();
                    break;
            }
            mc.UndoableDestroy();
            PopulateList();
            ShowResults($"MeshCollider replaced.");
        }

        private void RemoveAll(IButton button, ButtonEventData eventData)
        {
            World.BeginUndoBatch("Batch remove MeshColliders");
            _count = 0;
            ForeachMeshCollider(delegate (MeshCollider mc)
            {
                mc.UndoableDestroy();
                _count++;
            });
            World.EndUndoBatch();
            PopulateList();
            ShowResults($"{_count} matching MeshColliders removed.");
        }
        private void ReplaceAll(IButton button, ButtonEventData eventData)
        {
            World.BeginUndoBatch("Batch replace MeshColliders");
            _count = 0;
            ForeachMeshCollider(delegate (MeshCollider mc)
            {
                switch (replacementColliderComponent.Value)
                {
                    case ReplacementColliderComponent.BoxCollider:
                        var bc = mc.Slot.AttachComponent<BoxCollider>();
                        bc.CreateSpawnUndoPoint();
                        SetupNewCollider(bc, mc);
                        break;
                    case ReplacementColliderComponent.SphereCollider:
                        var sc = mc.Slot.AttachComponent<SphereCollider>();
                        sc.CreateSpawnUndoPoint();
                        SetupNewCollider(sc, mc);
                        break;
                    case ReplacementColliderComponent.ConvexHullCollider:
                        mc.Slot.AttachComponent<ConvexHullCollider>().CreateSpawnUndoPoint();
                        break;
                }
                mc.UndoableDestroy();
                _count++;
            });
            World.EndUndoBatch();
            PopulateList();
            ShowResults($"{_count} matching MeshColliders replaced with {replacementColliderComponent.ToString()}s.");
        }

        private void SetupNewCollider(BoxCollider bc, MeshCollider mc)
        {
            switch (setupBoundsType.Value)
            {
                case SetupBoundsType.None:
                    break;
                case SetupBoundsType.SetupFromLocalBounds:
                    bc.SetFromLocalBounds();
                    break;
                case SetupBoundsType.SetupFromGlobalBounds:
                    bc.SetFromGlobalBounds();
                    break;
            }
            if (PreserveColliderSettings)
            {
                bc.Type.Value = mc.Type.Value;
                bc.CharacterCollider.Value = mc.CharacterCollider.Value;
                bc.IgnoreRaycasts.Value = mc.IgnoreRaycasts.Value;
            }
            else
            {
                bc.Type.Value = setColliderType;
                bc.CharacterCollider.Value = SetCharacterCollider;
                bc.IgnoreRaycasts.Value = SetIgnoreRaycasts;
            }
        }

        private void SetupNewCollider(SphereCollider sc, MeshCollider mc)
        {
            switch (setupBoundsType.Value)
            {
                case SetupBoundsType.None:
                    break;
                case SetupBoundsType.SetupFromLocalBounds:
                    sc.SetFromLocalBounds();
                    break;
                case SetupBoundsType.SetupFromGlobalBounds:
                    sc.SetFromGlobalBounds();
                    break;
            }
            if (PreserveColliderSettings)
            {
                sc.Type.Value = mc.Type.Value;
                sc.CharacterCollider.Value = mc.CharacterCollider.Value;
                sc.IgnoreRaycasts.Value = mc.IgnoreRaycasts.Value;
            }
            else
            {
                sc.Type.Value = setColliderType;
                sc.CharacterCollider.Value = SetCharacterCollider;
                sc.IgnoreRaycasts.Value = SetIgnoreRaycasts;
            }
        }

        private void ShowResults(string results)
        {
            resultsText.Target.Content.Value = results;
        }
    }
}