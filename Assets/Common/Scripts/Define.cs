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
        Mask
    }

    public enum SeedTile_Type
    {
        // https://github.com/users/eggmong/projects/1/views/1?pane=issue&itemId=35747000
        Default = 0,    // 기본 씨앗
        Disappear,      // 안녕 씨앗
        Fake,           // 위장 씨앗
        Moving,         // 이동 씨앗
        Appear,         // 짜잔 씨앗
        Fade,           // 빼꼼 씨앗
    }
}
