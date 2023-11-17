﻿using Services.Input;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;
using GameServices.Settings;

namespace Combat.Ai
{
    public interface IMouse
    {
        bool IsActive { get; set; }
        Vector2 Position { get; }
        bool ThrustButton { get; }
        bool FireButton(int index);
    }

    public class NullMouse : IMouse
    {
        public bool IsActive { get; set; }
        public Vector2 Position => Vector2.zero;
        public bool ThrustButton => false;
        public bool FireButton(int index) { return false; }
    }

    public class InputSystemMouse : IMouse, ITickable, IInitializable, IDisposable
    {
        public InputSystemMouse(UnityEngine.Camera camera, MouseEnabledSignal mouseEnabledSignal, GameSettings settings)
        {
            _camera = camera;
            _actions = new InputActions().Mouse;
            _gameSettings = settings;
            _mouseEnabledSignal = mouseEnabledSignal;
            _mouseEnabledSignal.Event += OnMouseEnabled;
        }

        public void Initialize()
        {
            Enabled = _gameSettings.UseMouse;
        }

        public void Dispose()
        {
            Enabled = false;
        }

        public void OnMouseEnabled(bool enabled)
        {
            Enabled = enabled;
        }

        public bool Enabled
        {
            get => _initialized;
            set
            {
                if (_initialized == value) return;
                _initialized = value;

                if (_initialized)
                {
                    Subscribe(_actions.MouseLook, OnMouseMove);
                    Subscribe(_actions.Thrust, OnThrust);
                    Subscribe(_actions.Action1, OnAction1);
                    Subscribe(_actions.Action2, OnAction2);
                    _actions.Enable();
                }
                else
                {
                    _actions.Disable();
                    Unsubscribe(_actions.MouseLook, OnMouseMove);
                    Unsubscribe(_actions.Thrust, OnThrust);
                    Unsubscribe(_actions.Action1, OnAction1);
                    Unsubscribe(_actions.Action2, OnAction2);
                }
            }
        }

        private static void Subscribe(InputAction action, Action<InputAction.CallbackContext> callback)
        {
            action.performed += callback;
            action.canceled += callback;
            action.started += callback;
        }

        private static void Unsubscribe(InputAction action, Action<InputAction.CallbackContext> callback)
        {
            action.performed -= callback;
            action.canceled -= callback;
            action.started -= callback;
        }

        private void OnMouseMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            IsActive = true;
            _screenPosition = context.ReadValue<Vector2>();
            if (_camera) _worldPosition = _camera.ScreenToWorldPoint(_screenPosition);
        }

        private void OnThrust(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            _thrust = context.ReadValueAsButton();
        }

        private void OnAction1(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            _action1 = context.ReadValueAsButton();
        }

        private void OnAction2(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            _action2 = context.ReadValueAsButton();
        }

        public bool IsActive 
        {
            get => _isActive && !_pointerOverUI;
            set 
            {
                _isActive = value;  
            }
        }

        public Vector2 Position => _worldPosition;

        public void Tick()
        {
            if (IsActive && _camera) 
                _worldPosition = _camera.ScreenToWorldPoint(_screenPosition);

            _pointerOverUI = EventSystem.current.IsPointerOverGameObject();
        }

        public bool ThrustButton => !_pointerOverUI && _thrust;
        public bool FireButton(int index)
        {
            if (_pointerOverUI)
                return false;

            switch (index)
            {
                case 0: return _action1;
                case 1: return _action2;
                default: return false;
            }
        }

        private InputActions.MouseActions _actions;

        private bool _thrust;
        private bool _action1;
        private bool _action2;

        private bool _isActive;
        private bool _pointerOverUI;
        private UnityEngine.Camera _camera;

        private Vector2 _worldPosition;
        private Vector2 _screenPosition;

        private bool _initialized;
        private GameSettings _gameSettings;
        private readonly MouseEnabledSignal _mouseEnabledSignal;
    }
}