using ComputerMasterClass.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ComputerMasterClass
{ 
    public partial class Form1 : Form
    {
        // Константы игровой логики
        private const int MoveStep = 7;
        private const int BackgroundSpeed = 12;
        private const int MinRandomSpeed = 3;
        private const int MaxRandomSpeed = 8;
        private const int FPS = 60;
        private const string HeaderStartText = "Начать игру";
        private const string StartText = "Вы хотите начать игру?";
        private const string HeaderCrashText = "Поражение";
        private const string CarsCrashText = "Оба игрока проиграли!\nХотите начать игру заново?";
        private const string LoseFirstPlayerText = "Первый игрок проиграл\nХотите начать игру заново?";
        private const string LoseSecondPlayerText = "Второй игрок проиграл\nХотите начать игру заново?";

        // Визуальные элементы
        private PictureBox FirstBackgroundPicture;
        private PictureBox SecondBackgroundPicture;
        private PictureBox CarSprite1;
        private PictureBox CarSprite2;
        private PictureBox Obstacle1;
        private PictureBox Obstacle2;
        private PictureBox Obstacle3;

        // Координаты для восстановления исходного состояния расположения
        // (для повторного запуска)
        private Point PointCarSprite1;
        private Point PointCarSprite2;
        private Point PointObstacle1;
        private Point PointObstacle2;
        private Point PointObstacle3;

        // Переменные игровой логики
        private Random random = new Random();
        private bool PressedA;
        private bool PressedD;
        private bool PressedLeft;
        private bool PressedRight;

        public Form1()
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            InitializeComponent();
            InitializeTimers();
            InitializeBackground();
            InitializeGameComponents();
        }

        private void StartGame()
        {
            StartTimer();
        }

        //private void RestartGame()
        //{
        //    CarSprite1.Location = PointCarSprite1;
        //    CarSprite2.Location = PointCarSprite2;
        //    Obstacle1.Location = PointObstacle1;
        //    Obstacle2.Location = PointObstacle2;
        //    Obstacle3.Location = PointObstacle3;
        //    StartGame();
        //}

        private void RestartGame(string MainText, string Caption)
        {
            StopTimers();
            DialogResult result = MessageBox.Show(
                MainText,
                Caption,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if (result == DialogResult.Yes)
            {
                PressedA = false;
                PressedD = false;
                PressedLeft = false;
                PressedRight = false;
                CarSprite1.Location = PointCarSprite1;
                CarSprite2.Location = PointCarSprite2;
                Obstacle1.Location = PointObstacle1;
                Obstacle2.Location = PointObstacle2;
                Obstacle3.Location = PointObstacle3;
                StartTimer();
            }
            else Application.Exit();
        }

        private void InitializeBackground()
        {
            FirstBackgroundPicture = pictureBox1;

            SecondBackgroundPicture = new PictureBox();
            SecondBackgroundPicture.Image = pictureBox1.Image;
            SecondBackgroundPicture.Size = new Size(pictureBox1.Width, pictureBox1.Height); 
            SecondBackgroundPicture.Location = new Point(0, -pictureBox1.Height);
            this.Controls.Add(SecondBackgroundPicture);

            // Бесшовность движения
            this.DoubleBuffered = true;
            timer1.Interval = (int) (FPS / 7.5);
        }

        private void InitializeTimers()
        {
            int Interval = (int)(FPS / 7.5);
            timer1.Interval = Interval;
            timer2.Interval = Interval;
            timer3.Interval = Interval;
        }

        private void StartTimer()
        {
            timer1.Start();
            timer2.Start();
            timer3.Start();
        }

        private void StopTimers()
        {
            timer1.Stop();
            timer2.Stop();
            timer3.Stop();                
        }

        private void InitializeGameComponents()
        {
            CarSprite1 = pictureBox2;
            PointCarSprite1 = CarSprite1.Location;
            CarSprite2 = pictureBox3;
            PointCarSprite2 = CarSprite2.Location;
            Obstacle1 = pictureBox4;
            PointObstacle1 = Obstacle1.Location;
            Obstacle2 = pictureBox5;
            PointObstacle2 = Obstacle2.Location;
            Obstacle3 = pictureBox6;
            PointObstacle3 = Obstacle3.Location;
        }

        private void MoveSprite(PictureBox Sprite, int StepX, int StepY)
        {
            Sprite.Location = new Point(
                Sprite.Location.X + StepX,
                Sprite.Location.Y + StepY
            );
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            SlidingBackgroundProcess();
            int randomStepY = random.Next(MinRandomSpeed, MaxRandomSpeed + 1);
            MoveObstacle(randomStepY, Obstacle1);

            // Движение автомобилей
            if (PressedA && CarSprite1.Location.X >= 0)
                MoveSprite(CarSprite1, -MoveStep, 0);
            if (PressedD && (CarSprite1.Location.X + CarSprite1.Size.Width) <= FirstBackgroundPicture.Width)
                MoveSprite(CarSprite1, MoveStep, 0);
            if (PressedLeft && CarSprite2.Location.X >= 0)
                MoveSprite(CarSprite2, -MoveStep, 0);
            if (PressedRight && (CarSprite2.Location.X + CarSprite2.Size.Width) <= FirstBackgroundPicture.Width)
                MoveSprite(CarSprite2, MoveStep, 0);

            // Проверка на столкновение автомбилей
            if (IsColliding(CarSprite1, CarSprite2))
                RestartGame(CarsCrashText, HeaderCrashText);

            // Проверка на столкновение первого автомобиля с бомбами
            if (IsColliding(CarSprite1, Obstacle1) ||
                    IsColliding(CarSprite1, Obstacle2) ||
                    IsColliding(CarSprite1, Obstacle3))
                RestartGame(LoseFirstPlayerText, HeaderCrashText);

            if (IsColliding(CarSprite2, Obstacle1) ||
                    IsColliding(CarSprite2, Obstacle2) ||
                    IsColliding(CarSprite2, Obstacle3))
                RestartGame(LoseSecondPlayerText, HeaderCrashText);

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.A:
                    PressedA = true;
                    return;
                case Keys.D:
                    PressedD = true;
                    return;
                case Keys.Left:
                    PressedLeft = true;
                    return;
                case Keys.Right:
                    PressedRight = true;
                    return;
                default:
                    return;
            }
        }

        private bool IsColliding(PictureBox CarSprite, PictureBox StarSprite)
        {
            return CarSprite.Bounds.IntersectsWith(StarSprite.Bounds);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                StartText, HeaderStartText,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question
            );
            if (result == DialogResult.Yes) StartGame();
            else Application.Exit();
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.A:
                    PressedA = false;
                    return;
                case Keys.D:
                    PressedD = false;
                    return;
                case Keys.Left:
                    PressedLeft = false;
                    return;
                case Keys.Right:
                    PressedRight = false;
                    return;
                default:
                    return;
            }
        }

        private void MoveObstacle(int RandomStepY, PictureBox Obstacle)
        {
            MoveSprite(Obstacle, 0, RandomStepY);
            if (Obstacle.Location.Y >= FirstBackgroundPicture.Height)
                Obstacle.Location = new Point(
                    random.Next(0, FirstBackgroundPicture.Width - Obstacle.Width + 1),
                    0
                );
        }

        private void SlidingBackgroundProcess()
        {
            // Движение фонового изображения для имитации движения
            MoveSprite(FirstBackgroundPicture, 0, BackgroundSpeed);
            if (FirstBackgroundPicture.Location.Y >= FirstBackgroundPicture.Height)
            {
                var backgroundPoint = FirstBackgroundPicture.Location;
                backgroundPoint.Y = 0;
                FirstBackgroundPicture.Location = backgroundPoint;
            }
            MoveSprite(SecondBackgroundPicture, 0, BackgroundSpeed);
            if (SecondBackgroundPicture.Location.Y >= 0)
            {
                var backgroundPoint = SecondBackgroundPicture.Location;
                backgroundPoint.Y = -SecondBackgroundPicture.Height;
                SecondBackgroundPicture.Location = backgroundPoint;
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            int RandomStepY = random.Next(MinRandomSpeed, MaxRandomSpeed + 1);
            MoveObstacle(RandomStepY, Obstacle2);
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            int RandomStepY = random.Next(MinRandomSpeed, MaxRandomSpeed + 1);
            MoveObstacle(RandomStepY, Obstacle3);
        }
    }
}
