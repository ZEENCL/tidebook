// IRodInput.cs - the full rod input surface. Implement this once; each state only sees its own slice.
namespace Tidebook
{
    public interface IRodInput : ICastInput, IHookInput, IReelInput
    {
    }
}
