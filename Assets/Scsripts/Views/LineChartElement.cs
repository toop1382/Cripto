using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cripto.Game.Views
{
    // Simple runtime UI Toolkit line chart element for sparklines
    public class LineChartElement : VisualElement
    {
        private readonly List<float> _data = new();
        private Color _lineColor = Color.green;
        private float _min = 0f;
        private float _max = 1f;
        private const float Padding = 2f;

        // Grid config (deciles)
        public bool ShowGrid = true;
        public int GridDivisions = 10; // 10 intervals => 11 lines
        public bool ShowGridLabels = true; // auto hidden on small heights
        private readonly Color _gridLineColor = new Color(1f, 1f, 1f, 0.12f);

        // Overlay label for showing the latest price value
        private readonly Label _priceTag;
        private Color _priceLineColor = new Color(1f, 1f, 1f, 0.28f);

        // Grid labels pool (left side)
        private readonly VisualElement _gridLabelsLayer;
        private readonly List<Label> _gridLabels = new();

        public new class UxmlFactory : UxmlFactory<LineChartElement, UxmlTraits> { }

        public LineChartElement()
        {
            generateVisualContent += OnGenerateVisualContent;
            RegisterCallback<GeometryChangedEvent>(_ => { UpdatePriceOverlay(); UpdateGridOverlay(); });
            style.flexGrow = 1f;
            style.height = 40f;

            // Grid labels layer
            _gridLabelsLayer = new VisualElement();
            _gridLabelsLayer.pickingMode = PickingMode.Ignore;
            _gridLabelsLayer.style.position = Position.Absolute;
            _gridLabelsLayer.style.left = 0;
            _gridLabelsLayer.style.top = 0;
            _gridLabelsLayer.style.right = 0;
            _gridLabelsLayer.style.bottom = 0;
            Add(_gridLabelsLayer);

            // Create a small label that sticks to the right side to show current price
            _priceTag = new Label("0.000000");
            _priceTag.pickingMode = PickingMode.Ignore;
            _priceTag.style.position = Position.Absolute;
            _priceTag.style.right = 0;
            _priceTag.style.top = 0; // will be positioned dynamically
            _priceTag.style.paddingLeft = 4;
            _priceTag.style.paddingRight = 4;
            _priceTag.style.paddingTop = 1;
            _priceTag.style.paddingBottom = 1;
            _priceTag.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            _priceTag.style.color = Color.white;
            _priceTag.style.unityFontStyleAndWeight = FontStyle.Normal;
            _priceTag.style.fontSize = 10;
            Add(_priceTag);
        }

        public void SetColor(Color color)
        {
            _lineColor = color;
            // Use a subtle variant of the line color for the price line if desired
            _priceLineColor = new Color(1f, 1f, 1f, 0.28f);
            MarkDirtyRepaint();
        }

        public void SetData(IReadOnlyList<float> series)
        {
            _data.Clear();
            if (series != null && series.Count > 0)
                _data.AddRange(series);

            // cache min/max for scaling
            if (_data.Count > 0)
            {
                _min = _data.Min();
                _max = _data.Max();
                if (Mathf.Approximately(_min, _max))
                {
                    _max = _min + 1e-6f;
                }
            }
            UpdateGridOverlay();
            UpdatePriceOverlay();
            MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            var rect = contentRect;
            if (_data.Count < 2 || rect.width <= 0 || rect.height <= 0)
            {
                UpdateGridOverlay();
                UpdatePriceOverlay();
                return;
            }

            var painter = mgc.painter2D;
            float w = rect.width - Padding * 2f;
            float h = rect.height - Padding * 2f;
            int n = _data.Count;

            float xStep = n > 1 ? w / (n - 1) : w;

            // Map point function
            Vector2 Map(int i)
            {
                float t = (_data[i] - _min) / (_max - _min);
                float x = rect.xMin + Padding + i * xStep;
                float y = rect.yMax - Padding - t * h;
                return new Vector2(x, y);
            }

            // Draw grid lines (deciles)
            if (ShowGrid && GridDivisions > 0)
            {
                painter.strokeColor = _gridLineColor;
                painter.lineWidth = 1f;
                for (int d = 0; d <= GridDivisions; d++)
                {
                    float t = (float)d / GridDivisions;
                    float y = rect.yMax - Padding - t * h;
                    painter.BeginPath();
                    painter.MoveTo(new Vector2(rect.xMin + Padding, y));
                    painter.LineTo(new Vector2(rect.xMax - Padding, y));
                    painter.Stroke();
                }
            }

            // Draw the price line at the latest value (horizontal reference)
            float last = _data[n - 1];
            float tLast = (_max - _min) > 0 ? (last - _min) / (_max - _min) : 0f;
            float yLine = rect.yMax - Padding - tLast * h;
            painter.strokeColor = _priceLineColor;
            painter.lineWidth = 1f;
            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.xMin + Padding, yLine));
            painter.LineTo(new Vector2(rect.xMax - Padding, yLine));
            painter.Stroke();

            // Draw the main sparkline
            painter.lineWidth = 2f;
            painter.strokeColor = _lineColor;
            painter.BeginPath();
            var p0 = Map(0);
            painter.MoveTo(p0);
            for (int i = 1; i < n; i++)
            {
                painter.LineTo(Map(i));
            }
            painter.Stroke();

            // Update overlays
            UpdateGridOverlay();
            UpdatePriceOverlay();
        }

        private void EnsureGridLabels(int count)
        {
            // Make sure we have exactly 'count' labels pooled
            while (_gridLabels.Count < count)
            {
                var lbl = new Label("0");
                lbl.pickingMode = PickingMode.Ignore;
                lbl.style.position = Position.Absolute;
                lbl.style.left = 0;
                lbl.style.paddingLeft = 2;
                lbl.style.paddingRight = 2;
                lbl.style.paddingTop = 0;
                lbl.style.paddingBottom = 0;
                lbl.style.fontSize = 9;
                lbl.style.color = new Color(0.85f, 0.9f, 1f, 0.9f);
                lbl.style.backgroundColor = new Color(0f, 0f, 0f, 0.45f);
                _gridLabelsLayer.Add(lbl);
                _gridLabels.Add(lbl);
            }
            for (int i = 0; i < _gridLabels.Count; i++)
            {
                _gridLabels[i].style.display = i < count ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void UpdateGridOverlay()
        {
            var rect = contentRect;
            bool canShowLabels = ShowGridLabels && rect.height >= 60f; // hide on tiny sparklines
            if (!ShowGrid || GridDivisions <= 0 || _data.Count == 0)
            {
                _gridLabelsLayer.style.display = DisplayStyle.None;
                return;
            }
            _gridLabelsLayer.style.display = canShowLabels ? DisplayStyle.Flex : DisplayStyle.None;
            if (!canShowLabels) return;

            float min = _min, max = _max;
            if (Mathf.Approximately(min, max)) max = min + 1e-6f;

            int lines = GridDivisions + 1; // 0..divisions
            EnsureGridLabels(lines);

            float h = rect.height - Padding * 2f;
            for (int d = 0; d <= GridDivisions; d++)
            {
                float t = (float)d / GridDivisions;
                float y = rect.yMax - Padding - t * h;
                float value = Mathf.Lerp(min, max, t);

                var lbl = _gridLabels[d];
                lbl.text = FormatValue(value, max - min);
                float top = Mathf.Clamp(y - 6f, rect.yMin, rect.yMax - 12f);
                lbl.style.top = top;
            }
        }

        private static string FormatValue(float v, float range)
        {
            // fewer decimals when the range is large
            if (range >= 1f) return v.ToString("F2");
            if (range >= 0.01f) return v.ToString("F4");
            return v.ToString("F6");
        }

        private void UpdatePriceOverlay()
        {
            if (_data.Count == 0) { _priceTag.text = ""; return; }
            var rect = contentRect;
            if (rect.height <= 0)
            {
                _priceTag.text = _data[_data.Count - 1].ToString("F6");
                return;
            }

            float last = _data[_data.Count - 1];
            float min = _min, max = _max;
            if (Mathf.Approximately(min, max)) max = min + 1e-6f;
            float t = (last - min) / (max - min);
            float y = rect.yMax - Padding - t * (rect.height - Padding * 2f);

            _priceTag.text = last.ToString("F6");
            // Position the label vertically centered on the line (approximate: offset by half tag height after layout if available)
            // We use a simple offset fallback of 8px
            float top = Mathf.Clamp(y - 8f, rect.yMin, rect.yMax - 16f);
            _priceTag.style.top = top;
        }
    }
}
