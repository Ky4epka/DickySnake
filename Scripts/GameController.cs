using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

/*
    MainMenu
    Game
    GamePaused
    GameLoose
    GameWin
 */

public enum GameController_Difficulty
{
    Easy,
    Normal,
    Hard,
    Hardcore
}

public struct GameController_GameParams
{
    public GameController_Difficulty Difficulty;
    public string PlayerName;
    public int Points;

    public void Initialize()
    {
        PlayerName = "NewPlayer";
        Difficulty = GameController_Difficulty.Normal;
        Points = 0;
    }

}

public class GameController : MonoBehaviour, KeyboardEvents.IKeyHandler
{
    public Map Map = null;
    public Tilemap AssociatedTilemap = null;
    public Snake Snake = null;
    public SnakeController SnakeController = null;
    public FoodBase Food = null;
    public CameraController CameraController = null;
    public GameplayUI GUI = null;
    public sUIProcessor UI = null;
    public sUI_Menu_Group UI_PlayerNameMenu = null;
    public sUI_Menu_InputFieldControl UI_PlayerNameInput = null;
    public UI_Leaderboard UI_Leaderboard = null;
    public UI_LooseMenu UI_LooseMenu = null;
    public GameController_GameParams GameParams = new GameController_GameParams();

    public string UILayerName_MainMenu = "main_menu";
    public string UILayerName_PauseMenu = "pause_menu";
    public string UILayerName_VictoryMenu = "victory_menu";
    public string UILayerName_LooseMenu = "loose_menu";

    public int[] DifficultySnakeStepsPerSecond = null;

    public bool OnKeyDown(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.Escape:
                PauseGame();
                ActivateMenu(UILayerName_PauseMenu, true, true);
                break;

            default:
                return true;
        }

