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
    }

    
}
