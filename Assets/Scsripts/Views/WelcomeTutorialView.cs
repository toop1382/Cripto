using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cripto.Game.Services;

namespace Cripto.Game.Views
{
    /// <summary>
    /// Simple full-screen UI Toolkit overlay that shows a welcome and a short tutorial in Persian.
    /// Instantiated and controlled by WelcomeTutorialService.
    /// </summary>
    public class WelcomeTutorialView : MonoBehaviour
    {
        private UIDocument _document;
        private VisualElement _root;
        private Label _title;
        private Label _body;
        private Button _nextButton;
        private int _stepIndex;
        private List<(string title, string body)> _steps;
        private IWelcomeTutorialService _service;

        public void Construct(IWelcomeTutorialService service)
        {
            _service = service;
        }

        private void Awake()
        {
            _document = GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            BuildUI();
        }

        private void BuildUI()
        {
            _root = _document.rootVisualElement;
            _root.Clear();
            _root.style.position = Position.Absolute;
            _root.style.left = 0;
            _root.style.top = 0;
            _root.style.right = 0;
            _root.style.bottom = 0;
            _root.style.backgroundColor = new Color(0, 0, 0, 0.85f);
            _root.style.flexDirection = FlexDirection.Column;
            _root.style.alignItems = Align.Center;
            _root.style.justifyContent = Justify.Center;
            _root.visible = false;

            // Load shared stylesheet for Persian/RTL helpers if present
            var ss = Resources.Load<StyleSheet>("UIToolkit/common");
            if (ss != null) _root.styleSheets.Add(ss);

            var container = new VisualElement
            {
                style =
                {
                    maxWidth = 600,
                    paddingLeft = 16,
                    paddingRight = 16,
                    paddingTop = 16,
                    paddingBottom = 16,
                    backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.97f),
                    borderTopLeftRadius = 8,
                    borderTopRightRadius = 8,
                    borderBottomLeftRadius = 8,
                    borderBottomRightRadius = 8,
                    flexDirection = FlexDirection.Column,
                    alignItems = Align.Stretch
                }
            };

            _title = new Label
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 22,
                    marginBottom = 8, color = Color.white,
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };

            _body = new Label
            {
                style =
                {
                    whiteSpace = WhiteSpace.Normal,
                    unityTextAlign = TextAnchor.UpperRight,
                    marginBottom = 16, color = Color.white
                }
            };

            _nextButton = new Button(OnNextClicked) { text = "ادامه" };
            _nextButton.style.height = 36;

            container.Add(_title);
            container.Add(_body);
            container.Add(_nextButton);

            _root.Add(container);

            // Steps content (Farsi)
            _steps = new List<(string title, string body)>
            {
                ("خوش آمدید!",
                    "به بازی کریپتو خوش آمدید. در این بازی می‌توانید سکه‌ها را مشاهده کنید و اقدام به خرید و فروش کنید."),
                ("آموزش کوتاه",
                    "از لیست سکه‌ها یکی را انتخاب کنید تا جزئیات آن را ببینید. با دکمهٔ خرید یا فروش مقدار مورد نظر را وارد کنید."),
                ("نکته",
                    "موجودی کیف پول شما در بالای صفحه نمایش داده می‌شود. برای برگشت به لیست، دکمهٔ بازگشت را بزنید.")
            };
        }

        public void Show()
        {
            _stepIndex = 0;
            ApplyStep();
            _root.visible = true;
        }

        private void OnNextClicked()
        {
            _stepIndex++;
            if (_stepIndex >= _steps.Count)
            {
                _root.visible = false;
                _service?.MarkSeen();
                return;
            }

            ApplyStep();
        }

        private void ApplyStep()
        {
            var (t, b) = _steps[_stepIndex];
            _title.text = t;
            _body.text = b;
            _nextButton.text = _stepIndex == _steps.Count - 1 ? "شروع!" : "ادامه";
        }
    }
}