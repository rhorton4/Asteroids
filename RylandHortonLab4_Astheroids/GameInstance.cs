//*****************************************************************************************
//Class:        GameInstance
//Description:  This class holds all relevant rules and object references for the current
//              game being played.
//*****************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using InputLayerSpace;
using IrrKlang;
using System.Threading;
using System.Diagnostics;

namespace RylandHortonLab4_Astheroids
{
    public delegate void DelVoidVoid();
    class GameInstance
    {
        //declare vars
        public enum GameState { PreGame, InGame, PauseGame, PostGame }              //the various game-states that the game can be in
        protected GameState _currentState;                                          //the game's current state
        protected Graphics _gr;                                                     //the current graphics window
        protected Rectangle _client;                                                //rect of the client window
        protected InputLayer _gameInputs;                                           //input buffer to grab inputs from
        protected List<RenderableObj> _allRenderable = new List<RenderableObj>();   //list of all renderable objects to draw to screen
        protected List<RenderableObj> _newObjects = new List<RenderableObj>();      //list of objects ready to be added to the renderable list
        protected List<MovingObj> _killList = new List<MovingObj>();                //list of objects ready to be removed from the renderable list
        protected ISoundEngine _soundEng = new ISoundEngine();                      //sound engine
        protected Thread _gameThread;                                               //thread that the game runs in
        protected bool gameIsAlive = true;                                          //bool communicates to threads if game is closing
        protected Ship _myShip;                                                     //the player's ship
        protected ScoreDisplay _sd;                                                 //the player's score
        protected LifeDisplay _ld;                                                  //the player's lives
        protected static Random _rng = new Random();                                //random number generator
        protected double shotsFired = 0;                                            //keeps track of time between shots fired    
        protected Stopwatch _rockTimer;                                             //keeps time between rock spawns
        protected Stopwatch _menuDebounce = new Stopwatch();                        //slows buffering of menu inputs to make it controllable
        protected int _secondsBetweenRockSpawns = 20;                               //time between rock spawns. lessens every 10000 pts.
        protected Menu _currentMenu;                                                //holds a menu being displayed, if at all
        protected MainForm _host;                                                   //reference to the windows form
        protected int _bonusLivesEarned = 1;                                        //multiplier of score to award lives on 

        public bool IsGameRunning { get { return gameIsAlive; } }                   //returns if game should be closed to other forms

        protected const double fireRate = 5;                                        //the interval at which a shot will be fired
        protected const int _rocksSplitInto = 3;                                    //how many rocks an asteroid splits into
        protected const int _fontSize = 24;                                         //font size of onscreen words
        protected const int _baseScore = 100;                                       //base score of an asteroid
        protected const int _maxRocksToStart = 5;                                   //how many rocks begin on the screen
        protected const int _livesStarting = 3;                                     //how many lives the player starts with
        protected const int _tickRate = 30;                                         //milliseconds between frames
        protected const int _difficultyIncreaseFactor = 4;                           //seconds faster the rocks spawn
        protected const int _defaultDifficulty = 20;

