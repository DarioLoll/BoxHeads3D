using Unity.Netcode.Components;

public class ClientNetAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
