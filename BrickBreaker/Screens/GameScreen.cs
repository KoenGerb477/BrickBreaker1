﻿/*  Created by: 
 *  Project: Brick Breaker
 *  Date: 
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BrickBreaker.Screens;
using System.Media;
using System.Xml;
using System.Windows.Forms.Automation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace BrickBreaker
{
    public partial class GameScreen : UserControl
    {
        #region global values
        //image variables
        Image A = Properties.Resources.blocks_A1;
        Image B = Properties.Resources.blocks_B1;
        Image C = Properties.Resources.blocks_C1;
        Image D = Properties.Resources.blocks_D1;
        Image E = Properties.Resources.blocks_E1;
        Image Empty = Properties.Resources.blocks_empty1;
        Image rcCarTop = Properties.Resources.RC_top1;
        Image rcCarLeft = Properties.Resources.RC_top_left;
        Image rcCarRight = Properties.Resources.RC_top_right;
        Image ballig = Properties.Resources.toy_story_ball_down1;

        public static int width;
        public static int height;

        //player1 button control keys - DO NOT CHANGE
        Boolean leftArrowDown, rightArrowDown, spaceDown;

        // Game values
        int lives, counter, extraSpeed, counterInterval, difficulty; //0 is easy, 1 is medium, 2 is hard

        // Paddle and Ball objects
        Paddle paddle;
        Ball ball;

        List<Ball> balls = new List<Ball>();

        // lists for powerups
        List<Powers> powerList = new List<Powers>();

        Stopwatch breakTimer = new Stopwatch();
        Stopwatch gravityTimer = new Stopwatch();
        Stopwatch extendTimer = new Stopwatch();

        public static bool breakthroughBool;
        public static bool gravityBool;
        public static bool extendBool;

        // list of all blocks for current level
        List<Block> blocks = new List<Block>();

        // Brushes
        SolidBrush grayBrush = new SolidBrush(Color.Gray);
        SolidBrush ballBrush = new SolidBrush(Color.White);
        SolidBrush redBrush = new SolidBrush(Color.Red);
        SolidBrush yellowBrush = new SolidBrush(Color.Yellow);
        SolidBrush cyanBrush = new SolidBrush(Color.Cyan);
        SolidBrush greenBrush = new SolidBrush(Color.Green);
        SolidBrush purpleBrush = new SolidBrush(Color.Purple);

        //placeholder brushes for testing powerups
        SolidBrush breakThrough = new SolidBrush(Color.White);
        SolidBrush multiBall = new SolidBrush(Color.Blue);
        SolidBrush gravity = new SolidBrush(Color.Purple);
        SolidBrush extendPaddle = new SolidBrush(Color.Yellow);
        SolidBrush health = new SolidBrush(Color.Red);

        //declare random
        public static Random r = new Random();
        #endregion

        Rectangle rc_car = new Rectangle();

        Pen redbrush = new Pen(Color.Red);

        Stopwatch ballwatch = new Stopwatch();

        public GameScreen()
        {
            InitializeComponent();
            OnStart();
        }
        public void OnStart()
        {
            height = this.Height;
            width = this.Width;
            // make height and width variables


            //set life counter
            lives = 3;
            counter = extraSpeed = 0;

            List<Label> labels = new List<Label>();

            //set all button presses to false.
            leftArrowDown = rightArrowDown = spaceDown = false;

            //clear blocks list
            blocks.Clear();

            // setup starting paddle values and create paddle object

            int paddleWidth = 80;
            int paddleHeight = 105;
            int paddleX = ((this.Width / 2) - (paddleWidth / 2));
            int paddleY = (this.Height - paddleHeight) - 70;
            int paddleSpeed = 10;
            paddle = new Paddle(paddleX, paddleY, paddleWidth, paddleHeight, paddleSpeed, Color.White);

            // setup starting ball values
            int ballX = this.Width / 2 - 10;
            int ballY = this.Height - paddle.height - 80;

            // Creates a new ball
            int xSpeed = 8;
            int ySpeed = 8;
            int ballSize = 20;

            ball = new Ball(ballX, ballY, xSpeed, ySpeed, ballSize);
            balls.Add(ball);


            XmlReader reader = XmlReader.Create("Resources/firstLevel.xml");

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Text)
                {
                    Block b;
                    int x = Convert.ToInt32(reader.ReadString());
                    reader.ReadToFollowing("y");
                    int y = Convert.ToInt32(reader.ReadString());
                    reader.ReadToFollowing("hp");
                    int hp = Convert.ToInt32(reader.ReadString());
                    reader.ReadToFollowing("colour");
                    string color = reader.ReadString();
                    b = new Block(x, y, hp);

                    blocks.Add(b);
                }
            }

            reader.Close();

            rc_car.X = paddle.x;
            rc_car.Y = paddle.y;
            rc_car.Width = paddle.width;
            rc_car.Height = paddle.height;

            if (difficulty == 0)
            {
                counterInterval = 100;
            }
            else if (difficulty == 1)
            {
                counterInterval = 300;
            }
            else
            {
                counterInterval = 500;
            }

            // start the game engine loop
            gameTimer.Enabled = true;
        }
        private void GameScreen_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //player 1 button presses
            switch (e.KeyCode)
            {
                case Keys.Left:
                    leftArrowDown = true;
                    break;
                case Keys.Right:
                    rightArrowDown = true;
                    break;
                case Keys.Space:
                    spaceDown = true;
                    break;
                case Keys.Escape:
                    Application.Exit();
                    break;
                case Keys.P:
                    Form form = this.FindForm();
                    Pause_screen pausescreen = new Pause_screen();

                    pausescreen.Location = new Point((form.Width - pausescreen.Width) / 2, (form.Height - pausescreen.Height) / 2);

                    form.Controls.Add(pausescreen);
                    pausescreen.Focus();
                    form.Controls.Remove(this);
                    break;
            }
        }
        private void GameScreen_KeyUp(object sender, KeyEventArgs e)
        {
            //player 1 button releases
            switch (e.KeyCode)
            {
                case Keys.Left:
                    leftArrowDown = false;
                    break;
                case Keys.Right:
                    rightArrowDown = false;
                    break;
                case Keys.Space:
                    spaceDown = false;
                    break;
                default:
                    break;
            }
        }
        private void gameTimer_Tick(object sender, EventArgs e)
        {
            //test code to slow game down (will be removed for final game)
            if (spaceDown)
            {
                gameTimer.Interval = 100;
            }
            else
            {
                gameTimer.Interval = 1;
            }

            #region Move the paddle 
            if (leftArrowDown && paddle.x > 0)
            {
                paddle.Move("left");
            }
            if (rightArrowDown && paddle.x < (this.Width - paddle.width))
            {
                paddle.Move("right");
            }
            #endregion

            #region Move ball
            foreach (Ball b in balls)
            {
                b.Move();
            }
            #endregion

            #region collision
            // Check for collision with top and side walls
            foreach (Ball b in balls)
            {
                b.WallCollision(this);
            }

            // Check for ball hitting bottom of screen
            for (int i = 0; i < balls.Count; i++)
            {
                if (balls[i].BottomCollision(this))
                {
                    balls.RemoveAt(i);

                    if (balls.Count == 0)
                    {
                        gravityBool = false;
                        breakthroughBool = false;
                        extendBool = false;
                        gravityTimer.Reset();
                        gravityTimer.Stop();
                        breakTimer.Reset();
                        extendTimer.Reset();

                        lives--;

                        // Moves the ball back to origin
                        ball.xSpeed = 0;
                        ball.ySpeed = 0;
                        balls.Add(ball);
                        balls[i].x = ((paddle.x - (ball.size / 2)) + (paddle.width / 2));
                        balls[i].y = (this.Height - paddle.height) - 105;
                    }

                    if (lives == 0)
                    {
                        gameTimer.Enabled = false;
                        OnEnd();
                    }
                }
            }

            // Check for collision of ball with paddle, (incl. paddle movement)
            foreach (Ball b in balls)
            {
                b.PaddleCollision(paddle, extraSpeed);
            }

            // Check if ball has collided with any blocks
            foreach (Block b in blocks)
            {
                for (int i = 0; i < balls.Count; i++)
                {
                    if (balls[i].BlockCollision(b))
                    {
                        b.hp--;

                        //random chance to spawn a powerup
                        if (r.Next(1, 2) == 1)
                        {
                            Powers power = new Powers(b.x + (b.width / 2), b.y + (b.height / 2), "");
                            powerList.Add(power);
                        }
                        if (b.hp == 0)
                        {
                            blocks.Remove(b);
                        }

                        if (blocks.Count == 0)
                        {
                            gameTimer.Enabled = false;
                            OnEnd();
                        }

                        return;
                    }
                }

            }
            #endregion

            #region powers

            foreach (Powers p in powerList)
            {
                //move each powerBall
                p.Move();

                //check for paddle collision to see if the player deserves powerup
                if (p.Collision(paddle))
                {
                    //determine what kind of powerup it is
                    switch (p.type)
                    {
                        case "Breakthrough":
                            //unstoppable ball for duration of time
                            if (breakTimer.IsRunning == true)
                            {
                                breakTimer.Restart();
                            }
                            else
                            {
                                breakTimer.Start();
                                breakthroughBool = true;
                                ballBrush.Color = Color.LightBlue;
                            }
                            break;
                        case "Gravity":
                            //arc balls back upwards 
                            if (gravityTimer.IsRunning == true)
                            {
                                gravityTimer.Restart();
                            }
                            else
                            {
                                gravityTimer.Start();
                                gravityBool = true;
                                ballBrush.Color = Color.LightPink;
                            }
                            break;
                        case "Health":
                            //grants the player an extra life, capped at 5 lives
                            if (lives < 5)
                            {
                                lives++;
                            }
                            break;
                        case "MultiBall":
                            //creates a new ball 
                            Ball newBall = new Ball(ball.x, ball.y, ball.xSpeed * -1, ball.ySpeed, ball.size);
                            balls.Add(newBall);
                            break;
                        case "ExtendPaddle":
                            //extends paddle
                            if (extendTimer.IsRunning == true)
                            {
                                extendTimer.Restart();
                            }
                            else
                            {
                                extendTimer.Start();
                                extendBool = true;

                                paddle.width += 80;
                                paddle.x -= 40;
                                rc_car.X = paddle.x;
                                rc_car.Width = paddle.width;
                            }
                            break;
                    }
                    //delete the powerBall
                    powerList.Remove(p);
                    break;
                }
                // if powerBall goes offscreen, delete the ball
                if (p.y > this.Height + 50)
                {
                    powerList.Remove(p);
                    break;
                }
            }

            //check if duration has run out for each powerup

            //breakThrough
            if (4 < Convert.ToDouble(breakTimer.ElapsedMilliseconds / 1000))
            {
                breakTimer.Reset();
                ballBrush.Color = Color.White;
                breakthroughBool = false;
            }

            //extend poweru
            if (10 < Convert.ToDouble(extendTimer.ElapsedMilliseconds / 1000))
            {
                extendTimer.Reset();
                paddle.width -= 40;
                paddle.x += 20;
                rc_car.X = paddle.x;
                rc_car.Width = paddle.width;
                extendBool = false;
            }

            //gravity powerup
            if (7 < Convert.ToDouble(gravityTimer.ElapsedMilliseconds / 1000))
            {


                //speeding up the ball every 5 seconds
                counter++;
                if (counter % 5000 == 0)
                {
                    extraSpeed++;
                }

                if (counter % counterInterval == 0)
                {
                    foreach (Block b in blocks)
                    {
                        b.y += 10;
                        if (b.y >= paddle.y)
                        {
                            gameTimer.Enabled = false;
                            OnEnd();
                            break;
                        }
                    }
                }
                #endregion

                //redraw the screen
                Refresh();
            }
        }
        public void OnEnd()
        {
            breakTimer.Reset();
            gravityTimer.Reset();
            extendTimer.Reset();
            gravityBool = false;
            breakthroughBool = false;
            extendBool = false;

            // Goes to the game over screen
            Form form = this.FindForm();
            MenuScreen ps = new MenuScreen();

            ps.Location = new Point((form.Width - ps.Width) / 2, (form.Height - ps.Height) / 2);

            form.Controls.Add(ps);
            form.Controls.Remove(this);
        }

        public void GameScreen_Paint(object sender, PaintEventArgs e)
        {
            // Draws paddle

            //paddleBrush.Color = paddle.colour;
            e.Graphics.DrawRectangle(redbrush, paddle.x, paddle.y, paddle.width, paddle.height);
            if (leftArrowDown == true)
            {
                if (extendBool == true)
                {
                    paddle.width = 105 + 40;
                }
                else
                {
                    paddle.width = 105;
                }
                paddle.height = 80;
                paddle.width = 105;
                e.Graphics.DrawImage(rcCarRight, paddle.x, paddle.y);

            }
            else if (rightArrowDown == true)
            {
                if (extendBool == true)
                {
                    paddle.width = 105 + 40;
                }
                else
                {
                    paddle.width = 105;
                }
                paddle.height = 80;
                paddle.width = 105;
                e.Graphics.DrawImage(rcCarLeft, paddle.x, paddle.y);
            }
            else
            {
                paddle.height = 105;
                paddle.width = 80;
                e.Graphics.DrawImage(rcCarTop, paddle.x, paddle.y);
            }

            // Draws blocks
            foreach (Block b in blocks)
            {
                if (b.hp == 1)
                {
                    e.Graphics.DrawImage(A, b.x, b.y);
                }
                else if (b.hp == 2)
                {
                    e.Graphics.DrawImage(B, b.x, b.y);
                }
                else if (b.hp == 3)
                {
                    e.Graphics.DrawImage(C, b.x, b.y);
                }
                else if (b.hp == 4)
                {
                    e.Graphics.DrawImage(D, b.x, b.y);
                }
                else if (b.hp == 5)
                {
                    e.Graphics.DrawImage(E, b.x, b.y);
                }
                else if (b.hp > 5)
                {
                    e.Graphics.DrawImage(Empty, b.x, b.y);
                }
                else
                {
                    e.Graphics.FillRectangle(new SolidBrush(Color.Gray), b.x, b.y, b.width, b.height);
                }
            }


            // Draws powerups
            foreach (Powers p in powerList)
            {
                switch (p.type)
                {
                    case "Breakthrough":
                        e.Graphics.FillRectangle(breakThrough, p.x, p.y, Powers.powerupSize, Powers.powerupSize);
                        break;
                    case "Gravity":
                        e.Graphics.FillRectangle(gravity, p.x, p.y, Powers.powerupSize, Powers.powerupSize);
                        break;
                    case "Health":
                        e.Graphics.FillRectangle(health, p.x, p.y, Powers.powerupSize, Powers.powerupSize);
                        break;
                    case "MultiBall":
                        e.Graphics.FillRectangle(multiBall, p.x, p.y, Powers.powerupSize, Powers.powerupSize);
                        break;
                    case "ExtendPaddle":
                        e.Graphics.FillRectangle(extendPaddle, p.x, p.y, Powers.powerupSize, Powers.powerupSize);
                        break;
                }
            }

            // Draws balls
            foreach (Ball b in balls)
            {
                e.Graphics.DrawImage(ballig, b.x, Convert.ToInt32(b.y));
            }

            //Draw hearts
            int xVal = 10;
            for (int i = 0; i < lives; i++)
            {
                e.Graphics.DrawImage(ballig, xVal, this.Height - 40, 20, 20);
                xVal += 25;
            }

        }
    }
}