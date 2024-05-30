using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AngryBirds
{
    public partial class MainWindow : Window
    {
        private bool isDragging;// взята ли птица клавишей в руку?
        private Point startPoint; // создание точки

        private double birdVelocityX;
        private double birdVelocityY;

        private const double Gravity = 1;
        private const double MaxDistance = 1900; // Максимальное расстояние, на которое птица может лететь
        private const double PowerScale = 0.1; // Масштаб для силы запуска птицы

        private Random random = new Random();

        private double slingshotStartPositionX;
        private double slingshotStartPositionY;

        private double birdStartPositionX;
        private double birdStartPositionY;

        private double pigStartPositionX;
        private double pigStartPositionY;

        private double coverStartPositionX;
        private double coverStartPositionY;

        private Polyline birdPath; // Путь птицы

        public MainWindow()
        {
            InitializeComponent();

            // Привязка обработчиков событий для рогатки
            slingshot.MouseLeftButtonDown += Slingshot_MouseLeftButtonDown;
            slingshot.MouseMove += Slingshot_MouseMove;
            slingshot.MouseLeftButtonUp += Slingshot_MouseLeftButtonUp;

            // Привязка обработчика события для кнопки Restart
            restartButton.Click += RestartGame_Click;

            slingshotStartPositionX = Canvas.GetLeft(slingshot);
            slingshotStartPositionY = Canvas.GetTop(slingshot);

            birdStartPositionX = Canvas.GetLeft(bird);
            birdStartPositionY = Canvas.GetTop(bird);

            pigStartPositionX = Canvas.GetLeft(pig);
            pigStartPositionY = Canvas.GetTop(pig);

            coverStartPositionX = Canvas.GetLeft(cover);
            coverStartPositionY = Canvas.GetTop(cover);

            // Создание пути птицы
            birdPath = new Polyline
            {
                Stroke = Brushes.PaleVioletRed,
                StrokeThickness = 2
            };

            // Добавление пути птицы на холст
            gameCanvas.Children.Add(birdPath);
        }

        private void Slingshot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            startPoint = e.GetPosition(gameCanvas);
            slingshot.CaptureMouse();
        }

        private void Slingshot_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentPosition = e.GetPosition(gameCanvas);
                double offsetX = currentPosition.X - startPoint.X;
                double offsetY = currentPosition.Y - startPoint.Y;

                Canvas.SetLeft(slingshot, Canvas.GetLeft(slingshot) + offsetX);
                Canvas.SetTop(slingshot, Canvas.GetTop(slingshot) + offsetY);

                startPoint = currentPosition;
            }
        }
        private void Slingshot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                slingshot.ReleaseMouseCapture(); //отпустить захват мышь не захватывает объект

                LaunchBird(); //вызываем метод 
            }
        }

        private void LaunchBird() // запуск птицы функция такой
        {
            double birdX = Canvas.GetLeft(bird);
            double birdY = Canvas.GetTop(bird);
            double slingshotX = Canvas.GetLeft(slingshot);
            double slingshotY = Canvas.GetTop(slingshot);

            double offsetX = birdX - slingshotX;
            double offsetY = birdY - slingshotY;

            double distance = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
            double power = distance * PowerScale; // сила запуска пьтцы умножить 

            double directionX = offsetX / distance; //направление по икс
            double directionY = offsetY / distance;

            birdVelocityX = directionX * power;
            birdVelocityY = directionY * power;

            // Расчет угла полета птицы
            double angle = Math.Atan2(birdVelocityY, birdVelocityX) * 180 / Math.PI;
            bird.RenderTransform = new RotateTransform(angle); // разворот птицы на угол

            CompositionTarget.Rendering += GameLoop; //для изобажения 

            double velocity = Math.Sqrt(birdVelocityX * birdVelocityX + birdVelocityY * birdVelocityY);

            angleText.Text = "Angle: " + angle.ToString("F2");
            velocityText.Text = "Velocity: " + velocity.ToString("F2");
        }

        private void GameLoop(object sender, EventArgs e)
        {
            // Обновление позиции птицы
            double birdX = Canvas.GetLeft(bird) + birdVelocityX;
            double birdY = Canvas.GetTop(bird) + birdVelocityY + Gravity;

            Canvas.SetLeft(bird, birdX);
            Canvas.SetTop(bird, birdY);

            // Добавление текущей позиции птицы в путь
            birdPath.Points.Add(new Point(birdX + bird.Width / 2, birdY + bird.Height / 2)); // чтобы мы рисовались траекториб всередиге

            // Проверка столкновений
            CheckCollision();

            // Проверка, достигла ли птица максимального расстояния или упала вниз
            if (birdY > gameCanvas.ActualHeight || birdX > gameCanvas.ActualWidth || birdX < 0 || Math.Abs(birdX - Canvas.GetLeft(slingshot)) > MaxDistance)
            {
                CompositionTarget.Rendering -= GameLoop;
                MessageBox.Show("Птица не достигла своей цели! Попробуйте еще раз.");
                ResetBird();
            }

            // Обновление скорости птицы с учетом гравитации
            birdVelocityY += Gravity;
        }

        private void ResetBird()
        {
            Canvas.SetLeft(bird, birdStartPositionX);
            Canvas.SetTop(bird, birdStartPositionY);
            birdVelocityX = 0;
            birdVelocityY = 0;

            // Очистка пути птицы
            birdPath.Points.Clear();

            // Возврат рогатки в изначальное положение
            Canvas.SetLeft(slingshot, slingshotStartPositionX);
            Canvas.SetTop(slingshot, slingshotStartPositionY);
        }

        private void CheckCollision()
        {
            Rect birdRect = new Rect(Canvas.GetLeft(bird), Canvas.GetTop(bird), bird.Width, bird.Height); // 
            Rect pigRect = new Rect(Canvas.GetLeft(pig), Canvas.GetTop(pig), pig.Width, pig.Height);
            Rect coverRect = new Rect(Canvas.GetLeft(cover), Canvas.GetTop(cover), cover.Width, cover.Height);

            if (birdRect.IntersectsWith(pigRect))
            {
                // Птица столкнулась с свиньей
                CompositionTarget.Rendering -= GameLoop;
                MessageBox.Show("Поздравляем! Вы попали в свинью!");
                ResetBird();
            }

            if (birdRect.IntersectsWith(coverRect))
            {
                // Птица столкнулась с преградой
                birdVelocityX = -birdVelocityX;
                birdVelocityY = -birdVelocityY;

                // Случайное смещение преграды при столкновении
                double offsetX = random.Next(10, 15);
                double offsetY = random.Next(10, 15);
                Canvas.SetLeft(cover, Canvas.GetLeft(cover) + offsetX);
                Canvas.SetTop(cover, Canvas.GetTop(cover) + offsetY);
            }
        }

        private void RestartGame_Click(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= GameLoop;
            ResetBird();

            // Очистка преград и свиней от случайного смещения
            Canvas.SetLeft(cover, coverStartPositionX);
            Canvas.SetTop(cover, coverStartPositionY);
            Canvas.SetLeft(pig, pigStartPositionX);
            Canvas.SetTop(pig, pigStartPositionY);

            LaunchBird();
        }
    }
}