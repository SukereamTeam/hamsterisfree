public class Define
{
    public enum Scene
    {
        Intro,
        Lobby,
        Game,
    }

    public enum TileType
    {
        None,
        Exit,
        Seed,
        Monster
    }

    public enum MonsterTile_Direction
    {
        NONE = 0,
        LeftToRight,
        TopToBottom
    }
    
    public enum TileType_Sub
    {
        // https://github.com/SukereamTeam/hamsterisfree/issues/4
        // https://github.com/SukereamTeam/hamsterisfree/issues/20
        Default = 0,    // 기본 씨앗
        Disappear,      // 안녕 씨앗
        Fake,           // 위장 씨앗 (기회 카운트 -1)
        Heart,          // 하트 씨앗 (기회 카운트 +1)
        Moving,         // 이동 씨앗
        Fade,           // 빼꼼 씨앗
        Boss,           // Boss
    }
    
    public enum StageType
    {
        // https://github.com/eggmong/hamsterisfree/issues/5
        Default = 0,
        LimitTime,
        LimitTry
    }
    
    public enum TileSpriteName
    {
        Center = 0,
        Left,
        Right,
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Mask,
        Background,
        Map
    }

    public enum MapSize
    {
        Width = 8,
        Height = 11,
        
        In_XStart = 1,
        In_XEnd = 6,
        In_YStart = 0,
        In_YEnd = 8,
        Out_YStart = -1,
        Out_YEnd = 9
    }
    
}
