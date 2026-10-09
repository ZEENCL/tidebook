// IBiteTarget.cs - role: a biting fish the player can hook (used during the bite window).
namespace Tidebook
{
    public interface IBiteTarget
    {
        bool IsBiting { get; }
        bool Hook();
    }
}
