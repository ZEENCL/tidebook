// ICastInput.cs - input needed to charge, release and cancel a cast.
namespace Tidebook
{
    public interface ICastInput
    {
        bool ChargePressed { get; }
        bool ChargeReleased { get; }
        bool CancelPressed { get; }
    }
}