        //ctor takes information from host window and saves it
        public GameInstance(Graphics gr, Rectangle client, InputLayer inputs, MainForm hostWindow)
        {

            //host information stored here
            _host = hostWindow;
            _gameInputs = inputs;
            _gr = gr;
            _client = client;

            //initializes threads
            _rockTimer = new Stopwatch();
            _gameThread = new Thread(new ThreadStart(MainGameThread));
            _currentState = GameState.PreGame;
            _currentMenu = new Menu(_currentState, new PointF(_client.Width / 2, _client.Height / 2));
            _allRenderable.Add(_currentMenu);
            _gameThread.Start();

            //debugging line to disable controller override
            _gameInputs.ControllerEnabled = false;
        }
        //************************************************************************
        //Method Name:  NewGameInit()
        //Purpose       Initializes a new game for the player.
        //Parameters:   none
        //Returns:      void
        //************************************************************************
        private void NewGameInit()
        {
            //clears all renderable objects to start fresh. Add player ship
            _allRenderable.Clear();
            _secondsBetweenRockSpawns = _defaultDifficulty;
            _myShip = new Ship(new Point(_client.Width / 2, _client.Height / 2));
            _myShip.ActivateInvincibility();
            _allRenderable.Add(_myShip);

            //add score and life displays.
            _sd = new ScoreDisplay(new PointF(0, 0), _fontSize);
            _allRenderable.Add(_sd);

            _ld = new LifeDisplay(new PointF(_client.Width - 200, 0), _livesStarting, _fontSize);
            _allRenderable.Add(_ld);

            //add 5 rocks to start, and a test rock for debugging.
            for (int i = 0; i < _maxRocksToStart; ++i)
                _allRenderable.Add(new Rock(Rock.RockSize.Big, new PointF(_rng.Next(0, _client.Width), _rng.Next(0, _client.Height)), true));
            _allRenderable.Add(new TestRock(Rock.RockSize.Big, new PointF(0, 0), true));
            
        }
        //************************************************************************
        //Method name:  MainGameThread()
        //Purpose:      Hands off control to relevant method given the game's current state.
        //Parameters:   none
        //Returns:      void
        //************************************************************************
        public void MainGameThread()
        {
            //starts timer to add rocks to game.
            _rockTimer.Reset();
            _rockTimer.Start();
            
            //ensures user has not quit
            while (gameIsAlive)
            {
                using (BufferedGraphicsContext bgc = new BufferedGraphicsContext())
                {
                    using (BufferedGraphics bg = bgc.Allocate(_gr, _client))
                    {
                        //hand off to relevant handler method depending on game state
                        if (_currentState == GameState.InGame)
                            HandleGameLoop(bg);
                        else
                            HandleMenu();

                        //hand off to draw method for rendering
                        CleanUpAndDraw(bg);
                    }
                    //
                    Thread.Sleep(_tickRate);
                }                      
            }
            //quits all threads and form when user ends the game
            HandleExit();
        }
        //************************************************************************
        //Method name:  HandleGameLoop(BufferedGraphics bg)
        //Purpose:      Runs the game and applies all rules to the game objects.
        //Parameters:   BufferedGraphics bg - the graphics to check regions against
        //Returns:      void
        //************************************************************************
        private void HandleGameLoop(BufferedGraphics bg)
        {
            //rotates the ship on rotate controls
            if (_gameInputs.IsA_Down)
                _myShip.Rotate(RotationType.Left);
            if (_gameInputs.IsD_Down)
                _myShip.Rotate(RotationType.Right);

            //checks for forward or backward movement.
            if (_gameInputs.IsW_Down || _gameInputs.IsS_Down)
                _myShip.Move((_gameInputs.IsW_Down) ? Direction.Forward : Direction.Backward);
            else
                _myShip.Move(Direction.None);

            //shoots ship's weapon. Only shoots on intervals of fireRate.
            if (_gameInputs.IsSpace_Down)
            {
                if (shotsFired % fireRate == 0)
                {
                    shotsFired = 0;
                    _allRenderable.Add(new Bullet(new PointF(_myShip.Pos.X, _myShip.Pos.Y), _myShip.Heading));
                    _soundEng.Play2D(@"..\..\..\laserGun.wav");
                }
                shotsFired++;
            }            
            else
                shotsFired = 0;

            //checks if pause button is pressed.
            if (_gameInputs.IsEsc_Down)
            {
                _currentState = GameState.PauseGame;
                _currentMenu = new Menu(_currentState, new PointF(_client.Width / 2, _client.Height / 2));
                _allRenderable.Add(_currentMenu);
            }

            //checks to see if a new rock should be spawned.
            if (_rockTimer.Elapsed.Seconds > _secondsBetweenRockSpawns)
            {
                _rockTimer.Reset();
                _rockTimer.Start();
                _allRenderable.Add(new Rock(Rock.RockSize.Big, new PointF(_rng.Next(0, _client.Width), _rng.Next(0, _client.Height)), true));
            }

            //applies game rules to all objects.
            _allRenderable.ForEach(o =>
            {
                if (o is MovingObj)
                {
                    //move all objects one tick
                    MovingObj temp = (MovingObj)o;
                    temp.Tick(_client);

                    //applies rules to bullets. Checks for collisions and removes if necessary.
                    if (temp is Bullet)
                    {
                        Bullet currBullet = (Bullet)temp;
                        CheckBulletCollisionOnRocks(currBullet, bg.Graphics);
                        if (currBullet.FlaggedForRemoval)
                            _killList.Add(currBullet);
                    }

                    //applies rules to rocks. Checks for collisions with ships and ends game if necessary,
                    if (temp is Rock)
                    {
                        Rock currRock = (Rock)temp;
                        if (currRock.FlaggedForRemoval)
                            _killList.Add(currRock);

                        //Checks for ship collisions.
                        if (!_myShip.IsInvincible &&
                            !currRock.IsFadingIn &&
                            _myShip.IsClose(currRock) &&
                            _myShip.IsColliding(currRock, bg.Graphics))
                        {
                            _ld.TookDamage();
                            _myShip.SetPosition(new Point(_client.Width / 2, _client.Height / 2));
                            _soundEng.Play2D(@"..\..\..\takeDamage.wav");
                            if (_ld.CurrentLives > 0)
                                _myShip.ActivateInvincibility();

                            //ends game here if no more lives.
                            else
                            {
                                _currentState = GameState.PostGame;
                                _currentMenu = new Menu(GameState.PostGame, new PointF(_client.Width / 2, _client.Height / 2));
                                _newObjects.Add(_currentMenu);
                            }
                        }
                    }
                }                   
            });           
        }
        //************************************************************************
        //Method name:  CleanUpAndDraw(BufferedGraphics bg)
        //Purpose:      Removes objects flagged for removal, adds objects ready to be added,
        //              and renders to the current graphics.
        //Parameters:   BufferedGraphics bg - the graphics to render to.
        //Returns:      void
        //************************************************************************
        private void CleanUpAndDraw(BufferedGraphics bg)
        {
            _allRenderable.RemoveAll(o => _killList.Contains(o));
            _allRenderable.ForEach(o => o.Render(bg.Graphics));
            _allRenderable.AddRange(_newObjects);
            _newObjects.Clear();
            _killList.Clear();
            bg.Render();
        }
        //************************************************************************
        //Method name:  HandleMenu(BufferedGraphics bg)
        //Purpose:      Handles inputs and changes states depending on menu context
        //              displayed to user.
        //Parameters:   none
        //Returns:      void
        //************************************************************************ 
        private void HandleMenu()
        {  
            //moves cursor up.
            if (_gameInputs.IsW_Down && !_menuDebounce.IsRunning)
            {
                _currentMenu.MenuUp();
                _menuDebounce.Start();
            }               
            //moves cursor down.
            if (_gameInputs.IsS_Down && !_menuDebounce.IsRunning)
            {
                _currentMenu.MenuDown();
                _menuDebounce.Start();
            }                
            //checks for Enter key confirmation for current choice.
            //runs rules based on what was chosen.
            if (_gameInputs.IsEnter_Down)
            {
                switch (_currentMenu.UserSelected())
                {
                    case Menu.MenuOption.Exit:
                        gameIsAlive = false;
                        break;                    
                    case Menu.MenuOption.Start:
                    case Menu.MenuOption.Restart:
                        NewGameInit();
                        _currentState = GameState.InGame;
                        _allRenderable.Remove(_currentMenu);
                        _currentMenu = null;
                        _menuDebounce.Reset();
                        break;
                    case Menu.MenuOption.Resume:
                        _currentState = GameState.InGame;
                        _allRenderable.Remove(_currentMenu);
                        _currentMenu = null;
                        _menuDebounce.Reset();
                        break;
                }
            }
            //only 1 input every 100 ms.
            if (_menuDebounce.ElapsedMilliseconds >= 100)
                _menuDebounce.Reset();
        }
        //************************************************************************
        //Method name:  CheckBulletCollisionOnRocks(Bullet b, Graphics gr)
        //Purpose:      Checks to see if a bullet is hitting a rock.
        //Parameters:   Bullet b - the bullet to check
        //              Graphics gr - the graphics to check regions on
        //Returns:      void
        //************************************************************************
        private void CheckBulletCollisionOnRocks(Bullet b, Graphics gr)
        {
            //checks all rocks in renderable list
            _allRenderable.ForEach(o =>
            {
                if (o is Rock)
                {
                    Rock temp = (Rock)o;

                    //looks for collisions. If it is not close, collision is not checked.
                    if (!b.FlaggedForRemoval && 
                        b.IsClose(temp) &&
                        b.IsColliding(temp, gr))
                    {
                        //removes bullet on collision
                        b.Kill();
                        _soundEng.Play2D(@"..\..\..\rockExplode.wav");
                        
                        //adds smaller rocks if destroyed rock is big enough
                        if (temp.CurrentSize != Rock.RockSize.Small)
                        {
                            for (int i = 0; i < _rocksSplitInto; ++i)
                                _newObjects.Add(new Rock((temp.CurrentSize == Rock.RockSize.Big) ? Rock.RockSize.Medium : Rock.RockSize.Small, temp.Pos, false));
                        }
                        
                        //adds score. If high enough score achieved, adds a life and rocks spawn faster.
                        _sd.AddToScore(_baseScore * (int)temp.CurrentSize);
                        if (_sd.CurrentScore + _baseScore * (int)temp.CurrentSize > 10000 * _bonusLivesEarned)
                        {
                            ++_bonusLivesEarned;
                            _secondsBetweenRockSpawns -= _difficultyIncreaseFactor;
                            _ld.BonusLife();
                        }
                        
                        //remove the rock collided with.
                        temp.Kill();
                    }
                }
            });
        }
        //************************************************************************
        //Method name:  EndGame()
        //Purpose:      Ends the game.
        //Parameters:   none
        //Returns:      void
        //************************************************************************
        public void EndGame()
        {
            gameIsAlive = false;
        }
        //************************************************************************
        //Method name: HandleExit()
        //Purpose:     Exits all necessary threads when game ends.
        //Parameters:  none
        //Returns:     void
        //************************************************************************
        private void HandleExit()
        {
            try
            {
                _host.Invoke(new DelVoidVoid(_host.Close));
                _gameInputs.FormEnding();
            }
            catch
            {
                Console.Write("Game ended too early.");
            }
            
        }
    }
}
