using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Avalonia.Controls.Shapes;
using Avalonia;
using Avalonia.Media;

namespace Avalonia_Firework.Views
{
    public partial class MainWindow : Window
    {
        private Canvas _canvas;
        private Random _random;
        private Color[] _colorSet1; // Define the first set of colors
        private Color[] _colorSet2;
        public MainWindow()
        {
            InitializeComponent();
            _random = new Random();

            // Define the first set of colors (orangered and orange)
            _colorSet1 = new Color[] { Colors.OrangeRed, Colors.Orange };

            // Define the second set of colors (purple and cyan)
            _colorSet2 = new Color[] { Colors.Purple, Colors.Red };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            _canvas = this.FindControl<Canvas>("Canvas");
        }


        private async void Canvas_OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            await ShowFireworks(e.GetPosition(_canvas));
        }

        private async Task ShowFireworks(Point position)
        {
            const int numLines = 20; // Number of lines to form
            const int particlesPerLine = 45; // Number of particles per line
            const double maxAngleDeviation = Math.PI / 4; // Maximum deviation angle from the vertical
            const double maxParticleSpeed = 300; // Maximum particle speed
            const double gravity = 400; // Gravity effect
            const int maxAnimationDuration = 3000; // Maximum animation duration
            const int minAnimationDuration = 2000; // Minimum animation duration

            var particleTasks = new List<Task>();

            for (int i = 0; i < numLines; i++)
            {
                double angle = 2 * Math.PI * i / numLines + (_random.NextDouble() - 0.5) * maxAngleDeviation;

                for (int j = 0; j < particlesPerLine; j++)
                {
                    double speed = _random.NextDouble() * maxParticleSpeed;
                    Point target = CalculateTarget(position, angle, speed);

                    int animationDuration = _random.Next(minAnimationDuration, maxAnimationDuration);

                    var particleTask = CreateParticleAndAnimate(position, target, animationDuration, gravity);
                    particleTasks.Add(particleTask);
                }
            }

            await Task.WhenAll(particleTasks);
        }

        private Point CalculateTarget(Point position, double angle, double speed)
        {
            double x = position.X + speed * Math.Cos(angle);
            double y = position.Y + speed * Math.Sin(angle);
            return new Point(x, y);
        }

        private async Task CreateParticleAndAnimate(Point position, Point target, int duration, double gravity)
        {

            // Randomly decide whether to use color set 1 or color set 2
            Color[] chosenColorSet = _random.Next(2) == 0 ? _colorSet1 : _colorSet2;

            // Randomly choose a color from the chosen color set
            Color particleColor = chosenColorSet[_random.Next(chosenColorSet.Length)];

            // Generate a random opacity for the particle
            byte opacity = (byte)_random.Next(128, 256); // Opacity ranges from 50% to 100%

            // Adjust the opacity of the chosen color to create varying shades
            particleColor = Color.FromArgb(opacity, particleColor.R, particleColor.G, particleColor.B);

            // Create a semi-transparent SolidColorBrush using the generated color
            SolidColorBrush brush = new SolidColorBrush(particleColor);

            Ellipse particle = new Ellipse
            {
                Width = 5,
                Height = 5,

                Fill = brush
            };

            _canvas.Children.Add(particle);
            Canvas.SetLeft(particle, position.X);
            Canvas.SetTop(particle, position.Y);

            await AnimateParticle(particle, target, duration, gravity);
        }

        private async Task AnimateParticle(Ellipse particle, Point target, int duration, double gravity)
        {
            double startX = Canvas.GetLeft(particle);
            double startY = Canvas.GetTop(particle);

            double deltaX = target.X - startX;
            double deltaY = target.Y - startY;

            int interval = 16; // Roughly 60 frames per second
            int steps = duration / interval;

            for (int i = 0; i <= steps; i++)
            {
                double t = (double)i / steps;
                double newX = startX + deltaX * t;
                double newY = startY + deltaY * t + 0.5 * gravity * Math.Pow(t, 2);

                particle.SetValue(Canvas.LeftProperty, newX);
                particle.SetValue(Canvas.TopProperty, newY);

                await Task.Delay(interval);
            }

            _canvas.Children.Remove(particle);
        }
    }
}