        return false;
    }

    public bool OnKeyUp(KeyCode key)
    {
        return true;
    }

    public bool OnKeyRepeat(KeyCode key)
    {
        return true;
    }

    public void StartNewGame()
    {
        ResetGame();
        ResumeGame();
    }

    public void ResumeGame()
    {
        if (AssociatedTilemap != null)
        {
            AssociatedTilemap.gameObject.SetActive(true);
        }

        UI.ClearHistory();
        GUI.gameObject.SetActive(true);
        ActivateMenu("", false, false);
    }

    public void PauseGame()
    {
        if (AssociatedTilemap != null)
        {
            AssociatedTilemap.gameObject.SetActive(false);
        }

        UI.ClearHistory();
        GUI.gameObject.SetActive(false);
        Snake.StopMove();
    }

    public void ResetGame()
    {
        Map.ClearMapCells();
        Snake.Length = GlobalStorage.Instance.Default_SnakeLength;
        Snake.M_HeadPosition = new Vector2Int(Map.Size.x / 2, Map.Size.y / 2);
        Food.ReplaceOnMap();
        GameParams.Points = 0;
        RefreshGameParams();
    }

    public void ActivateMenu(string menu_name, bool activate, bool save_to_history)
    {
        if (activate)
        {
            KeyboardEvents.sKeyboard_Events.Current.Handlers.BringToFront(UI);
            UI.ActivateMenu(menu_name, false, save_to_history);
        }
        else
        {
            KeyboardEvents.sKeyboard_Events.Current.Handlers.BringToFront(this);
            UI.ActivateMenu(null as sUI_Menu_Group, false, false);
        }
    }

    public void HideAnyMenu()
    {
        UI.ActivateMenu(null as sUI_Menu_Group, false, false);
    }
        
    protected void RefreshGameParams()
    {
        Snake.StepsPerSecond = DifficultySnakeStepsPerSecond[(int)GameParams.Difficulty];
        UI_PlayerNameInput.Input.text = GameParams.PlayerName;
        GUI.PlayerName.text = GameParams.PlayerName;
        GUI.Points.text = GameParams.Points.ToString();
    }

    public void SetDifficulty(GameController_Difficulty value)
    {
        GameParams.Difficulty = value;
        RefreshGameParams();
    }

    public void SetPlayerName(string player_name)
    {
        GameParams.PlayerName = player_name;
        RefreshGameParams();
    }

    public void SetPlayerPoints(int points)
    {
        GameParams.Points = points;
        RefreshGameParams();
    }

    public void GameFinished(bool victory)
    {
        UpdateLeaderboard();

        if (victory)
        {
            ActivateMenu(UILayerName_VictoryMenu, true, false);
        }
        else
        {
            ActivateMenu(UILayerName_LooseMenu, true, false);
            UI_LooseMenu.SetPoints(GameParams.Points, UI_Leaderboard.PlayerIndex(GameParams.PlayerName) == 0);
        }

        PauseGame();
    }
    
    public void UpdateLeaderboard()
    {
        UI_Leaderboard.UpdateLine(new UI_LeaderboardLineData(GameParams.PlayerName, GameParams.Points));
        UI_Leaderboard.SaveToFile(GlobalStorage.Instance.LeaderboardFile);
    }
    
    public void OnUI_DifficultyEasy(sUI_Menu_BaseControl sender)
    {
        SetDifficulty(GameController_Difficulty.Easy);
    }

    public void OnUI_DifficultyNormal(sUI_Menu_BaseControl sender)
    {
        SetDifficulty(GameController_Difficulty.Normal);
    }

    public void OnUI_DifficultyHard(sUI_Menu_BaseControl sender)
    {
        SetDifficulty(GameController_Difficulty.Hard);
    }

    public void OnUI_DifficultyHardcore(sUI_Menu_BaseControl sender)
    {
        SetDifficulty(GameController_Difficulty.Hardcore);
    }

    public void OnUI_PlayerName(sUI_Menu_BaseControl sender)
    {
        if (!(sender is sUI_Menu_InputFieldControl))
            return;

        sUI_Menu_InputFieldControl icontrol = sender as sUI_Menu_InputFieldControl;
        SetPlayerName(icontrol.Input.text);
        StartNewGame();
        HideAnyMenu();
    }
    
    public void OnUI_PauseMenu_Resume(sUI_Menu_BaseControl sender)
    {
        ResumeGame();
    }

    public void OnUI_PauseMenu_Restart(sUI_Menu_BaseControl sender)
    {
        ResetGame();
        ResumeGame();
    }

    public void OnUI_PauseMenu_ToMainMenu(sUI_Menu_BaseControl sender)
    {
        PauseGame();
        ActivateMenu(UILayerName_MainMenu, true, true);
    }


    public void OnMapResize(Map map, Vector2Int new_size)
    {
        Snake.NodePool.PoolCapacity = new_size.x * new_size.y;
        GUI.MapSizeChanged(new_size);
        Snake.InvalidateGraphic();
    }

    public void OnSnakeMapOutOfBounds(Snake sender)
    {
        GameFinished(false);
    }

    public void OnSnakeCollideWithSelf(Snake sender)
    {
        GameFinished(false);
    }

    public void OnSnakeCollision(Snake sender, object collide_with)
    {
        if (collide_with is FoodBase)
        {
            FoodBase food = collide_with as FoodBase;

            if (GlobalStorage.Instance.CurrentMap.FreeCellsCount > 1)
            {
                food.ReplaceOnMap();
                Snake.Length++;
                SetPlayerPoints(Snake.Length - GlobalStorage.Instance.Default_SnakeLength);

                if (Snake.Length == Snake.NodePool.PoolCapacity)
                {
                    GameFinished(true);
                }
            }
        }
    }

    public void Initialize()
    {
        Random.InitState(Time.frameCount);
        Map.AssociatedTilemap = AssociatedTilemap;
        Map.OnResize.AddListener(OnMapResize);
        Snake.NodePool.UseAutoGrow = false;
        Map.Resize(GlobalStorage.Instance.Default_MapSize);
        Snake.OnMapOutOfBounds.AddListener(OnSnakeMapOutOfBounds);
        Snake.OnCollideWithSelf.AddListener(OnSnakeCollideWithSelf);
        Snake.OnCollision.AddListener(OnSnakeCollision);
        GameParams.Initialize();
        Food = new FoodBase();
        Food.Tile = GlobalStorage.Instance.FoodTile;

        KeyboardEvents.sKeyboard_Events.Current.Handlers.AddLast(this);
        KeyboardEvents.sKeyboard_Events.Current.Handlers.AddLast(SnakeController);
        KeyboardEvents.sKeyboard_Events.Current.Handlers.AddLast(UI);

        UI_Leaderboard.InitData();
        UI_Leaderboard.LoadFromFile(GlobalStorage.Instance.LeaderboardFile);
        PauseGame();
        ActivateMenu(UILayerName_MainMenu, true, true);
    }

    // Use this for initialization
    void Start()
    {
        Initialize();
    }

}
