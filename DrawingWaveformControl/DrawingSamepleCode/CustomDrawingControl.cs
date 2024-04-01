using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DrawingSamepleCode
{
    internal class CustomDrawingControl : Control
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var centerPoint = new Point(this.ActualWidth / 2, this.ActualHeight / 2);

            // 繪製線條
            Pen linePen = new Pen(Brushes.Black, 2);// 創建一個筆刷，定義線條的顏色（黑色）和寬度（2像素）
            drawingContext.DrawLine(linePen, new Point(10, 10), new Point(100, 10));// 使用DrawLine方法繪製線條，指定筆刷、起點和終點的坐標

            // 繪製矩形
            // 使用DrawRectangle方法繪製矩形，指定填充顏色（淺藍色）、無邊框（null）和矩形的位置及大小
            drawingContext.DrawRectangle(Brushes.LightBlue, null, new Rect(10, 20, 90, 60));

            // 繪製橢圓
            // 使用DrawEllipse方法繪製橢圓，指定填充顏色（淺綠色）、無邊框（null）、中心點坐標和橢圓的半徑（X軸和Y軸）
            drawingContext.DrawEllipse(Brushes.LightGreen, null, new Point(55, 120), 45, 30);


            // 繪製文字
            FormattedText formattedText = new FormattedText(
                "Hello, WPF!", // 文字內容
                System.Globalization.CultureInfo.CurrentCulture, // 使用當前文化信息
                FlowDirection.LeftToRight, // 文字流向
                new Typeface("Verdana"), // 字體
                16, // 字號
                Brushes.Black, // 文字顏色
                VisualTreeHelper.GetDpi(this).PixelsPerDip); //Render在不同DPI的顯示器上能夠自動調整

            var formattedTextPoint = centerPoint;
            formattedTextPoint.Offset(- formattedText.WidthIncludingTrailingWhitespace / 2, - formattedText.Height / 2);

            drawingContext.DrawText(formattedText, formattedTextPoint); // 指定文字的繪製位置 位於元件正中心
        }
    }
}
