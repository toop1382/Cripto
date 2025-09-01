using System;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Cripto.Game.ViewModels
{
    // MVVM ViewModel for player movement input
    // Exposes Direction as observable and implements IJoystickInput so existing consumers can read it.
    public class PlayerInputViewModel : IInitializable, IDisposable
    {
        private readonly Subject<Vector2> _directionSubject = new();
        private readonly Subject<bool> _pressedSubject = new();
        private Vector2 _direction;
        private bool _pressed;

        public ReadOnlyReactiveProperty<Vector2> DirectionStream { get; }
        public ReadOnlyReactiveProperty<bool> IsPressedStream { get; }

        public Vector2 Direction => _direction;
        public float Horizontal => _direction.x;
        public float Vertical => _direction.y;

        [Inject]
        public PlayerInputViewModel()
        {
            DirectionStream = _directionSubject.ToReadOnlyReactiveProperty(Vector2.zero);
            IsPressedStream = _pressedSubject.ToReadOnlyReactiveProperty(false);
        }

        public void Initialize()
        {
            // Nothing to start yet
        }

        public void Dispose()
        {
            _directionSubject?.OnCompleted();
            _pressedSubject?.OnCompleted();
            _directionSubject?.Dispose();
            _pressedSubject?.Dispose();
        }

        // Called by Views (e.g., joystick view) when pointer moves
        public void SetInput(Vector2 input)
        {
            input = Vector2.ClampMagnitude(input, 1f);
            if (_direction == input) return;
            _direction = input;
            _directionSubject.OnNext(_direction);
        }

        // Called by Views on pointer down/up
        public void SetPressed(bool pressed)
        {
            if (_pressed == pressed) return;
            _pressed = pressed;
            _pressedSubject.OnNext(_pressed);
            if (!pressed)
            {
                // Reset direction on release to keep model consistent
                SetInput(Vector2.zero);
            }
        }
    }
}
