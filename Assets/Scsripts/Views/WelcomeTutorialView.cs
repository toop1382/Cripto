using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cripto.Game.Services;

namespace Cripto.Game.Views
{
    /// <summary>
    /// Full-screen UI Toolkit overlay: first shows a guide banner to walk to the computer,
    /// then shows a short tutorial (first run only).
    /// </summary>
    public class WelcomeTutorialView : MonoBehaviour
    {
        private UIDocument _document;
        private VisualElement _root;
        private VisualElement _panel;
        private Label _title;
        private Label _body;
        private Button _nextButton;
        private VisualElement _guideBanner;
        private Label _guideText;
        private int _stepIndex;
        private List<(string title, string body)> _steps;
        private IWelcomeTutorialService _service;
        private bool _isIntro;

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
            _root.style.backgroundColor = new Color(0, 0, 0, 0.6f);
            _root.style.flexDirection = FlexDirection.Column;
            _root.style.alignItems = Align.Center;
            _root.style.justifyContent = Justify.FlexEnd;
            _root.visible = false;

            // Load shared stylesheet for Persian/RTL helpers if present
            var ss = Resources.Load<StyleSheet>("UIToolkit/common");
            if (ss != null) _root.styleSheets.Add(ss);

            // Guide banner (bottom)
            _guideBanner = new VisualElement
            {
                style =
                {
                    width = Length.Percent(100),
                    maxWidth = 800,
                    marginBottom = 12,
                    paddingLeft = 16,
                    paddingRight = 16,
                    paddingTop = 10,
                    paddingBottom = 10,
                    backgroundColor = new Color(0.08f, 0.08f, 0.12f, 0.95f),
                    borderTopLeftRadius = 10,
                    borderTopRightRadius = 10,
                    borderBottomLeftRadius = 10,
                    borderBottomRightRadius = 10,
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    justifyContent = Justify.SpaceBetween
                }
            };

            _guideText = new Label("برای شروع، در محیط به سمت کامپیوتر حرکت کن.")
            {
                style = { color = Color.white, unityTextAlign = TextAnchor.MiddleRight, fontSize = 16 }
            };
            _guideBanner.Add(_guideText);

            // Tutorial panel (center)
            _panel = new VisualElement
            {
                style =
                {
                    maxWidth = 640,
                    marginBottom = 80,
                    paddingLeft = 18,
                    paddingRight = 18,
                    paddingTop = 18,
                    paddingBottom = 18,
                    backgroundColor = new Color(0.12f, 0.12f, 0.16f, 0.98f),
                    borderTopLeftRadius = 12,
                    borderTopRightRadius = 12,
                    borderBottomLeftRadius = 12,
                    borderBottomRightRadius = 12,
                    flexDirection = FlexDirection.Column,
                    alignItems = Align.Stretch,
                    display = DisplayStyle.None
                }
            };

            _title = new Label
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 24,
                    marginBottom = 10,
                    color = Color.white,
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };

            _body = new Label
            {
                style =
                {
                    whiteSpace = WhiteSpace.Normal,
                    unityTextAlign = TextAnchor.UpperRight,
                    marginBottom = 16,
                    color = new Color(0.9f, 0.9f, 0.95f, 1f)
                }
            };

            _nextButton = new Button(OnNextClicked) { text = "ادامه" };
            _nextButton.style.height = 38;
            _nextButton.style.backgroundColor = new Color(0.2f, 0.5f, 0.9f, 1f);
            _nextButton.style.color = Color.white;
            _nextButton.style.borderTopLeftRadius = 8;
            _nextButton.style.borderTopRightRadius = 8;
            _nextButton.style.borderBottomLeftRadius = 8;
            _nextButton.style.borderBottomRightRadius = 8;

            _panel.Add(_title);
            _panel.Add(_body);
            _panel.Add(_nextButton);

            _root.Add(_panel);
            _root.Add(_guideBanner);

            // Steps content (Farsi)
            // After Intro, show anti‑fraud awareness tips specific to crypto safety.
            _steps = new List<(string title, string body)>
            {
                ("هشدار: سودِ قطعی؟",
                    "هرکس سود تضمینی می‌دهد، احتمالاً قصد کلاهبرداری دارد. همیشه تحقیق مستقل انجام بده و از منابع معتبر استفاده کن."),
                ("کلید خصوصی تو خصوصی است",
                    "هیچ‌وقت عبارت بازیابی (Seed Phrase) یا کلید خصوصی‌ات را در اختیار کسی قرار نده. حتی تیم‌های پشتیبانی واقعی هم آن را نمی‌خواهند."),
                ("لینک‌ها و قراردادها را بررسی کن",
                    "قبل از کلیک یا اتصال کیف‌پول، آدرس سایت و قرارداد را دقیق چک کن. از دامنه‌های جعلی، ایردراپ‌های مشکوک و لینک‌های ناشناس دوری کن.")
            };
        }

        // Show the intro panel first (what this game is).
        public void ShowIntro()
        {
            _isIntro = true;
            _root.visible = true;
            _guideBanner.style.display = DisplayStyle.None;
            _panel.style.display = DisplayStyle.Flex;

            _title.text = "خوش آمدید!";
            _body.text =
                "این تجربه فقط برای آموزش و آگاهی است؛ هدف ما کاهش کلاهبرداری در دنیای ارزهای دیجیتال با ارائه نکات ایمنی و شبیه‌سازی موقعیت‌هاست. لطفاً با دقت بخوان ";
            _nextButton.text = "ادامه";
        }

        // Show only the guide banner
        public void ShowGuide()
        {
            _isIntro = false;
            _root.visible = true;
            _guideBanner.style.display = DisplayStyle.Flex;
            _panel.style.display = DisplayStyle.None;
        }

        // Show the step-by-step tutorial
        public void ShowSteps()
        {
            _isIntro = false;
            _stepIndex = 0;
            ApplyStep();
            _root.visible = true;
            _guideBanner.style.display = DisplayStyle.None;
            _panel.style.display = DisplayStyle.Flex;
        }

        private void OnNextClicked()
        {
            if (_isIntro)
            {
                // After intro, switch to guide banner (don’t mark seen yet)
                ShowGuide();
                return;
            }

            _stepIndex++;
            if (_stepIndex >= _steps.Count)
            {
                _root.visible = false;
                _panel.style.display = DisplayStyle.None;
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